using System.Threading.Tasks;

namespace Chuck.Tests.Data.Discovery
{
    public class Normal
    {
        public void Method() { }

        public static void Static() { }

        public Task Async() { return Task.FromResult( 0 ); }

        public int Int() { return 0; }

        protected void Protected() { }

        private void Private() { }

        internal void Internal() { }
    }
}