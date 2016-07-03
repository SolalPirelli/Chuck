using System;
using System.Collections.Generic;

namespace Chuck.Infrastructure
{
    public sealed class TestFailureException : Exception
    {
        public IReadOnlyList<TestPartialResult> Results { get; }


        public TestFailureException( IReadOnlyCollection<TestPartialResult> results )
        {
            Results = new List<TestPartialResult>( results );
        }
    }
}