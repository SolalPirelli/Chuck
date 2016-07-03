using System;
using System.Collections.Generic;
using Chuck.Infrastructure;
using Chuck.Matchers.Infrastructure;

namespace Chuck
{
    public static class Assertions
    {
        public static void Assert( params Assertion[] assertions )
        {
            Assert( null, assertions );
        }

        public static void Assert( string context, params Assertion[] assertions )
        {
            bool success = true;
            var results = new List<TestPartialResult>();
            foreach( var assertion in assertions )
            {
                var result = assertion();
                if( context != null && result.FailureMessage != null )
                {
                    result = TestPartialResult.Failure(
                        context
                      + Environment.NewLine
                      + result.FailureMessage
                    );
                }

                success &= result.IsSuccess;

                results.Add( result );
            }

            if( !success )
            {
                throw new TestFailureException( results );
            }
        }

        // TODO Action/Func<X>/Func<Task>/Func<Task<X>> overloads for throws

        public static Assertion That<T>( T actualValue, Matcher<T> matcher )
        {
            return () =>
            {
                try
                {
                    var result = matcher.Match( actualValue );
                    if( result.IsSuccess )
                    {
                        return TestPartialResult.Success();
                    }

                    return TestPartialResult.Failure(
                        "expected: " + matcher.DescribeExpectation()
                      + Environment.NewLine
                      + "actual:   " + result.Description
                    );
                }
                catch( Exception e )
                {
                    return TestPartialResult.Error( e );
                }
            };
        }
    }
}