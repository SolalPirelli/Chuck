using System;
using System.Collections.Generic;

namespace Chuck.Infrastructure
{
    public sealed class TestPropertyBag
    {
        private readonly Dictionary<Type, Dictionary<string, object>> _values;


        public TestPropertyBag()
        {
            _values = new Dictionary<Type, Dictionary<string, object>>();
        }


        public bool Contains( Type owner, string name )
        {
            return _values.ContainsKey( owner ) && _values[owner].ContainsKey( name );
        }

        public T Get<T>( Type owner, string name )
        {
            Dictionary<string, object> ownerValues;
            if( !_values.TryGetValue( owner, out ownerValues ) )
            {
                throw new ArgumentException( $"The type '{owner}' has not set any properties." );
            }

            object value;
            if( !ownerValues.TryGetValue( name, out value ) )
            {
                throw new ArgumentException( $"No property with name '{name}' exists." );
            }

            if( !( value is T ) )
            {
                throw new ArgumentException( $"The property with name '{name}' is not of type '{typeof( T )}'." );
            }

            return (T) value;
        }

        public void Set( Type owner, string name, object value )
        {
            Dictionary<string, object> ownerValues;
            if( !_values.TryGetValue( owner, out ownerValues ) )
            {
                ownerValues = new Dictionary<string, object>();
                _values[owner] = ownerValues;
            }

            ownerValues[name] = value;
        }
    }
}