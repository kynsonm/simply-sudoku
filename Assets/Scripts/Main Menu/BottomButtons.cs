using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SoundClip;

public class BottomButtons : MonoBehaviour
{
    public GameObject BottomButtonHolder;

    // Profile Button
    public GameObject ProfileButton;
    public GameObject ProfileMenuCanvas;
    
    // Info Button
    public GameObject InfoButton;
    public GameObject InfoMenuCanvas;

    // Back Button
    public GameObject BackButton;

    // For sizing
    [Space(10f)]
    [SerializeField] RectTransform topPosReference;
    [SerializeField] RectTransform botPosReference;

    // Menu tweening
    [Space(10f)]
    [SerializeField] float animationMultiplier;
    [SerializeField] LeanTweenType easeOpenCurve, easeCloseCurve;

    // Tweening info
    [Space(10f)]
    [SerializeField] float rotationOffset;
    [SerializeField] LeanTweenType moveLeftEase, moveRightEase, resetEase;
    [SerializeField] int loopCount;
    [SerializeField] float animationTime;


    MenuButtons menuButtons;


    // Start is called before the first frame update
    void Start()
    {
        menuButtons = GameObject.FindObjectOfType<MenuButtons>();
        if (menuButtons == null) {
            Debug.Log("Menu Buttons could not be found");
        }

        SetOnClicks();

        StartCoroutine(TurnOffCanvases());
    }

    IEnumerator TurnOffCanvases() {
        ProfileMenuCanvas.SetActive(true);
        InfoMenuCanvas.SetActive(true);
        while (SavedLevels.GetSavedLevels() == null) {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        SetMenuDimensions();
        yield return new WaitForEndOfFrame();
        ProfileMenuCanvas.SetActive(false);
        InfoMenuCanvas.SetActive(false);
    }


    // Set on clicks for each button;
    void SetOnClicks() {
        Button butt = GetComponent<Button>(ProfileButton);
        GameObject obj1 = butt.gameObject;
        butt.onClick.AddListener(() => {
            ProfileButtonClick();
            StartCoroutine(TweenOnClick(obj1));
        });

        butt = GetComponent<Button>(InfoButton);
        GameObject obj2 = butt.gameObject;
        butt.onClick.AddListener(() => {
            InfoButtonClick();
            StartCoroutine(TweenOnClick(obj2));
        });

        butt = GetComponent<Button>(BackButton);
        GameObject obj3 = butt.gameObject;
        butt.onClick.AddListener(() => {
            BackButtonClick();
            StartCoroutine(TweenOnClick(obj3));
        });
    }

    // Tween button when clicked
    IEnumerator TweenOnClick(GameObject obj) {
        LeanTween.cancel(obj);
        float sequenceTime = animationTime / loopCount;

        float step1time = sequenceTime / 4f;
        float step2time = sequenceTime / 2f;
        float step3time = sequenceTime / 4f;

        for (int i = 0; i < loopCount; ++i) {
            LeanTween.rotateZ(obj, rotationOffset, step1time)
            .setEase(moveLeftEase)
            .setOnComplete(() => {
                LeanTween.rotateZ(obj, -rotationOffset, step2time)
                .setEase(moveRightEase)
                .setOnComplete(() => {
                    LeanTween.rotateZ(obj, 0f, step3time)
                    .setEase(resetEase)
                    .setOnComplete(() => {
                        obj.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
                    });
                });
            });
            yield return new WaitForSeconds(sequenceTime);
        }
    }


    // ----- ON CLICKS -----

    void Click(GameObject canvas, GameObject button, GameObject otherCanvas, GameObject otherButton) {
        GameObject menu = canvas.transform.Find("Menu").gameObject;
        if (menu.LeanIsTweening()) {
            return;
        }
        animationMultiplier = (animationMultiplier == 0f) ? 1f : animationMultiplier;

        if (otherCanvas.activeInHierarchy) {
            CloseMenu(otherCanvas.transform.Find("Menu").gameObject, otherButton);
        }

        if (ProfileMenuCanvas.activeInHierarchy) {
            CloseMenu(menu, button);
        } else {
            ProfileMenuCanvas.SetActive(true);
            OpenMenu(menu, button);
        }
    }

    // Pull up profile info menu
    void ProfileButtonClick() {
        GameObject menu = ProfileMenuCanvas.transform.Find("Menu").gameObject;
        GameObject other = InfoMenuCanvas.transform.Find("Menu").gameObject;

        if (menu.LeanIsTweening() || other.LeanIsTweening()) {
            return;
        }
        animationMultiplier = (animationMultiplier == 0f) ? 1f : animationMultiplier;

        Sound.Play(tap_something);

        if (InfoMenuCanvas.activeInHierarchy) {
            CloseMenu(other, InfoButton);
        }

        if (ProfileMenuCanvas.activeInHierarchy) {
            CloseMenu(menu, ProfileButton);
            GameObject.FindObjectOfType<MenuButtonsTweening>().FadeInButtons();
        } else {
            ProfileMenuCanvas.SetActive(true);
            menu.transform.Find("Bar").GetComponentInChildren<TextTheme>().ResetTextSize();
            OpenMenu(menu, ProfileButton);
        }
    }

    // Do info sequence to tell everybody information idk
    void InfoButtonClick() {
        GameObject menu = InfoMenuCanvas.transform.Find("Menu").gameObject;
        GameObject other = ProfileMenuCanvas.transform.Find("Menu").gameObject;

        if (menu.LeanIsTweening() || other.LeanIsTweening()) {
            return;
        }
        animationMultiplier = (animationMultiplier == 0f) ? 1f : animationMultiplier;

        Sound.Play(tap_something);

        if (ProfileMenuCanvas.activeInHierarchy) {
            CloseMenu(other, ProfileButton);
        }

        if (InfoMenuCanvas.activeInHierarchy) {
            CloseMenu(menu, InfoButton);
            GameObject.FindObjectOfType<MenuButtonsTweening>().FadeInButtons();
        } else {
            InfoMenuCanvas.SetActive(true);
            menu.transform.Find("Bar").GetComponentInChildren<TextTheme>().ResetTextSize();
            OpenMenu(menu, InfoButton);
        }
    }


    Vector3 defaultMenuPosition;

    void MoveMenu(GameObject menu, Vector3 buttonPosition, bool isOpening) {
        // Rect transform and CanvasGroup
        RectTransform rect = menu.GetComponent<RectTransform>();
        CanvasGroup canv = menu.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = menu.AddComponent<CanvasGroup>();
        }

        // Set position of the content
        if (isOpening) {
            RectTransform content = menu.transform.Find("Active Area").Find("Content").GetComponent<RectTransform>();
            content.localPosition = new Vector2(content.localPosition.x, 0f);
        }

        // Vars for tween time
        float time = animationMultiplier * Settings.AnimSpeedMultiplier;
        float delay = 0f;
        if (!isOpening) { time *= animationMultiplier; }
        else            { delay = 0.1f; }

        // Other vars
        rect.localScale = isOpening ? new Vector3(0f, 0f, 1f) : rect.localScale;
        Vector3 startPos = isOpening ? buttonPosition : defaultMenuPosition;
        Vector3 endPos = isOpening ? defaultMenuPosition : buttonPosition;
        Vector3 offset = endPos - startPos;
        LeanTweenType easeCurve = isOpening ? easeOpenCurve : easeCloseCurve;

        // Tween scale and position from profile button
        LeanTween.cancel(menu);
        LeanTween.value(menu, 0f, 1f, animationMultiplier * Settings.AnimSpeedMultiplier)
        .setEase(easeCurve)
        .setDelay(delay)
        .setOnStart(() => {
            rect.position = startPos;
        })
        .setOnUpdate((float value) => {
            // Find scale and alpha
            float scale = 0f;
            float alpha = 0f;
            if (isOpening) {
                scale = value;
                alpha = value * value;
            } else {
                scale = 1f - value;
                alpha = (1f - value) * (1f - value);
            }

            // Set scale, alpha, and position
            rect.localScale = new Vector3(scale, scale, 1f);
            canv.alpha = alpha;
            rect.position = startPos + value * offset;
        })
        .setOnComplete(() => {
            // Make sure that position and scale is good when things are done
            RectTransformOffset.Sides(rect, 0f);
            rect.position = defaultMenuPosition;
            rect.localScale = new Vector3(1f, 1f, 1f);
            if (!isOpening) {
                menu.transform.parent.gameObject.SetActive(false);
            }
        });
    }

