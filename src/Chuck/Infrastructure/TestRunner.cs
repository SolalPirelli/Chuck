using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Chuck.Utilities;

namespace Chuck.Infrastructure
{
    public sealed class TestRunner : IDisposable
    {
        private static readonly Task CompletedTask = Task.FromResult( 0 );


        private readonly LazyServiceProvider _services;
        private readonly TestContainerCreator _containers;
        private readonly TestPropertyBag _extraData;


        public TestRunner()
        {
            _services = new LazyServiceProvider();
            _containers = new TestContainerCreator( _services );
            _extraData = new TestPropertyBag();
        }
        

        public async Task RunAsync( Test test, ITestResultSink resultSink )
        {
            var skipAttribute = test.Method.GetCustomAttribute<SkipAttribute>()
                             ?? test.Type.GetCustomAttribute<SkipAttribute>();
            if( skipAttribute != null )
            {
                resultSink.Record( TestResult.Skipped( test.Method.Name, skipAttribute.Reason ) );
                return;
            }

            IEnumerable<Runner> runners = null;

            try
            {
                if( ReflectionUtilities.IsAsyncVoid( test.Method ) )
                {
                    throw new TestCreationException(
                        $"Method {ReflectionUtilities.GetNameWithParameters( test.Method )} is async but returns void, "
                       + "which is not allowed for test methods. "
                       + "Return Task instead."
                    );
                }

                if( ReflectionUtilities.HasReturnValue( test.Method ) )
                {
                    throw new TestCreationException(
                        $"Method {ReflectionUtilities.GetNameWithParameters( test.Method )} returns a value, "
                       + "which is not allowed for test methods."
                    );
                }

                runners = GetRunners( test );
            }
            catch( TestCreationException e )
            {
                resultSink.Record( TestResult.Skipped( test.Method.Name, e.Message ) );
                return;
            }
            catch( Exception e )
            {
                resultSink.Record( TestResult.Skipped( test.Method.Name, "Internal error." + Environment.NewLine + e.ToString() ) );
                return;
            }

            foreach( var runner in runners )
            {
                if( resultSink.IsClosed )
                {
                    return;
                }

                var start = DateTimeOffset.Now;
                var watch = Stopwatch.StartNew();

                var partialResults = await RunRunnerAsync( runner.Body );

                watch.Stop();

                var multipleResults = partialResults.Count > 1;
                int index = 0;

                var results = partialResults.Select( r =>
                {
                    var name = runner.Name;
                    if( multipleResults )
                    {
                        name += Environment.NewLine;
                        name += $"Assertion {index + 1}";
                    }

                    return r.Build( name, new TestExecutionTime( start, watch.Elapsed ) );
                } );

                foreach( var result in results )
                {
                    resultSink.Record( result );
                }
            }
        }

        public void Dispose()
        {
            _services.Dispose();
        }


        private IEnumerable<Runner> GetRunners( Test test )
        {
            if( test.Type.IsGenericType )
            {
                throw new TestCreationException(
                    $"Type {test.Type.FullName} has generic parameters, "
                   + "and thus cannot be used as a test container."
                   + Environment.NewLine
                   + "Either remove the parameters, or annotate it with the [NoTests] attribute."
                );
            }
            if( test.Method.IsGenericMethod )
            {
                throw new TestCreationException(
                    $"Method {ReflectionUtilities.GetNameWithParameters( test.Method )} has generic parameters, "
                   + "and thus cannot be used as a test method."
                );
            }

            var parameters = test.Method.GetParameters();
            var dataAttributes = test.Method.GetCustomAttributes<TestDataAttribute>();
            if( dataAttributes.Any() )
            {
                if( parameters.Length == 0 )
                {
                    throw new TestCreationException(
                        $"Method {ReflectionUtilities.GetNameWithParameters( test.Method )} takes no parameters, "
                       + "but test data was provided."
                       + Environment.NewLine
                       + "Either remove the test data, or add parameters to the method."
                    );
                }
            }
            else
            {
                if( parameters.Length != 0 )
                {
                    throw new TestCreationException(
                        $"Method {ReflectionUtilities.GetNameWithParameters( test.Method )} takes parameters, "
                       + "but no test data was provided."
                       + Environment.NewLine
                       + "Use attributes such as [InlineData] to provide data for parameterized tests."
                    );
                }

                dataAttributes = NullDataAttribute.Instance;
            }

            var context = new TestExecutionContext( test, _services, _extraData );

            return dataAttributes.SelectMany( d => d.GetData( context ) )
                                 .Select( d => GetRunner( test, d ) );
        }

