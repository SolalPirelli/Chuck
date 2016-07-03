using System;

namespace Chuck.Infrastructure
{
    public sealed class TestPartialResult
    {
        public bool IsSuccess => FailureMessage == null && Exception == null;

        /// <summary>
        /// Gets the failure message, if the test failed in an assertion.
        /// </summary>
        public string FailureMessage { get; }

        /// <summary>
        /// Gets the exception thrown by the test, if an error occurred during execution.
        /// </summary>
        public Exception Exception { get; }


        private TestPartialResult( string failureMessage, Exception exception )
        {
            FailureMessage = failureMessage;
            Exception = exception;
        }


        public TestResult Build( string name, TestExecutionTime executionTime = null )
        {
            return new TestResult( name, null, FailureMessage, Exception?.ToString(), executionTime );
        }


        public static TestPartialResult Success()
        {
            return new TestPartialResult( null, null );
        }

        public static TestPartialResult Failure( string message )
        {
            return new TestPartialResult( message, null );
        }

        public static TestPartialResult Error( Exception exception )
        {
            return new TestPartialResult( null, exception );
        }
    }
}