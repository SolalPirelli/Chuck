using System;

namespace Chuck.Infrastructure
{
    public interface ITestResultRecorder : ICancellable, IDisposable
    {
        void Record( TestResult result );
    }
}