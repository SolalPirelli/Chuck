using Chuck.Infrastructure;
using Chuck.Remoting;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using VS = Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Chuck.VisualStudio
{
    public sealed class VsTestResultSink : LongLivedMarshalByRefObject, ITestResultSink
    {
        private readonly string _source;
        private readonly ITestExecutionRecorder _recorder;


        public bool IsCancelled { get; private set; }


        public VsTestResultSink( string source, ITestExecutionRecorder recorder )
        {
            _source = source;
            _recorder = recorder;
        }


        public ITestResultRecorder Record( TestMethod testMethod )
        {
            return new VsTestResultRecorder( this, testMethod );
        }

        public void Close()
        {
            IsCancelled = true;
        }


        private sealed class VsTestResultRecorder : LongLivedMarshalByRefObject, ITestResultRecorder
        {
            private readonly VsTestResultSink _sink;
            private readonly VS.TestCase _vsTestCase;
            private VS.TestOutcome _finalOutcome;
            private bool _atLeastOneSkipped;


            public bool IsCancelled => _sink.IsCancelled;


            public VsTestResultRecorder( VsTestResultSink sink, TestMethod testMethod )
            {
                _sink = sink;
                _vsTestCase = ConvertToTestCase( testMethod );

                _sink._recorder.RecordStart( _vsTestCase );
            }

            public void Record( TestResult result )
            {
                var vsTestResult = ConvertToTestResult( result );
                _sink._recorder.RecordResult( vsTestResult );

                switch( vsTestResult.Outcome )
                {
                    case VS.TestOutcome.Failed:
                    case VS.TestOutcome.NotFound:
                        _finalOutcome = vsTestResult.Outcome;
                        break;

                    case VS.TestOutcome.Skipped:
                        _atLeastOneSkipped = true;
                        break;
                }
            }

            public override void Dispose()
            {
                if( _finalOutcome == VS.TestOutcome.None )
                {
                    _finalOutcome = _atLeastOneSkipped ? VS.TestOutcome.Skipped : VS.TestOutcome.Passed;
                }

                _sink._recorder.RecordEnd( _vsTestCase, _finalOutcome );

                base.Dispose();
            }


            private VS.TestCase ConvertToTestCase( TestMethod testMethod )
            {
                return new VS.TestCase( testMethod.FullyQualifiedName, VsTestExecutor.Uri, _sink._source );
            }

            private VS.TestResult ConvertToTestResult( TestResult testResult )
            {
                var result = new VS.TestResult( _vsTestCase )
                {
                    DisplayName = testResult.Name
                };

                if( testResult.ExecutionTime != null )
                {
                    result.StartTime = testResult.ExecutionTime.Start;
                    result.Duration = testResult.ExecutionTime.Duration;
                    result.EndTime = result.StartTime + result.Duration;
                }

                if( testResult.SkipReason != null )
                {
                    result.Outcome = VS.TestOutcome.Skipped;
                    result.Messages.Add( new VS.TestResultMessage( VS.TestResultMessage.AdditionalInfoCategory, testResult.SkipReason ) );
                }
                else if( testResult.FailureMessage != null )
                {
                    result.Outcome = VS.TestOutcome.Failed;
                    result.ErrorMessage = testResult.FailureMessage;
                }
                else if( testResult.ErrorMessage != null )
                {
                    result.Outcome = VS.TestOutcome.Failed;
                    result.ErrorMessage = testResult.ErrorMessage;
                }
                else
                {
                    result.Outcome = VS.TestOutcome.Passed;
                }

                return result;
            }
        }
    }
}