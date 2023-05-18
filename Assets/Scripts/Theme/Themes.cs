using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]

public static class Theme
{
    // Theme info
    public static string name;
    public static bool isRainbow;

    // Non-color colors
    public static Color black;
    public static Color dark_grey;
    public static Color grey;
    public static Color light_grey;
    public static Color white;

    // Text colors
    public static Color text_main;
    public static Color text_accent;
    public static Color text_background;

    // UI colors
    public static Color UI_main;
    public static Color UI_accent;
    public static Color UI_background;
    public static Color background;

    // Other colors
    public static Color color1;
    public static Color color2;
    public static Color color3;
    public static Color color4;
    public static Color color5;
    public static Color color6;

    // Change the theme
    public static void SetTheme(Look look) {
        // Set last theme being light or not
        if (Theme.name == null) { }
        else if (Theme.name.Contains("Light") || Theme.name.Contains("light")) {
            lastThemeWasLight = true;
        } else {
            lastThemeWasLight = false;
        }

        // Set each theme variable color
        Theme.name = look.name;
        Theme.isRainbow = look.isRainbow;
        if (Theme.isRainbow) {
            ThemeController.StartTechnicolorTween();
        } else {
            ThemeController.StopTechnicolorTween();
        }

        Theme.black = look.black;
        Theme.dark_grey = look.dark_grey;
        Theme.grey = look.grey;
        Theme.light_grey = look.light_grey;
        Theme.white = look.white;

        Theme.text_main = look.text_main;
        Theme.text_accent = look.text_accent;
        Theme.text_background = look.text_background;

        Theme.UI_main = look.UI_main;
        Theme.UI_accent = look.UI_accent;
        Theme.UI_background = look.UI_background;

        Theme.background = look.background;
        if (colorCompare(Theme.background, Color.black)) {
            Theme.background = UI_background;
        }

        Theme.color1 = look.color1;
        Theme.color2 = look.color2;
        Theme.color3 = look.color3;
        Theme.color4 = look.color4;
        Theme.color5 = look.color5;
        Theme.color6 = look.color6;

        // Set current theme being light or not
        if (Theme.name.Contains("Light") || Theme.name.Contains("light")) {
            thisThemeIsLight = true;
        } else {
            thisThemeIsLight = false;
        }

        //setMaterialShaders();
    }

    public static void Reset() {
        GameObject.FindObjectOfType<Themes>().UpdateTheme();
    }

    static bool colorCompare(Color lhs, Color rhs) {
        bool r = lhs.r == rhs.r;
        bool g = lhs.g == rhs.g;
        bool b = lhs.b == rhs.b;
        bool a = lhs.a <= 0.1f;
        return r && g && b && a;
    }

    static bool firstRun = true;
    static bool lastThemeWasLight = false;
    static bool thisThemeIsLight = false;
    static void setMaterialShaders() {
        if (lastThemeWasLight == thisThemeIsLight && !firstRun) {
            Debug.Log("Returning from setMaterialShaders() without doing anything");
            return;
        }
        firstRun = false;

        Color overlayColor = thisThemeIsLight ? Color.white : Color.black;

        Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
        foreach (Material mat in materials) {
            if (!mat.shader.name.Contains("Distance Field")) { continue; }

            mat.SetColor("_UnderlayColor", overlayColor);
        }
    }
}

public static class Font
{
    public static string name;
    public static TMP_FontAsset font;
    public static TMP_FontAsset bold;
    public static TMP_FontAsset thin;

    // Set new font
    public static void SetFont(FontLook font) {
        Font.name = font.name;
        Font.font = font.font;
        Font.bold = font.bold;
        Font.thin = font.thin;
    }
}

public static class Background
{
    public static string name;

    public static bool areNumbers;
    public static bool rotateX, rotateY, rotateZ;
    public static float rotationSpeedMultX, rotationSpeedMultY, rotationSpeedMultZ;

    public static int maxNumber;
    public static float maxStartXAngle, maxStartYAngle, maxStartZAngle;
    public static float minSizeMultiplier, maxSizeMultiplier;

    public static bool useThemeColors;
    public static bool useThemeLookTypes;
    public static float themeColorPercentage;

    public static bool useGlow;
    public static bool useAber;

    public static Sprite backgroundSprite;
    public static bool moveBackgroundImage;
    public static List<Sprite> sprites;

