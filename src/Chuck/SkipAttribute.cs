using System;

namespace Chuck
{
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true )]
    public sealed class SkipAttribute : Attribute
    {
        public string Reason { get; }


        public SkipAttribute( string reason )
        {
            if( reason == null )
            {
                throw new ArgumentNullException( nameof( reason ) );
            }

            Reason = reason;
        }
    }
}