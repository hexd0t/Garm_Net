using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace Garm.Audio
{
    public class SinWaveProvider : WaveProvider32
    {
        int _sample;

        public float Frequency { get; set; }
        public float WobbleStrength { get; set; }
        public float WobbleFrequency { get; set; }
        public float Amplitude { get; set; }
        public float CutOff { get; set; }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int sampleRate = WaveFormat.SampleRate;
            for (int n = 0; n < sampleCount; n++)
            {
                float t = (float)_sample/(float)sampleRate;
                buffer[n + offset] = (float)(Amplitude * Math.Sin(2 * Math.PI * (Frequency * t + WobbleStrength * Math.Sin(WobbleFrequency*t))));
                buffer[n + offset] = Math.Min(buffer[n + offset], Amplitude - (Amplitude*CutOff));
                buffer[n + offset] = Math.Max(buffer[n + offset], (Amplitude * CutOff) - Amplitude);
                _sample++;
                if (_sample >= int.MaxValue/2) _sample = 0;
            }
            return sampleCount;
        }
    }
}
