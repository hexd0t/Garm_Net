using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Garm.Base.Interfaces;
using NAudio.Dsp;
using NAudio.Wave;

namespace Garm.Audio.MusicAnalyzation
{
    public class CustomMusicAnalyzer : Base.Abstract.Base
    {
        protected const int AnalyzeFrameSize = 512;
        public CustomMusicAnalyzer(IRunManager manager) : base(manager)
        {
            var musicDirPath = Manager.Opts.Get<bool>("snd_ext_useSpecialFolder")
                ? Environment.GetFolderPath(Manager.Opts.Get<Environment.SpecialFolder>("snd_ext_specialFolder"))
                : "";
            musicDirPath = Path.Combine(musicDirPath, Manager.Opts.Get<string>("snd_ext_musicFolder"));
            var musicDir = Manager.Files.GetDir(musicDirPath);
            foreach (var filename in musicDir.Files.Where(filename => filename.ToLowerInvariant().Substring(filename.Length - 4).Equals(".mp3")))
            {
                AnalyzeFileMP3(Manager.Files.Get(filename));
            }
        }

        public MusicCharacteristics AnalyzeFileMP3(Stream file)
        {
            var bbuffer = new byte[AnalyzeFrameSize];
            
            using (var reader = new Mp3FileReader(file))
            {
                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    using (WaveStream aligned = new BlockAlignReductionStream(pcm))
                    {
                        Console.WriteLine(aligned.WaveFormat);
                        Console.WriteLine(aligned.Read(bbuffer, 0, AnalyzeFrameSize));
                    }
                }
            }
            return new MusicCharacteristics();
        }
    }
}
