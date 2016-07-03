using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chuck.Assertions;
using static Chuck.Matchers.Matchers;

namespace Chuck.VisualStudio.SelfTest
{
    public sealed class BasicTests
    {
        public void Success()
        {
            Assert(
                That( 1, Is( 1 ) )
            );
        }

        public void Failure()
        {
            Assert(
                That( 2 + 2, Is( 5 ) )
            );
        }

        [Skip( "Not implemented" )]
        public void Skipped()
        {
            throw new NotImplementedException();
        }

        public void Error()
        {
            throw new Exception( "Oops!" );
        }
    }
}