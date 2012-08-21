using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garm.Base.Helper
{
    public partial class FileManager
    {
        public class DirectoryInfo
        {
            public List<DirectoryInfo> Subdirectories;
            public List<string> Files;
            public string Name;
        }

        [Flags]
        public enum FileSource : byte
        {
            All = 0xFF,
            LocalUserData = 0x1,
            Network = 0x2,
            Local = 0x4,
            LocalCompressed = 0x8
        }
    }
}
