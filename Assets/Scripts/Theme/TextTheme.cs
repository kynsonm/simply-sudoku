using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TextTheme : MonoBehaviour
{
    [Range(0f, 1f)] public float Alpha;

    public FontType fontType;
    public bool IsBold;
    public bool IsItalic;
    public bool IsStrikethrough;
    public bool IsUnderline;

    public LookType lookType;

    public bool UseColor;
    public WhichColor color;

    public float MaxTextRatio;

    public bool updateFont = true;
    public bool updateTextSize = true;
    public bool updateTextColor = true;

    public bool usingSameTextSize = false;


    // Initializing stuff
    void OnEnable() { StartCoroutine(Start()); }
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        Reset();
    }

    // Resets font, text size, and color
    public void Reset() { Reset(true); }
    public void Reset(bool updateTextSize) {
        // If alpha isn't set, set it to 1f
        if (Alpha == 0f) { Alpha = 1f; }

        // If text ratio isn't set, set it to 1f
        if (MaxTextRatio == 0f) { MaxTextRatio = 1f; }

        CheckText(gameObject.GetComponent<TMP_Text>());

        // And update their looks
        if (updateTextColor) {
            ThemeController.UpdateColor(this);
        }
        if (updateFont) {
            UpdateFont();
        }
        if (updateTextSize && this.updateTextSize) {
            UpdateTextSize();
        }

        if (Theme.isRainbow && gameObject.activeInHierarchy) {
            StartCoroutine(TweenColor());
        }
    }

    // Update font type (main, bold, thin)
    public void UpdateFont() {
        if (!updateFont) { return; }

        TMP_Text text = GetComponent<TMP_Text>();

        switch (fontType) {
            case FontType.Main: text.font = Font.font;
                break;
            case FontType.Bold: text.font = Font.bold;
                break;
            case FontType.Thin: text.font = Font.thin;
                break;
        }

        text.fontStyle = FontStyles.Normal;
        if (IsBold)          { text.fontStyle |= FontStyles.Bold; }
        if (IsItalic)        { text.fontStyle |= FontStyles.Italic; }
        if (IsUnderline)     { text.fontStyle |= FontStyles.Underline; }
        if (IsStrikethrough) { text.fontStyle |= FontStyles.Strikethrough; }
    }

    // Update text size depending on MaxTextRatio
    public void UpdateTextSize() {
        if (gameObject.activeInHierarchy && !usingSameTextSize) {
            StartCoroutine(UpdateTextSizeEnumerator());
        }
    }
    public void ResetTextSize() {
        UpdateTextSize();
    }

    IEnumerator UpdateTextSizeEnumerator() {
        TMP_Text text = gameObject.GetComponent<TMP_Text>();

        CheckText(text);

        text.fontSizeMax = 10000f;
        yield return new WaitForEndOfFrame();
        
        text.fontSizeMax = MaxTextRatio * text.fontSize;
    }

    void CheckText(TMP_Text text) {
        if (text.text == null || text.text.Length == 0) { return; }
        string str = text.text;
        str = str.Replace("<tm>", Hex(LookType.text_main));
        str = str.Replace("<ta>", Hex(LookType.text_accent));
        str = str.Replace("<tb>", Hex(LookType.text_background));
        str = str.Replace("<um>", Hex(LookType.UI_main));
        str = str.Replace("<ua>", Hex(LookType.UI_accent));
        str = str.Replace("<ub>", Hex(LookType.UI_background));
        str = str.Replace("</c>", "</color>");
        text.text = str;
    }

    string Hex(LookType look) {
        string str = "<#";
        str += ColorUtility.ToHtmlStringRGB(ThemeController.GetColorFromLookType(look));
        str += ">";
        return str;
    }


    IEnumerator TweenColor() {
        if (!updateTextColor) { yield break; }

        TMP_Text text = gameObject.GetComponent<TMP_Text>();
        Color color = UseColor ? ThemeController.GetColorFromWhichColor(this.color) : ThemeController.GetColorFromLookType(lookType);

        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);

        while (true) {
            float hue = findHue(H);
            color = Color.HSVToRGB(hue, S, V);
            text.color = new Color(color.r, color.g, color.b, Alpha);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
    }
    float findHue(float H) {
        float hue = H + ThemeController.HueOffset;
        if (hue > 1f) { hue -= 1f; }
        return hue;
    }
}