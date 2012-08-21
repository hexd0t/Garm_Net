using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Garm.Options
{
    internal static partial class DefaultValues
    {
        public static void LoadDefaults(ref Dictionary<string, IEntry> dict)
        {
            dict.Add("sys_runMode", new Entry<Base.Enums.RunMode>(Base.Enums.RunMode.Play));
            dict.Add("sys_files_cacheThreshold", new Entry<int>(5000));
            dict.Add("sys_dirInfo_maxRecursionDepth", new Entry<int>(int.MaxValue));
            dict.Add("sys_files_cacheMaxRequestCount", new Entry<int>(6));//The maximum 
            dict.Add("sys_useDataSpecialFolder", new Entry<bool>(true, false, false, true));
            dict.Add("sys_dataSpecialFolder", new Entry<Environment.SpecialFolder>(Environment.SpecialFolder.MyDocuments, false, false, true));
            dict.Add("sys_dataFolder", new Entry<string>("Garm",false,false,true));
            dict.Add("sys_userPreferences", new Entry<string>(@"Garm\preferences.xml", false, false, true)); //This file will NOT be loaded through the File-Helper and therefore needs to include the complete path (it still uses the SpecialFolder setting if applicable)
#if DEBUG
            dict.Add("sys_initTimeout", new Entry<int>(10000));
            dict.Add("sys_shutdownTimeout", new Entry<int>(10000));
#else
            dict.Add("sys_initTimeout", new Entry<int>(60000));
            dict.Add("sys_shutdownTimeout", new Entry<int>(10000));
#endif

            dict.Add("srv_port", new Entry<short>(8846));
            
            dict.Add("gui_windowTitle_client", new Entry<string>("Garm workbench"));
            dict.Add("gui_windowTitle_debug", new Entry<string>("Garm debug"));
            dict.Add("gui_windowTitle_edit", new Entry<string>("Garm edit"));
            dict.Add("gui_consoleColor", new Entry<ConsoleColor>(ConsoleColor.Green));

            dict.Add("snd_synth_bpm", new Entry<double>(100));
            dict.Add("terrain_defaultPointsPerMeter", new Entry<float>(4,false,true,true));

            LoadDefaultsRender(ref dict);
            LoadDefaultsAudio(ref dict);
            LoadDefaultsLoc(ref dict);
        }
    }
}
