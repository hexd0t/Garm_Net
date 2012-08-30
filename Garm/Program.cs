using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Garm
{
    static class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Commandline parameters</param>
        [STAThread]
        static void Main(string[] args)
        {
#if DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            Base.Abstract.Base.IsApplication = true;

            if (args.Length > 0 && args[0] == "brutesettings")
            {
                //ToDo: Implement optimal settings benchmark
            }
            else
            {
                var instance = new RunManager(args);
                instance.Run();
            }
        }

#if DEBUG
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if(System.Diagnostics.Debugger.IsAttached)
                return;
            var ex = e.ExceptionObject as Exception;
            if(ex == null)
                Console.WriteLine("[Critial] An unhandled exception occured; cannot display data!");
            else
            {
                Console.WriteLine("[Critial] An unhandled exception occured:\n" + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
            }
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }
#endif
    }
}
