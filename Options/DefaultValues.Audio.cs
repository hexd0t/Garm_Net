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
            dict.Add("snd_externalMusicFolder", new Entry<string>(""));
            dict.Add("snd_", new Entry<string>(""));
        }
    }
}
