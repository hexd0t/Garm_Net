using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Garm.Audio.MusicAnalyzation;
using Garm.Audio.Synth;
using Garm.Base.Interfaces;
using NAudio.Wave;

namespace Garm.Audio
{
    public class AudioManager : Base.Abstract.Base
    {
        public WaveOutEvent MusicSynthOut;
        public SynthMusicProvider MusicSynthGen;
        public WaveOutEvent MusicExternalOut;
        public CustomMusicProvider MusicExternalProvider;

        public bool MusicSynth
        {
            get { if (MusicSynthOut == null) return false; return MusicSynthOut.PlaybackState == PlaybackState.Playing; }
            set
            {
                if (value != MusicSynth &&
                    MusicSynthOut != null)
                    if (value) MusicSynthOut.Play();
                    else MusicSynthOut.Pause();
            }
        }

        public bool MusicExternal
        {
            get { if (MusicExternalOut == null) return false; return MusicExternalOut.PlaybackState == PlaybackState.Playing; }
            set
            {
                if (value != MusicExternal &&
                    MusicExternalOut != null)
                    if (value) MusicExternalOut.Play();
                    else MusicExternalOut.Pause();
            }
        }

        public AudioManager(IRunManager manager) : base(manager)
        {
            MusicSynthOut = new WaveOutEvent();
            MusicSynthGen = new SynthMusicProvider(manager);
            MusicSynthOut.Init(MusicSynthGen);
            MusicExternalOut = new WaveOutEvent();
            MusicExternalProvider = new CustomMusicProvider();
            MusicExternalOut.Init(MusicExternalProvider);
        }

        public override void Dispose()
        {
            MusicSynthOut.Stop();
            MusicSynthOut.Dispose();
            MusicSynthGen.Dispose();
            MusicExternalOut.Stop();
            MusicExternalOut.Dispose();
            MusicExternalProvider.Dispose();
        }
    }
}
