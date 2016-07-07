using System.Reflection;
using Chuck.Infrastructure;
using Chuck.Remoting;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Chuck.VisualStudio
{
    public sealed class VsTestResultSinkFactory : LongLivedMarshalByRefObject, ITestResultSinkFactory
    {
        private readonly ITestExecutionRecorder _recorder;
        private readonly ICloseable _closeable;


        public VsTestResultSinkFactory( ITestExecutionRecorder recorder, ICloseable closeable )
        {
            _recorder = recorder;
            _closeable = closeable;
        }


        public ITestResultSink Create( Test test )
        {
            return new VsTestResultSink( test, _recorder, _closeable );
        }
    }
}