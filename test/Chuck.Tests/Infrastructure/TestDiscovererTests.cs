using System.Collections.Generic;
using System.Reflection;
using Chuck.Infrastructure;
using Xunit;

namespace Chuck.Tests.Infrastructure
{
    public sealed class TestDiscovererTests
    {
        [Fact]
        public void NoTestsIfAssemblyIsNotMarked()
        {
            var assembly = Assembly.Load( "Chuck" );
            var tests = new TestList();
            TestDiscoverer.DiscoverTests( assembly.Location, tests );

            Assert.Equal( 0, tests.Values.Count );
        }

        [Fact]
        public void AllTestsAreDiscovered()
        {
            var assembly = Assembly.Load( "Chuck.Tests.Data.Discovery" );
            var tests = new TestList();
            TestDiscoverer.DiscoverTests( assembly.Location, tests );

            var assemblyName = assembly.GetName().FullName;
            var ns = "Chuck.Tests.Data.Discovery";
            Assert.Equal( new[]
            {
                new TestMethod( assemblyName, ns + ".Nested+Inner", "Method" ),
                new TestMethod( assemblyName, ns + ".Normal", "Async" ),
                new TestMethod( assemblyName, ns + ".Normal", "Int" ),
                new TestMethod( assemblyName, ns + ".Normal", "Method" ),
                new TestMethod( assemblyName, ns + ".Normal", "Static" ),
                new TestMethod( assemblyName, ns + ".Skipped", "Method" ),
                new TestMethod( assemblyName, ns + ".Skipped", "SkippedMethod" ),
                new TestMethod( assemblyName, ns + ".Static", "Method" ),
                new TestMethod( assemblyName, ns + ".Visibilities", "Public" )
            }, tests.Values );
        }


        private sealed class TestList : ITestDiscoverySink
        {
            public List<TestMethod> Values { get; } = new List<TestMethod>();

            public bool IsCancelled => false;


            public void Discover( TestMethod testMethod )
            {
                Values.Add( testMethod );
            }
        }
    }
}