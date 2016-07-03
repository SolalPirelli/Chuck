using System;

namespace Chuck.Infrastructure
{
    [Serializable]
    public sealed class TestLocation
    {
        public string FilePath { get; }

        public int LineNumber { get; }

        public TestLocation( string filePath, int lineNumber )
        {
            FilePath = filePath;
            LineNumber = lineNumber;
        }
    }
}