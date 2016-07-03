namespace Chuck.Matchers.Infrastructure
{
    public abstract class Matcher<T>
    {
        public abstract string DescribeExpectation();
        public abstract MatchResult Match( T value );
    }
}