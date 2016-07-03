using System;

namespace Chuck.Infrastructure
{
    [Serializable]
    public sealed class TestResult
    {
        /// <summary>
        /// Gets the display name of the result.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the reason the test was skipped, or null if the test was not skipped.
        /// </summary>
        public string SkipReason { get; }

        /// <summary>
        /// Gets the failure message, if the test failed in an assertion.
        /// </summary>
        public string FailureMessage { get; }

        /// <summary>
        /// Gets a description of the exception thrown by the test, if an error occurred during execution.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Gets the execution time, if available.
        /// </summary>
        public TestExecutionTime ExecutionTime { get; }


        public TestResult( string name, string skipReason, string failureMessage, string errorMessage, TestExecutionTime executionTime )
        {
            Name = name;
            SkipReason = skipReason;
            FailureMessage = failureMessage;
            ErrorMessage = errorMessage;
            ExecutionTime = executionTime;
        }


        public static TestResult Skipped( string name, string reason )
        {
            return new TestResult( name, reason, null, null, null );
        }
    }
}