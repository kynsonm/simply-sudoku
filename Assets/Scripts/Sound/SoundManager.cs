using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static SoundClip;
using Random = UnityEngine.Random;


public class SoundManager : MonoBehaviour
{
    // Class for sacing sounds
    [Serializable] public class SoundAudio {
        // Basic info
        public string name;
        [TextArea(minLines:2, maxLines:5)] public string info;

        // Properties
        [Space(10f)]
        public float volumeMultiplier = 1f;
        public bool modulatePitch = false;

        // Audio info
        [Space(10f)]
        public SoundClip sound;
        public List<AudioClip> audio;

        // Audio retrieval using a bucket
        private bool useBucket = true;
        private List<int> bucket = null;
        public AudioClip GetAudio() {
            if (bucket == null || bucket.Count == 0) { resetBucket(); }
            if (!useBucket) { return audio[0]; }

            int bucketIndex = Random.Range(0, bucket.Count);
            int audioIndex = bucket[bucketIndex];
            bucket.RemoveAt(bucketIndex);

            string logName;
            if (audio[audioIndex] == null) {
                logName = "null sound";
            } else {
                logName = audio[audioIndex].name;
            }

            Debug.Log($"Playing {sound.ToString()} clip ({audioIndex} of {audio.Count}):  {logName}");
            return audio[audioIndex];
        }
        private void resetBucket() {
            useBucket = audio.Count != 1 && audio.Count != 0;
            bucket = new List<int>();
            for (int i = 0; i < audio.Count; ++i) {
                bucket.Add(i);
            }
        }

        // Constructor
        public SoundAudio(SoundClip sound_in, List<AudioClip> audio_in) {
            name = "SoundAudio.name";
            info = "SoundAUdio.info";
            sound = sound_in;
            audio = audio_in;
        }
    }

    // ----- VARIABLES -----

    // Lists of sounds
    [SerializeField] List<SoundAudio> musicSounds;
    [SerializeField] List<SoundAudio> generalSounds;
    [SerializeField] List<SoundAudio> gameSounds;
    [SerializeField] List<SoundAudio> successSounds;
    [SerializeField] List<SoundAudio> awardSounds;
    [SerializeField] public AudioClip defaultAudio;

    // For music searching
    [HideInInspector] public Dictionary<SoundClip, SoundAudio> sounds;

    AudioSource MusicAudioSource;


    IEnumerator Start() {
        //StartCoroutine(SetGlobalSoundVolume());

        yield return new WaitForSeconds(1f);

        // Add sound component to each scroll rect
        foreach (ScrollRect scrollRect in GameObject.FindObjectsOfType<ScrollRect>(true)) {
            string name = $"({scrollRect.transform.parent.parent.name} --> {scrollRect.transform.parent.name} --> {scrollRect.gameObject.name})";

            ScrollRectSound sound = scrollRect.gameObject.GetComponent<ScrollRectSound>();
            if (sound == null) {
                sound = scrollRect.gameObject.AddComponent<ScrollRectSound>();
                Debug.Log($"Adding ScrollRectSound to ({name})");
            } else {
                int length = scrollRect.gameObject.GetComponents<ScrollRectSound>().Length;
                Debug.LogWarning($"Object has {length} ScrolLRectSound components: {name}");
            }

            // Make their decelerations the same
            scrollRect.decelerationRate = 0.02f;
        }

        // Add sound to each Slider
        foreach (Slider slider in GameObject.FindObjectsOfType<Slider>(true)) {
            if (!slider.interactable) { continue; }

            string name = $"({slider.transform.parent.parent.name} --> {slider.transform.parent.name} --> {slider.gameObject.name})";

            SliderSound sound = slider.gameObject.GetComponent<SliderSound>();
            if (sound == null) {
                sound = slider.gameObject.AddComponent<SliderSound>();
                Debug.Log($"Adding SliderSound to {name}");
            }
        }
    }

    IEnumerator SetGlobalSoundVolume() {
        Sound.GlobalVolumeMultiplier = 0f;
        yield return new WaitForSeconds(0.75f);
        LeanTween.value(0f, 1f, 0.25f)
        .setOnUpdate((float value) => {
            Sound.GlobalVolumeMultiplier = value;
        })
        .setOnComplete(() => {
            Sound.GlobalVolumeMultiplier = 1f;
        });
    }

