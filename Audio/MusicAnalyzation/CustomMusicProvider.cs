using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Garm.Base.Interfaces;

namespace Garm.Audio.MusicAnalyzation
{
    public class CustomMusicProvider : NAudio.Wave.WaveProvider32, IDisposable
    {
        protected readonly IRunManager Manager;

        public CustomMusicProvider(IRunManager manager)
        {
            Manager = manager;
            //ToDo: Save and recall analyzation results

            var musicPath = Manager.Opts.Get<string>("snd_ext_musicFolder");
            if (Manager.Opts.Get<bool>("snd_ext_useSpecialFolder"))
                musicPath = Path.Combine(Environment.GetFolderPath(Manager.Opts.Get<Environment.SpecialFolder>("snd_ext_specialFolder"))??"",musicPath);

            
        }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {

            return 0;
        }

        public void Dispose()
        {
            
        }
    }
}
