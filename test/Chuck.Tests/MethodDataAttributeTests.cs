using System;
using System.Collections.Generic;
using System.Linq;
using Chuck.Infrastructure;
using Xunit;

namespace Chuck.Tests
{
    public sealed class MethodDataAttributeTests
    {
        [Fact]
        public void CannotProvideNullMethodName()
        {
            Assert.Throws<ArgumentNullException>( () => new MethodDataAttribute( null ) );
        }


        public void SingleArgument( int n )
        {
            // Nothing.
        }

        public static IEnumerable<int> SingleArgumentAmbiguous( int n )
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<int> SingleArgumentAmbiguous( string s )
        {
            throw new NotImplementedException();
        }

        public static double SingleArgumentWrongReturnType1()
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<string> SingleArgumentWrongReturnType2()
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<int> SingleArgumentIEnumerable()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }

        public static int[] SingleArgumentArray()
        {
            return new[] { 1, 2, 3 };
        }

        public static HashSet<int> SingleArgumentSet()
        {
            return new HashSet<int> { 1, 2, 3 };
        }

        [Theory]
        [Xunit.InlineData( "DoesNotExist" )]
        [Xunit.InlineData( nameof( SingleArgumentAmbiguous ) )]
        [Xunit.InlineData( nameof( SingleArgumentWrongReturnType1 ) )]
        [Xunit.InlineData( nameof( SingleArgumentWrongReturnType2 ) )]
        public void ErrorOnSingleArgumentMethod( string name )
        {
            var test = GetTest( nameof( SingleArgument ) );
            var context = new TestExecutionContext( test, new FakeServiceProvider(), new TestPropertyBag() );

            var attr = new MethodDataAttribute( name );

            Assert.Throws<TestCreationException>( () => attr.GetData( context ) );
        }

        [Theory]
        [Xunit.InlineData( nameof( SingleArgumentIEnumerable ) )]
        [Xunit.InlineData( nameof( SingleArgumentArray ) )]
        [Xunit.InlineData( nameof( SingleArgumentSet ) )]
        public void SingleArgumentMethod( string name )
        {
            var test = GetTest( nameof( SingleArgument ) );
            var context = new TestExecutionContext( test, new FakeServiceProvider(), new TestPropertyBag() );

            var attr = new MethodDataAttribute( name );

            var dataSets = attr.GetData( context ).ToArray();

            Assert.Equal( 3, dataSets.Length );

            for( int n = 0; n < 3; n++ )
            {
                Assert.Equal( $"SingleArgument({n + 1})", dataSets[n].Name );
                Assert.Equal( new object[] { n + 1 }, dataSets[n].Arguments );
            }
        }



        public void MultipleArguments( int a, int b, int c )
        {
            // Nothing.
        }

        public static IEnumerable<int[]> MultipleArgumentsAmbiguous( int n )
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<int[]> MultipleArgumentsAmbiguous( string s )
        {
            throw new NotImplementedException();
        }

        public static double MultipleArgumentsWrongReturnType1()
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<double> MultipleArgumentsWrongReturnType2()
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<int[]> MultipleArgumentsIEnumerable()
        {
            yield return new[] { 1, 2, 3 };
        }

        public static int[][] MultipleArgumentsArray()
        {
            return new[] { new[] { 1, 2, 3 } };
        }

        public static HashSet<int[]> MultipleArgumentsSet()
        {
            return new HashSet<int[]> { new[] { 1, 2, 3 } };
        }

        [Theory]
        [Xunit.InlineData( "DoesNotExist" )]
        [Xunit.InlineData( nameof( MultipleArgumentsAmbiguous ) )]
        [Xunit.InlineData( nameof( MultipleArgumentsWrongReturnType1 ) )]
        [Xunit.InlineData( nameof( MultipleArgumentsWrongReturnType2 ) )]
        public void ErrorOnMultipleArgumentsMethod( string name )
        {
            var test = GetTest( nameof( MultipleArguments ) );
            var context = new TestExecutionContext( test, new FakeServiceProvider(), new TestPropertyBag() );

            var attr = new MethodDataAttribute( name );

            Assert.Throws<TestCreationException>( () => attr.GetData( context ) );
        }

