using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Chuck.Infrastructure;

namespace Chuck.Remoting
{
    public sealed class RemoteTestProxy : LongLivedMarshalByRefObject
    {
        public void DiscoverTests( string assemblyLocation, ITestDiscoverySink discoverySink )
        {
            var assembly = Assembly.LoadFrom( assemblyLocation );
            
            foreach( var type in assembly.GetTypes().OrderBy( t => t.FullName ) )
            {
                TestDiscoverer.DiscoverTests( type, discoverySink );
            }

        }

        public WaitHandle RunTests( string assemblyLocation, ITestResultSinkFactory resultSinkFactory )
        {
            var waitHandle = new ManualResetEvent( false );
            RunTests( assemblyLocation, resultSinkFactory, waitHandle );
            return waitHandle;
        }

        public WaitHandle RunTests( IReadOnlyList<Test> tests, ITestResultSinkFactory resultSinkFactory )
        {
            var waitHandle = new ManualResetEvent( false );
            RunTests( tests, resultSinkFactory, waitHandle );
            return waitHandle;
        }


        private async void RunTests( string assemblyLocation, ITestResultSinkFactory resultSinkFactory, EventWaitHandle waitHandle )
        {
            try
            {
                // TODO this is suboptimal, have a sink that immediately executes instead
                var tests = new TestList();
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

        private async void RunTests( IReadOnlyList<Test> tests, ITestResultSinkFactory resultSinkFactory, EventWaitHandle waitHandle )
        {
            try
            {
                using( var runner = new TestRunner() )
                {
                    foreach( var test in tests )
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


        private sealed class TestList : ITestDiscoverySink
        {
            public bool IsClosed { get; } = false;

            public List<Test> Values { get; } = new List<Test>();


            public void Discover( Test test )
            {
                Values.Add( test );
            }
        }
    }
}