    void Awake() {
        // Create the dictionary before anybody can access it >:)
        transferFromAllSounds();

        // Find current music player if one exists
        if (MusicPlayerExists()) { return; }

        // Play music forever
        CreateMusicObject();
    }


    // ----- METHODS -----

    // Creates an object that plays music forever, between scenes
    public void CreateMusicObject() {
        if (MusicPlayerExists()) { return; }
        GameObject musicObj = Instantiate(new GameObject("Music Player"));
        musicObj.name = "Music Player";
        MusicAudioSource = musicObj.AddComponent<AudioSource>();
        MusicAudioSource.clip = Sound.Audio(music);
        MusicAudioSource.loop = true;
        MusicAudioSource.volume = Settings.MasterVolume * Settings.MusicVolume;
        MusicAudioSource.Play();
        DontDestroyOnLoad(musicObj);
    }

    // Returns music player if it exists
    bool MusicPlayerExists() {
        GameObject musicObj = FindMusicPlayer();
        if (musicObj != null) { return true; }
        return false;
    }
    GameObject FindMusicPlayer() {
        GameObject musicObj = GameObject.Find("Music Player");
        return musicObj;
    }

    // Update music volume
    public void UpdateMusicVolume() {
        if (MusicAudioSource != null) {
            MusicAudioSource.volume = Settings.MasterVolume * Settings.MusicVolume;
            return;
        }
        if (MusicPlayerExists()) {
            AudioSource audio = FindMusicPlayer().GetComponent<AudioSource>();
            if (audio != null) {
                MusicAudioSource = audio;
            } else {
                return;
            }
        } else {
            return;
        }
        MusicAudioSource.volume = Settings.MasterVolume * Settings.MusicVolume;
    }

    /*
    // Play a sound for a given input
    public AudioSource PlayWhileInput(SoundClip sound) {
        // Create audio source for the clip if necessary
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        audio.clip = Sound.Clip(sound);
        StartCoroutine(PlayWhileInputEnum(audio));
        return audio;
    }
    public AudioSource PlayWhileInput(SoundClip sound, GameObject objectToAddAudioSourceTo) {
        // Create audio source for the clip if necessary
        AudioSource audio = objectToAddAudioSourceTo.GetComponent<AudioSource>();
        if (audio != null) {
            StopCoroutine(PlayWhileInputEnum(audio));
            StartCoroutine(PlayWhileInputEnum(audio));
            return audio;
        }
        audio = objectToAddAudioSourceTo.AddComponent<AudioSource>();
        audio.clip = Sound.Clip(sound);
        StartCoroutine(PlayWhileInputEnum(audio));
        return audio;
    }
    IEnumerator PlayWhileInputEnum(AudioSource audio) {
        if (audio == null) { yield break; }

        audio.loop = true;
        audio.volume = Settings.SoundVolume * Settings.MasterVolume;

        if (!audio.isPlaying) {
            audio.Play();
        }

        // Loop the audio while input is detected
        bool repeat = Input.touchCount > 0 || Input.GetMouseButton(0);
        while (repeat) {
            yield return new WaitForSeconds(0.5f);
            repeat = Input.touchCount > 0 || Input.GetMouseButton(0);

            Debug.Log("Input detected: Continuing to play scroll sound");
        }

        // Stop and destroy it when done
        if (audio != null) {
            audio.Stop();
            UnityEngine.Object.Destroy(audio);
        }
    }
    */

    // Play a sound
    public AudioSource Play(SoundAudio soundAudio, int loopCount) {
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        audio.clip = soundAudio.GetAudio();
        audio.loop = false;
        audio.volume = findVolume(soundAudio);
        audio.pitch = findPitch(soundAudio);
        
        // Play audio if not looped
        if (loopCount == 0) {
            audio.Play();
            StartCoroutine(RemoveAudioSource(audio));
        }
        // Loop audio forever if loopCount is negative
        else if (loopCount < 0) {
            audio.loop = true;
            audio.Play();
        }
        // Loop a certain amount of times
        else {
            StartCoroutine(Loop(audio, loopCount));
        }

        return audio;
    }

    // Play a sound once <delay> has been played
    public AudioSource Play(SoundAudio soundAudio, int loopCount, AudioSource delay) {
        float startTime = delay.clip.length - delay.time;
        AudioSource audioSource = Play(soundAudio, loopCount);
        audioSource.Pause();
        audioSource.PlayScheduled(startTime);
        return audioSource;
    }

