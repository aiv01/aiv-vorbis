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

You need sound files in the popular Ogg/Vorbis format. Tiny sounds (like effects, noises, ...) are mapped to AudioClip, while long sounds (like background music) have to be streamed constantly from the disk.

```cs
AudioClip jumpSound = new AudioClip("Assets/jump.ogg");

AudioClip attackSound = new AudioClip("Assets/shot.ogg");
```

To play clips just issue the Play() command:

```cs
heroAudioSource.Play(jumpSound);
```

while if you need to play longer sounds from disk (instead of folly loading them in the memory like with AudioClip):

```cs
backgroundMusic.Stream("Assets/long_music.ogg");
```
