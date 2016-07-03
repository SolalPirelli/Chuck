using System.Runtime.Remoting.Channels;

namespace Chuck.VisualStudio
{
    public sealed class VsCompatibility
    {
        /// <summary>
        /// Magic operation that prevents crashes because MSTest does weird things.
        /// 
        /// Other test runners do this too, e.g. xUnit and Fixie; see http://xunit.codeplex.com/workitem/9749.
        /// </summary>
        public static void UnregisterChannels()
        {
            foreach( var chan in ChannelServices.RegisteredChannels )
            {
                ChannelServices.UnregisterChannel( chan );
            }
        }
    }
}