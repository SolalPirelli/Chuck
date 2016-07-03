namespace Chuck.Infrastructure
{
    public interface ITestResultSink : ICancellable
    {
        ITestResultRecorder Record( TestMethod testMethod );
    }
}