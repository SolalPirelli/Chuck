using System;
using System.Runtime.Remoting;

namespace Chuck.Remoting
{
    /// <summary>
    /// Custom MarshalByRefObject that has an explicitly managed lifetime.
    /// </summary>
    public abstract class LongLivedMarshalByRefObject : MarshalByRefObject, IDisposable
    {
        public override sealed object InitializeLifetimeService()
        {
            return null;
        }

        public virtual void Dispose()
        {
            RemotingServices.Disconnect( this );
        }
    }
}