using Chuck.Infrastructure;

namespace Chuck.Remoting
{
    public interface ITestResultSinkFactory
    {
        ITestResultSink Create( TestMethod testMethod );
    }
}