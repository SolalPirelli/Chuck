using System;
using System.Linq;
using System.Reflection;
using Chuck.Utilities;

namespace Chuck.Infrastructure
{
    [Serializable]
    public sealed class Test : IEquatable<Test>
    {
        // required since a test can be inherited
        public Type Type { get; }
        public MethodInfo Method { get; }


        public Test( Type type, MethodInfo method )
        {
            Type = type;
            Method = method;
        }


        public override bool Equals( object obj )
        {
            var other = obj as Test;
            return other != null && Equals( other );
        }

        public bool Equals( Test other )
        {
            return Type == other.Type
                && Method == other.Method;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() + 31 * Method.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Type.FullName}.{Method.Name}({string.Join( ", ", Method.GetParameters().Select( p => p.ParameterType.Name ) )})";
        }
    }
}