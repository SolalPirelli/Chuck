using System;
using Chuck.Infrastructure;
using Xunit;
using System.Linq;

namespace Chuck.Tests
{
    public sealed class InlineDataAttributeTests
    {
        [Fact]
        public void CannotProvideNullData()
        {
            Assert.Throws<ArgumentNullException>( () => new InlineDataAttribute( null ) );
        }

        public sealed class Data
        {
            private TestData _data;


            public Data()
            {
                var method = GetType().GetMethod( nameof( MyMethod ) );
                var context = new TestExecutionContext( method, new FakeServiceProvider(), new TestPropertyBag() );

                var attr = new InlineDataAttribute( 1, 2, 3 );

                var data = attr.GetData( context );

                Assert.Equal( 1, data.Count() );

                _data = data.First();
            }


            public void MyMethod()
            {
                // This method only exists for its name.
            }

            [Fact]
            public void Name()
            {
                Assert.Equal( $"{nameof( MyMethod )}(1, 2, 3)", _data.Name );
            }

            [Fact]
            public void Arguments()
            {
                Assert.Equal( new object[] { 1, 2, 3 }, _data.Arguments );
            }
        }

        private sealed class FakeServiceProvider : IServiceProvider
        {
            public object GetService( Type serviceType )
            {
                throw new NotImplementedException();
            }
        }
    }
}