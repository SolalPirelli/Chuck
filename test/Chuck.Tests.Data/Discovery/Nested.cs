namespace Chuck.Tests.Data
{
    public class Nested
    {
        public sealed class Inner
        {
            public void Method() { }
        }

        protected sealed class ProtectedInner
        {
            public void Method() { }
        }

        private sealed class PrivateInner
        {
            public void Method() { }
        }

        internal sealed class InternalInner
        {
            public void Method() { }
        }
    }
}