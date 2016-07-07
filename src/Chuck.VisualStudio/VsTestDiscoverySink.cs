using System.Reflection;
using Chuck.Infrastructure;
using Chuck.Remoting;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Chuck.VisualStudio
{
    public sealed class VsTestDiscoverySink : LongLivedMarshalByRefObject, ITestDiscoverySink
    {
        private readonly ITestCaseDiscoverySink _vsDiscoverySink;


        public bool IsClosed { get; private set; }


        public VsTestDiscoverySink( ITestCaseDiscoverySink vsDiscoverySink )
        {
            _vsDiscoverySink = vsDiscoverySink;
        }


        public void Discover( Test test )
        {
            if( IsClosed )
            {
                return;
            }

            var testCase = VsConverter.ConvertTestCase( test );
            testCase.SetPropertyValue( VsTestProperties.Test, test );

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