    void OpenMenu(GameObject menu, GameObject button) {
        menuButtons.CloseMenu(false);
        menuButtons.MoveButtons();
        MoveMenu(menu, button.GetComponent<RectTransform>().position, true);
        PlaySound(open_menu);
    }

    void CloseMenu(GameObject menu, GameObject button) {
        if (!menu.activeInHierarchy) { return; }
        PlaySound(close);
        MoveMenu(menu, button.GetComponent<RectTransform>().position, false);
    }

    public void CloseMenus() {
        if (ProfileMenuCanvas.activeInHierarchy) {
            CloseMenu(ProfileMenuCanvas.transform.Find("Menu").gameObject, ProfileButton);
        }
        if (InfoMenuCanvas.activeInHierarchy) {
            CloseMenu(InfoMenuCanvas.transform.Find("Menu").gameObject, InfoButton);
        }
        GameObject.FindObjectOfType<MenuButtonsTweening>().FadeInButtons();
    }

    AudioSource audioSource;
    public void PlaySound(SoundClip soundClip) {
        if (audioSource != null) { return; }
        audioSource = Sound.Play(soundClip);
    }


    // Go either close the menu or go back, depending on which
    //   menu is currently active
    public void BackButtonClick() {
        if (GetComponent<Button>(BackButton).gameObject.LeanIsTweening()
            || GetComponent<Button>(ProfileButton).gameObject.LeanIsTweening()
            || GetComponent<Button>(InfoButton).gameObject.LeanIsTweening())
        {
            Sound.Play(tap_nothing);
            return;
        }

        // Close info and player menus
        bool theseOpen = false;
        if (InfoMenuCanvas.activeSelf) {
            CloseMenu(InfoMenuCanvas.transform.Find("Menu").gameObject, InfoButton);
            theseOpen = true;
        }
        if (ProfileMenuCanvas.activeSelf) {
            CloseMenu(ProfileMenuCanvas.transform.Find("Menu").gameObject, ProfileButton);
            theseOpen = true;
        }
        if (theseOpen) {
            GameObject.FindObjectOfType<MenuButtonsTweening>().FadeInButtons();
            Sound.Play(tap_something);
            return;
        }

        // Find canvas currently on
        string name = "nah";
        int index = -1;
        for (int i = 0; i < menuButtons.canvases.Count; ++i) {
            GameObject menu = menuButtons.canvases[i];
            if (menu.activeSelf) {
                if (name != "nah") {
                    Debug.Log("More than one canvas is on!");
                }
                name = menu.name;
                index = i;
            }
        }
        if (name == "nah" || index == -1) {
            Debug.Log("No canvas is on!");
            Sound.Play(tap_nothing);
            return;
        }

        Sound.Play(tap_something);

        // If menu selected is Play Menu
        if (name.Contains("Play")) {
            GameObject controller = GameObject.Find("Play Menu Controller");
            if (controller == null) {
                Debug.Log("Play controller is null");
                return;
            }

            LevelSelectMenu menu = controller.GetComponent<LevelSelectMenu>();
            if (menu == null) {
                Debug.Log("Play menu level select is null");
            }

            menu.GoBack();
            return;
        }

        // If menu selected is Quickplay
        if (name.Contains("play")) {
            GameObject controller = GameObject.Find("Quickplay Menu Controller");
            if (controller == null) {
                Debug.Log("Quickplay controller is null");
                return;
            }

            LevelSelectMenu menu = controller.GetComponent<LevelSelectMenu>();
            if (menu == null) {
                Debug.Log("Quickplay menu level select is null");
            }

            menu.GoBack();
            return;
        }

        menuButtons.CloseMenu();
    }    


