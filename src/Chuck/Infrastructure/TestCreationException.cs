using System;

namespace Chuck.Infrastructure
{
    /// <summary>
    /// Exception thrown during the creation of test cases to signal that a precondition was violated.
    /// </summary>
    public sealed class TestCreationException:Exception
    {
        public TestCreationException( string message ) : base( message ) { }
    }
}