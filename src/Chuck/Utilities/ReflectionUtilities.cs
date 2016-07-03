using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using System;

namespace Chuck.Utilities
{
    public static class ReflectionUtilities
    {
        public static string GetFullName( MethodInfo method )
        {
            return method.DeclaringType.FullName + "." + method.Name;
        }

        public static string GetNameWithParameters( MethodInfo method )
        {
            return method.Name + "(" + string.Join( ", ", method.GetParameters().Select( p => p.ParameterType.Name ) ) + ")";
        }

        public static bool IsAwaitable( MethodInfo method )
        {
            return method.ReturnType == typeof( Task ) || method.ReturnType.IsSubclassOf( typeof( Task ) );
        }

        public static bool UsesAwait( MethodInfo method )
        {
            return method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;
        }

        public static bool HasReturnValue( MethodInfo method )
        {
            return method.ReturnType != typeof( void )
                && method.ReturnType != typeof( Task );
        }

        public static bool IsInstanceOf( object value, Type type )
        {
            if( value == null )
            {
                return !type.IsValueType;
            }

            return type.IsAssignableFrom( value.GetType() );
        }
    }
}