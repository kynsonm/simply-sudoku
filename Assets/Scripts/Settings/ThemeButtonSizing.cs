using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeButtonSizing : MonoBehaviour
{
    [SerializeField] bool IsBackgroundButton = false;

    bool updated = false;

    private void OnEnable() { Start(); }
    private IEnumerator Start()
    {
        if (updated) { yield break; }

        // Wait a few frames for its size to get setup
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // And size them
        if (IsBackgroundButton) { SizeBackgroundButton(); }
        else { SizeThemeButton(); }
    }

    // Just need to resize <background> rect
    void SizeThemeButton() {
        // Get rects
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        RectTransform background = gameObject.transform.Find("Background").GetComponent<RectTransform>();

        float mult = (float)Screen.height / 3040f;
        mult = Mathf.Sqrt(mult);

        float pad = Mathf.Abs(background.offsetMax.x);
        pad *= mult;
        RectTransformOffset.All(background, pad);

        updated = true;
    }

    // Need to resize <fill> and <outline> rects
    void SizeBackgroundButton() {
        // Get rects
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        RectTransform fill = gameObject.transform.Find("Fill").GetComponent<RectTransform>();
        RectTransform outline = gameObject.transform.Find("Outline").GetComponent<RectTransform>();

        float mult = (float)Screen.height / 3040f;
        mult = Mathf.Sqrt(mult);

        float pad = Mathf.Abs(fill.offsetMax.x);
        pad *= mult;
        RectTransformOffset.All(fill, 1.1f * pad);
        RectTransformOffset.All(outline, pad);

        updated = true;
    }
}
