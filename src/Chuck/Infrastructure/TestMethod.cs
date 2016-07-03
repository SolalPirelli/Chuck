using System;

namespace Chuck.Infrastructure
{
    [Serializable]
    public sealed class TestMethod
    {
        public string AssemblyName { get; }

        public string TypeName { get; }

        public string Name { get; }

        public string FullyQualifiedName { get; }


        public TestMethod( string assemblyName, string typeName, string name )
        {
            AssemblyName = assemblyName;
            TypeName = typeName;
            Name = name;
            FullyQualifiedName = TypeName + "." + Name;
        }

        public TestMethod( Type type, string name )
            : this( type.Assembly.FullName, type.FullName, name ) { }
    }
}