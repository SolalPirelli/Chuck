namespace Chuck.Infrastructure
{
    public interface ITestDiscoverySink : ICancellable
    {
        void Discover( TestMethod testMethod );
    }
}