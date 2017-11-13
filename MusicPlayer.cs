using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dmMusic
{
	public class MusicPlayer
	{

		private ISoundOut soundOut;
		private IWaveSource waveSource;

		public event EventHandler<PlaybackStoppedEventArgs> PlaybackStopped;

		int volumn;

		public PlaybackState PlaybackState
		{
			get
			{
				if (soundOut != null)
					return soundOut.PlaybackState;
				return PlaybackState.Stopped;
			}
		}

		public TimeSpan Position
		{
			get
			{
				if (waveSource != null)
					return waveSource.GetPosition();
				return TimeSpan.Zero;
			}
			set
			{
				if (waveSource != null)
					waveSource.SetPosition(value);
			}
		}

		public TimeSpan Length
		{
			get
			{
				if (waveSource != null)
					return waveSource.GetLength();
				return TimeSpan.Zero;
			}
		}

		public int Volume
		{
			get
			{
				return volumn;
			}
			set
			{
				volumn = value;

				if (soundOut != null)
				{
					soundOut.Volume = Math.Min(1.0f, Math.Max(volumn / 100f, 0f));
				}
			}
		}
		
		public void Open(string filename, MMDevice device)
		{
			CleanupPlayback();

			waveSource =
				CodecFactory.Instance.GetCodec(filename)
					.ToSampleSource()
					.ToStereo()
					.ToWaveSource();
			soundOut = new WasapiOut() { Latency = 100 };
			soundOut.Initialize(waveSource);

			if (PlaybackStopped != null)
				soundOut.Stopped += PlaybackStopped;
		}

		public void Play()
		{
			if (soundOut != null)
			{
				soundOut.Volume = Math.Min(1.0f, Math.Max(volumn / 100f, 0f));
				soundOut.Play();
			}
		}

		public void Pause()
		{
			if (soundOut != null)
				soundOut.Pause();
		}

		public void Stop()
		{
			if (soundOut != null)
				soundOut.Stop();
		}

		public void DisablePlaybackStopped()
		{
			if (soundOut != null && PlaybackStopped != null)
				soundOut.Stopped -= PlaybackStopped;
		}

		public void CleanupPlayback()
		{
			if (soundOut != null)
			{
				soundOut.Dispose();
				soundOut = null;
			}
			if (waveSource != null)
			{
				waveSource.Dispose();
				waveSource = null;
			}
		}
	}
}
