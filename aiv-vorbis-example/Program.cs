using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Vorbis;

namespace aiv_vorbis_example
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine(AudioDevice.Name);

            AudioSource source = new AudioSource();

            AudioSource secondSource = new AudioSource();

            AudioClip clip = new AudioClip("aiv_vorbis_example.Assets.jump.ogg");

            secondSource.Volume = 0.1f;
            secondSource.Stream("aiv_vorbis_example.Assets.jump.ogg", true);

            while (true)
            {
                source.Play(clip);

                Console.WriteLine("Play()");

                Console.ReadLine();
            }
        }
    }
}
