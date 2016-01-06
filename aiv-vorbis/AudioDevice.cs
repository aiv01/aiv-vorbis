using System;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Aiv.Vorbis
{
	
	public static class AudioDevice
	{
		private static AudioContext context;

		public static void Init ()
		{
			if (context != null)
				return;
			
			context = new AudioContext ();
		}

		public static string Name {
			get {
				Init ();
				return context.CurrentDevice;
			}
		}

		public static void CheckError (string msg)
		{
			Init ();
			ALError err = AL.GetError ();
			if (err != ALError.NoError)
				throw new Exception (msg + ": " + err.ToString ());
		}

		public static float Volume {
			get {
				Init ();
				float volume = 0;
				AL.GetListener (ALListenerf.Gain, out volume);
				return volume;
			}
			set {
				Init ();
				AL.Listener (ALListenerf.Gain, value);
			}
		}

		// Extensions methods
		public static float Clamp (this float n, float min, float max)
		{
			if (n < min)
				return min;
			if (n > max)
				return max;
			return n;
		}
	}

}

