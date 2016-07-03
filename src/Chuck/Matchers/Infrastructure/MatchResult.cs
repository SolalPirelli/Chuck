namespace Chuck.Matchers.Infrastructure
{
    public sealed class MatchResult
    {
        public bool IsSuccess => Description == null;

        public string Description { get; }


        private MatchResult( string description )
        {
            Description = description;
        }


        public static MatchResult Success()
        {
            return new MatchResult( null );
        }

        public static MatchResult Failure( string description )
        {
            return new MatchResult( description );
        }
    }
}