using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Garm.Base.Interfaces;

namespace Garm.Audio
{
    public class SynthMusicProvider : NAudio.Wave.WaveProvider32, IDisposable
    {
        public Dictionary<int, InstrumentSynth> Instruments;
        public float MusicVolume;
        protected List<InstrumentNoteInstance> CurrentNotes;
        protected double CurrentTime;
        protected readonly double SecondsPerSample;
        protected readonly IRunManager Manager;

        public SynthMusicProvider(IRunManager manager)
        {
            Manager = manager;
            Instruments = (manager.Opts.Get<Dictionary<int, string>>("snd_instruments")??new Dictionary<int, string>()).ToDictionary(val => val.Key, val => InstrumentSynth.Parse(val.Value));
            CurrentNotes = new List<InstrumentNoteInstance>();
            CurrentTime = 0;
            MusicVolume = 1;
            SecondsPerSample = 1 / (double)WaveFormat.SampleRate;
        }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            try
            {
                for (int i = offset; i < offset + sampleCount; i++)
                {
                    CurrentTime += SecondsPerSample;
                    buffer[i] = 0;
                    //Check for Notes to be added/removed
                    lock (CurrentNotes)
                    {
                        foreach (InstrumentNoteInstance note in CurrentNotes)
                        {
                            note.time += SecondsPerSample;
                            note.lastAmp = Instruments[note.Instrument].Read(note.time, note.freq, note.lastAmp);
                            buffer[i] += MusicVolume * note.lastAmp;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while generating Music: "+e.Message);
            }
            return sampleCount;
        }

        public void AddNote(InstrumentNoteInstance note)
        {
            lock (CurrentNotes)
            {
                CurrentNotes.Add(note);
            }
        }

        public void RemoveNote(InstrumentNoteInstance note)
        {
            lock (CurrentNotes)
            {
                CurrentNotes.Remove(note);
            }
        }

        public void Dispose()
        {
            Manager.Opts.Set("snd_instruments", Instruments.ToDictionary(val => val.Key, val => val.Value.ToString()), false);
        }
    }

    public class InstrumentNoteInstance
    {
        public int Instrument;
        public double time;
        public double freq;
        public float lastAmp;
    }
}
