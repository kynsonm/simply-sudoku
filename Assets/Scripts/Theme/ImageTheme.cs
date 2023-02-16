using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ImageTheme : MonoBehaviour
{
    RectTransform thisRect;
    [Range(0f, 1f)] public float Alpha;

    public LookType lookType;
    public bool useHalf = false;
    public Color halfColor;
    [Range(0f, 1f)] public float halfRatio;

    public bool updateColor = true;
    public bool UseColor;
    public WhichColor color;

    public bool ignorePPUUpdate = false;
    public float PPUMultiplier;
    bool isSliced = true;
    bool isLong = true;


    // Initializing stuff
    void OnEnable() { StartCoroutine(Start()); }
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        Reset();
    }

    public void Reset() {
        // If alpha isn't set, set it to 1f
        if (Alpha == 0f) { Alpha = 1f; }

        if (updateColor) {
            ThemeController.UpdateColor(this);
        }
        if (useHalf) {
            Half();
        }

        if (!ignorePPUUpdate && isSliced) {
            UpdatePPU();
        }

        if (Theme.isRainbow && gameObject.activeInHierarchy) {
            StartCoroutine(TweenColor());
        }
    }

    // Reset image theme after <delaySeconds> frames
    public void Reset(int delayFrames) {
        StartCoroutine(ResetDelay(delayFrames));
    }
    IEnumerator ResetDelay(int delayFrames) {
        for (int i = 0; i < delayFrames; ++i) {
            yield return new WaitForEndOfFrame();
        }
        Reset();
    }

    public void StopHalf() {
        useHalf = false;
        Reset();
    }


    public void Half() {
        Half(halfColor, halfRatio);
    }
    public void Half(WhichColor half, float ratio) {
        Half(ThemeController.GetColorFromWhichColor(half), ratio);
    }
    public void Half(LookType half, float ratio) {
        Half(ThemeController.GetColorFromLookType(half), ratio);
    }
    public void Half(Color half, float ratio) {
        // Update vars on this script
        useHalf = true;
        halfColor = half;
        halfRatio = ratio;

        // Get vars for halving color
        Color a;
        if (!updateColor)  { a = Color.white; }
        else if (UseColor) { a = ThemeController.GetColorFromWhichColor(color); }
        else               { a = ThemeController.GetColorFromLookType(lookType); }

        Color b = halfColor;
        float q = ratio, p = 1-q;

        // Find new, halved color
        Color col = new Color();
        col.r = (p * a.r  +  q * b.r);
        col.g = (p * a.g  +  q * b.g);
        col.b = (p * a.b  +  q * b.b);
        col.a = Alpha;

        // Set image color to halved color
        Image img = gameObject.GetComponent<Image>();
        if (img == null) {
            Debug.Log("No image component on " + gameObject.name);
            return;
        }
        img.color = col;
    }


    public void UpdatePPU() {
        Image img = gameObject.GetComponent<Image>();
        if (img == null || ignorePPUUpdate) { return; }
        if (gameObject.GetComponent<Image>().type != Image.Type.Sliced) {
            isSliced = false;
            return;
        }
        
        if (thisRect == null) {
            thisRect = gameObject.GetComponent<RectTransform>();
        }
        isLong = thisRect.rect.width > thisRect.rect.height;

        float size = isLong ? thisRect.rect.height : thisRect.rect.width;

        if (PPUMultiplier <= 0f) { PPUMultiplier = 1f; }

        img.pixelsPerUnitMultiplier = Function(size) * PPUMultiplier;
    }

    float Function(float size) {
        float a = 1.691267f;
        float b = 0.003931462f;
        return 1.025f * (a / (b * size));
    }


    IEnumerator TweenColor() {
        if (!updateColor) { yield break; }

        Image img = gameObject.GetComponent<Image>();
        Color color = UseColor ? ThemeController.GetColorFromWhichColor(this.color) : ThemeController.GetColorFromLookType(lookType);

        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);

        while (true) {
            float hue = findHue(H);
            color = Color.HSVToRGB(hue, S, V);

            if (useHalf) {
                Half(Color.white, halfRatio);
            } else {
                img.color = new Color(color.r, color.g, color.b, Alpha);
            }

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