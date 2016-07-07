using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Chuck.Infrastructure.Tests
{
    public sealed class TestRunnerTests
    {
        [Skip( "This class is skipped." )]
        public sealed class Skipped
        {
            public void Method() { }

            [Skip( "This method is skipped." )]
            public void SkippedMethod() { }
        }

        public sealed class Invalid
        {
            public int Int() { return 0; }

            public Task<int> AsyncInt() { return Task.FromResult( 0 ); }

            public async void AsyncVoid() { await Task.Delay( 0 ); }
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
            var type = typeof( TestRunnerTests ).GetNestedType( typeName );
            var method = type.GetMethod( methodName );
            var results = new ResultList();

            using( var runner = new TestRunner() )
            {
                await runner.RunAsync( new Test( type, method ), results );
            }


            Assert.Equal( 1, results.Values.Count );
            var result = results.Values[0];

            Assert.Equal( methodName, result.Name );
            Assert.Equal( expectedReason, result.SkipReason );
            Assert.Null( result.FailureMessage );
            Assert.Null( result.ErrorMessage );
            Assert.Null( result.ExecutionTime );
        }


        public sealed class Normal
        {
            public void Success() { }

            public Task AsyncSuccess() { return Task.FromResult( 0 ); }


            public void Error() { throw new Exception( "Error!" ); }

            public Task AsyncError()
            {
                var source = new TaskCompletionSource<int>();
                source.SetException( new Exception( "Error!" ) );
                return source.Task;
            }


            public void Failure() { throw new TestFailureException( new[] { TestPartialResult.Failure( "Failed!" ) } ); }

            public Task AsyncFailure()
            {
                var source = new TaskCompletionSource<int>();
                source.SetException( new TestFailureException( new[] { TestPartialResult.Failure( "Failed!" ) } ) );
                return source.Task;
            }


            public void IndirectSuccess()
            {
                throw new TestFailureException( new[] { TestPartialResult.Success() } );
            }

            public Task AsyncIndirectSuccess()
            {
                var source = new TaskCompletionSource<int>();
                source.SetException( new TestFailureException( new[] { TestPartialResult.Success() } ) );
                return source.Task;
            }


            public void IndirectError()
            {
                throw new TestFailureException( new[] { TestPartialResult.Error( new Exception( "Error!" ) ) } );
            }

            public Task AsyncIndirectError()
            {
                var source = new TaskCompletionSource<int>();
                source.SetException( new TestFailureException( new[] { TestPartialResult.Error( new Exception( "Error!" ) ) } ) );
                return source.Task;
            }
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