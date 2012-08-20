using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;
using NAudio.Wave;

namespace Garm.Audio.MusicAnalyzation
{
    public class CustomMusicAnalyzer
    {
        protected const int AnalyzeFrameSize = 512;
        public CustomMusicAnalyzer()
        {
        }

        public MusicCharacteristics AnalyzeFileMP3(Stream file)
        {
            var reader = new Wave16ToFloatProvider(new Mp3FileReader(file));
            var bbuffer = new byte[AnalyzeFrameSize];
            
            while(reader.Read(bbuffer, 0, AnalyzeFrameSize)>0)
            {
                for (int i = 0; i < AnalyzeFrameSize; i++ )
                    Console.Write(bbuffer[i].ToString("X"));
            }
            return new MusicCharacteristics();
        }
    }
}
