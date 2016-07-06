using System.Linq;
using System.Reflection;

namespace Chuck.Infrastructure
{
    public sealed class TestDiscoverer
    {
        public static void DiscoverTests( string assemblyLocation, ITestDiscoverySink discoverySink )
        {
            var assembly = Assembly.LoadFrom( assemblyLocation );
            if( assembly.GetCustomAttribute<TestAssemblyAttribute>() == null )
            {
                return;
            }

            foreach( var type in assembly.GetExportedTypes()
                                         .OrderBy( t => t.FullName )
                                         .Select( t => t.GetTypeInfo() ) )
            {
                DiscoverTests( type, discoverySink );
            }
        }

        private static void DiscoverTests( TypeInfo type, ITestDiscoverySink discoverySink )
        {
            if( type.IsValueType
             || ( type.IsAbstract && !type.IsSealed ) // C# 'static' is 'abstract sealed' in MSIL
             || type.GetCustomAttribute<NoTestsAttribute>() != null )
            {
                return;
            }

            foreach( var methodGroup in type.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static )
                                            .Where( m => m.DeclaringType != typeof( object ) )
                                            .GroupBy( m => m.Name )
                                            .OrderBy( g => g.Key )
                                            .Select( g => g.Key ) )
            {
                discoverySink.Discover( new TestMethod( type, methodGroup ) );
            }
        }
    }
}