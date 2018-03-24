using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BinarySerializer
{
    interface WrittingInterface : IDisposable
    {
        void Write(string format, params object[] arg);
        void WriteLine(string format, params object[] arg);
        void Write(char c);
    }

    sealed class StreamWritting : WrittingInterface
    {
        StreamWriter writter;
        public StreamWritting(Stream stream)
        {
            writter = new StreamWriter(stream);
        }
        public StreamWritting(string file)
        {
            writter = File.CreateText(file);
        }
        public void Dispose()
        {
            writter.Dispose();
        }
        public void Write(char c)
        {
            writter.Write(c);
        }
        public void Write(string format, params object[] arg)
        {
            writter.Write(format, arg);
        }

        public void WriteLine(string format, params object[] arg)
        {
            writter.WriteLine(format, arg);
        }
    }

    sealed class TextWritting : WrittingInterface
    {
        StringBuilder builder = new StringBuilder();
        public void Dispose()
        {

        }
        public void Write(char c)
        {
            builder.Append(c);
        }
        public void Write(string format, params object[] arg)
        {
            if (arg.Length > 0)
                builder.Append(string.Format(format, arg));
            else
                builder.Append(format);
        }
        public void WriteLine(string format, params object[] arg)
        {
            if (arg.Length > 0)
                builder.AppendLine(string.Format(format, arg));
            else
                builder.AppendLine(format);
        }
        public string GetText()
        {
            return builder.ToString();
        }
    }

    sealed class Writter : IDisposable
    {
        WrittingInterface writter;
        int spaces;
        int skip;
        public string GetString()
        {
            if (writter is TextWritting)
                return (writter as TextWritting).GetText();
            return string.Empty;
        }
        void AddSpaces()
        {
            if(spaces > 0)
                writter.Write(new string(' ', spaces));
        }
        public Writter()
        {
            writter = new TextWritting();
            spaces = skip = 0;
        }
        public Writter(Stream stream)
        {
            writter = new StreamWritting(stream);
            spaces = skip = 0;
        }
        public Writter(string file)
        {
            writter = new StreamWritting(file);
            spaces = skip = 0;
        }
        public void Write(string fmt,params object []args)
        {
            if (skip < 1)
            {
                AddSpaces();
                writter.Write(fmt, args);
            }
            else
            {
                writter.Write(' ');
                writter.Write(fmt, args);
                skip--;
            }
        }
        public void WriteNoSpace(string fmt, params object[] args)
        {
            writter.Write(fmt, args);
        }
        public void WriteLine(string fmt, params object[] args)
        {
            if (skip < 1)
            {
                AddSpaces();
                writter.WriteLine(fmt, args);
            }
            else
            {
                writter.Write(' ');
                writter.WriteLine(fmt, args);
                skip--;
            }
        }
        public void WriteLineNoSpace(string fmt, params object[] args)
        {
            writter.WriteLine(fmt, args);
        }
        public void BeginSpace(string fmt, params object[] args)
        {
            Write(fmt, args);
            writter.WriteLine(string.Empty);
            Write("{");
            writter.WriteLine(string.Empty);
            spaces++;
        }
        public void EndSpace(bool EndPrimitive = false)
        {
            spaces--;
            AddSpaces();
            writter.Write('}');
            if (EndPrimitive)
                writter.Write(';');
            writter.WriteLine(string.Empty);
        }
        public void SkipSpaces(int count)
        {
            skip += count;
        }
        public void Dispose()
        {
            writter.Dispose();
        }
    }

    sealed class SerializerPrinter : iSerializePrinter
    {
        ReflectionUtils RefUtils = new ReflectionUtils();
        Dictionary<object, int> References = new Dictionary<object, int>();
        HashSet<Type> SkippedTypes;
        SerializeFlags currentFlags;
        Writter writter;

        string CompleteGenericName(Type t)
        {
            string name = t.Name;
            var gen = t.GetGenericArguments();
            if (gen.Length > 0)
            {
                string s = string.Empty;
                foreach (var g in gen)
                    s += g.Name + ',';
                name = string.Format("{0}<{1}>", name, s);
            }
            return name;
        }

        bool IsSkipped(Type t)
        {
            if (t == null || SkippedTypes == null)
                return false;
            return SkippedTypes.Contains(t);
        }

        bool CustomParse(object val,string _tag, Action<object> OnWrite)
        {
            Type t = val.GetType();
            if (typeof(IDictionary).IsAssignableFrom(t))
            {
                IDictionary dict = (IDictionary)val;
                writter.BeginSpace("IDictionary({0}) {1} sz({2}) ", CompleteGenericName(t), _tag, dict.Count);
                int idx = 0;
                foreach (DictionaryEntry kvp in dict)
                {
                    writter.BeginSpace("key({0}) = ",idx);
                    OnWrite(kvp.Key);
                    writter.EndSpace();
                    writter.BeginSpace("value({0}) = ",idx);
                    OnWrite(kvp.Value);
                    writter.EndSpace();
                    ++idx;
                }
                writter.EndSpace();
                return true;
            }
            else if (typeof(IList).IsAssignableFrom(t))
            {
                IList list = (IList)val;
                writter.BeginSpace("IList({0}) {1} sz({2}) ", CompleteGenericName(t), _tag, list.Count);
                int idx = 0;
                foreach (var v in list)
                {
                    writter.BeginSpace("element({0}) ", idx++);
                    OnWrite(v);
                    writter.EndSpace();
                }
                writter.EndSpace();
                return true;
            }
            else if (typeof(ICollection).IsAssignableFrom(t))
            {
                ICollection list = (ICollection)val;
                writter.BeginSpace("ICollection({0}) {1} sz({2}) ", CompleteGenericName(t), _tag, list.Count);
                int idx = 0;
                foreach (var v in list)
                {
                    writter.BeginSpace("element({0}) ", idx++);
                    OnWrite(v);
                    writter.EndSpace();
                }
                writter.EndSpace();
                return true;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(t))
            {
                IEnumerable col = (IEnumerable)val;
                int sz = 0,idx = 0;
                foreach (var v in col)
                    sz++;
                writter.BeginSpace("IEnumerable({0}) {1} sz({2}) ", CompleteGenericName(t), _tag, sz);
                foreach (var v in col)
                {
                    writter.BeginSpace("element({0}) ", idx++);
                    OnWrite(v);
                    writter.EndSpace();
                }
                writter.EndSpace();
                return true;
            }
            return false;
        }

        bool HasFlag(SerializeFlags flag)
        {
            return (currentFlags & flag) == flag;
        }

        bool HasReferenceOrAdd(object obj)
        {
            if (!References.ContainsKey(obj))
            {
                References[obj] = obj.GetHashCode();
                return false;
            }
            return true;
        }

        void Print(object val, string _tag = "")
        {
            if (val == null)
            {
                writter.WriteLine("{0} = null;", _tag);
                return;
            }
            Type t = val.GetType();
            if(IsSkipped(t))
            {
                writter.WriteLine("--skip {0}--", t.Name);
                return;
            }
            if (t.IsPrimitive || t.IsEnum || t == typeof(string))
                writter.WriteLine("{0} {1} = {2};",t.Name, _tag,val.ToString());
            else if (t.IsArray)
            {
                Type arrType = t;
                t = t.GetElementType();
                writter.BeginSpace("{0}({1}) {2} ", HasReferenceOrAdd(val) ? "(Reference)" : "" + arrType.Name, t.Name, _tag);
                Array arr = val as Array;
                if (t.IsPrimitive || t.IsEnum || t == typeof(string))
                {
                    writter.Write("");
                    for (int i = 0; i < arr.Length; ++i)
                    {
                        writter.WriteNoSpace(arr.GetValue(i).ToString());
                        if(i < arr.Length-1)
                            writter.WriteNoSpace(",");
                    }
                    writter.WriteLineNoSpace("");
                    writter.EndSpace(true);
                    return;
                }
                else if (t.IsArray)
                {
                    foreach (var el in arr)
                        Print(el,_tag);
                }
                else if (t.IsClass || t.IsValueType)
                {
                    int idx = 0;
                    foreach (var el in arr)
                    {
                        if (el == null)
                            writter.WriteLine("element {0} of ({1}) {2} = null", idx, t.Name, _tag);
                        else if (!CustomParse(el, _tag, (_obj) => Print(_obj, _tag)))
                            Print(el, string.Format("element ({0}) of {1}", idx, _tag));
                        idx++;
                    }
                }
                writter.WriteLine("(end of {0}({1})", arrType.Name, t.Name);
                writter.EndSpace();
            }
            else if (t.IsClass || t.IsValueType)
            {
                if (!CustomParse(val,_tag, (_obj) => Print(_obj, _tag)))
                {
                    if (t.IsClass)
                        writter.BeginSpace("class {0} {1} ", HasReferenceOrAdd(val) ? "(Reference)" : "" + t.Name, _tag);
                    else
                        writter.BeginSpace("struct {0} {1} ", t.Name, _tag);
                    // Fields
                    var allf = RefUtils.CacheFields(t, HasFlag(SerializeFlags.SerializePrivateFields));
                    if (allf.Length > 0)
                    {
                        foreach (var f in allf)
                        {
                            object v = f.GetValue(val);
                            if (v == null)
                                writter.WriteLine("{0} {1} = null", f.IsPublic ? "public" : "---", f.Name);
                            else
                            {
                                writter.Write(f.IsPublic ? "public" : "---");
                                writter.SkipSpaces(1);
                                Print(v,f.Name);
                            }
                        }
                    }
                    if (HasFlag(SerializeFlags.SerializeProperties))
                    {
                        // Properties
                        var allp = RefUtils.CacheProperties(t, HasFlag(SerializeFlags.SerializePrivateProperties));
                        if (allp.Length > 0)
                        {
                            foreach (var p in allp)
                            {
                                object v = p.GetValue(val, null);
                                if (v == null)
                                    writter.WriteLine("property {0} = null", p.Name);
                                else
                                {
                                    writter.Write("property - ");
                                    writter.SkipSpaces(1);
                                    Print(v, p.Name);
                                }
                            }
                        }
                    }
                    writter.EndSpace();
                }
            }
            else
                throw new InvalidOperationException("cant serialize object type : " + t.Name);
        }

        #region InterfaceImpl

        public void PrintTo(object val, string entryName,Stream stream, SerializeFlags DataFlags = SerializeFlags.None)
        {
            writter = new Writter(stream);
            currentFlags = DataFlags;
            Print(val, entryName);
        }

        public void PrintTo(object val, string entryName,string file, SerializeFlags DataFlags = SerializeFlags.None)
        {
            currentFlags = DataFlags;
            using (writter = new Writter(file))
            {
                Print(val, entryName);
            }
        }

        public string PrintToString(object val, string entryName, SerializeFlags DataFlags = SerializeFlags.None)
        {
            currentFlags = DataFlags;
            using (writter = new Writter())
            {
                Print(val, entryName);
                return writter.GetString();
            }
        }

        public void Dispose()
        {
            RefUtils.Dispose();
            References.Clear();
        }

        public void SkipTypes(params Type[] skipTypes)
        {
            if (SkippedTypes == null)
                SkippedTypes = new HashSet<Type>(skipTypes);
            else
                SkippedTypes.UnionWith(skipTypes);
        }

        #endregion
    }
}
