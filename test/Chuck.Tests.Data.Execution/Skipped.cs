namespace Chuck.Tests.Data.Execution
{
    [Skip( "This class is skipped." )]
    public sealed class Skipped
    {
        public void Method() { }

        [Skip( "This method is skipped." )]
        public void SkippedMethod() { }
    }
}