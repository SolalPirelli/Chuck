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


        public async Task RunAsync( TestMethod testMethod, ITestResultSink resultRecorder )
        {
            var assembly = Assembly.Load( testMethod.AssemblyName );
            var type = assembly.GetType( testMethod.TypeName );
            var methods = type.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static )
                              .Where( m => m.Name == testMethod.Name );

            foreach( var method in methods )
            {
                if( resultRecorder.IsCancelled )
                {
                    return;
                }

                await RunAsync( method, resultRecorder );
            }
        }

        public void Dispose()
        {
            _services.Dispose();
        }


        private async Task RunAsync( MethodInfo method, ITestResultSink resultRecorder )
        {
            var skipAttribute = method.GetCustomAttribute<SkipAttribute>() ?? method.DeclaringType.GetCustomAttribute<SkipAttribute>();
            if( skipAttribute != null )
            {
                resultRecorder.Record( TestResult.Skipped( method.Name, skipAttribute.Reason ) );
                return;
            }

            IEnumerable<Runner> runners = null;

            try
            {
                if( ReflectionUtilities.HasReturnValue( method ) )
                {
                    throw new TestCreationException(
                        $"Method {ReflectionUtilities.GetNameWithParameters( method )} returns a value, "
                       + "which is not allowed for test methods."
                    );
                }

                runners = GetRunners( method );
            }
            catch( TestCreationException e )
            {
                resultRecorder.Record( TestResult.Skipped( method.Name, e.Message ) );
                return;
            }
            catch( Exception e )
            {
                resultRecorder.Record( TestResult.Skipped( method.Name, "Internal error." + Environment.NewLine + e.ToString() ) );
                return;
            }

            foreach( var runner in runners )
            {
                if( resultRecorder.IsCancelled )
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
                    resultRecorder.Record( result );
                }
            }
        }

        private IEnumerable<Runner> GetRunners( MethodInfo testMethod )
        {
            if( testMethod.DeclaringType.IsGenericType )
            {
                throw new TestCreationException(
                    $"Type {testMethod.DeclaringType.FullName} has generic parameters, "
                   + "and thus cannot be used as a test container."
                   + Environment.NewLine
                   + "Either remove the parameters, or annotate it with the [NoTests] attribute."
                );
            }
            if( testMethod.IsGenericMethod )
            {
                throw new TestCreationException(
                    $"Method {ReflectionUtilities.GetNameWithParameters( testMethod )} has generic parameters, "
                   + "and thus cannot be used as a test method."
                );
            }

            var parameters = testMethod.GetParameters();
            var dataAttributes = testMethod.GetCustomAttributes<TestDataAttribute>();
            if( dataAttributes.Any() )
            {
                if( parameters.Length == 0 )
                {
                    throw new TestCreationException(
                        $"Method {ReflectionUtilities.GetNameWithParameters( testMethod )} takes no parameters, "
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
                        $"Method {ReflectionUtilities.GetNameWithParameters( testMethod )} takes parameters, "
                       + "but no test data was provided."
                       + Environment.NewLine
                       + "Use attributes such as [InlineData] to provide data for parameterized tests."
                    );
                }

                dataAttributes = NullDataAttribute.Instance;
            }

            var context = new TestExecutionContext( testMethod, _services, _extraData );

            return dataAttributes.SelectMany( d => d.GetData( context ) )
                                 .Select( d => GetRunner( testMethod, d ) );
        }

        private Runner GetRunner( MethodInfo testMethod, TestData data )
        {
            var parameters = testMethod.GetParameters();

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

            var isAsync = ReflectionUtilities.IsAwaitable( testMethod );

            return new Runner( data.Name,
                () =>
                {
                    var container = _containers.GetService( testMethod.DeclaringType );
                    try
                    {
                        var result = testMethod.Invoke( container, data.Arguments );

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
                return new[] { new TestData( context.Method.Name, null ) };
            }
        }
    }
}