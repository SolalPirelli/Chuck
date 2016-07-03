using System;

namespace Chuck
{
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = false )]
    public sealed class TestAssemblyAttribute : Attribute { }
}