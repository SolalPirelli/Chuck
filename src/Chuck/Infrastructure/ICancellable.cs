namespace Chuck.Infrastructure
{
    public interface ICancellable
    {
        bool IsCancelled { get; }
    }
}