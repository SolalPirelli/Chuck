using Chuck.Infrastructure;
using Chuck.Remoting;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Chuck.VisualStudio
{
    public sealed class VsTestResultSinkFactory : LongLivedMarshalByRefObject, ITestResultSinkFactory
    {
        private readonly string _source;
        private readonly ITestExecutionRecorder _recorder;
        private readonly ICloseable _closeable;


        public VsTestResultSinkFactory( string source, ITestExecutionRecorder recorder, ICloseable closeable )
        {
            _source = source;
            _recorder = recorder;
            _closeable = closeable;
        }


        public ITestResultSink Create( TestMethod testMethod )
        {
            return new VsTestResultSink( testMethod, _source, _recorder, _closeable );
        }
    }
}