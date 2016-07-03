using System;

namespace Chuck
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = true )]
    public sealed class NoTestsAttribute : Attribute { }
}