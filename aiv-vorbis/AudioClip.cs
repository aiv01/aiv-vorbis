using System;
using NVorbis;
using OpenTK.Audio.OpenAL;

namespace Aiv.Vorbis
{
	public class AudioClip
	{
		private VorbisReader reader;

		private int bufferId;

		public int BufferId {
			get {
				return this.bufferId;
			}
		}

		public int Channels {
			get {
				return reader.Channels;
			}
		}

		public int SampleRate {
			get {
				return reader.SampleRate;
			}
		}

		public ALFormat format {
			get {
				return this.Channels == 2 ? ALFormat.StereoFloat32Ext : ALFormat.MonoFloat32Ext;
			}
		}

		public string[] Comments {
			get {
				return reader.Comments;
			}
		}

		public long Samples {
			get {
				return reader.TotalSamples;
			}
		}

		public AudioClip (string fileName)
		{
			this.reader = new VorbisReader (fileName);
			this.bufferId = AL.GenBuffer ();
			float[] buffer = new float[this.Samples * this.Channels];
			// ReadSamples could return less data than required for various reasons
			int count = this.reader.ReadSamples (buffer, 0, buffer.Length);
			AL.BufferData (this.bufferId, this.format, buffer, count * sizeof(float), this.SampleRate);
			AudioDevice.CheckError("loading data in OpenAL buffer");
		}

		~AudioClip ()
		{
			AL.DeleteBuffer (this.bufferId);
		}
	}
}