    public static void SetBackground(BackgroundObject background) {
        Background.name = background.name;

        Background.areNumbers = background.areNumbers;
        Background.rotateX = background.rotateX;
        Background.rotateY = background.rotateY;
        Background.rotateZ = background.rotateZ;
        
        Background.rotationSpeedMultX = background.rotationSpeedMultX;
        Background.rotationSpeedMultY = background.rotationSpeedMultY;
        Background.rotationSpeedMultZ = background.rotationSpeedMultZ;

        Background.maxNumber = background.maxNumber;
        Background.maxStartZAngle = background.maxStartZAngle;
        Background.maxStartXAngle = background.maxStartXAngle;
        Background.maxStartYAngle = background.maxStartYAngle;

        Background.minSizeMultiplier = background.minSizeMultiplier;
        Background.maxSizeMultiplier = background.maxSizeMultiplier;

        Background.useThemeColors = background.useThemeColors;
        Background.useThemeLookTypes = background.useThemeLookTypes;
        Background.themeColorPercentage = background.themeColorPercentage;

        Background.useGlow = background.useGlow;
        Background.useAber = background.useAber;

        Background.backgroundSprite = background.backgroundSprite;
        Background.moveBackgroundImage = background.moveBackgroundImage;
        Background.sprites = new List<Sprite>(background.sprites);

        spriteBucket = new List<int>();
    }


    // ----- OBJECT SELECTION -----

    // Bucket for retrieving the index of a background image in <sprites>
    //   Stores indexes to sprites in <sprites>
    static List<int> spriteBucket;
    // Uses Random.Range to get a random index, then removes it
    public static Sprite GetRandomSprite() {
        // If background is numbers, return nothing
        if (Background.areNumbers) { return null; }
        // Reset bucket if necessary
        if (spriteBucket == null || spriteBucket.Count == 0) {
            spriteBucket = new List<int>();
            for (int i = 0; i < sprites.Count; ++i) { spriteBucket.Add(i); }
        }
        // Remove from bucket, then return sprite at the index
        int buckIndex = Random.Range(0, spriteBucket.Count);

        Debug.Log($"Got bucket index {buckIndex} --> {spriteBucket[buckIndex]}. Current bucket count == {spriteBucket.Count}");

        int index = spriteBucket[buckIndex];
        spriteBucket.RemoveAt(buckIndex);
        return sprites[index];
    }
}


public class Themes : MonoBehaviour
{
    // All themes
    public List<Look> looks;
    // All fonts
    public List<FontLook> fontLooks;
    // Current theme index
    // Is static to be referenced by ThemeController
    public List<BackgroundObject> backgrounds;
    public static int themeIndex;
    // Current font index
    public static int fontIndex;
    // Current background
    public static int backgroundIndex;
    

    // Start is called before the first frame update
    void Start()
    {
        // Set saved theme, font, background
        themeIndex = Settings.ThemeIndex;
        fontIndex = Settings.FontIndex;
        backgroundIndex = Settings.BackgroundIndex;

        // Set the backgrounds
        UpdateTheme();
        UpdateFont();
        UpdateBackground();
    }
#if UNITY_EDITOR
    void Update() {
        if (!Application.isPlaying) {
            UpdateTheme();
        }
    }
#endif


    // Change the theme given a new index
    public void ChangeTheme(int newIndex) {
        if (newIndex == themeIndex) { return; }
        if (newIndex >= looks.Count) {
            newIndex = 0;
        }
        themeIndex = newIndex;
        Settings.ThemeIndex = themeIndex;
        UpdateTheme();
    }

    // Update the current Font given a new index
    public void ChangeFont(int newIndex) {
        if (newIndex == fontIndex) { return; }
        if (newIndex >= fontLooks.Count) {
            newIndex = newIndex % fontLooks.Count;
        }
        fontIndex = newIndex;
        Settings.FontIndex = fontIndex;
        UpdateFont();
    }

    public void ChangeBackground(int newIndex) {
        if (newIndex == backgroundIndex) { return; }
        if (newIndex >= backgrounds.Count) {
            newIndex = 0;
        }
        backgroundIndex = newIndex;
        Settings.BackgroundIndex = backgroundIndex;
        UpdateBackground();
    }
    public void DEBUG_NextBackground() {
        ++backgroundIndex;
        if (backgroundIndex >= backgrounds.Count) {
            backgroundIndex = 0;
        }
        Settings.BackgroundIndex = backgroundIndex;
        UpdateBackground();
    }


