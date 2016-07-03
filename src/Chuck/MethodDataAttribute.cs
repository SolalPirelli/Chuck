using System;
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
            MethodName = methodName;
        }


        public override IEnumerable<TestData> GetData( TestExecutionContext executionContext, TestContext testContext )
        {
            if( !CacheValues )
            {
                return GetValues( executionContext, testContext );
            }

            var containingType = ContainingType ?? testContext.Method.DeclaringType;
            var cacheKey = $"Data:{containingType.FullName}_{MethodName}";

            if( !executionContext.ExtraProperties.ContainsKey( cacheKey ) )
            {
                executionContext.ExtraProperties[cacheKey] = GetValues( executionContext, testContext );
            }

            return (TestData[]) executionContext.ExtraProperties[cacheKey];
        }

        private TestData[] GetValues( TestExecutionContext executionContext, TestContext testContext )
        {
            var containingType = ContainingType ?? testContext.Method.DeclaringType;

            var candidates = containingType.GetTypeInfo().GetMethods( BindingFlags.Public | BindingFlags.Static );
            if( !candidates.Any() )
            {
                throw new TestCreationException(
                    $"No method named {MethodName} found on type {containingType.FullName}."
                );
            }
            if( candidates.Skip( 1 ).Any() )
            {
                throw new TestCreationException(
                   $"More than one method named {MethodName} was found on type {containingType.FullName}"
                );
            }

            var method = candidates.Single();
            // TODO can we relax this to allow any enumerable of any array type?
            if( method.ReturnType != typeof( IEnumerable<object[]> ) )
            {
                throw new TestCreationException(
                    $"The return type of method {MethodName} must be IEnumerable<object[]>, not {method.ReturnType.Name}."
                );
            }

            var items = method.Invoke( null, method.GetParameters()
                                                   .Select( p => executionContext.Services.GetService( p.ParameterType ) )
                                                   .ToArray() );
            return ( (IEnumerable<object[]>) items ).Select( args =>
            {
                var name = PrettyPrinter.Print( testContext.Method, args );
                return new TestData( name, args );
            } ).ToArray();
        }
    }
}