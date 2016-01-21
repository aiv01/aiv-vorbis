using System;
using OpenTK.Audio.OpenAL;
using System.Threading;
using NVorbis;

namespace Aiv.Vorbis
{
	public class AudioSource : IDisposable
	{
		
		private int audioSourceId = -1;

		private Thread streamWorker;
		// required for streaming
		private int[] bufferIds;
		private bool streamLooping;

		private bool disposed;

		public bool IsPlaying {
			get {
				return AL.GetSourceState (this.audioSourceId) == ALSourceState.Playing;
			}
		}

		public bool IsLooping {
			get {
				if (this.streamWorker != null && this.streamWorker.IsAlive)
					return this.streamLooping;
				bool looping = false;
				AL.GetSource (this.audioSourceId, ALSourceb.Looping, out looping);
				return looping;
			}
		}

		public float Volume {
			get {
				float volume = 0;
				AL.GetSource (this.audioSourceId, ALSourcef.Gain, out volume);
				return volume.Clamp (this.minVolume, this.maxVolume);
			}
			set {
				AL.Source (this.audioSourceId, ALSourcef.Gain, value.Clamp (this.minVolume, this.maxVolume));
			}
		}

		public float Speed {
			get {
				float speed = 0;
				AL.GetSource (this.audioSourceId, ALSourcef.Pitch, out speed);
				return speed;
			}
			set {
				// avoid illegal values
				if (value < 0)
					value = 0;
				AL.Source (this.audioSourceId, ALSourcef.Pitch, value);
			}
		}

		private float minVolume;
		private float maxVolume;

		public AudioSource ()
		{
			AudioDevice.Init ();
			this.audioSourceId = AL.GenSource ();
			AudioDevice.CheckError ("allocating OpenAL source");
			AL.GetSource (this.audioSourceId, ALSourcef.MinGain, out this.minVolume);
			AL.GetSource (this.audioSourceId, ALSourcef.MaxGain, out this.maxVolume);
		}

		public void Stream (string fileName, bool loop = false)
		{
			this.Stop ();
			// kill the old thread if required
			if (this.streamWorker != null && this.streamWorker.IsAlive)
				this.streamWorker.Abort ();

			// set internal looping to false, otherwise streaming will not work !
			AL.Source (this.audioSourceId, ALSourceb.Looping, false);
			// cleanup buffer pointer, just for safety or broken implementations
			AL.Source (this.audioSourceId, ALSourcei.Buffer, 0);

			this.streamLooping = loop;
			
			this.SpawnThread (fileName);
		}


		public void Play (AudioClip clip, bool loop = false)
		{
			this.Stop ();
			// kill the streaming thread if required
			if (this.streamWorker != null && this.streamWorker.IsAlive)
				this.streamWorker.Abort ();
			AL.Source (this.audioSourceId, ALSourcei.Buffer, clip.BufferId);
			AL.Source (this.audioSourceId, ALSourceb.Looping, loop);
			AL.SourcePlay (this.audioSourceId);
		}

		public void Resume ()
		{
			AL.SourcePlay (this.audioSourceId);
		}

		public void Stop ()
		{
			AL.SourceStop (this.audioSourceId);
		}

		public void Pause ()
		{
			AL.SourcePause (this.audioSourceId);
		}

		private void SpawnThread (string fileName)
		{
			this.streamWorker = new Thread (new ParameterizedThreadStart (Streamer));
			this.streamWorker.Start (fileName);
		}

		private void Streamer (object arg)
		{
			string fileName = (string)arg;

			VorbisReader vreader = new VorbisReader (fileName);

			// 3 seconds buffering
			int bufLen = vreader.Channels * vreader.SampleRate * 3;
			float[] buffer = new float[bufLen];

			if (this.bufferIds == null) {
				this.bufferIds = AL.GenBuffers (3);
				AudioDevice.CheckError ("allocating OpenAL buffers for streaming");
			}

			ALFormat format = vreader.Channels == 2 ? ALFormat.StereoFloat32Ext : ALFormat.MonoFloat32Ext;

			// buffer data
			int i;
			for (i = 0; i < bufferIds.Length; i++) {
				int count = vreader.ReadSamples (buffer, 0, bufLen);
				// end of the stream ?
				if (count == 0) {
					if (!this.streamLooping)
						break;
					vreader.DecodedPosition = 0;
					count = vreader.ReadSamples (buffer, 0, bufLen);
				}
				AL.BufferData<float> (this.bufferIds [i], format, buffer, count * sizeof(float), vreader.SampleRate);
				AudioDevice.CheckError ("loading data in OpenAL buffer for streaming");
			}
			AL.SourceQueueBuffers (this.audioSourceId, i, bufferIds);

			AL.SourcePlay (this.audioSourceId);

			while (true) {
				Thread.Sleep (30);
				int processed = 0;
				AL.GetSource (this.audioSourceId, ALGetSourcei.BuffersProcessed, out processed);
				while (processed > 0) {
					int bufferId = AL.SourceUnqueueBuffer (this.audioSourceId);
					int count = vreader.ReadSamples (buffer, 0, bufLen);
					// end of the straem ?
					if (count == 0) {
						if (!this.streamLooping) {
							this.Stop ();
							// to reset status
							AL.Source (this.audioSourceId, ALSourcei.Buffer, 0);
							return;
						}
						vreader.DecodedPosition = 0;
						count = vreader.ReadSamples (buffer, 0, bufLen);
					}
					AL.BufferData<float> (bufferId, format, buffer, count * sizeof(float), vreader.SampleRate);
					AudioDevice.CheckError ("loading data in OpenAL buffer for streaming");
					AL.SourceQueueBuffer (this.audioSourceId, bufferId);
					processed--;
				}
			}
		}

		~AudioSource ()
		{
			if (!this.disposed)
				this.Dispose ();
			
		}

		public void Dispose ()
		{
			if (disposed)
				return;
			this.Stop ();
			// when an audio source dies, the streaming worker should be destroyed too
			if (this.streamWorker != null && this.streamWorker.IsAlive)
				this.streamWorker.Abort ();
			if (this.bufferIds != null)
				AL.DeleteBuffers (this.bufferIds);
			if (this.audioSourceId > -1)
				AL.DeleteSource (this.audioSourceId);
			this.disposed = true;
		}
	}
}

