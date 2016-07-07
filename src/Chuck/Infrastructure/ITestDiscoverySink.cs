namespace Chuck.Infrastructure
{
    public interface ITestDiscoverySink : ICloseable
    {
        void Discover( Test test );
    }
}