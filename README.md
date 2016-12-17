# aiv-vorbis
OpenAL + Ogg/Vorbis abstraction

Usage
-----

To play a sound you need an AudioSource, every object in your game making noise should be mapped to a different AudioSource to have correct mixing.

```cs
AudioSource heroAudioSource = new AudioSource();

AudioSource zombieAudioSource = new AudioSource();

AudioSource backgroundMusic = new AudioSource();

```
