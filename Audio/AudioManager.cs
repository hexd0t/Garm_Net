using Garm.Audio.MusicAnalyzation;
using Garm.Audio.Synth;
using Garm.Base.Interfaces;
using NAudio.Wave;

namespace Garm.Audio
{
    /// <summary>
    /// Provides all audio-related outputs to the views
    /// </summary>
    public class AudioManager : Base.Abstract.Base
    {
        /// <summary>
        /// Eventbased WaveOut for SynthMusic output
        /// </summary>
        public WaveOutEvent MusicSynthOut;
        /// <summary>
        /// Provides synthesised music based on ingame situation
        /// </summary>
        public SynthMusicProvider MusicSynthGen;
        /// <summary>
        /// Eventbased WaveOut for AnalyzedMusic output
        /// </summary>
        public WaveOutEvent MusicExternalOut;
        /// <summary>
        /// Provides external music analyzed to fit current ingame circumstances
        /// </summary>
        public CustomMusicProvider MusicExternalProvider;
        /// <summary>
        /// En-/Disables SynthMusic output
        /// </summary>
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
        /// <summary>
        /// En-/Disables AnalyzedMusic output
        /// </summary>
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

        /// <summary>
        /// Creates the default AudioManager and starts the default outputs
        /// </summary>
        /// <param name="manager">The runmanager instance to use for this instance</param>
        public AudioManager(IRunManager manager) : base(manager)
        {
            //Init SynthGen
            MusicSynthOut = new WaveOutEvent();
            MusicSynthGen = new SynthMusicProvider(manager);
            MusicSynthOut.Init(MusicSynthGen);
            //Init ExternalMusic
            MusicExternalOut = new WaveOutEvent();
            MusicExternalProvider = new CustomMusicProvider(manager);
            MusicExternalOut.Init(MusicExternalProvider);

            //Enable default music provider
            switch (manager.Opts.Get<short>("snd_musicProvider"))
            {
                case 1:
                    MusicExternal = true;
                    break;
                case 2:
                    MusicSynth = true;
                    break;
            }
        }

        /// <summary>
        /// Stops all audiooutput and releases all resources allocated for audio playback
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            //Dispose SynthGen
            MusicSynthOut.Stop();
            MusicSynthOut.Dispose();
            MusicSynthGen.Dispose();
            //Dispose ExternalMusic
            MusicExternalOut.Stop();
            MusicExternalOut.Dispose();
            MusicExternalProvider.Dispose();
        }
    }
}
