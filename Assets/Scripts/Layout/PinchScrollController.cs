using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PinchScrollController : MonoBehaviour
{
    // Variables
    [SerializeField] GameObject PinchScrollObject;
    [SerializeField] bool checkWithSettings;
    [SerializeField] bool scrollsOnX = false;
    [SerializeField] bool isGameBoard = false;
    PinchableScrollRect pinchScroll;
    RectTransform contentRect;

    // Checking vars
    bool lastSettingsCheck;
    Vector3 maxScale = Vector3.zero;
    float sensitivity = -1f;


    // Start is called before the first frame update
    void Start()
    {
        GetObjects();
        CheckSettings();
    }

    private void OnEnable() {
        StartCoroutine(UpdatePinch());
    }

    IEnumerator UpdatePinch() {
        while (true) {
            if (!gameObject.activeInHierarchy) { yield break; }
            yield return new WaitForSeconds(0.5f);

            // Make sure objects are good
            if (!CheckObjects()) {
                GetObjects();
                CheckSettings();
                if (!CheckObjects()) {
                    Debug.Log("Somethings wrong...");
                    continue;
                }
            }

            // Update pinch settings
            if (isGameBoard && (Settings.ResizableGameBoard != lastSettingsCheck)) {
                CheckSettings();
            }
            else if (!isGameBoard && (Settings.ResizableMenus != lastSettingsCheck)) {
                CheckSettings();
            }

            // Turn on non-dominant scrolling if scale is not 1
            bool pinchOn = (scrollsOnX) ? pinchScroll.vertical : pinchScroll.horizontal;
            if (!pinchOn && (contentRect.localScale.x != 1f || contentRect.localScale.y != 1f)) {
                if (scrollsOnX) {
                    pinchScroll.vertical = true;
                } else {
                    pinchScroll.horizontal = true;
                }
            }
            // Turn off non-dominant scrolling if scale is 1
            if (pinchOn && (contentRect.localScale.x == 1f || contentRect.localScale.y == 1f)) {
                pinchScroll.ResetZoom();
                pinchScroll.ResetContent();
                if (scrollsOnX) {
                    pinchScroll.vertical = false;
                } else {
                    pinchScroll.horizontal = false;
                }
            }

            // Turn on/off vertical if game board
            if (isGameBoard) {
                // Turn on vertical if scale is not 1
                if (!pinchScroll.vertical && (contentRect.localScale.x != 1f || contentRect.localScale.y != 1f)) {
                    pinchScroll.vertical = true;
                }
                // Turn off vertical if scale is 1
                if (pinchScroll.vertical && (contentRect.localScale.x == 1f || contentRect.localScale.y == 1f)) {
                    pinchScroll.ResetZoom();
                    pinchScroll.ResetContent();
                    pinchScroll.vertical = false;
                }
            }
        }
    }


    void CheckSettings() {
        if (pinchScroll == null) {
            Debug.Log("Pinch scroll is null");
            return;
        }
        
        if (checkWithSettings) {
            bool pinch = false;

            string log = gameObject.transform.parent.name + " --> " + gameObject.name + ":\n";
            log += "Settings allows pinch? " + Settings.ResizableMenus + " or " + Settings.ResizableGameBoard;
            Debug.Log(log);

            if (isGameBoard && Settings.ResizableGameBoard) {
                pinch = true;
            }
            else if (!isGameBoard && Settings.ResizableMenus) {
                pinch = true;
            }

            if (!pinch) {
                pinchScroll.upperScale = new Vector3(1f, 1f, 1f);
                pinchScroll.pinchSensitivity = 0f;
            } else {
                pinchScroll.upperScale = maxScale;
                pinchScroll.pinchSensitivity = sensitivity;
            }
        }

        lastSettingsCheck = isGameBoard ? Settings.ResizableGameBoard : Settings.ResizableMenus;
    }


    // ----- UTILITIES -----

    bool CheckObjects() {
        if (PinchScrollObject == null) { return false; }
        if (pinchScroll == null) { return false; }

        if (contentRect == null) { return false; }
        if (pinchScroll.content.name != contentRect.gameObject.name) { return false; }

        if (isGameBoard) {
            if (Settings.ResizableGameBoard != lastSettingsCheck) {
                return false;
            }
        } else {
            if (Settings.ResizableMenus != lastSettingsCheck) {
                return false;
            }
        }
        
        return true;
    }

    void GetObjects() {
        if (PinchScrollObject == null) {
            PinchScrollObject = gameObject;
        }

        if (pinchScroll == null) {
            pinchScroll = PinchScrollObject.GetComponent<PinchableScrollRect>();
            contentRect = pinchScroll.content;

            if (pinchScroll == null) {
                Debug.Log("No pinchable scroll rect on object " + PinchScrollObject.name);
            }
        }

        if (pinchScroll != null) {
            contentRect = pinchScroll.content;
        }

        if (maxScale == Vector3.zero) {
            maxScale = new Vector3(pinchScroll.upperScale.x, pinchScroll.upperScale.y, pinchScroll.upperScale.z);
        }
        if (sensitivity == -1) {
            sensitivity = pinchScroll.pinchSensitivity;
        }
    }
}
