using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    // ----- Settings -----

    // Gameplay
    public static bool HighlightTiles;
    public static bool ShowTimer;
    public static bool ResizableGameBoard;
    public static bool ShowEditNumbers;
    public static bool AlternateGameBoardColors;

    // Themes
    public static int ThemeIndex;
    public static int FontIndex;
    public static int BackgroundIndex;
    public static bool BackgroundActive;
    public static float BackgroundBlur;

    // Looks
    public static float PostProcessingStrength;
    public static float RenderScale;

    // Animations
    public static float AnimSpeedMultiplier;

    // Other
    public static int TargetFrameRate;
    public static bool LeftHanded;
    public static bool ResizableMenus;

    // Volume
    public static float MasterVolume;
    public static float MusicVolume;
    public static float SoundVolume;


    // ----- Functions -----

    public static void LoadSettings() {
        // Check if first time has has run
        if (!PlayerPrefs.HasKey(nameof(BackgroundActive))) {
            FirstRun();
        }

        // Gameplay
        HighlightTiles = PlayerPrefs.GetInt(nameof(HighlightTiles)) != 0;
        ShowTimer = PlayerPrefs.GetInt(nameof(ShowTimer)) != 0;
        ResizableGameBoard = PlayerPrefs.GetInt(nameof(ResizableGameBoard)) != 0;
        ShowEditNumbers = PlayerPrefs.GetInt(nameof(ShowEditNumbers)) != 0;
        AlternateGameBoardColors = PlayerPrefs.GetInt(nameof(AlternateGameBoardColors)) != 0;

        // Themes
        ThemeIndex = PlayerPrefs.GetInt(nameof(ThemeIndex));
        FontIndex = PlayerPrefs.GetInt(nameof(FontIndex));
        BackgroundIndex = PlayerPrefs.GetInt(nameof(BackgroundIndex));
        BackgroundActive = PlayerPrefs.GetInt(nameof(BackgroundActive)) != 0;
        BackgroundBlur = PlayerPrefs.GetFloat(nameof(BackgroundBlur));

        // Looks
        PostProcessingStrength = PlayerPrefs.GetFloat(nameof(PostProcessingStrength));
        RenderScale = PlayerPrefs.GetFloat(nameof(RenderScale));

        Themes themes = Object.FindObjectOfType<Themes>();
        if (themes != null) {
            themes.UpdateTheme(ThemeIndex);
            themes.UpdateFont(FontIndex);
            themes.UpdateBackground(BackgroundIndex);
        } else {
            Debug.Log("No themes object!");
        }

        // Animations
        AnimSpeedMultiplier = PlayerPrefs.GetFloat(nameof(AnimSpeedMultiplier));

        // Sound
        MasterVolume = PlayerPrefs.GetFloat(nameof(MasterVolume));
        MusicVolume = PlayerPrefs.GetFloat(nameof(MusicVolume));
        SoundVolume = PlayerPrefs.GetFloat(nameof(SoundVolume));

        // Other
        TargetFrameRate = PlayerPrefs.GetInt(nameof(TargetFrameRate));
        LeftHanded = PlayerPrefs.GetInt(nameof(LeftHanded)) != 0;
        ResizableMenus = PlayerPrefs.GetInt(nameof(ResizableMenus)) != 0;
    }

    private static void FirstRun() {
        // Gameplay
        HighlightTiles = true;
        ShowTimer = true;
        ResizableGameBoard = true;
        ShowEditNumbers = false;
        AlternateGameBoardColors = false;
        // Themes
        ThemeIndex = 0;
        FontIndex = 0;
        BackgroundIndex = 0;
        BackgroundActive = true;
        BackgroundBlur = 1f;
        // Looks
        PostProcessingStrength = 1f;
        RenderScale = 1f;
        // Animations
        AnimSpeedMultiplier = 0.65f;
        // Sound
        MasterVolume = 1f;
        MusicVolume = 1f;
        SoundVolume = 1f;
        // Other
        TargetFrameRate = 60;
        LeftHanded = false;
        ResizableMenus = true;

        SavedLevels.CLEAR();

        SaveSettings();
    }

    private static bool firstSave = true;

    public static void SaveSettings() {
        if (firstSave) {
            Debug.Log("Saving settings");
            firstSave = false;
        }

        // Gameplay
        PlayerPrefs.SetInt(nameof(HighlightTiles), HighlightTiles == true ? 1 : 0);
        PlayerPrefs.SetInt(nameof(ShowTimer), ShowTimer == true ? 1 : 0);
        PlayerPrefs.SetInt(nameof(ResizableGameBoard), ResizableGameBoard == true ? 1 : 0);
        PlayerPrefs.SetInt(nameof(ShowEditNumbers), ShowEditNumbers == true ? 1 : 0);
        PlayerPrefs.SetInt(nameof(AlternateGameBoardColors), AlternateGameBoardColors == true ? 1 : 0);
        // Themes
        PlayerPrefs.SetInt(nameof(ThemeIndex), ThemeIndex);
        PlayerPrefs.SetInt(nameof(FontIndex), FontIndex);
        PlayerPrefs.SetInt(nameof(BackgroundIndex), BackgroundIndex);
        PlayerPrefs.SetInt(nameof(BackgroundActive), BackgroundActive == true ? 1 : 0);
        PlayerPrefs.SetFloat(nameof(BackgroundBlur), BackgroundBlur);
        // Looks
        PlayerPrefs.SetFloat(nameof(RenderScale), RenderScale);
        PlayerPrefs.SetFloat(nameof(PostProcessingStrength), PostProcessingStrength);
        // Animations
        PlayerPrefs.SetFloat(nameof(AnimSpeedMultiplier), AnimSpeedMultiplier);
        // Sound
        PlayerPrefs.SetFloat(nameof(MasterVolume), MasterVolume);
        PlayerPrefs.SetFloat(nameof(MusicVolume), MusicVolume);
        PlayerPrefs.SetFloat(nameof(SoundVolume), SoundVolume);
        // Other
        PlayerPrefs.SetInt(nameof(TargetFrameRate), TargetFrameRate);
        PlayerPrefs.SetInt(nameof(LeftHanded), LeftHanded == true ? 1 : 0);
        PlayerPrefs.SetInt(nameof(ResizableMenus), ResizableMenus == true ? 1 : 0);
    }


    // ----- Constructor -----

    static Settings() {
        LoadSettings();
        if (AnimSpeedMultiplier == 0f) {
            AnimSpeedMultiplier = 0.1f;
        }
    }
}
