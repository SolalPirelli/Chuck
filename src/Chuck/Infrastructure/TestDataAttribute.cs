using System;
using System.Collections.Generic;

namespace Chuck.Infrastructure
{
    public abstract class TestDataAttribute : Attribute
    {
        public abstract IEnumerable<TestData> GetData( TestExecutionContext executionContext, TestContext testContext );
    }
}