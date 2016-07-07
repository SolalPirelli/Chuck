using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Chuck.VisualStudio
{
    [DefaultExecutorUri( VsTestExecutor.Id )]
    [FileExtension( ".exe" )]
    [FileExtension( ".dll" )]
    public sealed class VsTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests( IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink )
        {
            VsCompatibility.UnregisterChannels();

            foreach( var assemblyPath in sources )
            {
                using( var manager = new AppDomainManager( assemblyPath ) )
                using( var proxy = manager.CreateProxy() )
                {
                    logger.SendMessage( TestMessageLevel.Informational, $"Discovering: {assemblyPath}" );

                    using( var sink = new VsTestDiscoverySink( discoverySink ) )
                    {
                        try
                        {
                            proxy.DiscoverTests( assemblyPath, sink );
                        }
                        catch( Exception e )
                        {
                            logger.SendMessage( TestMessageLevel.Error, e.ToString() );
                        }
                    }
                }
            }
        }
    }
}