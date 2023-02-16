using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoButton : MonoBehaviour
{
    // ----- Variables -----
    [SerializeField] bool checkPosition;
    bool lastCheckPos;
    [SerializeField] Vector2 targetPosition;
    Vector2 lastTargetPos;

    [Space(10f)]
    [SerializeField] bool checkDimensions;
    bool lastCheckDim;
    [SerializeField] float dimensionMultiplier;
    float lastDimMult;
    
    [Space(10f)]
    [TextArea(minLines:7, maxLines:20)] [SerializeField]
    public string message;
    private Button thisButton;


    // ----- MonoBehaviour stuff -----
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        GetObjects();
        SetUpButton();
        yield return new WaitForEndOfFrame();
        SetPosition();
    }


    // ----- InfoButton Stuff -----

    // Give the button an onClick event
    void SetUpButton() {
        CheckMessage();
        SetDimensions();
        thisButton.onClick.AddListener(() => {
            Info.CreateInfo(gameObject, message);
        });
    }

    // Check the message if there are certain things like italics, bold, etc
    void CheckMessage() {
        // Not needed?
    }

    void SetDimensions() {
        if (!checkDimensions) { return; }

        RectTransform rect = gameObject.GetComponent<RectTransform>();
        bool isLong = rect.rect.width > rect.rect.height;

        if (dimensionMultiplier == 0f) { dimensionMultiplier = 1f; }

        float newSize;
        if (isLong) {
            newSize = rect.rect.height * dimensionMultiplier;
        } else {
            newSize = rect.rect.width * dimensionMultiplier;
        }
        rect.sizeDelta = new Vector2(newSize, 0f);
    }

    void SetPosition() {
        if (!checkPosition) { return; }

        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchoredPosition = targetPosition;
    }


    // ----- Utilities -----

    void GetObjects() {
        thisButton = gameObject.GetComponent<Button>();
        if (thisButton == null) {
            thisButton = gameObject.AddComponent<Button>();
        }

        if (message == null || message == "") {
            message = "~ Error: no message to display ~";
        }

        CheckObjects(true);
    }

    // Returns whether an object that is needed is null or not
    bool CheckObjects(bool logNulls) {
        bool allGood = true;
        if (thisButton == null) {
            if (logNulls) { Debug.Log("InfoButton: Object " + gameObject.name + " button is null"); }
            allGood = false;
        }
        if (message == null || message == "") {
            if (logNulls) { Debug.Log("InfoButton: Object " + gameObject.name + " has no message"); }
            allGood = false;
        }
        return allGood;
    }
    bool CheckObjects() {
        return CheckObjects(false);
    }
}