    // Loop an audio a given amount of times
    IEnumerator Loop(AudioSource audio, int loopCount) {
        for (int i = 0; i < loopCount; ++i) {
            audio.Play();
            yield return new WaitForSeconds(audio.clip.length);
        }
        if (audio == null) { yield break; }
        UnityEngine.Object.Destroy(audio);
    }

    // Remove component once it has played
    IEnumerator RemoveAudioSource(AudioSource audio) {
        yield return new WaitForSeconds(audio.clip.length);
        if (audio == null) { yield break; }
        UnityEngine.Object.Destroy(audio);
    }


    // ----- UTILITIES -----

    // Gets volume and pitch of the audio based on the SoundAudio class
    float findVolume(SoundAudio soundAudio) {
        float volume = (soundAudio.sound == music) ? Settings.MusicVolume : Settings.SoundVolume;
        volume *= soundAudio.volumeMultiplier;
        volume *= Settings.MasterVolume;
        volume *= Sound.GlobalVolumeMultiplier;

        volume = (volume >= 1f) ? 1f : volume;
        volume = (volume <= 0f) ? 0f : volume;
        return volume;
    }
    float findPitch(SoundAudio soundAudio) {
        if (!soundAudio.modulatePitch) { return 1f; }
        
        float pitch = Random.Range(0.85f, 1.15f);

        pitch = cubedVolumeRange(pitch);
        //pitch = quarticVolumeRange(pitch);

        return pitch;
    }

    // Changes pitch in various ways (cubed, quartic, etc)
    float cubedVolumeRange(float randomValue) {
        return 1f + (45f * Mathf.Pow(randomValue - 1f, 3));
    }
    float quarticVolumeRange(float randomValue) {
        float value = 1f + (300f * Mathf.Pow(randomValue - 1f, 3));
        if (randomValue <= 1f) {
            value *= -1f;
        }
        return value;
    }

    // Transfers all SoundAudio's from <allAudio> into the dictonary <sounds>
    void transferFromAllSounds() {
        sounds = new Dictionary<SoundClip, SoundAudio>();
        addSoundsToDictionary(musicSounds);
        addSoundsToDictionary(generalSounds);
        addSoundsToDictionary(gameSounds);
        addSoundsToDictionary(successSounds);
        addSoundsToDictionary(awardSounds);

        sounds.Add( invalid, new SoundAudio( invalid, new List<AudioClip>{defaultAudio} ) );
    }
    void addSoundsToDictionary(List<SoundAudio> soundAudioList) {
        foreach (SoundAudio soundAudio in soundAudioList) {
            // Check for conflicts
            if (sounds.ContainsKey(soundAudio.sound)) {
                string log = "Dictionary already contains key: " + soundAudio.sound.ToString() + "\n";

                // Add names of the clips aready in this SoundAudio
                string dictClips = " - Current clips in dict:\n --- {  ";
                foreach (var audio in sounds[soundAudio.sound].audio) {
                    dictClips += audio.name + ", ";
                }
                if (sounds[soundAudio.sound].audio == null || sounds[soundAudio.sound].audio.Count == 0) {
                    dictClips += "no audios!  }\n";
                } else {
                    dictClips += " }\n";
                }

                // Add names of the clips of SoundAudio we're trying to add
                string addClips = " - Trying to add clips to dict:\n --- {  ";
                foreach (var audio in soundAudio.audio) {
                    addClips += audio.name + ", ";
                }
                if (soundAudio.audio == null || soundAudio.audio.Count == 0) {
                    addClips += "no audios!  }\n";
                } else {
                    addClips += " }\n";
                }

                log += dictClips + addClips;
                Debug.LogWarning(log);
                continue;
            }

            // Check if SoundAudio.audio is fine
            if (soundAudio.audio == null || soundAudio.audio.Count == 0) {
                soundAudio.audio = new List<AudioClip>{defaultAudio};
            }
            // Check if volume is find
            soundAudio.volumeMultiplier = (soundAudio.volumeMultiplier <= 0.1f) ? 1f : soundAudio.volumeMultiplier;
            soundAudio.volumeMultiplier = (soundAudio.volumeMultiplier >= 4f) ? 4f : soundAudio.volumeMultiplier;
            sounds.Add(soundAudio.sound, soundAudio);
        }
    }
}