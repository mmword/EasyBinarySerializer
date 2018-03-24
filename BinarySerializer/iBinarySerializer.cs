using System;
using BinarySerializer;
using System.IO;
using System.Collections.Generic;

public interface iBinarySerializer<KeyType> : IDisposable, IEnumerable<KeyValuePair<KeyType,object>>
{
    event Action<KeyType, iBinarySerializer<KeyType>> OnSettingChange;

    event Action<iBinarySerializer<KeyType>> OnSettingsLoad;

    event Func<DefinedType, Type> OnTypeResolve;

    void EnsureGenericType<T>();

    void EnsureTypes(params Type[] types);

    void Declare<T>(KeyType key, T value, T defvalue, Action<T> handler = null);

    void Set<T>(KeyType key, T value);

    bool Get<T>(KeyType key, out T value);

    T Get<T>(KeyType key);

    bool HasKey(KeyType key);

    bool GetDefault<T>(KeyType key, out T value);

    Action<T> GetHandler<T>(KeyType key);

    void SetHandler<T>(KeyType key, Action<T> handler);

    void Save(string file, SerializeFlags KeyFlags = SerializeFlags.None, SerializeFlags DataFlags = SerializeFlags.None);

    LoadStatus Load(string file, SerializeFlags KeyFlags = SerializeFlags.None, SerializeFlags DataFlags = SerializeFlags.None);

    void Save(Stream stream, SerializeFlags KeyFlags = SerializeFlags.None, SerializeFlags DataFlags = SerializeFlags.None);

    LoadStatus Load(Stream stream, SerializeFlags KeyFlags = SerializeFlags.None, SerializeFlags DataFlags = SerializeFlags.None);

    byte []Save(SerializeFlags KeyFlags = SerializeFlags.None, SerializeFlags DataFlags = SerializeFlags.None);

    LoadStatus Load(byte[] data, SerializeFlags KeyFlags = SerializeFlags.None, SerializeFlags DataFlags = SerializeFlags.None);
}

public interface iSerializePrinter : IDisposable
{
    void PrintTo(object val, string entryName,Stream stream, SerializeFlags DataFlags = SerializeFlags.None);

    void PrintTo(object val,string entryName,string file, SerializeFlags DataFlags = SerializeFlags.None);

    string PrintToString(object val, string entryName, SerializeFlags DataFlags = SerializeFlags.None);

    void SkipTypes(params Type[] skipTypes);
}