using System;

namespace Chuck.Infrastructure
{
    [Serializable]
    public sealed class TestExecutionTime
    {
        public DateTimeOffset Start { get; }

        public TimeSpan Duration { get; }


        public TestExecutionTime( DateTimeOffset start, TimeSpan duration )
        {
            Start = start;
            Duration = duration;
        }
    }
}