using System;
using System.Linq;
using System.Reflection;

namespace Chuck.Infrastructure
{
    public sealed class TestDiscoverer
    {
        public static bool IsTestAssembly( Assembly assembly )
        {
            return assembly.GetCustomAttribute<TestAssemblyAttribute>() != null;
        }

        public static void DiscoverTests( Type type, ITestDiscoverySink discoverySink )
        {
            if( !( type.IsPublic || type.IsNestedPublic )
             || type.IsValueType
             || ( type.IsAbstract && !type.IsSealed ) // C# 'static' is 'abstract sealed' in MSIL
             || type.GetCustomAttribute<NoTestsAttribute>() != null )
            {
                return;
            }

            foreach( var method in type.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static )
                                       .Where( m => m.DeclaringType != typeof( object ) )
                                       .OrderBy( m => m.Name ) )
            {
                discoverySink.Discover( new Test( type, method ) );
            }
        }
    }
}