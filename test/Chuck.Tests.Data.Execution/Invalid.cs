using System.Threading.Tasks;

namespace Chuck.Tests.Data.Execution
{
    public sealed class Invalid
    {
        public int Int() { return 0; }

        public Task<int> AsyncInt() { return Task.FromResult( 0 ); }

        public async void AsyncVoid() { await Task.Delay( 0 ); }
    }
}