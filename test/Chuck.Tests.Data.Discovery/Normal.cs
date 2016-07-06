using System.Threading.Tasks;

namespace Chuck.Tests.Data.Discovery
{
    public sealed class Normal
    {
        public void Method() { }

        public static void Static() { }

        public Task Async() { return Task.FromResult( 0 ); }

        public int Int() { return 0; }
    }
}