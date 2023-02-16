using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]

public static class ThemeController
{
    // ----- UPDATE EVERYTHING EVERYWHERE -----

    public static float HueOffset;

    public static void ResetAll() {
        Theme.Reset();
    }


    // ----- UPDATE COLOR -----

    // Updates image color to selected image color type (main, accent, background)
    public static void UpdateColor(ImageTheme imageTheme) {
        Image image = imageTheme.gameObject.GetComponent<Image>();
        Color color;

        if (imageTheme.UseColor) {
            color = GetColorFromWhichColor(imageTheme.color);
        } else {
            color = GetColorFromLookType(imageTheme.lookType);
        }

        color.a = imageTheme.Alpha;
        image.color = CheckAlpha(color);
    }

    // Updates text color to selected image color type (main, accent, background)
    public static void UpdateColor(TextTheme textTheme) {
        TMP_Text text = textTheme.gameObject.GetComponent<TMP_Text>();
        Color color;

        if (textTheme.UseColor) {
            color = GetColorFromWhichColor(textTheme.color);
        } else {
            color = GetColorFromLookType(textTheme.lookType);
        }

        color.a = textTheme.Alpha;
        text.color = CheckAlpha(color);
    }


    // ----- SET COLOR TO SPECEFIC COLOR -----

    public static void SetColor(Color color, Image image) {
        image.color = color;
    }

    public static void SetColor(Color color, TMP_Text text) {
        text.color = color;
    }


    // Change UseColor variable on this ImageTheme and update its colors
    public static void ChangeUseColor(ImageTheme imageTheme, bool turnOn) {
        if (turnOn) {
            imageTheme.UseColor = true;
        } else {
            imageTheme.UseColor = false;
        }

        UpdateColor(imageTheme);
    }

    // Change UseColor variable on this TextTheme and update its colors
    public static void ChangeUseColor(TextTheme textTheme, bool turnOn) {
        if (turnOn) {
            textTheme.UseColor = true;
        } else {
            textTheme.UseColor = false;
        }

        UpdateColor(textTheme);
    }


    // Sets alpha to 1 if it is not set (aka set to 0)
    public static Color CheckAlpha(Color color) {
        if (color.a == 0f) {
            Color c = color;
            c.a = 1f;
            color = c;
        }
        return color;
    }


    // ----- TECHNICOLOR STUFF -----

    static LTDescr technicolorTween;
    public static void StartTechnicolorTween() {
        technicolorTween = LeanTween.value(0f, 1f, 10f)
        .setLoopCount(int.MaxValue)
        .setOnUpdate((float value) => {
            HueOffset = value;
        });
    }
    public static void StopTechnicolorTween() {
        if (technicolorTween != null) {
            technicolorTween.pause();
        }
        technicolorTween = null;

        foreach (TextTheme text in GameObject.FindObjectsOfType<TextTheme>(true)) {
            text.Reset();
        }
        foreach (ImageTheme img in GameObject.FindObjectsOfType<ImageTheme>(true)) {
            img.StopHalf();
            img.Reset();
        }

        if (GameManager.GameBoard == null) {
            return;
        }
        GameManager.GameBoard.ResetThemes();
    }


    // ----- UTILITIES -----

    public static Color GetColorFromLookType(LookType look) {
        switch (look)
        {
            case LookType.UI_main:         return Theme.UI_main;
            case LookType.UI_accent:       return Theme.UI_accent;
            case LookType.UI_background:   return Theme.UI_background;
            case LookType.text_main:       return Theme.text_main;
            case LookType.text_accent:     return Theme.text_accent;
            case LookType.text_background: return Theme.text_background;
            case LookType.background:      return Theme.background;
        }
        Debug.Log("No case for this Text LookType in GetColorFromLookType()");
        return Color.magenta;
    }

    // Returns corresponding theme color (1-6) depending on WhichColor enum (1-6)
    public static Color GetColorFromWhichColor(WhichColor color) {
        switch (color) {
            // Basic theme colors
            case WhichColor.black:      return Theme.black;
            case WhichColor.dark_grey:  return Theme.dark_grey;
            case WhichColor.light_grey: return Theme.light_grey;
            case WhichColor.grey:       return Theme.grey;
            case WhichColor.white:      return Theme.white;

            // Specific theme colors
            case WhichColor.Color1: return Theme.color1;
            case WhichColor.Color2: return Theme.color2;
            case WhichColor.Color3: return Theme.color3;
            case WhichColor.Color4: return Theme.color4;
            case WhichColor.Color5: return Theme.color5;
            case WhichColor.Color6: return Theme.color6;
        }
        Debug.Log("No case for this WhichColor in GetColorFromWhichColor()");
        return Color.magenta;
    }

    public static Color Half(Color original, Color half) {
        return Half(original, half, 0.5f);
    }
    public static Color Half(Color original, Color half, float ratio) {
        float q = ratio, p = 1f-q;
        Color a = original, b = half;
        Color col = new Color();
        col.r = (p * a.r  +  q * b.r);
        col.g = (p * a.g  +  q * b.g);
        col.b = (p * a.b  +  q * b.b);
        return col;
    }
}

// Holds which color the text/image wants to be
[System.Serializable]
public enum WhichColor
{
    black,
    dark_grey,
    light_grey,
    grey,
    white,
    Color1,
    Color2,
    Color3,
    Color4,
    Color5,
    Color6
}

// Used to set what an object type is
// For example, set an object as "Main" to give it main colors
[System.Serializable]
public enum LookType
{
    UI_main,
    UI_accent,
    UI_background,
    text_main,
    text_accent,
    text_background,
    background
}

// Used to set what font type a text is
[System.Serializable]
public enum FontType
{
    Main,
    Bold,
    Thin
}