using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace BinarySerializer
{
    sealed class BinarySerializerInternal<KeyType> : iBinarySerializer<KeyType>
    {
        const uint CURRENT_VERSION = 0x900;
        const uint TYPETABLE_TAG = 0x101A;
        const uint SETTINGSTABLE_TAG = 0x102A;
        const int ID_VALUE_TYPE = -1;

        enum ObjectType
        {
            Undefined = 0,
            Primitive = 0x100,
            Array,
            Class,
            Struct,
            Custom,
            Reference,
            Null,
        }

        static class NULL { }

        abstract class BaseEntry
        {
            protected static void WriteType(ObjectType objectType,string tag,int id,Type t, WrittingData data, BinaryWriter writter)
            {
                ObjectHeader header = ObjectHeader.Empty;
                header.type = objectType;
                header.typePosition = data.typeCollector[t];
                header.tag = tag;
                header.id = id;
                header.Write(writter);
            }
            public static ObjectHeader ReadEntry(BinaryReader reader)
            {
                ObjectHeader header = ObjectHeader.Empty;
                header.Read(reader);
                return header;
            }
        }

        abstract class PrimitiveEntry : BaseEntry
        {
            public static void WriteRawArray(Array val, Type t, BinaryWriter writter)
            {
                writter.Write(val.Length);
                if (t.IsEnum)
                    foreach (var e in (int[])val) writter.Write(e);
                else if (t == typeof(byte))
                    foreach(var e in (byte[])val) writter.Write(e);
                else if (t == typeof(sbyte))
                    foreach (var e in (sbyte[])val) writter.Write(e);
                else if (t == typeof(short))
                    foreach (var e in (short[])val) writter.Write(e);
                else if (t == typeof(int))
                    foreach (var e in (int[])val) writter.Write(e);
                else if (t == typeof(long))
                    foreach (var e in (long[])val) writter.Write(e);
                else if (t == typeof(ushort))
                    foreach (var e in (ushort[])val) writter.Write(e);
                else if (t == typeof(uint))
                    foreach (var e in (uint[])val) writter.Write(e);
                else if (t == typeof(ulong))
                    foreach (var e in (ulong[])val) writter.Write(e);
                else if (t == typeof(float))
                    foreach (var e in (float[])val) writter.Write(e);
                else if (t == typeof(double))
                    foreach (var e in (double[])val) writter.Write(e);
                else if (t == typeof(bool))
                    foreach (var e in (bool[])val) writter.Write(e);
                else if (t == typeof(decimal))
                    foreach (var e in (decimal[])val) writter.Write(e);
                else if (t == typeof(string))
                    foreach (var e in (string[])val) writter.Write(e);
                else
                    throw new ArgumentException("wrong type : " + t.Name);
            }

            public static void WriteRaw(object val, Type t, BinaryWriter writter)
            {
                if (t.IsEnum)
                    writter.Write((int)val);
                else if (t == typeof(byte))
                    writter.Write((byte)val);
                else if(t == typeof(sbyte))
                    writter.Write((sbyte)val);
                else if (t == typeof(short))
                    writter.Write((short)val);
                else if (t == typeof(int))
                    writter.Write((int)val);
                else if (t == typeof(long))
                    writter.Write((long)val);
                else if (t == typeof(ushort))
                    writter.Write((ushort)val);
                else if (t == typeof(uint))
                    writter.Write((uint)val);
                else if (t == typeof(ulong))
                    writter.Write((ulong)val);
                else if (t == typeof(float))
                    writter.Write((float)val);
                else if (t == typeof(double))
                    writter.Write((double)val);
                else if (t == typeof(bool))
                    writter.Write((bool)val);
                else if (t == typeof(decimal))
                    writter.Write((decimal)val);
                else if (t == typeof(string))
                    writter.Write((string)val);
                else
                    throw new ArgumentException("wrong type : " + t.Name);
            }

            public static void Write(object val,string tag,Type t, WrittingData data,BinaryWriter writter)
            {
                WriteType(ObjectType.Primitive, tag, ID_VALUE_TYPE, t, data, writter);
                WriteRaw(val, t, writter);
            }

            public static void WriteArray(Array val, string tag, Type t, WrittingData data, BinaryWriter writter)
            {
                WriteType(ObjectType.Array,tag,val.GetHashCode(), t, data, writter);
                WriteRawArray(val, t, writter);
            }

            public static object ReadRaw(BinaryReader reader,Type t)
            {
                object val = null;
                if (t.IsEnum)
                    val = Enum.ToObject(t, reader.ReadInt32());
                else if (t == typeof(byte))
                    val = reader.ReadByte();
                else if (t == typeof(sbyte))
                    val = reader.ReadSByte();
                else if (t == typeof(short))
                    val = reader.ReadInt16();
                else if (t == typeof(int))
                    val = reader.ReadInt32();
                else if (t == typeof(long))
                    val = reader.ReadInt64();
                else if (t == typeof(ushort))
                    val = reader.ReadUInt16();
                else if (t == typeof(uint))
                    val = reader.ReadUInt32();
                else if (t == typeof(ulong))
                    val = reader.ReadUInt64();
                else if (t == typeof(float))
                    val = reader.ReadSingle();
                else if (t == typeof(double))
                    val = reader.ReadDouble();
                else if (t == typeof(bool))
                    val = reader.ReadBoolean();
                else if (t == typeof(decimal))
                    val = reader.ReadDecimal();
                else if (t == typeof(string))
                    val = reader.ReadString();
                return val;
            }

            public static Array ReadRawArray(BinaryReader reader, Type t)
            {
                int sz = reader.ReadInt32();
                if (t == typeof(byte))
                {
                    byte[] arr = new byte[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadByte();
                    return arr;
                }
                else if (t == typeof(sbyte))
                {
                    sbyte[] arr = new sbyte[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadSByte();
                    return arr;
                }
                else if (t == typeof(short))
                {
                    short[] arr = new short[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadInt16();
                    return arr;
                }
                else if(t.IsEnum)
                {
                    Array arr = Array.CreateInstance(t, sz);
                    for (int i = 0; i < sz; ++i) arr.SetValue(Enum.ToObject(t,reader.ReadInt32()), i);
                    return arr;
                }
                else if (t == typeof(int))
                {
                    int[] arr = new int[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadInt32();
                    return arr;
                }
                else if (t == typeof(long))
                {
                    long[] arr = new long[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadInt64();
                    return arr;
                }
                else if(t == typeof(ushort))
                {
                    ushort[] arr = new ushort[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadUInt16();
                    return arr;
                }
                else if (t == typeof(uint))
                {
                    uint[] arr = new uint[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadUInt32();
                    return arr;
                }
                else if (t == typeof(ulong))
                {
                    ulong[] arr = new ulong[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadUInt64();
                    return arr;
                }
                else if (t == typeof(float))
                {
                    float[] arr = new float[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadSingle();
                    return arr;
                }
                else if (t == typeof(double))
                {
                    double[] arr = new double[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadDouble();
                    return arr;
                }
                else if (t == typeof(bool))
                {
                    bool[] arr = new bool[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadBoolean();
                    return arr;
                }
                else if (t == typeof(decimal))
                {
                    decimal[] arr = new decimal[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadDecimal();
                    return arr;
                }
                else if (t == typeof(string))
                {
                    string[] arr = new string[sz];
                    for (int i = 0; i < sz; ++i) arr[i] = reader.ReadString();
                    return arr;
                }
                else
                    throw new ArgumentException("wrong type : " + t.Name);
            }

            public static object Read(BinaryReader reader,Type t)
            {
                return ReadRaw(reader, t);
            }

            public static Array ReadArray(BinaryReader reader, IDictionary<int, Type> TypeMap,Type t)
            {
                return ReadRawArray(reader, t);
            }
        }

        abstract class ArrayEntry : BaseEntry
        {
            public static void Write(object val,string tag, WrittingData collector, BinaryWriter writter)
            {
                WriteType(ObjectType.Array,tag,val.GetHashCode(), val.GetType().GetElementType(), collector, writter);
                Array arr = val as Array;
                writter.Write(arr.Length);
            }
            public static Array Read(BinaryReader reader,ref Type t)
            {
                int sz = reader.ReadInt32();
                Array arr = Array.CreateInstance(t, sz);
                t = arr.GetType();
                return arr;
            }
        }

        abstract class ClassEntry : BaseEntry
        {
            public static void Write(object val,string tag, WrittingData collector, BinaryWriter writter)
            {
                WriteType(ObjectType.Class,tag,val.GetHashCode(), val.GetType(), collector, writter);
            }
            public static object Read(BinaryReader reader, Type t)
            {
                return Activator.CreateInstance(t);
            }
        }

        abstract class StructEntry : BaseEntry
        {
            public static void Write(object val, string tag, WrittingData collector, BinaryWriter writter)
            {
                WriteType(ObjectType.Struct,tag,ID_VALUE_TYPE, val.GetType(), collector, writter);
            }
            public static object Read(BinaryReader reader,Type t)
            {
                return Activator.CreateInstance(t);
            }
        }

        abstract class CustomEntry : BaseEntry
        {
            public static bool Parse(object val, WrittingData data, Action<object> OnWrite)
            {
                Type t = val.GetType();
                if (typeof(IDictionary).IsAssignableFrom(t))
                {
                    IDictionary dict = (IDictionary)val;
                    foreach (DictionaryEntry kvp in dict)
                    {
                        OnWrite(kvp.Key);
                        OnWrite(kvp.Value);
                    }
                    return true;
                }
                else if (typeof(IEnumerable).IsAssignableFrom(t))
                {
                    IEnumerable col = (IEnumerable)val;
                    foreach (var v in col)
                        OnWrite(v);
                    return true;
                }
                return false;
            }
            public static bool Write(object val, string tag, WrittingData data, BinaryWriter writter,Action<object> OnWrite)
            {
                Type t = val.GetType();
                if(typeof(IDictionary).IsAssignableFrom(t))
                {
                    WriteType(ObjectType.Custom,tag,val.GetHashCode(), val.GetType(), data, writter);
                    IDictionary dict = (IDictionary)val;
                    writter.Write(dict.Count);
                    foreach (DictionaryEntry kvp in dict)
                    {
                        OnWrite(kvp.Key);
                        OnWrite(kvp.Value);
                    }
                    return true;
                }
                else if(typeof(IList).IsAssignableFrom(t))
                {
                    WriteType(ObjectType.Custom,tag,val.GetHashCode(), val.GetType(), data, writter);
                    IList list = (IList)val;
                    writter.Write(list.Count);
                    foreach (var v in list)
                        OnWrite(v);
                    return true;
                }
                else if (typeof(ICollection).IsAssignableFrom(t))
                {
                    WriteType(ObjectType.Custom, tag, val.GetHashCode(), val.GetType(), data, writter);
                    ICollection list = (ICollection)val;
                    writter.Write(list.Count);
                    foreach (var v in list)
                        OnWrite(v);
                    return true;
                }
                else if(typeof(IEnumerable).IsAssignableFrom(t))
                {
                    WriteType(ObjectType.Custom, tag, val.GetHashCode(), val.GetType(), data, writter);
                    IEnumerable col = (IEnumerable)val;
                    int sz = 0;
                    foreach (var v in col)
                        sz++;
                    writter.Write(sz);
                    foreach (var v in col)
                        OnWrite(v);
                    return true;
                }
                return false;
            }
            public static object Read(BinaryReader reader,Func<object> OnRead, Type t)
            {
                if (typeof(IDictionary).IsAssignableFrom(t))
                {
                    IDictionary dict = (IDictionary)Activator.CreateInstance(t);
                    int sz = reader.ReadInt32();
                    for (int i = 0; i < sz; ++i)
                        dict.Add(OnRead(), OnRead());
                    return dict;
                }
                else if (typeof(IList).IsAssignableFrom(t))
                {
                    IList list = (IList)Activator.CreateInstance(t);
                    int sz = reader.ReadInt32();
                    for (int i = 0; i < sz; ++i)
                        list.Add(OnRead());
                    return list;
                }
                else if (typeof(IEnumerable).IsAssignableFrom(t))
                {
                    int sz = reader.ReadInt32();
                    object[] arr = new object[sz];
                    for (int i = 0; i < sz; ++i)
                        arr[i] = OnRead();
                    object __el = Array.Find(arr, x => x != null);
                    Array generic = Array.CreateInstance(__el.GetType(), sz);
                    for (int i = 0; i < sz; ++i)
                        generic.SetValue(arr[i], i);
                    return Activator.CreateInstance(t, generic);
                }
                return null;
            }
        }

        abstract class ReferenceEntry : BaseEntry
        {
            public static void Write(object val, string tag, WrittingData data, BinaryWriter writter)
            {
                WriteType(ObjectType.Reference, tag,val.GetHashCode(), val.GetType(), data, writter);
                writter.Write(data.References[val]);
            }
            public static object Read(BinaryReader reader,ReadingData data)
            {
                int usedID = reader.ReadInt32();
                return data.ReferenceMap[usedID];
            }
        }

        abstract class NullEntry : BaseEntry
        {
            public static void Write(string tag, WrittingData data, BinaryWriter writter)
            {
                WriteType(ObjectType.Null,tag,ID_VALUE_TYPE, typeof(NULL), data, writter);
            }
            public static void Read(BinaryReader reader)
            {

            }
        }

        sealed class Setting<T>
        {
            public const string keyName = "key";
            public const string valueName = "value";
            public const string defValueName = "defValue";
            public const string handlerName = "handler";
            public KeyType key;
            public T value;
            public T defValue;
            public Action<T> handler;
            public Setting()
            {
                key = default(KeyType);
                value = defValue = default(T);
                handler = null;
            }
            public Setting(KeyType key, T value, T defValue, Action<T> handler)
            {
                this.key = key;
                this.value = value;
                this.defValue = defValue;
                this.handler = handler;
            }
            public Setting(KeyType key)
            {
                this.key = key;
                this.value = this.defValue = default(T);
                this.handler = null;
            }
        }

        class Collector<T> where T : class
        {
            public Dictionary<T, int> Types { get; private set; }
            protected int position = 0;
            public Collector()
            {
                Types = new Dictionary<T, int>();
            }
            public void Add(T t)
            {
                if (!Types.ContainsKey(t))
                    Types[t] = position++;
            }
            public T[] GetSorted()
            {
                ICollection<KeyValuePair<T, int>> col = Types;
                KeyValuePair<T, int>[] arr = new KeyValuePair<T, int>[col.Count];
                col.CopyTo(arr, 0);
                Array.Sort(arr, (a, b) => a.Value.CompareTo(b.Value));
                return Array.ConvertAll(arr, x => x.Key);
            }
            public int this[T t]
            {
                get
                {
                    return Types[t];
                }
            }
        }

        sealed class WrittingData
        {
            public Collector<Type> typeCollector { get; private set; }
            public Dictionary<object, int> References { get; private set; }
            public SerializeFlags Flags { get; private set; }
            public WrittingData(SerializeFlags _flags)
            {
                Flags = _flags;
                References = new Dictionary<object, int>();
                typeCollector = new Collector<Type>();
            }
            public WrittingData(WrittingData share,SerializeFlags _flags)
            {
                Flags = _flags;
                References = share.References;
                typeCollector = share.typeCollector;
            }
            public WrittingData SharedInstanceWithFlags(SerializeFlags _flags)
            {
                return new WrittingData(this, _flags);
            }
            public bool HasFlags(SerializeFlags flags)
            {
                return (Flags & flags) == flags;
            }
            public bool HasReferenceOrAdd(object obj)
            {
                if (!References.ContainsKey(obj))
                {
                    References[obj] = obj.GetHashCode();
                    return false;
                }
                return true;
            }
            public void WriteTypeTable(BinaryWriter bwRes)
            {
                var arr = typeCollector.GetSorted();
                bwRes.Write(TYPETABLE_TAG);
                bwRes.Write(arr.Length);
                foreach (var t in arr)
                {
                    bwRes.Write(RequiredTypeName(t, Flags));
                    bwRes.Write(t.ToString());
                }
            }
        }

        sealed class ReadingData
        {
            public Dictionary<int, Type> TypeMap { get; private set; }
            public Dictionary<int, object> ReferenceMap { get; private set; }
            public SerializeFlags Flags { get; private set; }
            public ReadingData(SerializeFlags _flags)
            {
                Flags = _flags;
                TypeMap = new Dictionary<int, Type>();
                ReferenceMap = new Dictionary<int, object>();
            }
            public ReadingData(ReadingData share, SerializeFlags _flags)
            {
                Flags = _flags;
                TypeMap = share.TypeMap;
                ReferenceMap = share.ReferenceMap;
            }
            public ReadingData SharedInstanceWithFlags(SerializeFlags _flags)
            {
                return new ReadingData(this, _flags);
            }
            public bool HasFlags(SerializeFlags flags)
            {
                return (Flags & flags) == flags;
            }
            public void PropagateReference(object val,int id)
            {
                if (!ReferenceMap.ContainsKey(id))
                    ReferenceMap[id] = val;
            }
            public void ReadTypeTable(BinaryReader br,Func<string,string,Type> TypeDet)
            {
                if (br.ReadUInt32() != TYPETABLE_TAG)
                    throw new InvalidOperationException("stream reading has no collector/types tag , possible data flow out of state");
                int typesTableSize = br.ReadInt32();
                for (int i = 0; i < typesTableSize; ++i)
                {
                    string assemblyname = br.ReadString();
                    string name = br.ReadString();
                    TypeMap[i] = TypeDet(assemblyname,name);
                }
            }
        }

        class EnumerableSettings : IEnumerator<KeyValuePair<KeyType, object>>
        {
            IEnumerator<KeyValuePair<KeyType, object>> enumerator;
            public EnumerableSettings(Dictionary<KeyType,object> Settings)
            {
                enumerator = Settings.GetEnumerator();
            }
            public KeyValuePair<KeyType, object> Current
            {
                get
                {
                    var _el = enumerator.Current;
                    return new KeyValuePair<KeyType, object>(_el.Key, GetSettingFieldData<object>(_el.Value, Setting<KeyType>.valueName));
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
            public void Dispose()
            {
                enumerator.Dispose();
            }
            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }
            public void Reset()
            {
                enumerator.Reset();
            }
        }

        struct ObjectDesciption
        {
            public Type type;
            public object _object;
            public ObjectHeader header;
        }

        struct ObjectHeader
        {
            public uint sizeInBytes;        // object size + header size (in bytes)
            public uint headerSizeInBytes;  // header size only          (in bytes)
            public ObjectType type;
            public int typePosition;
            public int id;
            public string tag;
            public static ObjectHeader Empty
            {
                get
                {
                    return new ObjectHeader();
                }
            }
            public Type SerializedType(IDictionary<int,Type> Map)
            {
                Type t = null;
                Map.TryGetValue(typePosition, out t);
                return t;
            }
            public void Write(BinaryWriter writter)
            {
                long pos = writter.BaseStream.Position;
                writter.Write(sizeInBytes);
                writter.Write((int)type);
                writter.Write(typePosition);
                writter.Write(id);
                writter.Write(tag);
                headerSizeInBytes = (uint)(writter.BaseStream.Position - pos) + sizeof(uint);
                writter.Write(headerSizeInBytes);
            }
            public void Read(BinaryReader reader)
            {
                sizeInBytes = reader.ReadUInt32();
                type = (ObjectType)reader.ReadInt32();
                typePosition = reader.ReadInt32();
                id = reader.ReadInt32();
                tag = reader.ReadString();
                headerSizeInBytes = reader.ReadUInt32();
            }
        }

        struct BinaryHeader
        {
            public uint version;
            public uint crc32;
            public SerializeFlags KeyFlags;
            public SerializeFlags DataFlags;
            public static BinaryHeader Empty
            {
                get
                {
                    return new BinaryHeader();
                }
            }
            public void Write(BinaryWriter bw)
            {
                bw.Write(crc32);
                bw.Write(version);
                bw.Write((int)KeyFlags);
                bw.Write((int)DataFlags);
            }
            public void WriteWithoutCRC(BinaryWriter bw)
            {
                bw.Write(version);
                bw.Write((int)KeyFlags);
                bw.Write((int)DataFlags);
            }
            public void Read(BinaryReader br)
            {
                crc32 = br.ReadUInt32();
                version = br.ReadUInt32();
                KeyFlags = (SerializeFlags)br.ReadInt32();
                DataFlags = (SerializeFlags)br.ReadInt32();
            }
        }

        struct SettingTableHeader
        {
            public int settingsCount;
            public int keyTypePosition;
            public static SettingTableHeader Empty
            {
                get
                {
                    return new SettingTableHeader();
                }
            }
            public void Write(BinaryWriter writter)
            {
                writter.Write(SETTINGSTABLE_TAG);
                writter.Write(settingsCount);
                writter.Write(keyTypePosition);
            }
            public void Read(BinaryReader reader)
            {
                if (reader.ReadUInt32() != SETTINGSTABLE_TAG)
                    throw new InvalidOperationException("stream reading has no settings table tag , possible data flow out of state");
                settingsCount = reader.ReadInt32();
                keyTypePosition = reader.ReadInt32();
            }
        }

        Dictionary<KeyType, object> Settings = new Dictionary<KeyType, object>();
        Dictionary<string, Type> QualifiedTypesMap = new Dictionary<string, Type>();
        Dictionary<string, Type> NamesTypesMap = new Dictionary<string, Type>();
        ReflectionUtils RefUtils = new ReflectionUtils();

        public static Type SettingType<T>()
        {
            return typeof(Setting<T>);
        }

        static T GetSettingFieldData<T>(object setting,string data)
        {
            Type t = setting.GetType();
            return (T)t.GetField(data).GetValue(setting);
        }

        static void SetSettingFieldKey(object setting, KeyType key)
        {
            Type t = setting.GetType();
            t.GetField(Setting<KeyType>.keyName).SetValue(setting, key);
        }

        static void SetSettingFieldValue(object setting, object value)
        {
            Type t = setting.GetType();
            t.GetField(Setting<KeyType>.valueName).SetValue(setting, value);
        }

        static void SetSettingsAsCtor(object setting, KeyType key, object value, object defValue,object handler,SerializeFlags customFlags)
        {
            Type t = setting.GetType();
            t.GetField(Setting<KeyType>.keyName).SetValue(setting, key);
            t.GetField(Setting<KeyType>.valueName).SetValue(setting, value);
            t.GetField(Setting<KeyType>.defValueName).SetValue(setting, defValue);
            t.GetField(Setting<KeyType>.handlerName).SetValue(setting, handler);
        }

        static string RequiredTypeName(Type t,SerializeFlags flags)
        {
            if ((flags & SerializeFlags.SkipQualifiedNames) == SerializeFlags.SkipQualifiedNames)
                return t.ToString();
            return t.AssemblyQualifiedName;
        }

        static void CopyStreamTo(Stream source, Stream destination, int bufferSize = 4096)
        {
            byte[] buffer = new byte[bufferSize];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
                destination.Write(buffer, 0, read);
        }

        static object FormatterActivate(Type t,Action<object> OnMissingCtor = null)
        {
            if (t == null)
                throw new ArgumentException("type cant be null !");
            object instance = FormatterServices.GetUninitializedObject(t); // Activator.CreateInstance sometime crush when trying find default constructor (on IL/WEBGL)
            var constructor = t.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
                constructor.Invoke(instance, new object[] { });
            else if (OnMissingCtor != null)
                OnMissingCtor(instance);
            return instance;
        }

        Type GetTypeOrResolve(string QualifiedName,string Name)
        {
            Type t = Type.GetType(QualifiedName);
            if (t != null)
                return t;
            if (QualifiedTypesMap.Count > 0 && QualifiedTypesMap.TryGetValue(QualifiedName, out t))
                return t;
            if (NamesTypesMap.Count > 0 && NamesTypesMap.TryGetValue(Name, out t))
                return t;
            if (OnTypeResolve != null)
            {
                DefinedType dt = new DefinedType() { QualifiedName = QualifiedName, Name = Name };
                t = OnTypeResolve(dt);
                return t;
            }
            throw new InvalidOperationException("serializer cant resolve type with name : " + QualifiedName);
        }

        Type GetTypeOrResolve(string QualifiedName, string Name,SerializeFlags Flags)
        {
            if((Flags & SerializeFlags.SkipQualifiedNames) == SerializeFlags.SkipQualifiedNames)
            {
                Type t = null;
                if (NamesTypesMap.Count > 0 && NamesTypesMap.TryGetValue(Name, out t))
                    return t;
            }
            return GetTypeOrResolve(QualifiedName, Name);
        }

        bool SerializeKeyFromString(string s,out KeyType Key)
        {
            Key = default(KeyType);
            Type t = typeof(KeyType);
            if (t.IsEnum)
            {
                var names = Enum.GetNames(t);
                KeyType []values = (KeyType[])Enum.GetValues(t);
                for(int i = 0;i < names.Length;++i)
                {
                    if (names[i].Equals(s))
                    {
                        Key = values[i];
                        return true;
                    }
                }
                return false;
            }
            else if (t == typeof(string))
            {
                Key = (KeyType)(object)s;
                return true;
            }
            else
            {
                try
                {
                    Key = (KeyType)Convert.ChangeType(s, t);
                    return true;
                }
                catch(Exception)
                {
                    return false;
                }
            }
        }

        void CollectTypes(object val, WrittingData data)
        {
            Type t = null;
            if (val != null)
            {
                t = val.GetType();
                data.typeCollector.Add(t);
            }
            if (val == null)
                return;
            else if (t.IsPrimitive || t.IsEnum || t == typeof(string))
                return;
            else if (t.IsArray)
            {
                t = t.GetElementType();
                data.typeCollector.Add(t);
                Array arr = val as Array;
                if (t.IsPrimitive || t.IsEnum || t == typeof(string))
                    return;
                else if (t.IsArray)
                {
                    foreach (var el in arr)
                        CollectTypes(el, data);
                }
                else if (t.IsClass || t.IsValueType)
                {
                    foreach (var el in arr)
                    {
                        if (el == null)
                            continue;
                        else if (!CustomEntry.Parse(el, data, (_obj) => CollectTypes(_obj, data)))
                            CollectTypes(el, data);
                    }
                }
            }
            else if (t.IsClass || t.IsValueType)
            {
                if (!CustomEntry.Parse(val, data, (_obj) => CollectTypes(_obj, data)))
                {
                    // Fields
                    var allf = RefUtils.CacheFields(t, data.HasFlags(SerializeFlags.SerializePrivateFields));
                    if (allf.Length > 0)
                    {
                        foreach (var f in allf)
                        {
                            object v = f.GetValue(val);
                            if (v == null)
                                continue;
                            else
                                CollectTypes(v, data);
                        }
                    }
                    if (data.HasFlags(SerializeFlags.SerializeProperties))
                    {
                        // Properties
                        var allp = RefUtils.CacheProperties(t, data.HasFlags(SerializeFlags.SerializePrivateProperties));
                        if (allp.Length > 0)
                        {
                            foreach (var p in allp)
                            {
                                object v = p.GetValue(val, null);
                                if (v == null)
                                    continue;
                                else
                                    CollectTypes(v, data);
                            }
                        }
                    }
                }
            }
            else
                throw new InvalidOperationException("cant determine in type collector for object type : " + t.Name);
        }

        void WriteObject(object val,BinaryWriter writter, WrittingData data, string _tag = "")
        {
            Type t = null;
            if (val != null)
            {
                t = val.GetType();
                data.typeCollector.Add(t);
            }
            long SrcPos = writter.BaseStream.Position;
            if (val == null)
                NullEntry.Write(_tag, data, writter);
            else if (t.IsPrimitive || t.IsEnum || t == typeof(string))
                PrimitiveEntry.Write(val, _tag, t, data, writter);
            else if (t.IsArray)
            {
                if(data.HasReferenceOrAdd(val))
                {
                    ReferenceEntry.Write(val, _tag, data, writter);
                    return;
                }
                t = t.GetElementType();
                data.typeCollector.Add(t);
                ArrayEntry.Write(val, _tag, data, writter);
                Array arr = val as Array;
                if (t.IsPrimitive || t.IsEnum || t == typeof(string))
                    PrimitiveEntry.WriteRawArray(arr, t, writter);
                else if(t.IsArray)
                {
                    foreach (var el in arr)
                        WriteObject(el, writter, data, _tag);
                }
                else if (t.IsClass || t.IsValueType)
                {
                    foreach (var el in arr)
                    {
                        if (el == null)
                            NullEntry.Write(_tag, data, writter);
                        else if (!CustomEntry.Write(el, _tag, data, writter, (_obj) => WriteObject(_obj, writter, data, _tag)))
                            WriteObject(el, writter, data, _tag);
                    }
                }
            }
            else if (t.IsClass || t.IsValueType)
            {
                if (!CustomEntry.Write(val, _tag, data, writter, (_obj) => WriteObject(_obj, writter, data, _tag)))
                {
                    if (t.IsClass)
                    {
                        if(data.HasReferenceOrAdd(val))
                        {
                            ReferenceEntry.Write(val, _tag, data, writter);
                            return;
                        }
                        ClassEntry.Write(val, _tag, data, writter);
                    }
                    else
                        StructEntry.Write(val, _tag, data, writter);
                    // Fields
                    var allf = RefUtils.CacheFields(t, data.HasFlags(SerializeFlags.SerializePrivateFields));
                    writter.Write(allf.Length);
                    if (allf.Length > 0)
                    {
                        foreach (var f in allf)
                        {
                            object v = f.GetValue(val);
                            if (v == null)
                                NullEntry.Write(f.Name, data, writter);
                            else
                                WriteObject(v, writter, data, f.Name);
                        }
                    }
                    if(data.HasFlags(SerializeFlags.SerializeProperties))
                    {
                        // Properties
                        var allp = RefUtils.CacheProperties(t, data.HasFlags(SerializeFlags.SerializePrivateProperties));
                        writter.Write(allp.Length);
                        if (allp.Length > 0)
                        {
                            foreach (var p in allp)
                            {
                                object v = p.GetValue(val,null);
                                if (v == null)
                                    NullEntry.Write(p.Name, data, writter);
                                else
                                    WriteObject(v, writter, data, p.Name);
                            }
                        }
                    }
                }
            }
            else
                throw new InvalidOperationException("cant serialize object type : " + t.Name);
            long CurrentPos = writter.BaseStream.Position;
            BinaryReader br = new BinaryReader(writter.BaseStream);
            br.BaseStream.Position = SrcPos;
            ObjectHeader header = ObjectHeader.Empty;
            header.Read(br);
            header.sizeInBytes = (uint)(CurrentPos - SrcPos);
            writter.BaseStream.Position = SrcPos;
            header.Write(writter);
            writter.BaseStream.Position = CurrentPos;
        }

        ObjectDesciption ReadObject(BinaryReader reader, ReadingData data)
        {
            long SrcPos = reader.BaseStream.Position;
            ObjectHeader header = BaseEntry.ReadEntry(reader);
            Type serializedType = header.SerializedType(data.TypeMap);
            if(serializedType == null)
            {
                ObjectDesciption desc;
                desc.type = null;
                desc._object = null;
                desc.header = header;
                return desc;
            }
            switch (header.type)
            {
                case ObjectType.Primitive:
                    {
                        ObjectDesciption desc;
                        desc.type = serializedType;
                        desc._object = PrimitiveEntry.Read(reader, desc.type);
                        desc.header = header;
                        return desc;
                    }
                case ObjectType.Array:
                    {
                        ObjectDesciption desc;
                        desc.type = serializedType;
                        Array arr = ArrayEntry.Read(reader, ref desc.type);
                        data.PropagateReference(arr, header.id);
                        Type _t = desc.type.GetElementType();
                        if (_t.IsPrimitive || _t.IsEnum || _t == typeof(string))
                            arr = PrimitiveEntry.ReadRawArray(reader, _t);
                        else if (_t.IsClass || _t.IsValueType)
                        {
                            for (int i = 0; i < arr.Length; ++i)
                            {
                                var field = ReadObject(reader, data);
                                if(field.type != null)
                                    arr.SetValue(field._object, i);
                            }
                        }
                        desc._object = arr;
                        desc.header = header;
                        return desc;
                    }
                case ObjectType.Class:
                case ObjectType.Struct:
                    {
                        ObjectDesciption desc;
                        desc.type = serializedType;
                        if (header.type == ObjectType.Class)
                        {
                            desc._object = ClassEntry.Read(reader, desc.type);
                            data.PropagateReference(desc._object, header.id);
                        }
                        else
                            desc._object = StructEntry.Read(reader, desc.type);
                        // Fields
                        var fields = RefUtils.CacheFields(desc.type,data.HasFlags(SerializeFlags.SerializePrivateFields));
                        int numFieldsObjects = reader.ReadInt32();
                        if (fields.Length > 0)
                        {
                            var FieldMap = new Dictionary<string, FieldInfo>(StringComparer.InvariantCultureIgnoreCase);
                            foreach (var f in fields)
                                FieldMap[f.Name] = f;
                            for (int i = 0; i < numFieldsObjects; ++i)
                            {
                                long fieldSrcPos = reader.BaseStream.Position;
                                ObjectDesciption _field = ReadObject(reader, data);
                                if (_field.type == null)
                                    reader.BaseStream.Position = fieldSrcPos + _field.header.sizeInBytes;
                                else
                                {
                                    if(FieldMap.ContainsKey(_field.header.tag))
                                        FieldMap[_field.header.tag].SetValue(desc._object, _field._object);
                                    else
                                    {
                                        BindingFlags _flags = BindingFlags.Instance | BindingFlags.Public;
                                        if (data.HasFlags(SerializeFlags.SerializePrivateFields))
                                            _flags |= BindingFlags.NonPublic;
                                        foreach (var field in desc.type.GetFields(_flags))
                                        {
                                            var attribs = (SameName[])field.GetCustomAttributes(typeof(SameName), false);
                                            if (attribs.Length > 0 && Array.FindIndex(attribs, x => x.Has(_field.header.tag)) >= 0)
                                            {
                                                field.SetValue(desc._object, _field._object);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if(data.HasFlags(SerializeFlags.SerializeProperties))
                        {
                            // Properties
                            var properties = RefUtils.CacheProperties(desc.type, data.HasFlags(SerializeFlags.SerializePrivateProperties));
                            int numPropertiesObjects = reader.ReadInt32();
                            if (properties.Length > 0)
                            {
                                var PropertyMap = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);
                                foreach (var f in properties)
                                    PropertyMap[f.Name] = f;
                                for (int i = 0; i < numPropertiesObjects; ++i)
                                {
                                    long propertySrcPos = reader.BaseStream.Position;
                                    ObjectDesciption _field = ReadObject(reader, data);
                                    if (_field.type == null)
                                        reader.BaseStream.Position = propertySrcPos + _field.header.sizeInBytes;
                                    else
                                    {
                                        if(PropertyMap.ContainsKey(_field.header.tag))
                                            PropertyMap[_field.header.tag].SetValue(desc._object, _field._object, null);
                                        else
                                        {
                                            BindingFlags _flags = BindingFlags.Instance | BindingFlags.Public;
                                            if (data.HasFlags(SerializeFlags.SerializePrivateFields))
                                                _flags |= BindingFlags.NonPublic;
                                            foreach (var property in desc.type.GetProperties(_flags))
                                            {
                                                var attribs = (SameName[])property.GetCustomAttributes(typeof(SameName), false);
                                                if (attribs.Length > 0 && Array.FindIndex(attribs, x => x.Has(_field.header.tag)) >= 0)
                                                {
                                                    property.SetValue(desc._object, _field._object,null);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        desc.header = header;
                        return desc;
                    }
                case ObjectType.Custom:
                    {
                        ObjectDesciption desc;
                        desc.type = serializedType;
                        desc._object = CustomEntry.Read(reader, () => ReadObject(reader, data)._object, desc.type);
                        desc.header = header;
                        data.PropagateReference(desc._object, header.id);
                        return desc;
                    }
                case ObjectType.Null:
                    {
                        ObjectDesciption desc;
                        desc.type = serializedType;
                        desc._object = null;
                        NullEntry.Read(reader);
                        desc.header = header;
                        return desc;
                    }
                case ObjectType.Reference:
                    {
                        ObjectDesciption desc;
                        desc.type = serializedType;
                        desc._object = ReferenceEntry.Read(reader, data);
                        desc.header = header;
                        return desc;
                    }
                default: throw new InvalidOperationException("wrong object type : " + header.type);
            }
        }

        #region ImplInterface

        public event Action<KeyType, iBinarySerializer<KeyType>> OnSettingChange;
        public event Action<iBinarySerializer<KeyType>> OnSettingsLoad;
        public event Func<DefinedType, Type> OnTypeResolve;

        public void Declare<T>(KeyType key, T value, T defvalue, Action<T> handler = null)
        {
            object setup_obj = null;
            if (Settings.TryGetValue(key, out setup_obj))
            {
                Setting<T> setup = (Setting<T>)setup_obj;
                if (setup != null)
                {
                    setup.defValue = defvalue;
                    setup.handler = handler;
                }
            }
            else
            {
                Setting<T> setup = new Setting<T>(key, value, defvalue, handler);
                Settings[key] = setup;
            }
            EnsureGenericType<T>();
        }

        private bool GetInternal<T>(KeyType key,out Setting<T> setup)
        {
            object setup_obj = null;
            setup = default(Setting<T>);
            if (Settings.TryGetValue(key, out setup_obj))
            {
                try
                {
                    setup = (Setting<T>)setup_obj;
                    return true;
                }
                catch (InvalidCastException)
                {
                    return false;
                }
            }
            return false;
        }

        public T Get<T>(KeyType key)
        {
            T res = default(T);
            Get(key, out res);
            return res;
        }

        public bool Get<T>(KeyType key, out T value)
        {
            value = default(T);
            Setting<T> setup = null;
            if (GetInternal(key,out setup) && setup != null)
            {
                value = setup.value;
                return true;
            }
            return false;
        }

        public void Set<T>(KeyType key, T value)
        {
            object setup_obj = null;
            if (Settings.TryGetValue(key, out setup_obj))
            {
                Setting<T> setup = (Setting<T>)setup_obj;
                if (setup != null)
                {
                    setup.value = value;
                    if (setup.handler != null)
                        setup.handler(value);
                    if (OnSettingChange != null)
                        OnSettingChange(key, this);
                }
            }
            else
                throw new KeyNotFoundException("setting with that key is not exist : " + key);
        }

        public bool GetDefault<T>(KeyType key, out T value)
        {
            object setup_obj = null;
            value = default(T);
            if (Settings.TryGetValue(key, out setup_obj))
            {
                Setting<T> setup = (Setting<T>)setup_obj;
                if (setup != null)
                {
                    value = setup.defValue;
                    return true;
                }
            }
            return false;
        }

        public Action<T> GetHandler<T>(KeyType key)
        {
            Setting<T> setting = null;
            if (GetInternal(key, out setting))
                return setting.handler;
            return null;
        }

        public void SetHandler<T>(KeyType key, Action<T> handler)
        {
            Setting<T> setting = null;
            if (GetInternal(key, out setting))
                setting.handler = handler;
        }

        public bool HasKey(KeyType key)
        {
            return Settings.ContainsKey(key);
        }

        public void Save(Stream stream, SerializeFlags KeyFlags, SerializeFlags DataFlags)
        {
            if (Settings == null || Settings.Count < 1)
                return;
            bool wantCRC = (DataFlags & SerializeFlags.CRC32) == SerializeFlags.CRC32;
            bool wantBinaryKeys = (KeyFlags & SerializeFlags.BinaryKeys) == SerializeFlags.BinaryKeys;
            if (!typeof(KeyType).IsPrimitive && wantBinaryKeys)
                KeyFlags |= SerializeFlags.BinaryKeys;
            WrittingData _data = new WrittingData(DataFlags);
            WrittingData _binaryKeys = wantBinaryKeys ? _data.SharedInstanceWithFlags(KeyFlags) : null;
            _data.typeCollector.Add(typeof(NULL));
            _data.typeCollector.Add(typeof(KeyType));
            foreach (var val in Settings.Values)
                CollectTypes(val, _data);
            Stream layer = wantCRC ? new Crc32Stream(stream) : stream;
            BinaryWriter bwRes = new BinaryWriter(layer);
            BinaryHeader _header = new BinaryHeader() { version = CURRENT_VERSION, crc32 = 0, DataFlags = DataFlags, KeyFlags = KeyFlags };
            SettingTableHeader _settings = new SettingTableHeader() { settingsCount = Settings.Count,keyTypePosition = _data.typeCollector[typeof(KeyType)] };
            stream.Write(BitConverter.GetBytes(_header.crc32), 0, sizeof(uint)); // write without crc writter
            _header.WriteWithoutCRC(bwRes); // write witout uint src value
            _data.WriteTypeTable(bwRes);
            _settings.Write(bwRes);
            if (!wantCRC)
            {
                foreach (var kvp in Settings)
                {
                    if (!wantBinaryKeys)
                        bwRes.Write(kvp.Key.ToString());
                    else
                        WriteObject(kvp.Key, bwRes, _binaryKeys);
                    WriteObject(GetSettingFieldData<object>(kvp.Value, Setting<KeyType>.valueName), bwRes, _data);
                }
            }
            else
            {
                Crc32Stream crc = layer as Crc32Stream;
                foreach (var kvp in Settings)
                {
                    using (BinaryWriter bw_memory = new BinaryWriter(new MemoryStream()))
                    {
                        if (!wantBinaryKeys)
                            bw_memory.Write(kvp.Key.ToString());
                        else
                            WriteObject(kvp.Key, bw_memory, _binaryKeys);
                        WriteObject(GetSettingFieldData<object>(kvp.Value, Setting<KeyType>.valueName), bw_memory, _data);
                        bw_memory.BaseStream.Seek(0, SeekOrigin.Begin);
                        CopyStreamTo(bw_memory.BaseStream, crc);
                    }
                }
                long pos = crc.Position;
                uint crc_code = crc.GetHash();
                crc.Position = 0;
                crc.SourceStream.Write(BitConverter.GetBytes(crc_code), 0, sizeof(uint)); // write without crc writter
                crc.Position = pos;
            }
        }

        public LoadStatus Load(Stream stream, SerializeFlags KeyFlags, SerializeFlags DataFlags)
        {
            uint preComputeHash = 0;
            bool wantKeepTypes = (DataFlags & SerializeFlags.KeepDeclaredTypes) == SerializeFlags.KeepDeclaredTypes;
            bool wantCRC = (DataFlags & SerializeFlags.CRC32) == SerializeFlags.CRC32;
            bool wantKeyType = (KeyFlags & SerializeFlags.ValidateKeyType) == SerializeFlags.ValidateKeyType;
            if (!wantKeepTypes && Settings.Count > 0)
                Settings.Clear();
            if (wantCRC)
            {
                stream.Seek(sizeof(uint), SeekOrigin.Begin); // skip crc value
                preComputeHash = Crc32.Compute(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
            BinaryReader br = new BinaryReader(stream);
            BinaryHeader _header = new BinaryHeader();
            _header.Read(br);
            if (_header.version != CURRENT_VERSION)
                return LoadStatus.BINARY_HEADER_VERSION_ERROR;
            if (wantCRC && preComputeHash != _header.crc32)
                return LoadStatus.BAD_CRC32;
            ReadingData data = new ReadingData(_header.DataFlags);
            ReadingData binaryKeys = (_header.KeyFlags & SerializeFlags.BinaryKeys) == SerializeFlags.BinaryKeys ? data.SharedInstanceWithFlags(_header.KeyFlags) : null;
            SettingTableHeader settings = SettingTableHeader.Empty;
            data.ReadTypeTable(br, (qname,name) => GetTypeOrResolve(qname,name,DataFlags));
            settings.Read(br);
            if(wantKeyType)
            {
                if (data.TypeMap[settings.keyTypePosition] != typeof(KeyType))
                    return LoadStatus.BAD_KEYTYPE;
            }
           // string genericSettingName = RequiredTypeName(typeof(Setting<object>), DataFlags);
           // string defaultTypeName = RequiredTypeName(typeof(object), DataFlags);
            for (int i = 0; i < settings.settingsCount; ++i)
            {
                bool hasKey = false;
                KeyType key = default(KeyType);
                if (binaryKeys != null)
                {
                    long SrcPos = br.BaseStream.Position;
                    var keyObject = ReadObject(br, binaryKeys);
                    if (keyObject.type != null)
                    {
                        key = (KeyType)keyObject._object;
                        hasKey = true;
                    }
                    else
                        br.BaseStream.Position = SrcPos + keyObject.header.sizeInBytes;
                }
                else
                {
                    string keyName = br.ReadString();
                    hasKey = SerializeKeyFromString(keyName, out key);
                }
                if (hasKey)
                {
                    long SrcPos = br.BaseStream.Position;
                    var obj = ReadObject(br, data);
                    if (obj.type != null)
                    {
                        if (!Settings.ContainsKey(key))
                        {
                            // string settingName = genericSettingName.Replace(defaultTypeName, RequiredTypeName(obj.type, DataFlags));
                            // var genericType = GetTypeOrResolve(settingName, settingName, DataFlags);
                            var genericType = typeof(Setting<>).MakeGenericType(typeof(KeyType),obj.type);
                            object setting = FormatterActivate(genericType);
                            SetSettingsAsCtor(setting, key, obj._object, null, null,SerializeFlags.None);
                            Settings[key] = setting;
                        }
                        else
                        {
                            object setting = Settings[key];
                            SetSettingFieldValue(setting, obj._object);
                            SetSettingFieldKey(setting, key);
                        }
                    }
                    else
                        br.BaseStream.Position = SrcPos + obj.header.sizeInBytes;
                }
                else
                {
                    long SrcPos = br.BaseStream.Position;
                    ObjectHeader header = BaseEntry.ReadEntry(br);
                    br.BaseStream.Position = SrcPos + header.sizeInBytes;
                }
            }
            if (OnSettingsLoad != null)
                OnSettingsLoad(this);
            return LoadStatus.OK;
        }

        public byte[] Save(SerializeFlags KeyFlags,SerializeFlags DataFlags )
        {
            using (MemoryStream memory = new MemoryStream())
            {
                Save(memory, KeyFlags, DataFlags);
                return memory.ToArray();
            }
        }

        public LoadStatus Load(byte[] data, SerializeFlags KeyFlags, SerializeFlags DataFlags)
        {
            if (data == null || data.Length < 1)
                return LoadStatus.BAD_MEMORY;
            using (MemoryStream memory = new MemoryStream(data))
            {
                return Load(memory, KeyFlags, DataFlags);
            }
        }

        public void Save(string file, SerializeFlags KeyFlags, SerializeFlags DataFlags)
        {
            using (var fs = File.Create(file))
            {
                Save(fs, KeyFlags, DataFlags);
            }
        }

        public LoadStatus Load(string file, SerializeFlags KeyFlags, SerializeFlags DataFlags)
        {
            if (!File.Exists(file))
                return LoadStatus.BAD_FILE;
            using (var fs = File.OpenRead(file))
            {
                return Load(fs, KeyFlags, DataFlags);
            }
        }

        public void EnsureTypes(params Type[] types)
        {
            foreach(var srcType in types)
            {
                if(srcType != null)
                {
                    Type setting = typeof(Setting<>).MakeGenericType(typeof(KeyType),srcType);
                    NamesTypesMap[srcType.ToString()] = srcType;
                    NamesTypesMap[setting.ToString()] = setting;
                    QualifiedTypesMap[srcType.AssemblyQualifiedName] = srcType;
                    QualifiedTypesMap[setting.AssemblyQualifiedName] = setting;
                }
            }
        }

        public void EnsureGenericType<T>()
        {
            Type srcType = typeof(T);
            Type setting = typeof(Setting<T>);
            NamesTypesMap[srcType.ToString()] = srcType;
            NamesTypesMap[setting.ToString()] = setting;
            QualifiedTypesMap[srcType.AssemblyQualifiedName] = srcType;
            QualifiedTypesMap[setting.AssemblyQualifiedName] = setting;
        }

        public IEnumerator<KeyValuePair<KeyType, object>> GetEnumerator()
        {
            return new EnumerableSettings(Settings);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumerableSettings(Settings);
        }

        public void Dispose()
        {
            Settings.Clear();
            QualifiedTypesMap.Clear();
            NamesTypesMap.Clear();
            RefUtils.Dispose();
        }

        #endregion
    }
}