using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using static SoundClip;

public class SettingsMenu : MonoBehaviour
{
    // ----- VARIABLES -----

    public Transform SettingsHolder;
    public GameObject TitleObject;

    [Space(10f)]
    public GameObject sliderObject;
    public GameObject onOffObject;
    public GameObject optionsObject;
    [Space(5f)]
    public GameObject themeObject;
    public GameObject themeButtonPrefab;
    public GameObject backgroundButtonPrefab;

    [Space(10f)]
    public float SettingTitleDivider;
    public float SettingHeightDivider;
    public float TitleOptionPaddingDivider;
    float titleHeight, settingHeight, padding, stuffHeight;


    // ----- METHODS -----

    // Create all settings and set size
    public void CreateSettings() {
        // Destory whats already in there
        foreach (Transform child in SettingsHolder) {
            GameObject.Destroy(child.gameObject);
        }

        // Cehck vars so we don't get any infinities
        SettingHeightDivider = (SettingHeightDivider == 0) ? 1f : SettingHeightDivider;
        SettingTitleDivider = (SettingTitleDivider == 0) ? 1f : SettingTitleDivider;
        TitleOptionPaddingDivider = (TitleOptionPaddingDivider == 0) ? 1f : TitleOptionPaddingDivider;

        // Find useful vars
        settingHeight = Screen.height / SettingHeightDivider;
        titleHeight = settingHeight / SettingTitleDivider;
        padding = titleHeight / TitleOptionPaddingDivider;

        titleHeight -= 0.5f * padding;
        stuffHeight = settingHeight - titleHeight - (0.5f * padding);

        // Create all the settings objects
        CreateSound();
        CreateGameplay();
        CreateAccessibility();
        CreateLooks();
        CreateOther();

        // Set size of the scroll's content
        StartCoroutine(SetSize());
    }
    IEnumerator SetSize() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Get total size of the content
        float size = 0f;
        foreach (Transform child in SettingsHolder) {
            size += child.GetComponent<RectTransform>().sizeDelta.y;
        }

        VerticalLayoutGroup vert = SettingsHolder.GetComponent<VerticalLayoutGroup>();
        size += (SettingsHolder.childCount-1) * vert.spacing;
        size += vert.padding.top + vert.padding.bottom;
        
