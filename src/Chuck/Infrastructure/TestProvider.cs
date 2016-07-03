using System.Linq;
using System.Reflection;

namespace Chuck.Infrastructure
{
    public sealed class TestProvider
    {
        private static readonly TypeInfo ObjectTypeInfo = typeof( object ).GetTypeInfo();


        public static void LoadTests( string assemblyLocation, ITestDiscoverySink discoverySink )
        {
            var assembly = Assembly.LoadFrom( assemblyLocation );
            if( assembly.GetCustomAttribute<TestAssemblyAttribute>() == null )
            {
                return;
            }

            foreach( var type in assembly.GetExportedTypes().Select( t => t.GetTypeInfo() ) )
            {
                LoadTests( type, type, discoverySink );
            }
        }

        private static void LoadTests( TypeInfo currentType, TypeInfo derivedType, ITestDiscoverySink discoverySink )
        {
            if( currentType == ObjectTypeInfo
             || currentType.IsValueType
             || currentType.IsAbstract
             || currentType.GetCustomAttribute<NoTestsAttribute>() != null )
            {
                return;
            }

            foreach( var methodGroup in currentType.DeclaredMethods.GroupBy( m => m.Name ).Select( g => g.Key ) )
            {
                discoverySink.Discover( new TestMethod( derivedType, methodGroup ) );
            }

            LoadTests( currentType.BaseType.GetTypeInfo(), derivedType, discoverySink );
        }
    }
}