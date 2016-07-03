using System.Reflection;

namespace Chuck.Infrastructure
{
    public sealed class TestContext
    {
        public MethodInfo Method { get; }


        public TestContext( MethodInfo method )
        {
            Method = method;
        }
    }
}