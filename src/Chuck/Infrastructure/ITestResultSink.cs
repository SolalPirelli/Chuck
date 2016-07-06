using System;

namespace Chuck.Infrastructure
{
    public interface ITestResultSink : ICancellable, IDisposable
    {
        void Record( TestResult result );
    }
}