    // Update global Theme with selected this.theme
    public void UpdateTheme() {
        Theme.SetTheme(looks[themeIndex]);
        foreach (TextTheme text in GameObject.FindObjectsOfType<TextTheme>(true)) {
            text.Reset();
        }
        foreach (ImageTheme img in GameObject.FindObjectsOfType<ImageTheme>(true)) {
            img.Reset();
        }
        if (GameManager.GameBoard == null) {
            return;
        }
        GameManager.GameBoard.ResetThemes();
    }
    public void UpdateTheme(int index) {
        ChangeTheme(index);
    }


    public void UpdateFont() {
        Font.SetFont(fontLooks[fontIndex]);
        foreach (TextTheme text in GameObject.FindObjectsOfType<TextTheme>(true)) {
            text.UpdateFont();
        }
    }
    public void UpdateFont(int index) {
        ChangeFont(index);
    }


    public void UpdateBackground() {
        Background.SetBackground(backgrounds[backgroundIndex]);
        Background.rotationSpeedMultX = (Background.rotationSpeedMultX <= 0.01f) ? 0.01f : Background.rotationSpeedMultX;
        Background.rotationSpeedMultY = (Background.rotationSpeedMultY <= 0.01f) ? 0.01f : Background.rotationSpeedMultY;
        Background.rotationSpeedMultZ = (Background.rotationSpeedMultZ <= 0.01f) ? 0.01f : Background.rotationSpeedMultZ;
        GameObject.FindObjectOfType<BackgroundAnimator>().StartMakingObjects();
        GameObject.FindObjectOfType<BackImageMover>().ResetBackground(true);
        BackgroundObjectSaver.LoadObjects(true);
    }
    public void UpdateBackground(int index) {
        ChangeBackground(index);
    }
}

// Class to hold a theme
// Used in a list of themes
// Serializable to be editable in unity editor
[System.Serializable]
public class Look
{
    public string name;
    [TextArea(minLines:2, maxLines:5)] public string info;

    [Space(10f)]
    public string ID = "0";
    public int cost = 0;
    public IAPTier iapTier = IAPTier.invalid;

    // Basic info
    public bool isRainbow;

    // Non-color colors
    [Space(10f)]
    public Color black;
    public Color dark_grey;
    public Color grey;
    public Color light_grey;
    public Color white;

    // Text colors
    [Space(10f)]
    public Color text_main;
    public Color text_accent;
    public Color text_background;

    // UI colors
    [Space(10f)]
    public Color UI_main;
    public Color UI_accent;
    public Color UI_background;
    public Color background;

    // Colors lol
    [Space(10f)]
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    public Color color5;
    public Color color6;
}

// Class to hold a font
// Used in a list of fonts
// Serializable to be editable in unity editor
[System.Serializable]
public class FontLook
{
    public string name;
    [TextArea(minLines:2, maxLines:5)] public string info;

    [Space(10f)]
    public string ID = "0";
    public int cost = 0;
    public IAPTier iapTier = IAPTier.invalid;

    [Space(10f)]
    public TMP_FontAsset font;
    public TMP_FontAsset bold;
    public TMP_FontAsset thin;
}

// Class to hold a background object type
// Used in a list of backgrounds
// Determines whethere Background Animator uses numbers or images,
//     and which sprites to use
[System.Serializable]
public class BackgroundObject
{
    public string name = "New Background Object";
    [TextArea(minLines:3, maxLines:5)] public string info;

    // Purchasing info
    [Space(10f)]
    public string ID = "0";
    public int cost = 0;
    public IAPTier iapTier = IAPTier.invalid;

    [Space(10f)]
    public bool areNumbers = false;
    public bool rotateX = false, rotateY = false, rotateZ = true;
    
    [Min(0.01f)] public float rotationSpeedMultX = 1f;
    [Min(0.01f)] public float rotationSpeedMultY = 1f;
    [Min(0.01f)] public float rotationSpeedMultZ = 1f;

    [Space(10f)]
    public int maxNumber = 20;
    public float maxStartXAngle = 0f;
    public float maxStartYAngle = 0f;
    public float maxStartZAngle = 360f;

    [Space(10f)]
    public float minSizeMultiplier = 0.07f;
    public float maxSizeMultiplier = 0.25f;

    [Space(10f)]
    public bool useThemeColors = true;
    public bool useThemeLookTypes = false;
    [Range(0f, 1f)]
    public float themeColorPercentage = 0.25f;

    [Space(10f)]
    public bool useGlow = false;
    public bool useAber = false;

    [Space(10f)]
    public Sprite backgroundSprite;
    public bool moveBackgroundImage = true;

    [Space(10f)]
    public List<Sprite> sprites;
}