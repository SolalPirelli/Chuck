using System;

namespace Chuck.Infrastructure
{
    public sealed class TestExecutionContext
    {
        public Test Test { get; }
        public IServiceProvider Services { get; }
        public TestPropertyBag ExtraData { get; }


        public TestExecutionContext( Test test, IServiceProvider services, TestPropertyBag extraData )
        {
            Test = test;
            Services = services;
            ExtraData = extraData;
        }
    }
}