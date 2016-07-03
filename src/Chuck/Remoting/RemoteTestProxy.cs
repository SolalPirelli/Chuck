using System.Collections.Generic;
using System.Threading;
using Chuck.Infrastructure;

namespace Chuck.Remoting
{
    public sealed class RemoteTestProxy : LongLivedMarshalByRefObject
    {
        public void LoadTests( string assemblyLocation, ITestDiscoverySink discoverySink )
        {
            TestProvider.LoadTests( assemblyLocation, discoverySink );
        }

        public WaitHandle RunTests( string assemblyLocation, ITestResultSink resultSink )
        {
            var waitHandle = new ManualResetEvent( false );
            RunTests( assemblyLocation, resultSink, waitHandle );
            return waitHandle;
        }

        public WaitHandle RunTests( IReadOnlyList<TestMethod> testMethods, ITestResultSink resultSink )
        {
            var waitHandle = new ManualResetEvent( false );
            RunTests( testMethods, resultSink, waitHandle );
            return waitHandle;
        }


        private async void RunTests( string assemblyLocation, ITestResultSink resultSink, EventWaitHandle waitHandle )
        {
            try
            {
                var tests = new ListDiscoverySink();
                LoadTests( assemblyLocation, tests );

                using( var runner = new TestRunner() )
                {
                    foreach( var test in tests.Values )
                    {
                        await runner.RunAsync( test, resultSink );
                    }
                }
            }
            finally
            {
                waitHandle.Set();
            }
        }

        private async void RunTests( IReadOnlyList<TestMethod> testMethods, ITestResultSink resultSink, EventWaitHandle waitHandle )
        {
            try
            {
                using( var runner = new TestRunner() )
                {
                    foreach( var test in testMethods )
                    {
                        await runner.RunAsync( test, resultSink );
                    }
                }
            }
            finally
            {
                waitHandle.Set();
            }
        }


        private sealed class ListDiscoverySink : ITestDiscoverySink
        {
            public bool IsCancelled { get; } = false;

            public List<TestMethod> Values { get; } = new List<TestMethod>();


            public void Discover( TestMethod testMethod )
            {
                Values.Add( testMethod );
            }
        }
    }
}