    // ----- UTILITIES -----

    // Get component recursivley in children
    T GetComponent<T>(GameObject obj) {
        T component = obj.GetComponent<T>();
        if (component != null) {
            return component;
        }

        foreach (Transform trans in obj.transform) {
            component = GetComponent<T>(trans.gameObject);
            if (component != null) {
                return component;
            }
        }

        if (component == null) {
            Debug.Log("Could not find component " + nameof(T) + " in object " + obj.name);
        }

        return component;
    }

    // Get button object and check if it is null
    Button GetButton(GameObject obj) {
        Button butt = obj.GetComponent<Button>();
        if (butt == null) {
            butt = obj.GetComponentInChildren<Button>();
            if (butt == null) {
                Debug.Log("Could not find button on " + nameof(ProfileButton));
            } else {
                Debug.Log("Got button from children on " + nameof(ProfileButton));
            }
        }
        return butt;
    }


    // ----- DIMENSIONS -----

    // Sets dimensions and positions of game info and player info canvases
    void SetMenuDimensions() {
        SetMenuDimensions(ProfileMenuCanvas, WhichColor.Color5);
        SetMenuDimensions(InfoMenuCanvas, WhichColor.Color6);
    }
    void SetMenuDimensions(GameObject canvas, WhichColor color) {
        // Get menu height/position from top/bot references
        RectTransform menuRect = canvas.transform.Find("Menu").GetComponent<RectTransform>();
        float top = topPosReference.anchoredPosition.y;
        float bot = botPosReference.anchoredPosition.y;
        menuRect.sizeDelta = new Vector2(menuRect.sizeDelta.x, top-bot);
        menuRect.anchoredPosition = new Vector2(0f, botPosReference.position.y);
        defaultMenuPosition = new Vector3(menuRect.position.x, menuRect.position.y, menuRect.position.z);

        // Get title height from play canvas
        RectTransform titleRect = menuRect.transform.Find("Bar").GetComponent<RectTransform>();
        float height = menuButtons.OUT_BarHeight;
        titleRect.sizeDelta = new Vector2(menuRect.rect.width, height);

        // Get bottom bar height from 1/8th title size
        RectTransform botRect = menuRect.transform.Find("Bottom").GetComponent<RectTransform>();
        botRect.sizeDelta = new Vector2(botRect.sizeDelta.x, 0.125f * height);

        // Set size of active area
        menuRect = menuRect.transform.Find("Active Area").GetComponent<RectTransform>();
        menuRect.sizeDelta = new Vector2(menuRect.sizeDelta.x, top-bot - height);

        // Need to set colors too
        // of bar and bottom bar
        ImageTheme titleTheme = titleRect.GetComponent<ImageTheme>();
        titleTheme.color = color;
        titleTheme.Reset();
        ImageTheme botTheme = botRect.GetComponent<ImageTheme>();
        botTheme.color = color;
        botTheme.Reset();
    }
}
 