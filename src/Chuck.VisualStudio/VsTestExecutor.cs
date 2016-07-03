using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private readonly List<VsTestResultSink> _openSinks = new List<VsTestResultSink>();


        public void RunTests( IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle )
        {
            foreach( var assemblyPath in sources )
            {
                using( var manager = new AppDomainManager( assemblyPath ) )
                using( var proxy = manager.CreateProxy() )
                using( var sink = new VsTestResultSink( assemblyPath, frameworkHandle ) )
                {
                    _openSinks.Add( sink );

                    proxy.RunTests( assemblyPath, sink ).WaitOne();

                    _openSinks.Remove( sink );
                }
            }
        }

        public void RunTests( IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle )
        {
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

                    using( var sink = new VsTestResultSink( assemblyTests.Key, frameworkHandle ) )
                    {
                        _openSinks.Add( sink );

                        try
                        {
                            proxy.RunTests( testMethods, sink ).WaitOne();
                        }
                        catch( Exception e )
                        {
                            frameworkHandle.SendMessage( TestMessageLevel.Error, e.ToString() );
                        }

                        _openSinks.Remove( sink );
                    }
                }
            }

        }

        public void Cancel()
        {
            foreach( var sink in _openSinks.ToArray() ) // Clone the collection to avoid race conditions
            {
                sink.Close();
            }
        }
    }
}