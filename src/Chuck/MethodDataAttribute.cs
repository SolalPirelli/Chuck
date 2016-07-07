using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chuck.Infrastructure;
using Chuck.Utilities;

namespace Chuck
{
    public sealed class MethodDataAttribute : TestDataAttribute
    {
        public string MethodName { get; }

        public Type ContainingType { get; set; }

        public bool CacheValues { get; set; }


        public MethodDataAttribute( string methodName )
        {
            if( methodName == null )
            {
                throw new ArgumentNullException( nameof( methodName ) );
            }

            MethodName = methodName;
            CacheValues = true;
        }


        public override IEnumerable<TestData> GetData( TestExecutionContext context )
        {
            if( !CacheValues )
            {
                return GetValues( context );
            }

            var containingType = ContainingType ?? context.Test.Type;
            var cacheKey = $"{containingType.FullName}_{MethodName}";

            if( !context.ExtraData.Contains( typeof( MethodDataAttribute ), cacheKey ) )
            {
                context.ExtraData.Set( typeof( MethodDataAttribute ), cacheKey, GetValues( context ) );
            }

            return context.ExtraData.Get<TestData[]>( typeof( MethodDataAttribute ), cacheKey );
        }

        private TestData[] GetValues( TestExecutionContext context )
        {
            var containingType = ContainingType ?? context.Test.Type;

            var candidates = containingType.GetTypeInfo()
                                           .GetMethods( BindingFlags.Public | BindingFlags.Static )
                                           .Where( m => m.Name == MethodName )
                                           .ToArray();
            if( candidates.Length == 0 )
            {
                throw new TestCreationException(
                    $"No static method named {MethodName} found on type {containingType.FullName}."
                );
            }
            if( candidates.Length > 1 )
            {
                throw new TestCreationException(
                   $"More than one static method named {MethodName} was found on type {containingType.FullName}"
                );
            }

            var method = candidates[0];
            var targetParameters = context.Test.Method.GetParameters();
            if( targetParameters.Length == 1 )
            {
                var targetType = typeof( IEnumerable<> ).MakeGenericType( targetParameters[0].ParameterType );

                if( !targetType.IsAssignableFrom( method.ReturnType ) )
                {
                    throw new TestCreationException(
                        "Error while creating data for a test method with a single argument."
                      + Environment.NewLine
                     + $"The return type of method {MethodName} must be convertible "
                     + $"to IEnumerable<{targetParameters[0].ParameterType}>, "
                     + $"not {method.ReturnType.Name}."
                    );
                }

            }
            else
            {

                if( !typeof( IEnumerable<IEnumerable> ).IsAssignableFrom( method.ReturnType ) )
                {
                    throw new TestCreationException(
                        "Error while creating data for a test method with multiple arguments."
                      + Environment.NewLine
                     + $"The return type of method {MethodName} must be an enumerable of enumerables, "
                      + "such as IEnumerable<object[]>, IEnumerable<int>[] or HashSet<string[]>, "
                     + $"not {method.ReturnType.Name}."
                    );
                }
            }

            var items = method.Invoke( null, method.GetParameters()
                                                   .Select( p => context.Services.GetService( p.ParameterType ) )
                                                   .ToArray() );

            return ( (IEnumerable) items ).Cast<object>().Select( arg =>
            {
                object[] arrayArgs;

                if( targetParameters.Length == 1 )
                {
                    arrayArgs = new[] { arg };
                }
                else
                {
                    arrayArgs = ( (IEnumerable) arg ).Cast<object>().ToArray();
                }

                var name = PrettyPrinter.Print( context.Test.Method, arrayArgs );
                return new TestData( name, arrayArgs );
            } ).ToArray();
        }
    }
}