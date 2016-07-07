using System;
using Chuck.Infrastructure;
using Chuck.Remoting;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Chuck.VisualStudio
{
    public sealed class VsTestDiscoverySink : LongLivedMarshalByRefObject, ITestDiscoverySink
    {
        private readonly string _source;
        private readonly ITestCaseDiscoverySink _vsDiscoverySink;


        public bool IsClosed { get; private set; }


        public VsTestDiscoverySink( string source, ITestCaseDiscoverySink vsDiscoverySink )
        {
            _source = source;
            _vsDiscoverySink = vsDiscoverySink;
        }


        public void Discover( TestMethod testMethod )
        {
            if( IsClosed )
            {
                return;
            }

            var testCase = new TestCase( testMethod.FullyQualifiedName, VsTestExecutor.Uri, _source )
            {
                DisplayName = testMethod.Name
            };

            testCase.SetPropertyValue( VsTestProperties.Name, testMethod.Name );
            testCase.SetPropertyValue( VsTestProperties.TypeName, testMethod.TypeName );
            testCase.SetPropertyValue( VsTestProperties.AssemblyName, testMethod.AssemblyName );

            var location = default( TestLocation ); // TODO
            if( location != null )
            {
                testCase.CodeFilePath = location.FilePath;
                testCase.LineNumber = location.LineNumber;
            }

            _vsDiscoverySink.SendTestCase( testCase );
        }

        public void Close()
        {
            IsClosed = true;
        }
    }
}