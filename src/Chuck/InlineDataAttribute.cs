using System;
using System.Collections.Generic;
using Chuck.Infrastructure;
using Chuck.Utilities;

namespace Chuck
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true, Inherited = true )]
    public sealed class InlineDataAttribute : TestDataAttribute
    {
        private object[] _values;

        public InlineDataAttribute( params object[] values )
        {
            _values = values;
        }

        public override IEnumerable<TestData> GetData( TestExecutionContext executionContext, TestContext testContext )
        {
            var name = PrettyPrinter.Print( testContext.Method, _values );
            return new[] { new TestData( name, _values ) };
        }
    }
}