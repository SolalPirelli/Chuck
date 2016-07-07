using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Xunit;

namespace Chuck.Infrastructure.Tests
{
    public class TestDiscovererTests
    {
        [Fact]
        public void UnmarkedAssemblyIsNotTestAssembly()
        {
            var assemblyName = new AssemblyName( Guid.NewGuid().ToString() );
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
            Assert.False( TestDiscoverer.IsTestAssembly( assembly ) );
        }

        [Fact]
        public void MarkedAssemblyIsTestAssembly()
        {
            var assemblyName = new AssemblyName( Guid.NewGuid().ToString() );
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
            assembly.SetCustomAttribute(
                new CustomAttributeBuilder( typeof( TestAssemblyAttribute ).GetConstructor( Type.EmptyTypes ), Type.EmptyTypes )
            );
            Assert.True( TestDiscoverer.IsTestAssembly( assembly ) );
        }


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

        [Fact]
        public void NormalClass()
        {
            AssertTests( typeof( Normal ),
                GetTest( "Normal", "Async" ),
                GetTest( "Normal", "Int" ),
                GetTest( "Normal", "Method" ),
                GetTest( "Normal", "Static" )
            );
        }


        public static class Static
        {
            public static void Method() { }
        }

        [Fact]
        public void StaticClass()
        {
            AssertTests( typeof( Static ),
                GetTest( "Static", "Method" )
            );
        }


        public struct Struct
        {
            public void Method() { }
        }

        protected sealed class Protected
        {
            public void Method() { }
        }

        private sealed class Private
        {
            public void Method() { }
        }

        internal sealed class Internal
        {
            public void Method() { }
        }

        [NoTests]
        public sealed class MarkedWithNoTests
        {
            public void Method() { }
        }

        [Theory]
        [Xunit.InlineData( typeof( Struct ) )]
        [Xunit.InlineData( typeof( Protected ) )]
        [Xunit.InlineData( typeof( Private ) )]
        [Xunit.InlineData( typeof( Internal ) )]
        [Xunit.InlineData( typeof( MarkedWithNoTests ) )]
        public void NoTests( Type type )
        {
            var tests = new TestList();
            TestDiscoverer.DiscoverTests( type, tests );

            Assert.Empty( tests.Values );
        }


        private static void AssertTests( Type type, params Test[] expectedTests )
        {
            var tests = new TestList();
            TestDiscoverer.DiscoverTests( type, tests );

            Assert.Equal( expectedTests, tests.Values );
        }

        private static Test GetTest( string typeName, string methodName )
        {
            var type = typeof( TestDiscovererTests ).GetNestedType( typeName );
            var method = type.GetMethod( methodName );
            return new Test( type, method );
        }

        private sealed class TestList : ITestDiscoverySink
        {
            public List<Test> Values { get; } = new List<Test>();

            public bool IsClosed => false;


            public void Discover( Test test )
            {
                Values.Add( test );
            }
        }
    }
}