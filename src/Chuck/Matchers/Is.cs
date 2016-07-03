using System;
using System.Collections.Generic;
using Chuck.Infrastructure;
using Chuck.Matchers.Infrastructure;
using Chuck.Utilities;

namespace Chuck.Matchers
{
    public static partial class Matchers
    {
        public static Matcher<T> Is<T>( T expected )
        {
            return new IsMatcher<T>( expected );
        }

        private sealed class IsMatcher<T> : Matcher<T>
        {
            private readonly T _expected;


            public IsMatcher( T expected )
            {
                _expected = expected;
            }


            public override string DescribeExpectation()
            {
                return PrettyPrinter.Print( _expected );
            }

            public override MatchResult Match( T value )
            {
                if( EqualityComparer<T>.Default.Equals( _expected, value ) )
                {
                    return MatchResult.Success();
                }

                return MatchResult.Failure( PrettyPrinter.Print( value ) );
            }
        }
    }
}