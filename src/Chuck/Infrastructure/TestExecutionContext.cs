using System;
using System.Collections.Generic;

namespace Chuck.Infrastructure
{
    public sealed class TestExecutionContext
    {
        public IServiceProvider Services { get; }
        // TODO consider a smart dic with Get<T> and owner types to ensure namespacing
        public Dictionary<string, object> ExtraProperties { get; }


        public TestExecutionContext( IServiceProvider services )
        {
            Services = services;
            ExtraProperties = new Dictionary<string, object>();
        }
    }
}