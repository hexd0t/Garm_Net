// created on 10/12/2002 at 21:00
using System;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace NAudio.Mixer 
{
	/// <summary>
	/// Represents a Mixer source
	/// </summary>
	public class MixerSource 
	{
		private MixerInterop.MIXERLINE mixerLine;
        private IntPtr mixerHandle;
		private int nDestination;
		private int nSource;
		
		/// <summary>
		/// Creates a new Mixer Source
		/// </summary>
		/// <param name="nMixer">Mixer ID</param>
		/// <param name="nDestination">Destination ID</param>
		/// <param name="nSource">Source ID</param>
        public MixerSource(IntPtr mixerHandle, int nDestination, int nSource) 
		{
			mixerLine = new MixerInterop.MIXERLINE();
			mixerLine.cbStruct = Marshal.SizeOf(mixerLine);
			mixerLine.dwDestination = nDestination;
			mixerLine.dwSource = nSource;
            MmException.Try(MixerInterop.mixerGetLineInfo(mixerHandle, ref mixerLine, MixerFlags.GetLineInfoOfSource), "mixerGetLineInfo");
            this.mixerHandle = mixerHandle;
			this.nDestination = nDestination;
			this.nSource = nSource;
		}
		
		/// <summary>
		/// Source Name
		/// </summary>
		public String Name 
		{
			get 
			{
				return mixerLine.szName;
			}
		}
		
		/// <summary>
		/// Source short name
		/// </summary>
		public String ShortName 
		{
			get 
			{
				return mixerLine.szShortName;
			}
		}
		
		/// <summary>
		/// Number of controls
		/// </summary>
		public int ControlsCount 
		{
			get 
			{
				return mixerLine.cControls;
			}
		}
		
		/// <summary>
		/// Retrieves the specified control
		/// </summary>
		/// <param name="nControl"></param>
		/// <returns></returns>
		public MixerControl GetControl(int nControl) 
		{
			if(nControl < 0 || nControl >= ControlsCount) 
			{
				throw new ArgumentOutOfRangeException("nControl");
			}
            return MixerControl.GetMixerControl(mixerHandle, (int)mixerLine.dwLineID, nControl, Channels, MixerFlags.Mixer);
		}
		
		/// <summary>
		/// Number of channels
		/// </summary>
		public int Channels 
		{
			get 
			{
				return mixerLine.cChannels;
			}			
		}

		/// <summary>
		/// Source type description
		/// </summary>
		public String TypeDescription 
		{
			get 
			{
                switch (mixerLine.dwComponentType)
                {
                    case MixerLineComponentType.SourceUndefined:
                        return "Undefined";
                    case MixerLineComponentType.SourceDigital:
                        return "Digital";
                    case MixerLineComponentType.SourceLine:
                        return "Line Level";
                    case MixerLineComponentType.SourceMicrophone:
                        return "Microphone";
                    case MixerLineComponentType.SourceSynthesizer:
                        return "Synthesizer";
                    case MixerLineComponentType.SourceCompactDisc:
                        return "Compact Disk";
                    case MixerLineComponentType.SourceTelephone:
                        return "Telephone";
                    case MixerLineComponentType.SourcePcSpeaker:
                        return "PC Speaker";
                    case MixerLineComponentType.SourceWaveOut:
                        return "Wave Out";
                    case MixerLineComponentType.SourceAuxiliary:
                        return "Auxiliary";
                    case MixerLineComponentType.SourceAnalog:
                        return "Analog";
                    default:
                        return "Invalid";
                }
			}				
		}		
	}
}
