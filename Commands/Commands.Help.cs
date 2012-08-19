using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Garm.Commands
{
    internal static partial class Commands
    {
        public static void Help(string command = "")
        {
            switch (command.ToLower())
            {
                case "set":
                    Console.WriteLine("The set command can be used to nonpermanently set any engine-variable. For reading variables, see 'help var', for persistent changes see 'help setu'.\n"+
                        "Usage: set [[<Assembly] <Type>] <Key> <Value>");
                    break;
                case "setu":
                    Console.WriteLine("The setu command can be used to permanently set any engine-variable. For reading variables, see 'help var', for nonpersistent changes see 'help set'.\n" +
                        "Usage: set [[<Assembly] <Type>] <Key> <Value>");
                    break;
                case "var":
                    Console.WriteLine();
                    break;
                case "":
                    Console.WriteLine("Garm RTS\n"+
                        "Available commands: (view help x for detailed information on command x)\n" +
                        "  set [[<Assembly] <Type>] <Key> <Value>\n" +
                        "  setu [[<Assembly] <Type>] <Key> <Value>\n" +
                        "  quit/stop/exit\n" +
                        "  var");
                    break;
                default:
                    Console.WriteLine("No help available for '"+command+"'.");
                    Help();
                    break;
            }
        }
    }
}
