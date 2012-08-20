using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace Garm.Audio.Synth
{
    public class InstrumentSynth
    {
        protected double amplitude;
        public double Amplitude { get { return amplitude; } set { amplitude = value; } }

        protected bool loop;
        public bool Loop { get { return loop; } set { loop = value; if (value) { length = fadeAt = -1; } } }

        protected double length;
        public double Length { get { return length; } set { length = value; if (value == -1) { fadeAt = -1; loop = true; } else { loop = false; if (fadeAt < 0) { fadeAt = 0; } if (fadeAt > length) { fadeAt = value; } } } }

        protected double fadeAt;
        public double FadeAt { get { return fadeAt; } set { fadeAt = value; if (value == -1) { length = -1; loop = true; } else { loop = false; if (length < value) { length = value; } } } }

        protected FadeMode fade;
        public FadeMode Fade { get { return fade; } set { fade = value; if (value == FadeMode.None) { loop = true; length = -1; fadeAt = -1; } else { loop = false; if (fadeAt < 0) { fadeAt = 0; } if (length < 0) { length = fadeAt; } } } }

        protected double wobbleStrength;
        public double WobbleStrength { get { return wobbleStrength; } set { wobbleStrength = value; } }

        protected double wobbleFrequency;
        public double WobbleFrequency { get { return wobbleFrequency; } set { wobbleFrequency = value; } }

        protected double amplitudeBeatStrength;//Beat = Schwebung
        public double AmplitudeBeatStrength { get { return amplitudeBeatStrength; } set { amplitudeBeatStrength = value; } }

        protected double amplitudeBeatFrequency;
        public double AmplitudeBeatFrequency { get { return amplitudeBeatFrequency; } set { amplitudeBeatFrequency = value; } }

        protected FreqgenMode baseFreqGen;
        public FreqgenMode BaseFreqGen { get { return baseFreqGen; } set { baseFreqGen = value; } }

        protected FreqgenMode amplitudeBeatFreqGen;
        public FreqgenMode AmplitudeBeatFreqGen { get { return amplitudeBeatFreqGen; } set { amplitudeBeatFreqGen = value; } }

        protected FreqgenMode wobbleFreqGen;
        public FreqgenMode WobbleFreqGen { get { return wobbleFreqGen; } set { wobbleFreqGen = value; } }

        protected double passFactor;
        public double PassFactor { get { return passFactor; } set { passFactor = value; } }

        public double FundamentalStrength { get; set; }
        public double Overtone1Strength { get; set; }
        public double Overtone2Strength { get; set; }
        public double Overtone3Strength { get; set; }
        public double Overtone4Strength { get; set; }
        public String Name = "";

        public float Read(double time, double frequency, float prev = 0)
        {
            if(!loop && time > length)
                return 0f;
            var amp = amplitude + amplitudeBeatStrength * FreqGen(time * amplitudeBeatFrequency, amplitudeBeatFreqGen);
            if (time >= fadeAt)
                amp *= FadeGen((time - fadeAt) / (length - fadeAt), Fade);
            var wobble = wobbleStrength * FreqGen(time * wobbleFrequency, wobbleFreqGen);
            var fundamental = FreqGen(time * frequency + wobble, baseFreqGen);
            var ov1 = FreqGen((time * frequency + wobble) * 2, baseFreqGen);
            var ov2 = FreqGen((time * frequency + wobble) * 3, baseFreqGen);
            var ov3 = FreqGen((time * frequency + wobble) * 4, baseFreqGen);
            var ov4 = FreqGen((time * frequency + wobble) * 5, baseFreqGen);
            var freqgen_outp = amp * (FundamentalStrength * fundamental
                                    + ov1 * Overtone1Strength
                                    + ov2 * Overtone2Strength
                                    + ov3 * Overtone3Strength
                                    + ov4 * Overtone4Strength);
            return (float)(prev + passFactor*(freqgen_outp-prev));
        }

        /// <summary>
        /// Provides the value of a given frequencygenerator at x, scaled to a periodic length of 1
        /// </summary>
        /// <param name="x">Time-Input</param>
        /// <param name="mode">The frequencygenerator to use</param>
        /// <returns>The current value in -1...1 range</returns>
        protected double FreqGen(double x, FreqgenMode mode)
        {
            switch (mode)
            {
                default:
                case FreqgenMode.Sin:
                    return Math.Sin(2 * Math.PI * x);
                case FreqgenMode.SinQubic:
                    var u = Math.Sin(2 * Math.PI * x);
                    return u * u * u;
                case FreqgenMode.Triangular:
                    return (x%1<0.5)?(x % 0.5):(1-(x%5));
                case FreqgenMode.Sawtooth:
                    return (x % 1) * 2 - 1;
                case FreqgenMode.ReverseSawtooth:
                    return ((-x) % 1) * 2 - 1;
            }
        }

        protected const double Sqrt3 = 1.73205080756887729352;
        /// <summary>
        /// Provides an amplitudemultiplyer for fading
        /// </summary>
        /// <param name="x">Time-Input, scaled from 0...1 (start fade...finish)</param>
        /// <param name="mode">FadeMode to use</param>
        /// <returns>Multiplier ranging from 1...0</returns>
        protected double FadeGen(double x, FadeMode mode)
        {
            x = Math.Min(1, Math.Max(0, x));//Crop to 0...1 range
            switch (mode)
            {
                default:
                case FadeMode.None:
                    return 1;
                case FadeMode.Quadratic:
                    return -1 * x * x + 1;
                case FadeMode.Cubical:
                    return -1 * x * x * x + 1;
                case FadeMode.Smooth:
                    var u = (2 * x - 1)/Sqrt3;
                    var v = u*u*u;
                    return 0.75 * Sqrt3 * (v - u) + 0.5;
                case FadeMode.Linear:
                    return 1 - x;
                case FadeMode.Circle:
                    return Math.Sqrt(1 - (x * x));
            }
        }

        public override string ToString()
        {
            try
            {
                var values = new List<byte[]>(18);
                values.Add(BitConverter.GetBytes(amplitude));
                values.Add(BitConverter.GetBytes((int)amplitudeBeatFreqGen));
                values.Add(BitConverter.GetBytes(amplitudeBeatStrength));
                values.Add(BitConverter.GetBytes(amplitudeBeatFrequency));
                values.Add(BitConverter.GetBytes((int)wobbleFreqGen));
                values.Add(BitConverter.GetBytes(wobbleStrength));
                values.Add(BitConverter.GetBytes(wobbleFrequency));
                values.Add(BitConverter.GetBytes(loop));
                values.Add(BitConverter.GetBytes(fadeAt));
                values.Add(BitConverter.GetBytes((int)fade));
                values.Add(BitConverter.GetBytes(length));
                values.Add(Encoding.Unicode.GetBytes(Name));
                values.Add(BitConverter.GetBytes(FundamentalStrength));
                values.Add(BitConverter.GetBytes(Overtone1Strength));
                values.Add(BitConverter.GetBytes(Overtone2Strength));
                values.Add(BitConverter.GetBytes(Overtone3Strength));
                values.Add(BitConverter.GetBytes(Overtone4Strength));
                values.Add(BitConverter.GetBytes((int)baseFreqGen));
                return String.Join(",", values.Select(Convert.ToBase64String));
            }
            catch (Exception e)
            {
                Console.WriteLine("[Error] Cannot serialize instrument!");
            }
            return "err";
        }

        public static InstrumentSynth Parse(string inp)
        {
            var values = inp.Split(new[] { "," }, StringSplitOptions.None).Select(Convert.FromBase64String).ToList();
            var result = new InstrumentSynth();
            result.amplitude = BitConverter.ToDouble(values[0], 0);
            result.amplitudeBeatFreqGen = (FreqgenMode)BitConverter.ToInt32(values[1], 0);
            result.amplitudeBeatStrength = BitConverter.ToDouble(values[2], 0);
            result.amplitudeBeatFrequency = BitConverter.ToDouble(values[3], 0);
            result.wobbleFreqGen = (FreqgenMode)BitConverter.ToInt32(values[4], 0);
            result.wobbleStrength = BitConverter.ToDouble(values[5], 0);
            result.wobbleFrequency = BitConverter.ToDouble(values[6], 0);
            result.loop = BitConverter.ToBoolean(values[7], 0);
            result.fadeAt = BitConverter.ToDouble(values[8], 0);
            result.fade = (FadeMode)BitConverter.ToInt32(values[9], 0);
            result.length = BitConverter.ToDouble(values[10], 0);
            result.Name = Encoding.Unicode.GetString(values[11]);
            result.FundamentalStrength = BitConverter.ToDouble(values[12], 0);
            result.Overtone1Strength = BitConverter.ToDouble(values[13], 0);
            result.Overtone2Strength = BitConverter.ToDouble(values[14], 0);
            result.Overtone3Strength = BitConverter.ToDouble(values[15], 0);
            result.Overtone4Strength = BitConverter.ToDouble(values[16], 0);
            result.baseFreqGen = (FreqgenMode)BitConverter.ToInt32(values[17], 0);
            return result;
        }

        public static int FrequencyToPitch(double frequency)
        { return (int)Math.Round(Math.Log(frequency / 440, 2) * 12); }
        public static int FrequencyToPitch(int frequency)
        { return FrequencyToPitch((double)frequency); }
        public static double PitchToFrequency(int pitch)
        { return 440 * Math.Pow(2, ((double)pitch) / 12); }
        public static double PitchToFrequency(Pitch pitch)
        { return PitchToFrequency((int)pitch); }
        public enum FadeMode { None, Linear, Smooth, Quadratic, Cubical, Circle }
        public enum FreqgenMode { Sin, SinQubic, Triangular, Sawtooth, ReverseSawtooth }
    }
}
