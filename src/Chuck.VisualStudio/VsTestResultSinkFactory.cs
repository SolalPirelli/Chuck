using Chuck.Infrastructure;
using Chuck.Remoting;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Chuck.VisualStudio
{
    public sealed class VsTestResultSinkFactory : LongLivedMarshalByRefObject, ITestResultSinkFactory
    {
        private readonly string _source;
        private readonly ITestExecutionRecorder _recorder;
        private readonly ICancellable _cancellable;


        public VsTestResultSinkFactory( string source, ITestExecutionRecorder recorder, ICancellable cancellable )
        {
            _source = source;
            _recorder = recorder;
            _cancellable = cancellable;
        }


        public ITestResultSink Create( TestMethod testMethod )
        {
            return new VsTestResultSink( testMethod, _source, _recorder, _cancellable );
        }
    }
}