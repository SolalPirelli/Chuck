using System;
using System.Threading.Tasks;
using Chuck.Infrastructure;

namespace Chuck.Tests.Data.Execution
{
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
}