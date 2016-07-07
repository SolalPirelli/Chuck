using Chuck.Infrastructure;
using Chuck.Remoting;

namespace Chuck.VisualStudio
{
    public sealed class CloseableSource : LongLivedMarshalByRefObject
    {
        private bool _closed;


        public ICloseable Create()
        {
            return new Closeable( this );
        }

        public void Close()
        {
            _closed = true;
        }


        private sealed class Closeable : LongLivedMarshalByRefObject, ICloseable
        {
            private readonly CloseableSource _source;


            public bool IsClosed => _source._closed;


            public Closeable( CloseableSource source )
            {
                _source = source;
            }
        }
    }
}