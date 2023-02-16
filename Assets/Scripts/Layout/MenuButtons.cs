using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SoundClip;

[ExecuteInEditMode]

public class MenuButtons : MonoBehaviour
{
    [SerializeField] GameObject MainMenuCanvas;
    [SerializeField] GameObject MenuButtonTweeningObject;
    [SerializeField] GameObject HeightMax, HeightMin;
    [SerializeField] GameObject MenuButtonHolder;
    [Range(0f, 30f)] [SerializeField] float barHeightMultiplier;
    [Range(0f, 3f )] [SerializeField] float squaredBarHeight;
    float menuButtonHeight;

    [SerializeField] public List<GameObject> children = new List<GameObject>();
    public List<GameObject> canvases;
    public List<GameObject> menus;
    [SerializeField] List<TMP_Text> makeTextSizeSameAsButtons;

    [SerializeField] float FadeCanvasInMult;
    [SerializeField] float FadeCanvasOutMult;
    [SerializeField] float FadeChildInMult;
    [SerializeField] float FadeChildOutMult;
    [SerializeField] float FadeOthersInMult;
    [SerializeField] float FadeOthersOutMult;

    public float OUT_BarHeight;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        CheckFadeMults();
        
        foreach (GameObject obj in canvases) {
            if (obj.name == "Settings Canvas") { continue; }
            obj.SetActive(false);
        }

        yield return new WaitForEndOfFrame();

