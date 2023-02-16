using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoundClip;

// Holds all the sounds that can be played
public enum SoundClip
{
    //  Music  //
    music = 0,

    //  General sounds  //
    hint_button = 1,
    scene_change_open = 2, scene_change_close = 3,
    tap_something = 4, tap_nothing = 5,
    drag_menu = 6, slide_menu = 7, open_menu = 31,
    confirm = 8, decline = 9, close = 10,

    //  Game sounds  //
    /*select_spot = 11,   deselect_spot = 12,*/
    number_button = 13, edit_button = 14,  erase_button = 15, undo_button = 16, redo_button = 17, game_hint_button = 18,
    reset_button = 19,  pause_button = 20, play_button = 21,  info_button = 22,

    //  Success sounds  //
    success_lowest = 23, success_low = 24, success_med = 25, success_large = 26, success_funky = 27,

    //  Award sounds  //
    award_available = 28, award_collection = 29,
    level_up = 30,

    invalid = 32
}

// Used to play a sound from anywhere
public static class Sound
{
    // ----- VARIABLES -----

    static SoundManager soundManager;

    public static float GlobalVolumeMultiplier;


    // ----- METHODS -----

    /*
    // Play sound on loop while input
    public static AudioSource PlayWhileInput(SoundClip sound) {
        if (!GetSoundManager()) { return null; }
        return soundManager.PlayWhileInput(sound);
    }
    public static AudioSource PlayWhileInput(SoundClip sound, GameObject objectToAddAudioSourceTo) {
        if (!GetSoundManager()) { return null; }
        return soundManager.PlayWhileInput(sound, objectToAddAudioSourceTo);
    }
    */

    // Plays a sound backward
    public static AudioSource PlayBackward(SoundClip sound) {
        AudioSource audio = Play(sound);
        audio.Stop();
        audio.pitch = -1f;
        audio.timeSamples = audio.clip.samples - 1;
        audio.Play();
        return audio;
    }

    // Plays a sound without looping
    public static AudioSource Play(SoundClip sound) {
        return Play(sound, 0);
    }
    // Plays a sound once <delay> is done playing
    public static AudioSource Play(SoundClip sound, AudioSource delay) {
        return Play(sound, 0, delay);
    }

    // Plays a sound if <actuallyPlayIt> is true
    // Used in game actions so that 
    public static AudioSource Play(SoundClip sound, bool actuallyPlayIt) {
        if (!actuallyPlayIt) { return null; }
        return Play(sound);
    }

    // Loops it <loopCount> times
    public static AudioSource Play(SoundClip sound, int loopCount) {
        if (!GetSoundManager()) { return null; }
        SoundManager.SoundAudio soundAudio = GetSoundAudio(sound);
        return soundManager.Play(soundAudio, loopCount);
    }
    // Play sound, looped <loopCount> times, after <delay> has been played
    public static AudioSource Play(SoundClip sound, int loopCount, AudioSource delay) {
        if (!GetSoundManager()) { return null; }
        SoundManager.SoundAudio soundAudio = GetSoundAudio(sound);
        return soundManager.Play(soundAudio, loopCount, delay);
    }

    // Change music volume
    public static void UpdateMusicVolume() {
        soundManager.UpdateMusicVolume();
    }


    // ----- UTILITIES -----

    // Get SoundAudio class from SoundManager given a SoundClip
    static SoundManager.SoundAudio GetSoundAudio(SoundClip sound) {
        if (soundManager == null) { return null; }
        if (!soundManager.sounds.ContainsKey(sound)) {
            Debug.LogWarning("Sounds dictionary does not countain sound for " + sound.ToString());
            return soundManager.sounds[invalid];
        }
        if (soundManager.sounds[sound] == null) {
            Debug.LogWarning($"Sounds dictionary at index {sound.ToString()} is null");
            return soundManager.sounds[invalid];
        }
        return soundManager.sounds[sound];
    }

    // Get AudioClip from inputted Sounds <sound>
    public static AudioClip Clip    (SoundClip sound) { return ClipFromSound(sound); }
    public static AudioClip Audio   (SoundClip sound) { return ClipFromSound(sound); }
    public static AudioClip GetClip (SoundClip sound) { return ClipFromSound(sound); }
    public static AudioClip ClipFromSound(SoundClip sound) {
        if (soundManager == null) { return null; }
        if (!soundManager.sounds.ContainsKey(sound)) {
            Debug.LogWarning("Sounds dictionary does not countain sound for " + sound.ToString());
            return soundManager.defaultAudio;
        }
        if (soundManager.sounds[sound] == null) {
            Debug.LogWarning($"Sounds dictionary at index {sound.ToString()} is null");
            return soundManager.defaultAudio;
        }
        return soundManager.sounds[sound].GetAudio();
    }

    // Returns true if the SoundManager wants this audio to be modulated randomly
    static bool ModulatePitch(SoundClip sound) {
        if (soundManager == null) { return false; }
        if (!soundManager.sounds.ContainsKey(sound) || soundManager.sounds[sound] == null) {
            return false;
        }
        return soundManager.sounds[sound].modulatePitch;
    }

    // Gets volume from each sound type
    public static float Volume(SoundClip sound) {
        switch (sound) {
            case music: return Settings.MusicVolume;
            default:    return Settings.SoundVolume;
        }
    }

    // Get soundManager object
    static bool GetSoundManager() {
        // We already have it
        if (soundManager != null) {return true;}

        // Try to find it
        soundManager = GameObject.FindObjectOfType<SoundManager>();
        if (soundManager == null) {
            Debug.LogError("Could not find sound manager!!");
        }

        // Return whether it was found
        return (soundManager == null);
    }

    // Constructor
    static Sound() {
        GetSoundManager();
    }
}