        [Theory]
        [Xunit.InlineData( nameof( MultipleArgumentsIEnumerable ) )]
        [Xunit.InlineData( nameof( MultipleArgumentsArray ) )]
        [Xunit.InlineData( nameof( MultipleArgumentsSet ) )]
        public void MultipleArgumentsMethod( string name )
        {
            var test = GetTest( nameof( MultipleArguments ) );
            var context = new TestExecutionContext( test, new FakeServiceProvider(), new TestPropertyBag() );

            var attr = new MethodDataAttribute( name );

            var dataSets = attr.GetData( context ).ToArray();

            Assert.Equal( 1, dataSets.Length );
            Assert.Equal( $"{nameof( MultipleArguments )}(1, 2, 3)", dataSets[0].Name );
            Assert.Equal( new object[] { 1, 2, 3 }, dataSets[0].Arguments );
        }



        public sealed class DataProvider
        {
            public static int[][] GetData()
            {
                return new[] { new[] { 1, 2, 3 } };
            }
        }

        [Fact]
        public void UsingOtherType()
        {
            var test = GetTest( nameof( MultipleArguments ) );
            var context = new TestExecutionContext( test, new FakeServiceProvider(), new TestPropertyBag() );

            var attr = new MethodDataAttribute( nameof( DataProvider.GetData ) )
            {
                ContainingType = typeof( DataProvider )
            };

            var dataSets = attr.GetData( context ).ToArray();

            Assert.Equal( 1, dataSets.Length );
            Assert.Equal( $"{nameof( MultipleArguments )}(1, 2, 3)", dataSets[0].Name );
            Assert.Equal( new object[] { 1, 2, 3 }, dataSets[0].Arguments );
        }



        public interface IIntProvider
        {
            int Get();
        }

        public sealed class MyIntProvider : IIntProvider
        {
            public int Count { get; private set; }

            public int Get()
            {
                Count++;

                return 42;
            }
        }

        public static IEnumerable<int> DataWithProvider( IIntProvider provider )
        {
            yield return provider.Get();
        }

        [Fact]
        public void WithInjectedService()
        {
            var test = GetTest( nameof( SingleArgument ) );
            var serviceProvider = new FakeServiceProvider();
            serviceProvider.Services.Add( typeof( IIntProvider ), new MyIntProvider() );
            var context = new TestExecutionContext( test, serviceProvider, new TestPropertyBag() );

            var attr = new MethodDataAttribute( nameof( DataWithProvider ) );

            var dataSets = attr.GetData( context ).ToArray();

            Assert.Equal( 1, dataSets.Length );
            Assert.Equal( $"{nameof( SingleArgument )}(42)", dataSets[0].Name );
            Assert.Equal( new object[] { 42 }, dataSets[0].Arguments );
        }


        [Theory]
        [Xunit.InlineData( true, 1 )]
        [Xunit.InlineData( false, 2 )]
        public void Cache( bool cache, int expected )
        {
            var test = GetTest( nameof( SingleArgument ) );
            var serviceProvider = new FakeServiceProvider();
            var intProvider = new MyIntProvider();
            serviceProvider.Services.Add( typeof( IIntProvider ), intProvider );
            var context = new TestExecutionContext( test, serviceProvider, new TestPropertyBag() );

            var attr = new MethodDataAttribute( nameof( DataWithProvider ) )
            {
                CacheValues = cache
            };

            attr.GetData( context ).ToArray();
            attr.GetData( context ).ToArray();

            Assert.Equal( expected, intProvider.Count );
        }


        private static Test GetTest( string name )
        {
            var type = typeof( MethodDataAttributeTests );
            return new Test( type, type.GetMethod( name ) );
        }

        private sealed class FakeServiceProvider : IServiceProvider
        {
            public Dictionary<Type, object> Services { get; } = new Dictionary<Type, object>();


            public object GetService( Type serviceType )
            {
                return Services[serviceType];
            }
        }
    }
}