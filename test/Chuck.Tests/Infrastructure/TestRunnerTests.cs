using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Chuck.Infrastructure.Tests
{
    public sealed class TestRunnerTests
    {
        [Theory]
        [Xunit.InlineData( "Not.An.Assembly", "Type", "Method",
                           "Could not load assembly Not.An.Assembly." )]
        [Xunit.InlineData( "Chuck.Tests", "NotAType", "Method",
                           "Type 'NotAType' not found in assembly Chuck.Tests." )]
        [Xunit.InlineData( "Chuck.Tests", "Chuck.Infrastructure.Tests.TestRunnerTests", "NotAMethod",
                           "Method 'NotAMethod' not found in type Chuck.Infrastructure.Tests.TestRunnerTests." )]
        public async Task InvalidMethod( string assemblyName, string typeName, string methodName, string expectedMessage )
        {
            var method = new TestMethod( assemblyName, typeName, methodName );

            using( var runner = new TestRunner() )
            {
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => runner.RunAsync( method, new ResultList() )
                );

                Assert.Equal( expectedMessage, ex.Message );
            }
        }

        [Theory]
        [Xunit.InlineData( "Skipped", "Method", 
                           "This class is skipped." )]
        [Xunit.InlineData( "Skipped", "SkippedMethod", 
                           "This method is skipped." )]
        [Xunit.InlineData( "Invalid", "Int",
                           "Method Int() returns a value, which is not allowed for test methods." )]
        [Xunit.InlineData( "Invalid", "AsyncInt",
                           "Method AsyncInt() returns a value, which is not allowed for test methods." )]
        [Xunit.InlineData( "Invalid", "AsyncVoid",
                           "Method AsyncVoid() is async but returns void, which is not allowed for test methods. Return Task instead." )]
        public async Task SkippedMethods( string typeName, string methodName, string expectedReason )
        {
            var method = GetTestMethod( typeName, methodName );
            var results = new ResultList();

            using( var runner = new TestRunner() )
            {
                await runner.RunAsync( method, results );
            }


            Assert.Equal( 1, results.Values.Count );
            var result = results.Values[0];

            Assert.Equal( methodName, result.Name );
            Assert.Equal( expectedReason, result.SkipReason );
            Assert.Null( result.FailureMessage );
            Assert.Null( result.ErrorMessage );
            Assert.Null( result.ExecutionTime );
        }


        private TestMethod GetTestMethod( string typeName, string methodName )
        {
            var assembly = Assembly.Load( "Chuck.Tests.Data.Execution" );
            return new TestMethod( assembly.GetType( "Chuck.Tests.Data.Execution." + typeName ), methodName );
        }

        private sealed class ResultList : ITestResultSink
        {
            public List<TestResult> Values { get; } = new List<TestResult>();

            public bool IsClosed => false;


            public void Record( TestResult result )
            {
                Values.Add( result );
            }

            public void Dispose()
            {
                // Nothing.
            }
        }
    }
}