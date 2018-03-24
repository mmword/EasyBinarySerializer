using System;
using System.Collections.Generic;

namespace BinarySerializer
{
    [Flags]
    public enum SerializeFlags
    {
        None = 0,
       // Compressed = 1 << 0,
        CRC32 = 1 << 1,
        SerializePrivateFields = 1 << 2,
        SerializeProperties = 1 << 3,
        SerializePrivateProperties = 1 << 4,
        BinaryKeys = 1 << 5,
        KeepDeclaredTypes = 1 << 6,
        SkipQualifiedNames = 1 << 7,
        ValidateKeyType = 1 << 8,
        AllFields = SerializePrivateFields | SerializeProperties | SerializePrivateProperties
    }

    public enum LoadStatus
    {
        OK,
        BINARY_HEADER_VERSION_ERROR,
        BAD_CRC32,
        BAD_MEMORY,
        BAD_FILE,
        BAD_KEYTYPE
    }

    public struct DefinedType
    {
        public string QualifiedName;
        public string Name;
        public string TypeName
        {
            get
            {
                int pos = Name.LastIndexOf("FileSettings.");
                if (pos >= 0)
                    return Name.Substring(pos, (Name.Length - pos - 1));
                return string.Empty;
            }
        }
        public string ShortTypeName
        {
            get
            {
                string s = TypeName;
                if (!string.IsNullOrEmpty(s))
                {
                    int pos = s.LastIndexOf('+');
                    if (pos >= 0 && (pos+1) < s.Length)
                        s = s.Substring(pos+1);
                }
                return s;
            }
        }
    }

    public class SameName : Attribute
    {
        HashSet<string> Names;
        public SameName(params string []names)
        {
            Names = new HashSet<string>(names);
        }
        public SameName(bool invariant,params string[] names)
        {
            if (invariant)
                Names = new HashSet<string>(names, StringComparer.InvariantCultureIgnoreCase);
            else
                Names = new HashSet<string>(names);
        }
        public bool Has(string name)
        {
            return Names.Contains(name);
        }
    }

    public abstract class Factory
    {
        static public iBinarySerializer<KeyType> QuerySerializerInterface<KeyType>()
        {
            return new BinarySerializerInternal<KeyType>();
        }
        static public iSerializePrinter QuerySerializerPrinterInterace()
        {
            return new SerializerPrinter();
        }
    }
}
