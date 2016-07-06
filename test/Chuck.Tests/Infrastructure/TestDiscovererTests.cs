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
            var assembly = Assembly.Load( "Chuck.Tests.Data" );
            var tests = new TestList();
            TestDiscoverer.DiscoverTests( assembly.Location, tests );

            var assemblyName = assembly.GetName().FullName;
            Assert.Equal( new[]
            {
                new TestMethod( assemblyName, "Chuck.Tests.Data.Nested+Inner", "Method" ),
                new TestMethod( assemblyName, "Chuck.Tests.Data.Normal", "Async" ),
                new TestMethod( assemblyName, "Chuck.Tests.Data.Normal", "Int" ),
                new TestMethod( assemblyName, "Chuck.Tests.Data.Normal", "Method" ),
                new TestMethod( assemblyName, "Chuck.Tests.Data.Normal", "Static" ),
                new TestMethod( assemblyName, "Chuck.Tests.Data.Skipped", "Method" ),
                new TestMethod( assemblyName, "Chuck.Tests.Data.Skipped", "SkippedMethod" ),
                new TestMethod( assemblyName, "Chuck.Tests.Data.Static", "Method" ),
                new TestMethod( assemblyName, "Chuck.Tests.Data.Visibilities", "Public" )
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