namespace Chuck.Infrastructure
{
    public sealed class TestData
    {
        public string Name { get; }
        public object[] Arguments { get; }

        
        public TestData(string name, object[] arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }
}