using Chuck.Infrastructure;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Chuck.VisualStudio
{
    public static class VsTestProperties
    {
        public static readonly TestProperty Test
            = TestProperty.Register( "ChuckTest", "Chuck test", typeof( Test ), typeof( VsTestProperties ) );
    }
}