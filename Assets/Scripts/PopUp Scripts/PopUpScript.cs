using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Events;

public class PopUpScript : MonoBehaviour
{
    // ----- Variables -----

    [SerializeField] GameObject BlurCanvas;
    [SerializeField] GameObject PopUpPrefab;

    [Space(10f)]
    [SerializeField] float popUpWidthMultiplier;
    [SerializeField] float yesNoHeightMultiplier;
    [SerializeField] float fontSizeMultiplier;

    [Space(10f)]
    [SerializeField] List<GameObject> makeUninteractableOnClick;

    GameObject popUpMade;


    // ----- Monobehaviour stuff -----

    // Start is called before the first frame update
    void Start()
    {
        PopUp.SetPopUpHolder(gameObject);
        gameObject.transform.Find("Close Button").GetComponent<Button>().onClick.AddListener(() => {
            ClosePopUp();
        });
        BlurCanvas.SetActive(false);
        gameObject.SetActive(false);
    }


    // ----- PopUp stuff -----

    // Start the process (instantiate & set texts)
    public GameObject CreatePopUp(string title, string info, bool turnOffBlur) {
        // Turn off interactability of things we don't want interactable
        foreach (GameObject off in makeUninteractableOnClick) {
            CanvasGroup group = off.GetComponent<CanvasGroup>();
            if (group == null) {
                group = off.AddComponent<CanvasGroup>();
            }
            group.blocksRaycasts = false;
        }

        ActivateBlurImage(!turnOffBlur);

        // Turn on the canvases and tween them in
        Blur.BlurMaterial blurMaterial = Blur.FindMaterial(Blur.BlurType.UI);
        CanvasGroup popCanv = gameObject.GetComponent<CanvasGroup>();
        LeanTween.value(0f, 1f, Settings.AnimSpeedMultiplier)
        .setEase(LeanTweenType.easeInOutCubic)
        .setOnUpdate((float value) => {
            popCanv.alpha = value;
            blurMaterial.SetRadius(value);
        })
        .setOnComplete(() => {
            blurMaterial.SetRadius(1f);
        });

        BlurCanvas.SetActive(true);

        // Instantiate it
        GameObject obj = Instantiate(PopUpPrefab, PopUp.PopUpHolder);
        popUpMade = obj;

        // Set text fields
        TMP_Text text = obj.transform.Find("Info Area").Find("Info Area").GetComponent<TMP_Text>();
        text.text = info;
        fontSizeMultiplier = (fontSizeMultiplier == 0f) ? 1f : fontSizeMultiplier;
        text.fontSize = fontSizeMultiplier * Mathf.Min(Screen.width, Screen.height);
        // Title text
        text = obj.transform.Find("Info Area").Find("Title").GetComponent<TMP_Text>();
        text.text = title;
        text.fontSize = 1.25f * fontSizeMultiplier * Mathf.Min(Screen.width, Screen.height);

        // Set height of bottom buttons
        RectTransform bot = obj.transform.Find("Bottom Buttons").GetComponent<RectTransform>();
        yesNoHeightMultiplier = (yesNoHeightMultiplier == 0f) ? 1f : yesNoHeightMultiplier;
        bot.sizeDelta = new Vector2(0f, yesNoHeightMultiplier * Screen.height);

        // Set width of popUp
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(popUpWidthMultiplier * (float)Screen.width, rect.rect.height);

        StartCoroutine(CreatePopUpEnum(obj));

        return obj;
    }
    public GameObject CreatePopUp(string title, string info) {
        return CreatePopUp(title, info, false);
    }

    // Enum for setting sizes of stuff n shtuff
    IEnumerator CreatePopUpEnum(GameObject popUp) {
        for (int i = 0; i < 10; ++i) {
            yield return new WaitForEndOfFrame();
        }

        // Get rects
        RectTransform info = popUp.transform.Find("Info Area").Find("Info Area").GetComponent<RectTransform>();
        RectTransform infoArea = info.transform.parent.GetComponent<RectTransform>();
        RectTransform title = infoArea.transform.Find("Title").GetComponent<RectTransform>();

        // Set info text area
        TMP_Text infoText = info.gameObject.GetComponent<TMP_Text>();
        Debug.Log("infosize == " + infoText.textBounds.size.y);
        info.sizeDelta = new Vector2(0f, infoText.textBounds.size.y);

        // Set total info
        VerticalLayoutGroup vert = infoArea.gameObject.GetComponent<VerticalLayoutGroup>();
        float infoHeight = title.rect.height + info.sizeDelta.y + vert.padding.top + vert.padding.bottom + vert.spacing;
        infoArea.sizeDelta = new Vector2(0f, infoHeight);

        // Set popup height
        RectTransform rect = popUp.GetComponent<RectTransform>();
        RectTransform buttons = popUp.transform.Find("Bottom Buttons").GetComponent<RectTransform>();
        VerticalLayoutGroup popVert = popUp.GetComponent<VerticalLayoutGroup>();
        float rectHeight = infoArea.sizeDelta.y + buttons.rect.height + popVert.padding.top + popVert.padding.bottom + popVert.spacing;
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, rectHeight);
        rect.position = new Vector3( Screen.width / 2, Screen.height / 2, rect.position.z );
    }


    // Close a pop up menu with added onComplete action
    public void ClosePopUp(UnityAction onComplete) {
        // Turn off the canvases and tween them out
        Blur.BlurMaterial blurMaterial = Blur.FindMaterial(Blur.BlurType.UI);
        CanvasGroup popCanv = gameObject.GetComponent<CanvasGroup>();
        UnityAction complete = delegate {
            Object.Destroy(popUpMade);
            BlurCanvas.SetActive(false);
            gameObject.SetActive(false);
            if (GameManager.IsPaused) {
                GameManager.GameBoard.Resume();
            }
        };

        // Tween it
        LeanTween.value(1f, 0f, Settings.AnimSpeedMultiplier)
        .setEase(LeanTweenType.easeInOutCubic)
        .setOnUpdate((float value) => {
            blurMaterial.SetRadius(value);
            popCanv.alpha = value;
        })
        .setOnComplete(() => {
            complete.Invoke();
            if (onComplete != null) {
                onComplete.Invoke();
            }

            blurMaterial.SetRadius(0f);

            ActivateBlurImage(true);
        });

        // Turn on interactability of things we now want interactable
        foreach (GameObject off in makeUninteractableOnClick) {
            CanvasGroup group = off.GetComponent<CanvasGroup>();
            if (group == null) {
                group = off.AddComponent<CanvasGroup>();
            }
            group.blocksRaycasts = true;
        }
    }
    public void ClosePopUp() {
        ClosePopUp(null);
    }

    void ActivateBlurImage(bool turnOn) {
        Transform PopUpCanvas = PopUp.PopUpHolder;
        if (PopUpCanvas.GetComponentInChildren<Image>(true) != null) {
            Image img = PopUpCanvas.GetComponentInChildren<Image>(true);
            if (img.material != null && img.material.name.Contains("Blur")) {
                img.gameObject.SetActive(turnOn);
            }
        }
    }
}
