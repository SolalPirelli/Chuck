using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Chuck.Utilities
{
    public static class PrettyPrinter
    {
        public static string Print(object value)
        {
            return value.ToString(); // TODO
        }

        public static string Print(MethodInfo method, object[] args)
        {
            return $"{method.Name} ({string.Join( ", ", args.Select( Print ) )})";
        }
    }
}