using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Garm.Options
{
    internal class UserPreferences
    {
        public string PrefFile { get; private set; }
        public UserPreferences(string file)
        {
            PrefFile = file;
        }

        public void Load( Options options )
        {
            if (!File.Exists(PrefFile))
            {
                Console.WriteLine("[Info] No user preferences defined");
                return;
            }
            try
            {
                var fs = File.Open(PrefFile, FileMode.Open);
                using (var reader = XmlReader.Create(fs))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            if (reader.Name == "option")
                            {
                                Type type;
                                if (String.IsNullOrWhiteSpace(reader["assembly"]) ||
                                    String.IsNullOrWhiteSpace(reader["type"]))
                                    type = Type.GetType(reader["type"] ?? "System.String");
                                else
                                {
                                    type = Assembly.LoadFrom(reader["assembly"]).GetType(reader["type"]);
                                }

                                string key = reader["key"];
                                if (String.IsNullOrWhiteSpace(key))
                                {
                                    Console.WriteLine("[Warning] UserPreference of type '"+type.FullName+"' has no key defined, skipping");
                                    continue;
                                }
                                reader.Read();
                                options.ParseSet(key, type, reader.Value, false);
                            }
                            else if (reader.Name == "dict")
                            {
                                Type typeA;
                                if (String.IsNullOrWhiteSpace(reader["assemblyA"]) ||
                                    String.IsNullOrWhiteSpace(reader["typeA"]))
                                    typeA = Type.GetType(reader["typeA"] ?? "System.String");
                                else
                                {
                                    typeA = Assembly.LoadFrom(reader["assemblyA"]).GetType(reader["typeA"]);
                                }
                                Type typeB;
                                if (String.IsNullOrWhiteSpace(reader["assemblyB"]) ||
                                    String.IsNullOrWhiteSpace(reader["typeB"]))
                                    typeB = Type.GetType(reader["typeB"] ?? "System.String");
                                else
                                {
                                    typeB = Assembly.LoadFrom(reader["assemblyB"]).GetType(reader["typeB"]);
                                }
                                var typeDict = typeof(Dictionary<,>).MakeGenericType(new[] { typeA, typeB });

                                string key = reader["key"];
                                if (String.IsNullOrWhiteSpace(key))
                                {
                                    Console.WriteLine("[Warning] UserPreferenced Dict has no key defined, skipping");
                                    continue;
                                }
                                var dict = Activator.CreateInstance(typeDict);
                                var addmethod = typeDict.GetMethod("Add", new[]{typeA, typeB});
                                while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "dict" ))
                                {
                                    reader.Read();
                                    if (reader.IsStartElement() && reader.Name == "row")
                                    {
                                        string index = reader["index"];
                                        reader.Read();
                                        string value = reader.Value;
                                        addmethod.Invoke(dict, new[] { Options.Parse(typeA, index), Options.Parse(typeB, value) });
                                    }
                                }
                                typeof(Options).GetMethod("Set").MakeGenericMethod(new[] { typeDict }).Invoke(options, new[] { key, dict, false });
                            }
                            else if (reader.Name == "list")
                            {
                                Type typeA;
                                if (String.IsNullOrWhiteSpace(reader["assemblyA"]) ||
                                    String.IsNullOrWhiteSpace(reader["typeA"]))
                                    typeA = Type.GetType(reader["typeA"] ?? "System.String");
                                else
                                {
                                    typeA = Assembly.LoadFrom(reader["assemblyA"]).GetType(reader["typeA"]);
                                }
                                
                                var typeList = typeof(List<>).MakeGenericType(new[] { typeA });

                                string key = reader["key"];
                                if (String.IsNullOrWhiteSpace(key))
                                {
                                    Console.WriteLine("[Warning] UserPreferenced List has no key defined, skipping");
                                    continue;
                                }
                                var list = Activator.CreateInstance(typeList);
                                var addmethod = typeList.GetMethod("Add", new[] { typeA });
                                while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "dict"))
                                {
                                    reader.Read();
                                    if (reader.IsStartElement() && reader.Name == "row")
                                    {
                                        string index = reader["index"];
                                        reader.Read();
                                        string value = reader.Value;
                                        addmethod.Invoke(list, new[] { Options.Parse(typeA, index) });
                                    }
                                }
                                typeof(Options).GetMethod("Set").MakeGenericMethod(new[] { typeList }).Invoke(options, new[] { key, list, false });
                            }
                        }
                    }
                }
                fs.Close();
            }
            catch
            {
                Console.WriteLine("[Warning] Cannot load user preferences");
            }
        }

        public void Save(Options options)
        {
            var uservars = options.GetListCopy(Options.OptionLevel.User);
            var defaultvars = options.GetListCopy(Options.OptionLevel.Default);
            var writequeue = new List<KeyValuePair<string, KeyValuePair<Type, object>>>(uservars.Count);
            var cheats = options.Cheats;
            foreach (var uservar in uservars)
            {
                if (!defaultvars.ContainsKey(uservar.Key))
                {//Non-default variable
                    writequeue.Add(new KeyValuePair<string, KeyValuePair<Type, object>>(uservar.Key, 
                        new KeyValuePair<Type, object>(uservar.Value.DataType, uservar.Value.Data)));
                    continue;
                }
                if(uservar.Value.Data.Equals(defaultvars[uservar.Key].Data))
                    continue;// Uservar contains same setting as default, skip (even if cheat!)
                if (!cheats && defaultvars[uservar.Key].Cheat)
                    cheats = true;
                writequeue.Add(new KeyValuePair<string, KeyValuePair<Type, object>>(uservar.Key,
                    new KeyValuePair<Type, object>(uservar.Value.DataType, uservar.Value.Data)));
            }

            var settings = new XmlWriterSettings
                               {
                                   Indent = true,
                                   IndentChars = "\t",
                                   Encoding = Encoding.Unicode
                               };
            if (!Directory.Exists(PrefFile))
            {
                var dirname = Path.GetDirectoryName(PrefFile);
                Directory.CreateDirectory(dirname);
            }

            using (XmlWriter writer = XmlWriter.Create(PrefFile, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("settings");
                writer.WriteAttributeString("cheats", cheats.ToString());

                foreach (var setting in writequeue)
                {
                    if (setting.Value.Value is IEnumerable)
                    {
                        if (!setting.Value.Key.IsGenericType)
                        {
                            Console.WriteLine("[Warning] Cannot serialize non-generic IEnumerables, skipping key '" + setting.Key + "'");
                            continue;
                        }
                        if(setting.Value.Value is IList)
                            writer.WriteStartElement("list");
                        else if (setting.Value.Value is IDictionary)
                            writer.WriteStartElement("dict");
                        else
                        {
                            Console.WriteLine("[Warning] Cannot serialize IEnumerable of type '" + setting.Value.Key + "', skipping key '"+setting.Key+"'");
                            continue;
                        }

                        writer.WriteAttributeString("key", setting.Key);

                        Type typeA = setting.Value.Key.GetGenericArguments()[0];
                        var typestringA = TypeToMinimalString(typeA);
                        writer.WriteAttributeString("typeA", typestringA[0]);
                        if (typestringA.Length > 1)
                            writer.WriteAttributeString("assemblyA", typestringA[1]);

                        if (setting.Value.Value is IDictionary)
                        {
                            Type typeB = setting.Value.Key.GetGenericArguments()[1];
                            var typestringB = TypeToMinimalString(typeB);
                            writer.WriteAttributeString("typeB", typestringB[0]);
                            if (typestringB.Length > 1)
                                writer.WriteAttributeString("assemblyB", typestringB[1]);
                            var fu =typeof(UserPreferences).GetMethod("WriteDict", BindingFlags.NonPublic|BindingFlags.Static);
                            var fu2 = fu.MakeGenericMethod(new[] { typeA, typeB });
                            fu2.Invoke(null, new[] { setting.Value.Value, writer });
                        }
                        else
                            typeof(UserPreferences).GetMethod("WriteList").MakeGenericMethod(new[] { typeA }).Invoke(null, new[] { setting.Value.Value, writer });

                        writer.WriteEndElement();
                        continue;
                    }
                    writer.WriteStartElement("option");

                    writer.WriteAttributeString("key", setting.Key);

                    var typestring = TypeToMinimalString(setting.Value.Key);
                    writer.WriteAttributeString("type", typestring[0]);
                    if(typestring.Length>1)
                        writer.WriteAttributeString("assembly", typestring[1]);

                    writer.WriteString(setting.Value.Value.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
        }

        protected string[] TypeToMinimalString(Type t)
        {

            if (Type.GetType(t.ToString()) != null) //No assembly-specification needed
                return new[] { t.ToString() };

            string assemblyFile = Path.GetFileName(t.Assembly.Location);
            if (File.Exists(assemblyFile)
                && (Assembly.LoadFrom(assemblyFile) ?? Assembly.GetExecutingAssembly()
                   ).GetType(t.ToString()) != null)
                return new[] { t.ToString(), assemblyFile };

            return new[] { t.AssemblyQualifiedName };
        }
        protected static void WriteDict<TKey, TValue>(IDictionary<TKey, TValue> dict, XmlWriter writer)
        {
            foreach (var value in dict)
            {
                writer.WriteStartElement("row");
                writer.WriteAttributeString("index", value.Key.ToString());
                writer.WriteValue(value.Value.ToString());
                writer.WriteEndElement();
            }
        }
        protected static void WriteList<TValue>(IList<TValue> list, XmlWriter writer)
        {
            foreach (var value in list)
            {
                writer.WriteStartElement("row");
                writer.WriteValue(value.ToString());
                writer.WriteEndElement();
            }
        }
        protected static object Parse(Type type, string value)
        {
            object obj;

            MethodInfo parsemethod = type.GetMethod("Parse", new[] { typeof(string) });
            if (parsemethod != null)
            {
                obj = parsemethod.Invoke(null, new object[] { value });
            }
            else
            {
                obj = type.IsEnum ? Enum.Parse(type, value) : value;
            }
            return obj;
        }
    }
}
