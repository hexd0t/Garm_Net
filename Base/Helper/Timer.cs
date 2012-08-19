using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Garm.Base.Helper
{
    public class HTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        private long lastTime;
        private long freq;

        // Constructor
        public HTimer()
        {
            lastTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported
                throw new Win32Exception();
            }
            QueryPerformanceCounter(out lastTime);
        }

        public double Peek
        {
            get
            {
                long currentTime;
                QueryPerformanceCounter(out currentTime);
                return (double)(currentTime - lastTime) / freq;
            }
        }

        public double Elapsed
        {
            get
            {
                long currentTime;
                QueryPerformanceCounter(out currentTime);
                double result = (double) (currentTime - lastTime)/freq;
                lastTime = currentTime;
                return result;
            }
        }
    }
}
