using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderHandle : MonoBehaviour
{
    [SerializeField] GameObject SliderBackground;
    [Space(10f)]
    [SerializeField] float HeightOfHandleMultiplier;
    [SerializeField] float WidthOfHandleMultiplier;
    RectTransform rect;
    RectTransform backgroundRect;

    // When enabled, set handle sizing
    private void OnEnable() {
        StartCoroutine(OnEnableEnum());
    }
    IEnumerator OnEnableEnum() {
        // Make sure everything is good
        if (!CheckObjects()) {
            GetObjects();
            if (!CheckObjects()) {
                // Restart enum if objects werent found
                Debug.Log("Bad objects on slider handle");
                yield return new WaitForSeconds(1f);
                StartCoroutine(OnEnableEnum());
                yield break;
            }
        }

        // Sets sizing a frame later
        yield return new WaitForEndOfFrame();
        //yield return new WaitForSeconds(0.1f);
        SetSize();
    }

    // Set sizing of the slider handle
    void SetSize() {
        float backHeight = backgroundRect.rect.height;
        float width = WidthOfHandleMultiplier * backHeight;
        float height = HeightOfHandleMultiplier * backHeight;
        rect.sizeDelta = new Vector2(width, height);
    }

    // Return true if all objects are NOT null
    bool CheckObjects() {
        if (rect == null) { return false; }
        if (SliderBackground == null) { return false; }
        if (backgroundRect == null) { return false; }
        return true;
    }
    // Gets necessary variables
    void GetObjects() {
        if (rect == null) { rect = gameObject.GetComponent<RectTransform>(); }
        if (SliderBackground == null) {
            SliderBackground = gameObject.transform.parent.parent.Find("Background").gameObject;
            if (SliderBackground == null) { return; }
        }
        if (backgroundRect == null) {
            backgroundRect = SliderBackground.GetComponent<RectTransform>();
        }
    }
}
