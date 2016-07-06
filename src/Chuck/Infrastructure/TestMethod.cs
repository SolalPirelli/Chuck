using System;

namespace Chuck.Infrastructure
{
    [Serializable]
    public sealed class TestMethod : IEquatable<TestMethod>
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


        public override bool Equals( object obj )
        {
            var other = obj as TestMethod;
            return other != null && Equals( other );
        }

        public bool Equals( TestMethod other )
        {
            return AssemblyName == other.AssemblyName
                && TypeName == other.TypeName
                && Name == other.Name;
        }

        public override int GetHashCode()
        {
            var hash = 31;
            hash += 7 * AssemblyName.GetHashCode();
            hash += 7 * TypeName.GetHashCode();
            hash += 7 * Name.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return $"{TypeName}.{Name} ({AssemblyName})";
        }
    }
}