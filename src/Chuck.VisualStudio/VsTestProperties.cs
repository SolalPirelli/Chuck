using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Chuck.VisualStudio
{
    public static class VsTestProperties
    {
        public static readonly TestProperty Name
            = TestProperty.Register( "ChuckName", "Chuck name", typeof( string ), typeof( VsTestProperties ) );

        public static readonly TestProperty TypeName
            = TestProperty.Register( "ChuckTypeName", "Chuck type name", typeof( string ), typeof( VsTestProperties ) );

        public static readonly TestProperty AssemblyName
            = TestProperty.Register( "ChuckAssemblyName", "Chuck assembly name", typeof( string ), typeof( VsTestProperties ) );
    }
}