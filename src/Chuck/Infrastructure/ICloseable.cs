namespace Chuck.Infrastructure
{
    // TODO this is required since cancellationtokens aren't serializable, but it's not very nice...
    public interface ICloseable
    {
        bool IsClosed { get; }
    }
}