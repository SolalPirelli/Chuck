using Chuck.Infrastructure;
using Chuck.Remoting;

namespace Chuck.VisualStudio
{
    public sealed class Cancellable : LongLivedMarshalByRefObject, ICancellable
    {
        public bool IsCancelled { get; private set; }


        public void Cancel()
        {
            IsCancelled = true;
        }
    }
}