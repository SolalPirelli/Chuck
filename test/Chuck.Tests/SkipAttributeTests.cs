using System;
using Xunit;

namespace Chuck.Tests
{
    public sealed class SkipAttributeTests
    {
        [Fact]
        public void CannotProvideNullReason()
        {
            Assert.Throws<ArgumentNullException>( () => new SkipAttribute( null ) );
        }

        [Fact]
        public void ReasonIsSet()
        {
            var attr = new SkipAttribute( "Hello, world!" );

            Assert.Equal( "Hello, world!", attr.Reason );
        }
    }
}