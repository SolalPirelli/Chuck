namespace Chuck.Tests.Data.Discovery
{
    [Skip( "This class is skipped." )]
    public sealed class Skipped
    {
        public void Method() { }

        [Skip( "This method is skipped." )]
        public void SkippedMethod() { }
    }
}