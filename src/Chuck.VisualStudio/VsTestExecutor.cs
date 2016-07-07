using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chuck.Infrastructure;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Chuck.VisualStudio
{
    [ExtensionUri( Id )]
    public sealed class VsTestExecutor : ITestExecutor
    {
        public const string Id = "executor://Chuck/v1";
        public static readonly Uri Uri = new Uri( Id );


        private CloseableSource _closeableSource = new CloseableSource();


        public void RunTests( IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle )
        {
            foreach( var assemblyPath in sources )
            {
                using( var manager = new AppDomainManager( assemblyPath ) )
                using( var proxy = manager.CreateProxy() )
                using( var sinkFactory = new VsTestResultSinkFactory( frameworkHandle, _closeableSource.Create() ) )
                {
                    proxy.RunTests( assemblyPath, sinkFactory ).WaitOne();
                }
            }
        }

        public void RunTests( IEnumerable<TestCase> testCases, IRunContext runContext, IFrameworkHandle frameworkHandle )
        {
            foreach( var assemblyTests in testCases.GroupBy( t => t.Source ) )
            {
                using( var manager = new AppDomainManager( assemblyTests.Key ) )
                using( var proxy = manager.CreateProxy() )
                {
                    var tests = assemblyTests.Select( testCase =>
                    {
                        var test = testCase.GetPropertyValue<Test>( VsTestProperties.Test, null );

                        if( test == null )
                        {
                            frameworkHandle.SendMessage( TestMessageLevel.Error, $"Test {test} is missing properties." );
                        }

                        return test;
                    } ).Where( t => t != null ).ToArray();

                    using( var sinkFactory = new VsTestResultSinkFactory( frameworkHandle, _closeableSource.Create() ) )
                    {
                        try
                        {
                            proxy.RunTests( tests, sinkFactory ).WaitOne();
                        }
                        catch( Exception e )
                        {
                            frameworkHandle.SendMessage( TestMessageLevel.Error, e.ToString() );
                        }
                    }
                }
            }

        }

        public void Cancel()
        {
            _closeableSource.Close();
            _closeableSource = new CloseableSource();
        }
    }
}