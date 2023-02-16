using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckOnScreen : MonoBehaviour
{
    [SerializeField] Transform BackgroundImageHolder;
    List<RectTransform> backgroundRects;

    // Start is called before the first frame update
    void Awake() { Start(); }
    void Start() { Reset(); }


    // Resets all variables, gets new ones, and starts their coroutines
    public void Reset() {
        StopAllCoroutines();
        StartCoroutine(ResetEnum());
    }
    IEnumerator ResetEnum() {
        Debug.LogWarning("RESETTING CHECKONSCREEN");

        // Reset everything
        SetVars();

        yield return new WaitForEndOfFrame();

        // Get new rects
        backgroundRects = new List<RectTransform>();
        yield return new WaitForEndOfFrame();
        foreach (Transform child in BackgroundImageHolder) {
            backgroundRects.Add(child.GetComponent<RectTransform>());
        }

        yield return new WaitForEndOfFrame();
        GameObject.FindObjectOfType<BackImageMover>().BackgroundImagesHolder.GetComponent<GridLayoutGroup>().enabled = false;

        // Start their coroutines
        // Staggers it a frame to prevent lag spikes every half second (maybe?)
        foreach (RectTransform rect in backgroundRects) {
            StartCoroutine(CheckOnScreenEnum(rect));
            yield return new WaitForEndOfFrame();
        }
    }


    // Check if the given image (rect) is on screen every 0.5 seconds
    IEnumerator CheckOnScreenEnum(RectTransform rect) {
        if (rect == null) { yield break; }
        rect.gameObject.SetActive(true);

        bool isOn = gameObject.activeInHierarchy;
        bool onScreen;
        while (true) {
            if (rect == null) { yield break; }

            // Set <onScreen>
            onScreen = OnScreen(rect);

            // Turn it on of on screen
            if (onScreen && !isOn) {
                isOn = true;
                rect.gameObject.SetActive(true);
            }

            // Otherwise, turn it off
            else if (!onScreen && isOn) {
                isOn = false;
                rect.gameObject.SetActive(false);
            }

            // Wait a half second
            yield return new WaitForSeconds(1f);
        }
    }


    // ----- OnScreen() -----
    // Checks if given rectTransform is on screen

    float padMult;
    float pad;

    float minX, minY, maxX, maxY;
    bool OnScreen(RectTransform rect) {
        bool checkX = rect.position.x - 0.5f * rect.rect.width <= maxX && rect.position.x + 0.5f * rect.rect.width >= minY;
        bool checkY = rect.position.y - 0.5f * rect.rect.height <= maxY && rect.position.y + 0.5f * rect.rect.height >= minX;
        if (checkX && checkY) { return true; }
        return false;
    }


    // Initializes variables to make OnScreen() faster
    void SetVars() {
        padMult = 0.1f;
        pad = padMult * Mathf.Max(Screen.width, Screen.height);

        minX = -pad;
        minY = -pad;
        maxX = Screen.width + pad;
        maxY = Screen.height + pad;
    }
}
