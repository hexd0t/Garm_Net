using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Garm.Options
{
    internal static partial class DefaultValues
    {
        private static void LoadDefaultsAudio(ref Dictionary<string, IEntry> dict)
        {
            dict.Add("snd_musicProvider", new Entry<short>(0));
            dict.Add("snd_ext_musicFolder", new Entry<string>(""));
            dict.Add("snd_ext_useSpecialFolder", new Entry<bool>(true));
            dict.Add("snd_ext_specialFolder", new Entry<Environment.SpecialFolder>(Environment.SpecialFolder.MyMusic));
        }
    }
}
