using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Garm
{
    public partial class RunManager
    {
        /// <summary>
        /// Requests a console-window to be opened
        /// </summary>
        /// <returns>Success of opening the window</returns>
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        /// <summary>
        /// Attaches the calling process to the console of the specified process.
        /// </summary>
        /// <param name="dwProcessId">[in] Identifier of the process, usually will be ATTACH_PARENT_PROCESS</param>
        /// <returns>If the function succeeds, the return value is nonzero
        /// If the function fails, the return value is zero.
        /// To get extended error information, call Marshal.GetLastWin32Error</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId = 0x0ffffffff);

        private StreamMultiplexer _outputMultiplexer;
        /// <summary>
        /// Creates a StreamMultiplexer and redirects Console-Output to it
        /// </summary>
        internal void InstallMultiplexer ()
        {
            _outputMultiplexer = new StreamMultiplexer();
            Console.SetOut(_outputMultiplexer);
        }

        internal bool consoleShown = false;

        /// <summary>
        /// Shows a console window where all Debug-Messages are displayed
        /// </summary>
        internal void ShowConsole()
        {
            if(consoleShown)
                return;
            consoleShown = true;
            AllocConsole();
            AttachConsole();
            Console.Title = "Garm";
            _outputMultiplexer.Targets.Add(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
    }

    internal class StreamMultiplexer : TextWriter
    {
        public readonly List<TextWriter> Targets;

        public StreamMultiplexer()
        {
            Targets = new List<TextWriter>(2);
        }

        public override Encoding Encoding
        {
            get { return Targets[0].Encoding; }
        }

        public override void Write(char[] buffer, int index, int count)
        {
#if DEBUG
            var st = new StackTrace(true);
            StackFrame frame = null;
            var frames = st.GetFrames();
            if (frames != null)
                for (int i = 1; i < frames.Length; i++)
                {
                    if (frames[i].GetMethod().DeclaringType.ToString().Substring(0, 4).ToLower().Equals("garm"))
                    {
                        frame = frames[i];
                        break;
                    }
                }
#endif
            foreach (var target in Targets)
            {
#if DEBUG
                target.Write("[" + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":"
                + DateTime.Now.Second.ToString("00"));
                if(frame != null)
                    target.Write("@" + frame.GetMethod().DeclaringType.ToString().Replace("Garm.", "") + ":" + frame.GetMethod().Name);
                target.Write("] ");
#endif
                target.Write(buffer, 0, count);
            }
        }
    }
}
