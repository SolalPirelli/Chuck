using System;
using System.Reflection;

namespace Chuck.Infrastructure
{
    public sealed class TestExecutionContext
    {
        public MethodInfo Method { get; }
        public IServiceProvider Services { get; }
        public TestPropertyBag ExtraData { get; }


        public TestExecutionContext( MethodInfo method, IServiceProvider services,TestPropertyBag extraData )
        {
            Method = method;
            Services = services;
            ExtraData = extraData;
        }
    }
}