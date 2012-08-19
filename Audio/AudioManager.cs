using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Garm.Base.Interfaces;
using NAudio.Wave;

namespace Garm.Audio
{
    public class AudioManager : Base.Abstract.Base
    {
        public WaveOutEvent MusicSynthOut;
        public SynthMusicProvider MusicSynthGen;

        public bool MusicSynth { get { if (MusicSynthOut == null)return false; return MusicSynthOut.PlaybackState == PlaybackState.Playing; }
            set { if (value != MusicSynth && MusicSynthOut != null) {if(value)MusicSynthOut.Play();else MusicSynthOut.Pause();}  }
        }

        public AudioManager(IRunManager manager) : base(manager)
        {
            MusicSynthOut = new WaveOutEvent();
            MusicSynthGen = new SynthMusicProvider(manager);
            MusicSynthOut.Init(MusicSynthGen);
            
        }

        public override void Dispose()
        {
            MusicSynthOut.Stop();
            MusicSynthOut.Dispose();
            MusicSynthGen.Dispose();
        }
    }
}
