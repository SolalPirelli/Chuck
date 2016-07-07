using System.Collections.Generic;
using System.Threading;
using Chuck.Infrastructure;

namespace Chuck.Remoting
{
    public sealed class RemoteTestProxy : LongLivedMarshalByRefObject
    {
        public void DiscoverTests( string assemblyLocation, ITestDiscoverySink discoverySink )
        {
            TestDiscoverer.DiscoverTests( assemblyLocation, discoverySink );
        }

        public WaitHandle RunTests( string assemblyLocation, ITestResultSinkFactory resultSinkFactory )
        {
            var waitHandle = new ManualResetEvent( false );
            RunTests( assemblyLocation, resultSinkFactory, waitHandle );
            return waitHandle;
        }

        public WaitHandle RunTests( IReadOnlyList<TestMethod> testMethods, ITestResultSinkFactory resultSinkFactory )
        {
            var waitHandle = new ManualResetEvent( false );
            RunTests( testMethods, resultSinkFactory, waitHandle );
            return waitHandle;
        }


        private async void RunTests( string assemblyLocation, ITestResultSinkFactory resultSinkFactory, EventWaitHandle waitHandle )
        {
            try
            {
                // TODO this is suboptimal, have a sink that immediately executes instead
                var tests = new ListDiscoverySink();
                DiscoverTests( assemblyLocation, tests );

                using( var runner = new TestRunner() )
                {
                    foreach( var test in tests.Values )
                    {
                        using( var resultSink = resultSinkFactory.Create( test ) )
                        {
                            await runner.RunAsync( test, resultSink );
                        }
                    }
                }
            }
            finally
            {
                waitHandle.Set();
            }
        }

        private async void RunTests( IReadOnlyList<TestMethod> testMethods, ITestResultSinkFactory resultSinkFactory, EventWaitHandle waitHandle )
        {
            try
            {
                using( var runner = new TestRunner() )
                {
                    foreach( var test in testMethods )
                    {
                        using( var resultSink = resultSinkFactory.Create( test ) )
                        {
                            await runner.RunAsync( test, resultSink );
                        }
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
            public bool IsClosed { get; } = false;

            public List<TestMethod> Values { get; } = new List<TestMethod>();


            public void Discover( TestMethod testMethod )
            {
                Values.Add( testMethod );
            }
        }
    }
}