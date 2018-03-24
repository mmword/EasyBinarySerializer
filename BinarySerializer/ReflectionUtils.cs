using System;
using System.Collections.Generic;
using System.Reflection;

namespace BinarySerializer
{
    class ReflectionUtils : IDisposable
    {
        Dictionary<Type, FieldInfo[]> FieldsCache = new Dictionary<Type, FieldInfo[]>();
        Dictionary<Type, PropertyInfo[]> PropertyCache = new Dictionary<Type, PropertyInfo[]>();
        public FieldInfo[] CacheFields(Type t, bool includePrivate = false)
        {
            BindingFlags _flags = BindingFlags.Public | BindingFlags.Instance;
            if (includePrivate)
                _flags |= BindingFlags.NonPublic;
            System.Reflection.FieldInfo[] fi = null;
            if (!FieldsCache.TryGetValue(t, out fi))
                FieldsCache[t] = fi = t.GetFields(_flags);
            return fi;
        }
        public PropertyInfo[] CacheProperties(Type t, bool includePrivate = false)
        {
            BindingFlags _flags = BindingFlags.Public | BindingFlags.Instance;
            if (includePrivate)
                _flags |= BindingFlags.NonPublic;
            System.Reflection.PropertyInfo[] fi = null;
            if (!PropertyCache.TryGetValue(t, out fi))
                PropertyCache[t] = fi = t.GetProperties(_flags);
            return fi;
        }
        public void Dispose()
        {
            FieldsCache.Clear();
            PropertyCache.Clear();
        }
    }
}