        // And set its size
        RectTransform rect = SettingsHolder.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, size);

        // Reset position of ScrollView
        RectTransform scrollView = rect.transform.parent.GetComponent<RectTransform>();
        scrollView.anchoredPosition = new Vector2(0f, 0f);

        yield return new WaitForEndOfFrame();

        // Make all the texts the same size
        SameTextSize sameText = SettingsHolder.gameObject.AddComponent<SameTextSize>();
        sameText.parentNamesToSkip = new List<string>();
        sameText.parentNamesToSkip.Add("Themes");
        sameText.parentNamesToSkip.Add("Fonts");
        sameText.parentNamesToSkip.Add("Text");
        sameText.parentNamesToSkip.Add("Backgrounds");
        sameText.Reset();

        yield return new WaitForEndOfFrame();
        enumRunning = false;
        allSettingsMade = true;

        //TurnOffLayoutGroups(SettingsHolder);
    }

    // Creates all sound settings
    // Master vol, music vol, sound effect vol
    void CreateSound() {
        // Make the title
        MakeTitle("Sound");

        // Master volume slider
        Slider slider1 = SetSlider("Master Volume", Settings.MasterVolume, 
        "Controls the total volume of the game\n<i>Multiplies other volume settings", 
        (float value) => {
            Settings.MasterVolume = value;
            Sound.UpdateMusicVolume();
        });

        // Music volume slider
        Slider slider2 = SetSlider("Music Volume", Settings.MusicVolume,
        "Controls how loud the music is",
        (float value) => {
            Settings.MusicVolume = value;
            Sound.UpdateMusicVolume();
        });

        // Sound effect volume slider
        Slider slider3 = SetSlider("Effect Volume", Settings.SoundVolume,
        "Controls how loud the sound effects are",
        (float value) => {
            Settings.SoundVolume = value;
        });
    }

    // Create optoins that have to do with how the game looks
    // Active background, theme, font, anim speed multiplier, target fps
    void CreateLooks() {
        // Make the title
        MakeTitle("Looks");

        // Open shop for themes
        var openMenuObjs = SetOnOff("Want more themes or coins?", "");
        openMenuObjs.Item2.SetActive(false);
        openMenuObjs.Item3.SetActive(false);
        openMenuObjs.Item1.onClick.RemoveAllListeners();
        openMenuObjs.Item1.onClick.AddListener(() => {
            ShopMenu shopMenu = GameObject.FindObjectOfType<ShopMenu>();
            if (shopMenu != null) { shopMenu.TurnOn(); }
        });
        openMenuObjs.Item1.GetComponentInChildren<TMP_Text>(true).text = "Open Shop!";
        ImageTheme imgTheme = openMenuObjs.Item1.gameObject.GetComponent<ImageTheme>();
        imgTheme.UseColor = true;
        imgTheme.color = WhichColor.Color4;

        // Theme and Font
        StartCoroutine(MakeThemes());

        // Anim speed
        Slider slider = SetSlider("Animation Time", Settings.AnimSpeedMultiplier,
        "Controls how fast the game's aniamtions are.\n<i>Left is faster and right is slower!</i>",
        (float value) => {
            Settings.AnimSpeedMultiplier = value;
        });
        slider.minValue = 0.1f;

        // Post processing strength slider
        /*
        Volume[] volumes = GameObject.FindObjectsOfType<Volume>(true);
        Volume backBlur = GameObject.FindObjectOfType<BackgroundAnimator>().gameObject.GetComponentInChildren<Volume>();
        Slider volStr = SetSlider("Post Processing Strength", Settings.PostProcessingStrength,
        "Changes how strong the post-processing effects are\n<i>For example, bloom and vignette</i>",
        (float value) => {
            Settings.PostProcessingStrength = value;
            foreach (Volume volume in volumes) {
                if (volume == backBlur) { continue; }
                volume.weight = value;
            }
        });
        volStr.onValueChanged.Invoke(Settings.PostProcessingStrength);
        */

        // FPS
        GameObject obj = SetOptions("Target FPS",
        "Controls how smoothly the game looks overall. Higher is better!\n<i>Note: Higher values will cause your battery to drain faster</i>",
        new int[]{30, 45, 60, 90, 120, 144, 165, 10000},
        (int value) => {
            Framerate.SetFramerate(value);
        });

        // Render scale
        /*
        GameObject scale = SetOptions("Game Render Scale",
        "Constrols the target resolution of the game. Anything lower than 1 will improve performance at the cost of visuals",
        new float[]{0.25f, 0.33f, 0.5f, 0.75f, 0.9f, 1f, 2f, 3f},
        (float value) => {
            Resolution.SetResolution(value);

            // idk what to do here
            SceneLoader.LoadScene(SceneLoader.Scene.MainMenu);
        });
        */
    }

    // Creates all gameplay settings
    // Highlight tiles, show timer, resizable board
    void CreateGameplay() {
        // Make the title
        MakeTitle("Gameplay");

        // Highlight tiles
        Tuple<Button, GameObject, GameObject> objs1;
        objs1 = SetOnOff("Highlight Game Tiles",
        "If on, this will highlight all game tiles that \"interact\" with the selected tile. In other words, it also highlights all the tiles in the row, column, and box");
        objs1.Item1.onClick.AddListener(() => {
            Settings.HighlightTiles = !Settings.HighlightTiles;
            if (Settings.HighlightTiles) {
                objs1.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn off highlights";
                objs1.Item2.SetActive(false);
                objs1.Item3.SetActive(true);
            } else {
                objs1.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn on highlights";
                objs1.Item2.SetActive(true);
                objs1.Item3.SetActive(false);
            }
        });
        objs1.Item1.onClick.Invoke();
        objs1.Item1.onClick.Invoke();

        // Show edit numbers in game
        EditButtons editButtons = GameObject.FindObjectOfType<EditButtons>();
        Tuple<Button, GameObject, GameObject> objs4 = SetOnOff("Show Edit Buttons",
        "If on, this will show a separate set of numbers only for placing edits.\n" +
        "If off, you will need to turn on \"Edit mode\" before placing edits.\n" +
        "<i>Note: If you are in a game, this will reload the scene</i>");
        objs4.Item1.onClick.AddListener(() => {
            if (editButtons != null) {
                editButtons.Activate();
            } else {
                Settings.ShowEditNumbers = !Settings.ShowEditNumbers;
            }
            if (Settings.ShowEditNumbers) {
                objs4.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn off edit buttons";
                objs4.Item2.SetActive(false);
                objs4.Item3.SetActive(true);
            } else {
                objs4.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn on edit buttons";
                objs4.Item2.SetActive(true);
                objs4.Item3.SetActive(false);                
            }
        });
        objs4.Item1.onClick.Invoke();
        objs4.Item1.onClick.Invoke();
        EditButtons.UpdatedEditButtons = false;

        // Alternate game board colors
        Tuple<Button, GameObject, GameObject> objs5 = SetOnOff("Alternate Game Board Colors",
        "If on, this will alternate the colors of each box on the board.\n" +
        "If off, the entire board will just be one color.\n" +
        "This makes the board easier to interpret, overall");
        objs5.Item1.onClick.AddListener(() => {
            Settings.AlternateGameBoardColors = !Settings.AlternateGameBoardColors;
            if (GameManager.GameBoard != null && GameManager.GameBoard.Rows() > 0) { 
                Debug.LogWarning("RESETTING GAME BOARD THEMES");
                GameManager.GameBoard.ResetThemes();
            }
            if (Settings.AlternateGameBoardColors) {
                objs5.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn off alternate colors";
                objs5.Item2.SetActive(false);
                objs5.Item3.SetActive(true);
            } else {
                objs5.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn on alternate colors";
                objs5.Item2.SetActive(true);
                objs5.Item3.SetActive(false);                
            }
        });
        objs5.Item1.onClick.Invoke();
        objs5.Item1.onClick.Invoke();

        // Show timer
        Tuple<Button, GameObject, GameObject> objs2 = SetOnOff("Show Timer",
        "If on, this will show a timer while you are in game to visually keep track of how long it has taken you");
        objs2.Item1.onClick.AddListener(() => {
            Settings.ShowTimer = !Settings.ShowTimer;
            if (Settings.ShowTimer) {
                objs2.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn off timer";
                objs2.Item2.SetActive(false);
                objs2.Item3.SetActive(true);
            } else {
                objs2.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn on timer";
                objs2.Item2.SetActive(true);
                objs2.Item3.SetActive(false);
            }
        });
        objs2.Item1.onClick.Invoke();
        objs2.Item1.onClick.Invoke();

        // Resizable game board
        Tuple<Button, GameObject, GameObject> objs3 = SetOnOff("Resizeable Game Board",
        "If on, you will allow you to be able to pinch-zoom the game board. This makes it easier to see smaller numbers");
        objs3.Item1.onClick.AddListener(() => {
            Settings.ResizableGameBoard = !Settings.ResizableGameBoard;
            if (Settings.ResizableGameBoard) {
                objs3.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn off";
                objs3.Item2.SetActive(false);
                objs3.Item3.SetActive(true);
            } else {
                objs3.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn on";
                objs3.Item2.SetActive(true);
                objs3.Item3.SetActive(false);
            }
        });
        objs3.Item1.onClick.Invoke();
        objs3.Item1.onClick.Invoke();
    }

    // Creates all accessibility settings
    // Left handed, resizable menus
    void CreateAccessibility() {
        // Make the title
        MakeTitle("Accessibility");

        // Resizable menus
        Tuple<Button, GameObject, GameObject> objs1 = SetOnOff("Resizable menus",
        "If on, you can pinch-zoom many menus to make them easier to read");
        objs1.Item1.onClick.AddListener(() => {
            Settings.ResizableMenus = !Settings.ResizableMenus;
            if (Settings.ResizableMenus) {
                objs1.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn off";
                objs1.Item2.SetActive(false);
                objs1.Item3.SetActive(true);
            } else {
                objs1.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn on";
                objs1.Item2.SetActive(true);
                objs1.Item3.SetActive(false);
            }
        });
        objs1.Item1.onClick.Invoke();
        objs1.Item1.onClick.Invoke();

        // Left handed
        Tuple<Button, GameObject, GameObject> objs2 = SetOnOff("Left Handed Mode",
        "If on, certain menus will be flipped to allow easier access when using your left hand to click things");
        objs2.Item1.onClick.AddListener(() => {
            Settings.LeftHanded = !Settings.LeftHanded;
            if (Settings.LeftHanded) {
                objs2.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn off";
                objs2.Item2.SetActive(false);
                objs2.Item3.SetActive(true);
            } else {
                objs2.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn on";
                objs2.Item2.SetActive(true);
                objs2.Item3.SetActive(false);
            }
        });
        objs2.Item1.onClick.Invoke();
        objs2.Item1.onClick.Invoke();
    }

    // Creates all other settings
    // nothing yet
    void CreateOther() {

    }


    // ----- INDIVIDUAL OPTION CREATION -----

    // Set options area
    GameObject SetOptions(string title, string info, int[] values, UnityAction<int> onClick) {
        GameObject obj = Instantiate(optionsObject, SettingsHolder);
        StartCoroutine(SetOptionsEnum(obj, title, info, values, onClick));
        return obj;
    }
    GameObject SetOptions(string title, string info, float[] values, UnityAction<float> onClick) {
        GameObject obj = Instantiate(optionsObject, SettingsHolder);
        StartCoroutine(SetOptionsEnum(obj, title, info, values, onClick));
        return obj;
    }
    IEnumerator SetOptionsEnum(GameObject obj, string title, string info, int[] values, UnityAction<int> onClick) {
        // Instantiate onOff
        obj.name = title;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, settingHeight);

        // Set text size
        RectTransform textRect = obj.transform.Find("Title").GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(0f, titleHeight);
        textRect.anchoredPosition = new Vector2(0f, 0f);
        textRect.gameObject.GetComponent<TMP_Text>().text = title;

        // Set info
        textRect.GetComponentInChildren<InfoButton>().message = info;

        // Make option objects
        Transform optionsParent = obj.transform.Find("Options");
        GameObject option = optionsParent.GetChild(0).gameObject;
        for (int i = 0; i < values.Length; ++i) {
            // Make each option
            GameObject optionObj = Instantiate(option, optionsParent);
            optionObj.name = values[i].ToString();
            // Set text
            string text = values[i].ToString();
            if (values[i] > 170) { text = "165+"; }
            optionObj.GetComponentInChildren<TMP_Text>().text = text;
            // Set on click
            Button butt = optionObj.GetComponent<Button>();
            int value = values[i];
            butt.onClick.RemoveAllListeners();
            butt.onClick.AddListener(() => {
                onClick.Invoke(value);
                Sound.Play(tap_something);
            });
            // Update image theme stuff
            optionObj.GetComponent<ImageTheme>().Reset(3);
        }

        // Destroy original option
        Destroy(option);

        yield return new WaitForEndOfFrame();
        float size = optionsParent.GetComponent<GridEditor>().ResetSizes();

        Debug.Log($"Size == {textRect.rect.height} + {size} + {padding} = {textRect.rect.height + size + padding}");

        // Set sizes
        rect.sizeDelta = new Vector2(0f, textRect.rect.height + size + padding);
        
        // Reset text sizes
        yield return new WaitForEndOfFrame();
        foreach (Transform child in optionsParent) {
            child.GetComponentInChildren<TextTheme>().ResetTextSize();
        }
    }

    IEnumerator SetOptionsEnum(GameObject obj, string title, string info, float[] values, UnityAction<float> onClick) {
        // Instantiate onOff
        obj.name = title;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, settingHeight);

        // Set text size
        RectTransform textRect = obj.transform.Find("Title").GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(0f, titleHeight);
        textRect.anchoredPosition = new Vector2(0f, 0f);
        textRect.gameObject.GetComponent<TMP_Text>().text = title;

        // Set info
        textRect.GetComponentInChildren<InfoButton>().message = info;

        // Make option objects
        Transform optionsParent = obj.transform.Find("Options");
        GameObject option = optionsParent.GetChild(0).gameObject;
        for (int i = 0; i < values.Length; ++i) {
            // Make each option
            GameObject optionObj = Instantiate(option, optionsParent);
            optionObj.name = values[i].ToString();
            // Set text
            string text = values[i].ToString();
            optionObj.GetComponentInChildren<TMP_Text>().text = text;
            // Set on click
            Button butt = optionObj.GetComponent<Button>();
            float value = values[i];
            butt.onClick.RemoveAllListeners();
            butt.onClick.AddListener(() => {
                Sound.Play(tap_something);
                onClick.Invoke(value);
            });
            // Update image theme stuff
            optionObj.GetComponent<ImageTheme>().Reset(3);
        }

        // Destroy original option
        Destroy(option);

        yield return new WaitForEndOfFrame();
        float size = optionsParent.GetComponent<GridEditor>().ResetSizes();

        Debug.Log($"Size == {textRect.rect.height} + {size} + {padding} = {textRect.rect.height + size + padding}");

        // Set sizes
        rect.sizeDelta = new Vector2(0f, textRect.rect.height + size + padding);
        
        // Reset text sizes
        yield return new WaitForEndOfFrame();
        foreach (Transform child in optionsParent) {
            child.GetComponentInChildren<TextTheme>().ResetTextSize();
        }
    }


    // Set activation setting
    Tuple<Button, GameObject, GameObject> SetOnOff(string title, string info) {
        // Instantiate onOff
        GameObject obj = Instantiate(onOffObject, SettingsHolder);
        obj.name = title;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, settingHeight);

        // Set text size
        RectTransform textRect = obj.transform.Find("Title").GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(0f, titleHeight);
        textRect.anchoredPosition = new Vector2(0f, 0f);
        textRect.gameObject.GetComponent<TMP_Text>().text = title;

        // Set button size
        Transform activate = obj.transform.Find("Activate");
        RectTransform buttonRect = activate.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0f, stuffHeight);
        buttonRect.anchoredPosition = new Vector2(0f, 0f);

        // Set info stuff
        InfoButton infoButt = textRect.transform.GetComponentInChildren<InfoButton>();
        infoButt.message = info;
        if (info.Length == 0) {
            infoButt.gameObject.SetActive(false);
        }

        // Set button action
        Button butt = activate.GetComponentInChildren<Button>();
        butt.onClick.AddListener(() => { Sound.Play(tap_something); });
        GameObject x = activate.Find("X").gameObject;
        GameObject check = activate.Find("Check").gameObject;
        Tuple<Button, GameObject, GameObject> objs = new Tuple<Button, GameObject, GameObject>(butt, x, check);
        return objs;
    }


    // Set a slider with <title> to do <valueChange> when changed
    Slider SetSlider(string title, float value, string info, UnityAction<float> valueChange) {
        // Instantiate slider
        GameObject obj = Instantiate(sliderObject, SettingsHolder);
        obj.name = title;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, settingHeight);

        // Set text size
        RectTransform textRect = obj.transform.Find("Title").GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(0f, titleHeight);
        textRect.anchoredPosition = new Vector2(0f, 0f);
        textRect.gameObject.GetComponent<TMP_Text>().text = title;

        // Set slider size
        RectTransform sliderRect = obj.transform.Find("Slider").GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(0f, stuffHeight);
        sliderRect.anchoredPosition = new Vector2(0f, 0f);
        RectTransformOffset.Sides(sliderRect, titleHeight);

        // Set info button
        InfoButton infoButt = obj.transform.Find("Title").GetComponentInChildren<InfoButton>();
        infoButt.message = info;

        // Set slider onValueChanged
        Slider slider = sliderRect.GetComponent<Slider>();
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener((float vlaue) => {
            valueChange.Invoke(vlaue);
            //Sound.Play(drag_menu);
        });
        slider.value = value;
        slider.onValueChanged.Invoke(value);

        return slider;
    }

    // Make theme settings
    IEnumerator MakeThemes() {
        // Make them and the objects
        GameObject theme = Instantiate(themeObject, SettingsHolder);
        theme.name = "Themes";
        GameObject font = Instantiate(themeObject, SettingsHolder);
        font.name = "Fonts";

        // Background holder
        GameObject back = Instantiate(themeObject, SettingsHolder);
        back.name = "Backgrounds";

        // Active background
        Tuple<Button, GameObject, GameObject> objs = SetOnOff("Active Background", 
        "Turn on and off the moving objects in the background");
        objs.Item1.onClick.AddListener(() => {
            Settings.BackgroundActive = GameObject.FindObjectOfType<BackgroundAnimator>().ActivateBackground();
            if (Settings.BackgroundActive) {
                objs.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn off background";
                objs.Item2.SetActive(false);
                objs.Item3.SetActive(true);
                GameObject.FindObjectOfType<BackImageMover>().Resume();
            } else {
                objs.Item1.gameObject.GetComponentInChildren<TMP_Text>().text = "Turn on background";
                objs.Item2.SetActive(true);
                objs.Item3.SetActive(false);
                GameObject.FindObjectOfType<BackImageMover>().Pause();
            }
        });
        objs.Item1.onClick.Invoke();
        objs.Item1.onClick.Invoke();

        // Change background blur slider
        Blur.BlurMaterial blurMaterial = Blur.FindMaterial(Blur.BlurType.Background);
        Slider blur = SetSlider("Background Blur", Settings.BackgroundBlur,
        "Changes how blurry the background image is",
        (float value) => {
            blurMaterial.SetRadius(value);
            Settings.BackgroundBlur = value;
        });

        yield return new WaitForEndOfFrame();

        float themeHeight = CreateThemeButtons.MakeThemeButtons(theme.transform.Find("Options"), themeButtonPrefab);
        float fontHeight = CreateThemeButtons.MakeFontButtons(font.transform.Find("Options"), themeButtonPrefab);
        float backHeight = CreateThemeButtons.MakeBackgroundButtons(back.transform.Find("Options"), backgroundButtonPrefab);

        // Set theme sizes
        RectTransform title = theme.transform.Find("Title").GetComponent<RectTransform>();
        title.gameObject.GetComponent<TMP_Text>().text = "Theme";
        title.sizeDelta = new Vector2(0f, titleHeight);
        theme.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, titleHeight + themeHeight + padding);
        
        // Set font sizes
        title = font.transform.Find("Title").GetComponent<RectTransform>();
        title.gameObject.GetComponent<TMP_Text>().text = "Font";
        title.sizeDelta = new Vector2(0f, titleHeight);
        font.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, titleHeight + fontHeight + padding);

        // Set background sizes
        title = back.transform.Find("Title").GetComponent<RectTransform>();
        title.gameObject.GetComponent<TMP_Text>().text = "Background";
        title.sizeDelta = new Vector2(0f, titleHeight);
        back.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, titleHeight + backHeight + padding);

        yield return new WaitForEndOfFrame();

        // Make sure all theyre texts are the same
        SameTextSize themeText = theme.transform.Find("Options").gameObject.AddComponent<SameTextSize>();
        SameTextSize fontText = font.transform.Find("Options").gameObject.AddComponent<SameTextSize>();
        SameTextSize backText = font.transform.Find("Options").gameObject.AddComponent<SameTextSize>();
    }

    // Instantiate section title and set its size
    void MakeTitle(string type) {
        // Instantiate title
        GameObject obj = Instantiate(TitleObject, SettingsHolder);
        obj.name = type + " Settings";
        RectTransform rect = obj.GetComponent<RectTransform>();
        float height = 0.7f * settingHeight;
        rect.sizeDelta = new Vector2(0f, height);

        // Find sizes
        float barSize = 0.1f * height;
        float barPos = 0.1f * height;
        float textSize = height - barSize - barPos;

        // Set text
        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
        text.text = type.ToString();
        RectTransform textRect = text.gameObject.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(0f, textSize);
        textRect.anchoredPosition = new Vector2(0f, 0f);

        // Set bar
        RectTransform bar = obj.transform.Find("Bar").GetComponent<RectTransform>();
        bar.sizeDelta = new Vector2(0f, barSize);
        bar.anchoredPosition = new Vector2(0f, barPos);
    }


    // ----- MONOBEHAVIOUR STUFF -----

    bool enumRunning = false;

    void Start() {
        Settings.LoadSettings();
        if (SettingsHolder.childCount == 0) {
            if (!enumRunning && SettingsHolder.gameObject.activeInHierarchy && SettingsHolder != null) {
                CreateSettings();
            }
        }
        UpdateSettingsMenu();
    }

    // Turn setting menu on, create settings, and turn it off again
    bool allSettingsMade = false;
    void Awake() {
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

        allSettingsMade = false;
        Start();
        SetActiveRecursively(SettingsHolder.gameObject, true);
        StartCoroutine(AwakeEnum());
    }
    IEnumerator AwakeEnum() {
        while (!allSettingsMade) {
            yield return new WaitForEndOfFrame();
        }
        SetActiveRecursively(SettingsHolder.gameObject, false);
    }

    // Set active the object and any parent it has
    void SetActiveRecursively(GameObject obj, bool turnOn) {

        if (turnOn) {
            Debug.Log("Turning on " + obj.name);
        } else {
            Debug.Log("Turning off " + obj.name);
        }

        if (obj.transform.parent != null) {
            SetActiveRecursively(obj.transform.parent.gameObject, turnOn);
        } else {
            obj.SetActive(turnOn);
        }
    }

    // Update is called once per frame
    void UpdateSettingsMenu()
    {
        StartCoroutine(UpdateSettingsMenuEnum());
    }
    IEnumerator UpdateSettingsMenuEnum() {
        if (SettingsHolder.childCount == 0) {
            if (!enumRunning && SettingsHolder.gameObject.activeInHierarchy && SettingsHolder != null) {
                CreateSettings();
            }
        }
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(UpdateSettingsMenuEnum());
    }

    // Turn off all layout groups
    void TurnOffLayoutGroups(Transform parent) {
        if (parent == null) { return; }

        VerticalLayoutGroup vert = parent.GetComponent<VerticalLayoutGroup>();
        if (vert != null) { vert.enabled = false; }

        HorizontalLayoutGroup hor = parent.GetComponent<HorizontalLayoutGroup>();
        if (hor != null) { hor.enabled = false; }

        GridLayoutGroup grid = parent.GetComponent<GridLayoutGroup>();
        if (grid != null) { grid.enabled = false; }

        foreach (Transform child in parent) {
            TurnOffLayoutGroups(child);
        }
    }
}