        private Runner GetRunner( Test test, TestData data )
        {
            var parameters = test.Method.GetParameters();

            if( data.Arguments == null )
            {
                if( parameters.Length > 0 )
                {
                    throw new TestCreationException(
                        $"No arguments specified, but there are {parameters.Length} parameters."
                    );
                }
            }
            else if( data.Arguments.Length != parameters.Length )
            {
                throw new TestCreationException(
                    $"{data.Arguments.Length} arguments specified, not matching the {parameters.Length} parameters."
                );
            }

            var isAsync = ReflectionUtilities.IsAwaitable( test.Method );

            return new Runner( data.Name,
                () =>
                {
                    var container = _containers.GetService( test.Type );
                    try
                    {
                        var result = test.Method.Invoke( container, data.Arguments );

                        if( isAsync )
                        {
                            return (Task) result;
                        }

                        return CompletedTask;
                    }
                    finally
                    {
                        ( container as IDisposable )?.Dispose();
                    }
                }
            );
        }

        private async Task<IReadOnlyList<TestPartialResult>> RunRunnerAsync( Func<Task> runner )
        {
            try
            {
                await runner();

                return new[] { TestPartialResult.Success() };
            }
            catch( TestFailureException e )
            {
                return e.Results;
            }
            catch( Exception e )
            {
                return new[] { TestPartialResult.Error( e ) };
            }
        }


        private sealed class Runner
        {
            public string Name { get; }

            public Func<Task> Body { get; }


            public Runner( string name, Func<Task> body )
            {
                Name = name;
                Body = body;
            }
        }

        private sealed class LazyServiceProvider : IServiceProvider, IDisposable
        {
            private readonly Dictionary<Type, Lazy<object>> _services = new Dictionary<Type, Lazy<object>>();


            public object GetService( Type serviceType )
            {
                if( !_services.ContainsKey( serviceType ) )
                {
                    var ctor = serviceType.GetTypeInfo()
                                          .DeclaredConstructors
                                          .FirstOrDefault( c => c.GetParameters().Length == 0 );
                    if( ctor == null )
                    {
                        throw new TestCreationException(
                            $"Type {serviceType} does not have a public parameterless constructor."
                        );
                    }

                    _services.Add( serviceType, new Lazy<object>( () => ctor.Invoke( null ) ) );
                }

                return _services[serviceType];
            }

            public void Dispose()
            {
                foreach( var lazy in _services.Values.Where( l => l.IsValueCreated ) )
                {
                    ( lazy.Value as IDisposable )?.Dispose();
                }
            }
        }

        private sealed class TestContainerCreator : IServiceProvider
        {
            private readonly IServiceProvider _servicesProvider;
            private readonly Dictionary<Type, Func<object>> _creators = new Dictionary<Type, Func<object>>();

            public TestContainerCreator( IServiceProvider servicesProvider )
            {
                _servicesProvider = servicesProvider;
            }


            public object GetService( Type serviceType )
            {
                AddContainer( serviceType );
                return _creators[serviceType]();
            }

            private void AddContainer( Type type )
            {
                if( _creators.ContainsKey( type ) )
                {
                    return;
                }

                if( type.IsAbstract && type.IsSealed )
                {
                    // It's a static type
                    _creators.Add( type, () => null );
                    return;
                }

                var ctors = type.GetTypeInfo().DeclaredConstructors;
                if( !ctors.Any() )
                {
                    throw new TestCreationException(
                        $"Type {type} is a test container but does not have any public constructors."
                       + Environment.NewLine
                       + "If you did not mean this type to be a test container, annotate it with the [NoTests] attribute."
                    );
                }
                if( ctors.Skip( 1 ).Any() )
                {
                    throw new TestCreationException(
                        $"Type {type} is a test container but has more than one public constructor, which is ambiguous."
                       + Environment.NewLine
                       + "If you did not mean this type to be a test container, annotate it with the [NoTests] attribute."
                    );
                }

                var ctor = ctors.Single();
                var parameters = ctor.GetParameters();
                foreach( var parameter in parameters )
                {
                    // Force initialization
                    _servicesProvider.GetService( parameter.ParameterType );
                }

                _creators.Add( type, () =>
                {
                    var args = parameters.Select( p => _servicesProvider.GetService( p.ParameterType ) );
                    return ctor.Invoke( args.ToArray() );
                } );
            }

        }

        private sealed class NullDataAttribute : TestDataAttribute
        {
            public static readonly NullDataAttribute[] Instance = new[] { new NullDataAttribute() };

            public override IEnumerable<TestData> GetData( TestExecutionContext context )
            {
                return new[] { new TestData( context.Test.Method.Name, null ) };
            }
        }
    }
}