using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Garm.Options
{
    internal static partial class DefaultValues
    {
        private static void LoadDefaultsLoc(ref Dictionary<string, IEntry> dict)
        {
            dict.Add("loc_mapDepth", new Entry<string>("Depth", false, false, true));
            dict.Add("loc_mapProperties", new Entry<string>("Map properties", false, false, true));
            dict.Add("loc_mapWidth", new Entry<string>("Width", false, false, true));
            dict.Add("loc_meter", new Entry<string>("meter(s)", false, false, true));
            dict.Add("loc_ok", new Entry<string>("Ok", false, false, true));
        }
    }
}
