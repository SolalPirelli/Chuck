using System;
using System.Collections.Generic;
using System.Linq;
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


        private Cancellable _cancellable;


        public void RunTests( IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle )
        {
            _cancellable = new Cancellable();

            foreach( var assemblyPath in sources )
            {
                using( var manager = new AppDomainManager( assemblyPath ) )
                using( var proxy = manager.CreateProxy() )
                using( var sinkFactory = new VsTestResultSinkFactory( assemblyPath, frameworkHandle,_cancellable ) )
                {
                    proxy.RunTests( assemblyPath, sinkFactory ).WaitOne();
                }
            }
        }

        public void RunTests( IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle )
        {
            _cancellable = new Cancellable();

            foreach( var assemblyTests in tests.GroupBy( t => t.Source ) )
            {
                using( var manager = new AppDomainManager( assemblyTests.Key ) )
                using( var proxy = manager.CreateProxy() )
                {
                    var testMethods = assemblyTests.Select( test =>
                    {
                        var assemblyName = test.GetPropertyValue<string>( VsTestProperties.AssemblyName, null );
                        var typeName = test.GetPropertyValue<string>( VsTestProperties.TypeName, null );
                        var name = test.GetPropertyValue<string>( VsTestProperties.Name, null );

                        if( name == null || typeName == null || assemblyName == null )
                        {
                            frameworkHandle.SendMessage( TestMessageLevel.Error, $"Test {test.DisplayName} is missing properties." );
                            return null;
                        }

                        return new TestMethod( assemblyName, typeName, name );
                    } ).Where( t => t != null ).ToArray();

                    using( var sinkFactory = new VsTestResultSinkFactory( assemblyTests.Key, frameworkHandle,_cancellable ) )
                    {
                        try
                        {
                            proxy.RunTests( testMethods, sinkFactory ).WaitOne();
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
            _cancellable.Cancel();
        }
    }
}