using System;

namespace Chuck.Infrastructure
{
    public interface ITestResultSink : ICloseable, IDisposable
    {
        void Record( TestResult result );
    }
}