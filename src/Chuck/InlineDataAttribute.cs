using System;
using System.Collections.Generic;
using Chuck.Infrastructure;
using Chuck.Utilities;

namespace Chuck
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true, Inherited = true )]
    public sealed class InlineDataAttribute : TestDataAttribute
    {
        private readonly object[] _values;


        public InlineDataAttribute( params object[] values )
        {
            if( values == null )
            {
                throw new ArgumentNullException( nameof( values ) );
            }

            _values = values;
        }


        public override IEnumerable<TestData> GetData( TestExecutionContext context )
        {
            var name = PrettyPrinter.Print( context.Test.Method, _values );
            return new[] { new TestData( name, _values ) };
        }
    }
}