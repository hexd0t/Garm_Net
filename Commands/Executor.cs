using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Garm.Base.Interfaces;

namespace Garm.Commands
{
    public class Executor : Base.Abstract.Base, ICommandExecutor
    {
        public Executor(IRunManager manager) : base(manager)
        {
        }

        public void Parse (string command)
        {
            if(String.IsNullOrWhiteSpace(command))
                return;
            var parts = ParseParts(command);
            switch (parts[0].ToLower())
            {
                case "quit":
                case "stop":
                case "exit":
                    Manager.DoRun = false;
                    break;
                case "help":
                    Commands.Help(parts.Length>1?parts[1]:"");
                    break;
                case "var":
                    switch (parts.Length)
                    {
                        case 1:
                            var info = Manager.Opts.GetStats();
                            Console.WriteLine("Options stats: "+info[0]+"/R, "+info[1]+"/U, "+info[2]+"/D");
                            break;
                        case 2:
                            var var_type = Manager.Opts.GetType(parts[1]);
                            if (var_type == null)
                            {
                                Console.WriteLine("Option '"+parts[1]+"' is not defined!");
                                break;
                            }
                            var content = typeof(IOptionsProvider).GetMethod("Get").MakeGenericMethod(new[] { var_type }).Invoke(Manager.Opts, new object[] { parts[1] });
                            var enumerable = content as IEnumerable;
                            if (enumerable != null)
                            {
                                var sb = new StringBuilder();
                                sb.AppendLine("Enumerating '" + parts[1] + "' of type " + var_type + ":");
                                var enumerator = enumerable.GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    var line = "  " + enumerator.Current;
                                    for (int i = 0; i < line.Length; i += Console.WindowWidth - (i == 0 ? 0 : 4))
                                    {
                                        sb.Append(i == 0 ? "" : "    ");
                                        sb.Append(line.Substring(i, Math.Min(Console.WindowWidth - (i==0?0:4) , line.Length-i)));
                                    }
                                    sb.AppendLine();
                                }
                                Console.Write(sb.ToString());
                                break;
                            }
                            Console.WriteLine("'"+parts[1]+"' of type "+var_type+" contains: '"+content+"'");
                            break;
                    }
                    break;
                case "set":
                    try
                    {
                        switch (parts.Length)
                        {
                            case 3: //Key/Value only
                                var oldType = Manager.Opts.GetType(parts[1]);
                                if (oldType == null)
                                {
#if DEBUG
                                    Console.WriteLine("[Info] Option '" + parts[1] + "' is not defined yet, defaulting type to string");
#endif
                                    oldType = typeof(string);
                                }
                                Manager.Opts.ParseSet(parts[1], oldType, parts[2]);
                                break;
                            case 4: //Type, Key, Value
                                var type = Type.GetType(parts[1]);
                                if(type == null)
                                {
                                    Console.WriteLine("[Warning] Type '" + parts[1] + "' is not defined, defaulting type to string");
                                    type = typeof(string);
                                }
                                Manager.Opts.ParseSet(parts[2], type, parts[3]);
                                break;
                            case 5: //Assembly, Type, Key, Value
                                try
                                {
                                    type = Assembly.LoadFrom(parts[1]).GetType(parts[2]);
                                }
                                catch (FileNotFoundException)
                                {
                                    Console.WriteLine("[Warning] Assembly '" + parts[1] + "' not found, defaulting type to string");
                                    type = typeof(string);
                                }

                                if (type == null)
                                {
                                    Console.WriteLine("[Warning] Type '"+parts[1]+"'->'" + parts[2] + "' is not defined, defaulting type to string");
                                    type = typeof(string);
                                }
                                Manager.Opts.ParseSet(parts[3], type, parts[4]);
                                break;
                            default:
                                Console.WriteLine("[Warning] Cannot parse '" + command + "', see 'help set'");
                                break;
                        }

                    }
                    catch
                    {
                        Console.WriteLine("[Warning] Cannot parse '" + command + "', check your spelling and see 'help set'");
                    }
                    break;
                case "setu":
                    try
                    {
                        switch (parts.Length)
                        {
                            case 3: //Key/Value only
                                var oldType = Manager.Opts.GetType(parts[1]);
                                if (oldType == null)
                                {
#if DEBUG
                                    Console.WriteLine("[Info] Option '" + parts[1] + "' is not defined yet, defaulting type to string");
#endif
                                    oldType = typeof(string);
                                }
                                Manager.Opts.ParseSet(parts[1], oldType, parts[2], false);
                                break;
                            case 4: //Type, Key, Value
                                var type = Type.GetType(parts[1]);
                                if (type == null)
                                {
                                    Console.WriteLine("[Warning] Type '" + parts[1] + "' is not defined, defaulting type to string");
                                    type = typeof(string);
                                }
                                Manager.Opts.ParseSet(parts[2], type, parts[3], false);
                                break;
                            case 5: //Assembly, Type, Key, Value
                                try
                                {
                                    type = Assembly.LoadFrom(parts[1]).GetType(parts[2]);
                                }
                                catch (FileNotFoundException)
                                {
                                    Console.WriteLine("[Warning] Assembly '" + parts[1] + "' not found, defaulting type to string");
                                    type = typeof(string);
                                }

                                if (type == null)
                                {
                                    Console.WriteLine("[Warning] Type '" + parts[1] + "'->'" + parts[2] + "' is not defined, defaulting type to string");
                                    type = typeof(string);
                                }
                                Manager.Opts.ParseSet(parts[3], type, parts[4], false);
                                break;
                            default:
                                Console.WriteLine("[Warning] Cannot parse '" + command + "', see 'help set'");
                                break;
                        }

                    }
                    catch
                    {
                        Console.WriteLine("[Warning] Cannot parse '" + command + "', check your spelling and see 'help set'");
                    }
                    break;
                case "load":
                    {
                        if (parts.Length != 2)
                        {
                            Console.WriteLine("[Warning] Cannot parse '" + command + "', see 'help load'");
                            break;
                        }

                    }
                    break;
#region Shortcuts
                case "wireframe":
                    Parse("set rndr_wireframe "+(Manager.Opts.Get<bool>("rndr_wireframe")?"false":"true"));
                    break;
                case "incorporeal":
                    Parse("set rndr_incorporeal " + (Manager.Opts.Get<bool>("rndr_incorporeal") ? "false" : "true"));
                    break;
                case "cull":
                    Parse("set rndr_cull " + (Manager.Opts.Get<bool>("rndr_cull") ? "false" : "true"));
                    break;
#endregion
                default:
                    Console.WriteLine("[Warning] Cannot parse '" + command + "', see 'help'");
                    break;
            }
        }

        protected string[] ParseParts (string input)
        {
            bool insidequotes = false;
            var parts = new List<string>();
            var current = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case ' ':
                        if (insidequotes)
                            current.Append(input[i]);
                        else
                        {
                            parts.Add(current.ToString());
                            current.Clear();
                        }
                        break;
                    case '"':
                    case '\'':
                        insidequotes = !insidequotes;
                        break;
                    default:
                        current.Append(input[i]);
                        break;
                }
            }
            if(current.Length>0)
                parts.Add(current.ToString());
            return parts.ToArray();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
