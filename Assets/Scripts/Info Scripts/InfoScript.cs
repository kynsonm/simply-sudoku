using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SoundClip;

public class InfoScript : MonoBehaviour
{
    // PUT THIS SCRIPT ON THE CANVAS!

    // ----- Variables -----
    [SerializeField] GameObject InfoPrefab;
    [SerializeField] float minMenuHeightMult;
    [SerializeField] float edgeSpacingMult;
    [SerializeField] float fontSizeMult;
    //[SerializeField] float infoSpacingMult;
    [SerializeField] float animationSpeedMult;
    Button closeButton;

    GameObject infoObject;
    bool onBottomOfButton = false;

    public bool infoOpen = false;


    // ----- MonoBehaviour methods -----
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(true);
        GetObjects();
        gameObject.SetActive(false);
    }
    void OnEnable() { UpdateInfoScript(); }
    // Update is called once per frame
    void UpdateInfoScript()
    {
        StartCoroutine(UpdateInfoScriptEnum());
        StartCoroutine(CheckCloseInfo());
    }
    IEnumerator UpdateInfoScriptEnum() {
        bool getObjects = true;
        while (getObjects) {
            yield return new WaitForSeconds(0.05f);
            // Get useful objects
            if (!CheckObjects()) {
                GetObjects();
            } else {
                getObjects = false;
            }
        }
    }
    IEnumerator CheckCloseInfo() {
        while (true) {
            // Dont do anything while info is not open
            if (!infoOpen) {
                yield return new WaitForSeconds(0.5f);
                continue;
            }
            // Check for inputs when info IS open
            if (Input.touchCount > 0 || Input.anyKeyDown || Input.anyKey) {
                CloseInfo();
                infoOpen = false;
            }
            // Wait a frame
            yield return new WaitForEndOfFrame();
        }
    }


    // ----- Create info menu -----

    // Need to do this in an enumerator since we need to wait for stuff to update
    IEnumerator CreateInfoEnum(GameObject obj, string message, bool centerIt) {
        // Close info if one is already open
        if (enumRunning || infoOpen) {
            Debug.LogWarning("Closing prev info button first");
            CloseInfo(obj, message);
            yield break;
            //yield return new WaitForSeconds(1.1f * animationSpeedMult * Settings.AnimSpeedMultiplier);
        }

        Sound.Play(info_button);

        enumRunning = true;
        infoOpen = true;

        DestroyChildrenOf(gameObject);

        // Instantiate info object and set alpha to 0.001
        GameObject info = Instantiate(InfoPrefab, gameObject.transform);
        info.GetComponent<CanvasGroup>().alpha = 0.001f;
        infoObject = info;

        // Set its dimensions to the default
        RectTransform rect = info.GetComponent<RectTransform>();
        float size = 0.75f * (float)Screen.width;
        rect.sizeDelta = new Vector2(size, size);

        // And set the text n text bounds
        TMP_Text text = info.GetComponentInChildren<TMP_Text>();
        text.text = message;
        text.fontSize = Screen.width * fontSizeMult;
        SetBounds(text.gameObject);
        yield return new WaitForEndOfFrame();

        // Set horizontal and vertical size of the background
        Vector2 newSizeDelta = new Vector2(rect.sizeDelta.x, text.textBounds.size.y);
        if (text.bounds.size.x < text.gameObject.GetComponent<RectTransform>().rect.width) {
            float diff = text.gameObject.GetComponent<RectTransform>().rect.width - text.bounds.size.x;
            newSizeDelta.x -= diff;
        }
        rect.sizeDelta = newSizeDelta;

        yield return new WaitForEndOfFrame();
        SetBounds(text);

        // Find position
        yield return new WaitForEndOfFrame();
        rect.position = FindPosition(rect, obj.GetComponent<RectTransform>());
        if (centerIt) {
            rect.position = new Vector3(Screen.width/2, Screen.height/2, rect.position.z);
        }

        // Tween alpha and position

        CanvasGroup canvas = info.GetComponent<CanvasGroup>();
        float time = animationSpeedMult * Settings.AnimSpeedMultiplier;
        float startHeight = onBottomOfButton ? rect.rect.height : -rect.rect.height;

        // Tween background up
        RectTransform back = info.transform.Find("Back").GetComponent<RectTransform>();
        back.localPosition = new Vector3(back.localPosition.x, startHeight, back.localPosition.z);
        LeanTween.moveLocalY(back.gameObject, 0f, time)
        .setEase(LeanTweenType.easeInOutCubic);

        // Tween message up
        RectTransform mes = info.transform.Find("Message").GetComponent<RectTransform>();
        mes.localPosition = new Vector3(mes.localPosition.x, startHeight, mes.localPosition.z);
        LeanTween.moveLocalY( mes.gameObject, 0f, time)
        .setEase(LeanTweenType.easeInOutCubic)

        // Tween fade in
        .setOnStart( () =>
            LeanTween.value(0f, 1f, 0.6f * time)
            .setEase(LeanTweenType.easeInCubic)
            .setOnUpdate((float value) => {
                canvas.alpha = value;
            })
        )
        .setOnComplete(() => {
            enumRunning = false;
            infoOpen = true;
        });

        enumRunning = false;
    }

    Vector3 FindPosition(RectTransform infoPopupRect, RectTransform buttonRect) {
        Vector3 position = buttonRect.position;

        Vector2 buttonSize = new Vector2(buttonRect.rect.width, buttonRect.rect.height);
        Vector2 popupSize = new Vector2(infoPopupRect.rect.width, infoPopupRect.rect.height);

        Debug.Log($"Button Size: {buttonSize.ToString()}\nPopup Size: {popupSize.ToString()}");

        float offsetY = (buttonSize.y /* / 2f */) + (popupSize.y / 2f);
        float offsetX = (buttonSize.x / 2f) + (popupSize.x / 2f);

        position.y += offsetY;

        // If above is too high, put it below
        if (position.y + popupSize.y / 2f > Screen.height) {
            position.y = buttonRect.position.y;
            position.y -= offsetY;
            Debug.Log("Info popup is off the top");
        }
        if (position.y - popupSize.y / 2f < 0f) {
            position.y = buttonRect.position.y;
            position.y += offsetY;
            Debug.Log("Info popup is off the bottom!");
        }

        // Shift it left or right
        float edgeSpacing = edgeSpacingMult * Mathf.Min(Screen.width, Screen.height);
        if (position.x - popupSize.x / 2f < 0f) {
            Debug.Log("Its off the screen LEFT");
            position.x = edgeSpacing + popupSize.x / 2f;
        }
        if (position.x + popupSize.x / 2f > Screen.width) {
            Debug.Log("Its off the screen RIGHT");
            position.x = (Screen.width - edgeSpacing) - popupSize.x / 2f;
        }



        /*
        infoRect.position = buttonRect.position;

        Vector3 startPos = infoRect.position;
        Vector2 size = infoRect.sizeDelta / 2f;
        Vector2 oriSize = new Vector2(buttonRect.rect.width, buttonRect.rect.height) / 2f;
        Vector3 pos = new Vector3(startPos.x, startPos.y, startPos.z);

        // Why is this so off? idk
        float spacing = -0.85f * oriSize.y;

        // If above is too high, put it below
        pos.y += (spacing + size.y + oriSize.y);
        if (pos.y + size.y > Screen.height) {
            pos.y = startPos.y;
            pos.y -= ((1.1f * spacing) + size.y + oriSize.y);

            onBottomOfButton = true;

            if (pos.y - size.y < 0f) {
                // Its off the bottom too :(
            }
        } else {
            onBottomOfButton = false;
        }

        // Shift it left or right
        float edgeSpacing = edgeSpacingMult * Mathf.Min(Screen.width, Screen.height);
        if (pos.x - size.x < 0f) {
            //Debug.Log("Its off the screen LEFT");
            pos.x = edgeSpacing + size.x;
        }
        if (pos.x + size.x > Screen.width) {
            //Debug.Log("Its off the screen RIGHT");
            pos.x = (Screen.width - edgeSpacing) - size.x;
        }
        */

        return position;
    }

    bool enumRunning = false;

    // Create info dialogue box
    public void CreateInfo(GameObject obj, string message) {
        CreateInfo(obj, message, false);
    }
    public void CreateInfo(GameObject obj, string message, bool centerIt) {
        gameObject.SetActive(true);
        StartCoroutine(CreateInfoEnum(obj, message, centerIt));
    }

    public void CloseInfo() {
        CloseInfo(null, null);
    }
    public void CloseInfo(GameObject obj, string message) {
        if (!infoOpen) { return; }

        Sound.Play(close);

        enumRunning = true;

        CanvasGroup canvas = infoObject.GetComponent<CanvasGroup>();
        RectTransform rect = infoObject.GetComponent<RectTransform>();

        float time = animationSpeedMult * Settings.AnimSpeedMultiplier;

        // Tween background down
        RectTransform back = infoObject.transform.Find("Back").GetComponent<RectTransform>();
        LeanTween.moveLocalY(back.gameObject, -rect.rect.height, time)
        .setEase(LeanTweenType.easeInOutCubic);
        // Tween message down
        RectTransform mes = infoObject.transform.Find("Message").GetComponent<RectTransform>();
        LeanTween.moveLocalY( mes.gameObject, -rect.rect.height, time)
        .setEase(LeanTweenType.easeInOutCubic)
        .setOnStart(() => {
            LeanTween.value(1f, 0f, 0.8f * time)
            .setDelay(0.2f * time)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float value) => {
                if (canvas != null) {
                    canvas.alpha = value;
                }
            })
            .setOnComplete(() => {
                gameObject.SetActive(false);
                GameObject.Destroy(infoObject);
                infoObject = null;

                infoOpen = false;
                enumRunning = false;
                if (obj != null && message != null) {
                    CreateInfo(obj, message);
                }
            });
        });
    }


    // ----- Set bounds depending on either background size or text size -----

    // Sets the bounds of the text object depending on the size of the background
    void SetBounds(GameObject text) {
        RectTransform rect = text.GetComponent<RectTransform>();
        float bound = getBoundNum(text.transform.parent);

        bound *= 1.5f;
        rect.offsetMax = new Vector2(-bound, rect.offsetMax.y);
        rect.offsetMin = new Vector2( bound, rect.offsetMin.y);
    }

    // Sets the bounds of the gameobject depending on text size
    void SetBounds(TMP_Text text) {
        RectTransform rect = text.gameObject.GetComponent<RectTransform>();
        float offset = getNewBackSizeOffset(rect);

        rect = text.transform.parent.GetComponent<RectTransform>();
        //rect = text.transform.parent.Find("Back").GetComponent<RectTransform>();
        Vector2 size = rect.sizeDelta;
        rect.sizeDelta = new Vector2(size.x + offset, size.y + offset);
    }


    // ----- Bound number stuff -----

    // Get bound number from PPU multiplier
    float getBoundNum(float PPUMultiplier) {
        float a = 2.169623f;
        float b = 0.06034574f;
        return a / (b * PPUMultiplier);
    }
    // Gets PPU mult from parent
    float getBoundNum(Transform parent) {
        float PPU = parent.GetComponentInChildren<Image>().pixelsPerUnitMultiplier;
        return getBoundNum(PPU);
    }

    float getNewBackSizeOffset(RectTransform textRect) {
        float size = Mathf.Min(textRect.rect.width, textRect.rect.height);
        float newSize = size;
        float bound = getBoundNum(Function(newSize));

        int count = 0;
        while (newSize - (1.5f * bound) < size) {
            newSize += 1f;
            bound = getBoundNum(Function(newSize));

            ++count;
            if (count > 500) {
                Debug.Log("Count too high!");
                break;
            }
        }

        return newSize - size;
    }

    // Gets PPU multiplier from smallest dimension of the rect
    float Function(float size) {
        float a = 1.691267f;
        float b = 0.003931462f;
        return (a / (b * size));
    }


    // ----- Utilities -----

    // Destory children of inputted object
    // BUT skip the close button ;)
    void DestroyChildrenOf(GameObject obj) {
        foreach (Transform child in obj.transform) {
            if (child.name == "Close Button") { continue; }
            GameObject.Destroy(child.gameObject);
        }
    }

    // Gets all useful objects
    void GetObjects() {
        Info.InfoScriptObject = gameObject;
        Info.infoScript = this;
        closeButton = transform.Find("Close Button").GetComponent<Button>();
        CheckObjects(true);
    }

    // Returns true if everything is good
    // Returns false if any variables we need are bad
    bool CheckObjects(bool logNulls) {
        bool allGood = true;
        if (Info.InfoScriptObject == null) {
            if (logNulls) { Debug.Log("Info script object is null"); }
            allGood = false;
        }
        if (Info.infoScript == null) {
            if (logNulls) { Debug.Log("Info script is null"); }
            allGood = false;
        }
        if (closeButton == null) {
            if (logNulls) { Debug.Log("This button is missing"); }
            allGood = false;
        }
        return allGood;
    }
    bool CheckObjects() {
        return CheckObjects(false);
    }
}