        GetMenuButtonHolder();
        GetChildren();
        if (squaredBarHeight == 0f) {
            SetBarDimensions(barHeightMultiplier);
        } else {
            SetBarDimensions(squaredBarHeight * squaredBarHeight * squaredBarHeight);
        }
        SetMenuDimensions();
        StartCoroutine(UpdateFontSizes());
        SetBarColors();
    }

    void CheckFadeMults() {
        if (FadeCanvasInMult == 0f)  { FadeCanvasInMult = 0.01f; }
        if (FadeCanvasOutMult == 0f) { FadeCanvasOutMult = 0.01f; }
        if (FadeChildInMult == 0f)   { FadeChildInMult = 0.01f; }
        if (FadeChildOutMult == 0f)  { FadeChildOutMult = 0.01f; }
        if (FadeOthersInMult == 0f)  { FadeOthersInMult = 0.01f; }
        if (FadeOthersOutMult == 0f) { FadeOthersOutMult = 0.01f; }
    }


    // ----- Colors and dimensions -----

    void SetBarDimensions(float heightMult) {
        // Get size from one of this gameobjects children
        GetChildren();
        menuButtonHeight = children[0].GetComponent<RectTransform>().sizeDelta.y;
        
        float widthMult = ((float)Screen.width + 20f) / children[0].GetComponent<RectTransform>().sizeDelta.x;
        Vector3 scale = new Vector3(widthMult, heightMult, 1f);

        // Set each Bar in children to new_dimen
        int index = 0;
        foreach (Transform child in MenuButtonHolder.transform) {
            GameObject Bar = child.Find("Back Bar").gameObject;
            RectTransform rect = Bar.GetComponent<RectTransform>();
            rect.localScale = scale;
            rect.sizeDelta = new Vector2(0, 0);
            ++index;
        }
    }

    // Set dimensions of every menu
    void SetMenuDimensions() {
        for (int i = 0; i < menus.Count; ++i) {
            SetMenuDimensions(i);
        }
    }
    // Set dimensions of just one menu
    void SetMenuDimensions(int index) {
        GameObject menu = menus[index];
        Vector2 size;
        GameObject obj;
        float height;
        
        // Set the size of the total menu
        height = HeightMax.GetComponent<RectTransform>().position.y;
        height -= HeightMin.GetComponent<RectTransform>().position.y;
        size = new Vector2(menu.GetComponent<RectTransform>().sizeDelta.x, height);
        menu.GetComponent<RectTransform>().sizeDelta = size;

        // Set size and color of the bottom bar
        obj = menu.transform.Find("Bottom").gameObject;
        size = new Vector2(20f, 0.125f * menuButtonHeight * barHeightMultiplier);
        obj.GetComponent<RectTransform>().sizeDelta = size;
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        ImageTheme theme = obj.GetComponent<ImageTheme>();
        theme.color = children[index].transform.Find("Back Bar").GetComponent<ImageTheme>().color;

        // Set size of the bar/banner of the menu
        OUT_BarHeight = menuButtonHeight * barHeightMultiplier;
        obj = menu.transform.Find("Bar").gameObject;
        size = new Vector2(Screen.width+20, menuButtonHeight * barHeightMultiplier);
        obj.GetComponent<RectTransform>().sizeDelta = size;

        // Set the theme color of the bar/banner
        theme = obj.GetComponent<ImageTheme>();
        theme.color = children[index].transform.Find("Back Bar").GetComponent<ImageTheme>().color;

        // Set the position and size of the close button
        RectTransform rect = menus[index].transform.Find("Close Button").GetComponent<RectTransform>();
        float end_pos = HeightMax.GetComponent<RectTransform>().position.y;
        size = rect.anchoredPosition;
        size.y = 0.5f * (Screen.height - end_pos);
        rect.anchoredPosition = size;

        size = rect.sizeDelta;
        size.y = Screen.height - end_pos;
        rect.sizeDelta = size;

        // Set the font size of the title
        float font = children[index].transform.Find("Button").GetComponentInChildren<TMP_Text>().fontSizeMax;
        obj.GetComponentInChildren<TMP_Text>().fontSizeMax = font;

        // Set the size of the active area
        height -= obj.GetComponent<RectTransform>().sizeDelta.y;
        obj = menu.transform.Find("Active Area").gameObject;
        size = obj.GetComponent<RectTransform>().sizeDelta;
        size.y = height;
        obj.GetComponent<RectTransform>().sizeDelta = size;

        // Reset position of active area to 0
        size = obj.GetComponent<RectTransform>().anchoredPosition;
        size.y = 0;
        obj.GetComponent<RectTransform>().anchoredPosition = size;

        ++index;
    }

    IEnumerator UpdateFontSizes() {
        yield return new WaitForSeconds(0.25f);

        // Then, get the min font size of the new maxes
        float min_size = 10000f;
        foreach (Transform child in MenuButtonHolder.transform) {
            GameObject text_obj = child.Find("Button").Find("Text (TMP)").gameObject;
            float text_size = text_obj.GetComponent<TMP_Text>().fontSize;
            if (text_size < min_size) {
                min_size = text_size;
            }
        }

        // Finally, update all their font sizes to min_size
        foreach (Transform child in MenuButtonHolder.transform) {
            GameObject text_obj = child.Find("Button").Find("Text (TMP)").gameObject;
            text_obj.GetComponent<TMP_Text>().fontSizeMax = min_size;
        }

        SetMenuDimensions();

        // Set all texts in makeTextSizeSameAsButtons to the max too
        foreach (TMP_Text txt in makeTextSizeSameAsButtons) {
            txt.fontSizeMax = min_size;
        }
    }

    void SetBarColors() {
        int count = 0;
        foreach (Transform child in MenuButtonHolder.transform) {
            ImageTheme th = child.Find("Back Bar").GetComponent<ImageTheme>();
            th.color = ColorFromIndex(count);
            ++count;
        }
    }


    // ----- Utilities -----

    void GetMenuButtonHolder() {
        if (MenuButtonHolder == null) {
            MenuButtonHolder = gameObject;
        }
    }

    // Put all children in <children>
    // Set all of their onButtonPress to <MenuButtonClick(int)>
    // Set all of <menus>'s closeButton's onButtonPress to index
    void GetChildren() {
        children.Clear();
        foreach (Transform child in MenuButtonHolder.transform) {
            children.Add(child.gameObject);
        }
    }

    WhichColor ColorFromIndex(int index) {
        index = index % 6;
        switch (index) {
            case 0: return WhichColor.Color1;
            case 1: return WhichColor.Color2;
            case 2: return WhichColor.Color3;
            case 3: return WhichColor.Color4;
            case 4: return WhichColor.Color5;
            default: return WhichColor.Color6;
        }
    }


    // ----- Menu selection -----

    // Moves the main menu buttons to the side (for player info and app info)
    public void MoveButtons() {
        float childTime = FadeChildInMult * Settings.AnimSpeedMultiplier;
        // Tween buttons out
        for (int i = 0; i < children.Count; ++i) {
            GameObject child = children[i];
            Vector3 pos = child.GetComponent<RectTransform>().localPosition;
            pos.x = Screen.width;
            LeanTween.moveLocal(child, pos, childTime)
            .setEase(LeanTweenType.easeInOutQuart)
            .setOnComplete(() => {
                child.GetComponent<CanvasGroup>().alpha = 0.002f;
            });

            RectTransform rect = children[i].transform.Find("Back Bar").GetComponent<RectTransform>();
            Vector3 backPos = rect.localPosition;
            backPos.x = -(1.05f * Screen.width);
            rect.localPosition = backPos;
        }
    }

    // 1) Fade out main menu
    // 2) Fade in selection menu @ position of menu button
    // 3) Tween position of menu to top of the screen
    //      (but don't cover the top buttons!)
    public void MenuButtonPress(int index) {
        Sound.Play(SoundClip.tap_something);

        GameObject menu = menus[index];
        menu.SetActive(true);
        canvases[index].SetActive(true);

        SetImageScaleInChildren(menu);

        GameObject bar = children[index].transform.Find("Back Bar").gameObject;

        // Set initial position of menu
        SetPosition(menu, bar);
        SetMenuDimensions(index);

        float childTime = FadeChildInMult * Settings.AnimSpeedMultiplier;

        // Move button to center and logo to the left
        RectTransform logoRect = children[index].transform.Find("Logos").GetComponent<RectTransform>();
        RectTransform buttRect = children[index].transform.Find("Button").GetComponent<RectTransform>();

        // Get button and logo variables
        Vector3 buttPos = buttRect.position;
        buttPos.x = children[index].GetComponent<RectTransform>().position.x;
        Vector3 logoPos = logoRect.localPosition;
        if (Settings.LeftHanded) {
            logoPos.x = Screen.width;
        } else {
            logoPos.x = -Screen.width;
        }

        // Move the button to the center
        LeanTween.move(buttRect.gameObject, buttPos, Settings.AnimSpeedMultiplier * 0.33f)
        .setEase(LeanTweenType.easeInOutQuart)
        .setOnComplete(() => {
            CanvasGroup canv = children[index].GetComponent<CanvasGroup>();
            LeanTween.value(children[index], 1f, 0f, 0.33f * Settings.AnimSpeedMultiplier)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnStart(() => {
                if (canv == null) { canv = children[index].AddComponent<CanvasGroup>(); }
            })
            .setOnUpdate((float value) => {
                canv.alpha = value;
            });

            buttPos.y = HeightMax.GetComponent<RectTransform>().position.y;
            LeanTween.move(buttRect.gameObject, buttPos, childTime).setEase(LeanTweenType.easeInOutQuart);
        });

        // Move the logo to the left of the screen
        LeanTween.moveLocal(logoRect.gameObject, logoPos, 0.6f * childTime)
        .setEase(LeanTweenType.easeInSine);

        // Tween buttons out and selected button up
        for (int i = 0; i < children.Count; ++i) {
            if (i == index) { continue; }
            GameObject child = children[i];
            Vector3 pos = child.GetComponent<RectTransform>().localPosition;
            pos.x = Screen.width;
            LeanTween.moveLocal(child, pos, childTime)
            .setEase(LeanTweenType.easeInOutQuart)
            .setOnComplete(() => {
                child.GetComponent<CanvasGroup>().alpha = 0.002f;
            });
        }

        // Fade this canvas in
        CanvasGroup canv = canvases[index].GetComponent<CanvasGroup>();
        LeanTween.value(canvases[index], 0f, 1f, 0.66f * Settings.AnimSpeedMultiplier)
        .setEase(LeanTweenType.easeInOutQuart)
        .setOnStart(() => {
            if (canv == null) { canv = canvases[index].AddComponent<CanvasGroup>(); }
        })
        .setOnUpdate((float value) => {
            canv.alpha = value;
        });

        // Fade in blur
        Blur.BlurMaterial blurMaterial = Blur.FindMaterial(Blur.BlurType.Other);
        blurMaterial.SetRadius(0f);
        LeanTween.value(0f, 1f, 0.33f * Settings.AnimSpeedMultiplier)
        .setDelay(0.33f * Settings.AnimSpeedMultiplier)
        .setEase(LeanTweenType.easeInOutSine)
        .setOnUpdate((float value) => {
            blurMaterial.SetRadius(value * value);
        });

        // Move selected menu up (on a delay)
        Vector3 endPos = menu.GetComponent<RectTransform>().position;
        endPos.y = HeightMax.GetComponent<RectTransform>().position.y;

        LeanTween.move(menu, endPos, childTime)
        .setDelay(Settings.AnimSpeedMultiplier * 0.33f)
        .setEase(LeanTweenType.easeInOutQuart)
        .setOnStart(() => {
            Sound.Play(slide_menu);
        });
    }

    public void CloseMenu(int index) {
        CloseMenu(index, true);
    }
    public void CloseMenu(int index, bool moveButtonsBackIn) {
        // If this menu is not active, don't do anything
        if (!menus[index].activeInHierarchy) { return; }
        // If its already moving, don't do anything
        if (menus[index].LeanIsTweening()) { return; }

        Sound.Play(slide_menu);

        GameObject bar = children[index].transform.Find("Back Bar").gameObject;
        float end = GetPosition(bar);

        // Set up initial positions of everything
        foreach (GameObject obj in children) {
            obj.GetComponent<CanvasGroup>().alpha = 0.001f;
            obj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            Vector3 backPos = obj.transform.Find("Back Bar").GetComponent<RectTransform>().localPosition;
            backPos.x = -(1.05f * Screen.width);
            obj.transform.Find("Back Bar").GetComponent<RectTransform>().localPosition = backPos;
        }

        // Find position off the screen
        Vector3 pos = menus[index].GetComponent<RectTransform>().localPosition;
        pos.x = 1.05f * Screen.width;

        // Move selected menu to the right, off the screen
        LeanTween.moveLocal(menus[index], pos, Settings.AnimSpeedMultiplier)
        .setEase(LeanTweenType.easeInOutQuart)
        .setOnStart(() => {
            if (moveButtonsBackIn) {
                MenuButtonTweeningObject.GetComponent<MenuButtonsTweening>()
                .FadeInButtons();
            }
        })
        .setOnComplete(() => {
            canvases[index].SetActive(false);
            menus[index].GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        });
    }
    // Close ALL menus
    public void CloseMenu() {
        CloseMenu(true);
    }
    public void CloseMenu(bool moveButtonsBackIn) {
        for (int i = 0; i < menus.Count; ++i) {
            if (!canvases[i].activeSelf) { continue; }
            CloseMenu(i, moveButtonsBackIn);
        }
    }

    void SetPosition(GameObject menu, GameObject bar) {
        Vector3 pos = menu.GetComponent<RectTransform>().anchoredPosition;
        pos.y = GetPosition(bar);
        menu.GetComponent<RectTransform>().anchoredPosition = pos;
    }
    float GetPosition(GameObject bar) {
        float pos = bar.GetComponent<RectTransform>().position.y;
        pos += (bar.GetComponent<RectTransform>().rect.height / 2f);
        return pos;
    }

    void SetImageScaleInChildren(GameObject menu) {
        if (menu.GetComponent<ImageTheme>() != null) {
            menu.GetComponent<ImageTheme>().UpdatePPU();
        }

        foreach (Transform child in menu.transform) {
            SetImageScaleInChildren(child.gameObject);
        }
    }
}
