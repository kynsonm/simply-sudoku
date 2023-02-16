using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SoundClip;

public static class CreateThemeButtons
{
    // ----- VARIABLES -----

    // Theme script and other variables
    static Themes themes;

    // Utilities
    static readonly string color = "<color=#00000000>";
    static readonly string endCol = "</color>";
    static readonly string pad = color + "--" + endCol;


    // Constructor
    static CreateThemeButtons() {
        themes = GameObject.FindObjectOfType<Themes>();
        if (themes == null) {
            Debug.LogError("Themes object of CreateThemeButtons is null");
        }
    }

    // Makes theme buttons in the settings menu depending on
    //   how many themes there are in <themes.looks>
    public static float MakeThemeButtons(Transform themeParent, GameObject themePrefab) {
        // Destroy everything in it first
        DestroyChildrenOf(themeParent);
        // Then make new buttons
        for (int i = 0; i < themes.looks.Count; ++i) {
            // Instantiate new obj
            Look look = themes.looks[i];
            GameObject obj = GameObject.Instantiate(themePrefab, themeParent);
            obj.name = look.name + " Button";

            // Set looks
            Transform trans = obj.transform;
            trans.Find("Background").GetComponent<Image>().color = look.UI_background;
            trans.GetComponent<Image>().color = look.UI_accent;

            // Destroy font texts
            Object.Destroy(trans.Find("Bold").gameObject);
            Object.Destroy(trans.Find("Thin").gameObject);

            // Set text and color
            TMP_Text text = trans.GetComponentInChildren<TMP_Text>();
            text.color = look.UI_main;
            text.text = pad + look.name + pad;

            // Set onClick
            int index = i;
            trans.GetComponent<Button>().onClick.AddListener(
                delegate {
                    Sound.Play(tap_something);
                    themes.ChangeTheme(index);
                    Settings.SaveSettings();
                }
            );

            // Add unlockable script
            if (look.ID.Length <= 3 || look.cost == 0) { continue; }
            Unlockable unlock = obj.AddComponent<Unlockable>();
            unlock.ID = look.ID;
            unlock.unlockName = themes.looks[i].name + " Theme";
            unlock.unlockDescription = look.info;
            unlock.Setup(look.iapTier, look.cost);
        }
        
        return themeParent.GetComponent<GridEditor>().ResetSizes();
    }

    // Makes font buttons in the settings menu depending on
    //   how many fonts there are in <themes.fontLooks>
    public static float MakeFontButtons(Transform fontParent, GameObject themePrefab) {
        // Destroy everything in it first
        DestroyChildrenOf(fontParent);
        // Then make new buttons
        for (int i = 0; i < themes.fontLooks.Count; ++i) {
            // Get objects
            FontLook font = themes.fontLooks[i];
            GameObject obj = GameObject.Instantiate(themePrefab, fontParent);
            obj.name = font.name + " Button";
            Transform trans = obj.transform;

            // Change colors
            ImageTheme theme = trans.Find("Background").gameObject.GetComponent<ImageTheme>();
            theme.updateColor = true;
            theme.lookType = LookType.UI_background;
            ThemeController.UpdateColor(theme);

            theme = trans.gameObject.GetComponent<ImageTheme>();
            theme.updateColor = true;
            theme.lookType = LookType.UI_accent;
            ThemeController.UpdateColor(theme);

            // Turn off update fonts
            trans.Find("Main").GetComponent<TextTheme>().updateFont = false;
            trans.Find("Bold").GetComponent<TextTheme>().updateFont = false;
            trans.Find("Thin").GetComponent<TextTheme>().updateFont = false;

            // Set texts
            TMP_Text text = trans.Find("Main").GetComponent<TMP_Text>();
            text.text = pad + "123" + color + "--------" + pad;
            text.font = font.font;

            text = trans.Find("Bold").GetComponent<TMP_Text>();
            text.text = pad + color + "----" + endCol + "456" + color + "----" + pad;
            text.font = font.bold;

            text = trans.Find("Thin").GetComponent<TMP_Text>();
            text.text = pad + color + "--------" + endCol + "789" + pad;
            text.font = font.thin;

            // Add font button onClick fxn
            int index = i;
            trans.GetComponent<Button>().onClick.AddListener(
                delegate {
                    Sound.Play(tap_something);
                    themes.ChangeFont(index);
                    Settings.SaveSettings();
                }
            );

            // Add unlockable script
            if (font.ID.Length <= 3 || font.cost == 0) { continue; }
            Unlockable unlock = obj.AddComponent<Unlockable>();
            unlock.ID = font.ID;
            unlock.unlockName = themes.fontLooks[i].name + " Font";
            unlock.unlockDescription = font.info;
            unlock.Setup(font.iapTier, font.cost);
        }

        return fontParent.GetComponent<GridEditor>().ResetSizes();
    }

    public static float MakeBackgroundButtons(Transform backParent, GameObject backPrefab) {
        // Destroy everything in it first
        DestroyChildrenOf(backParent);
        string pad = color + "--" + endCol;
        // Then make new buttons
        for (int i = 0; i < themes.backgrounds.Count; ++i) {
            // Get objects
            BackgroundObject back = themes.backgrounds[i];
            GameObject obj = GameObject.Instantiate(backPrefab, backParent);
            obj.name = back.name + " Button";
            Transform trans = obj.transform;

            // Set name
            TMP_Text name = trans.Find("Name").GetComponent<TMP_Text>();
            name.text = pad + back.name + pad;

            // Set background
            trans = trans.Find("Fill");
            Image img = trans.Find("Background").GetComponent<Image>();
            img.sprite = back.backgroundSprite;

            // Set image/text
            if (back.areNumbers) {
                GameObject.Destroy(trans.Find("Image").gameObject);
                trans.GetComponentInChildren<TMP_Text>().text = Random.Range(1, 10).ToString();
            } else {
                GameObject.Destroy(trans.Find("Text").gameObject);
                if (back.sprites != null && back.sprites.Count > 0) {
                    trans.Find("Image").GetComponent<Image>().sprite = back.sprites[0];
                } else {
                    GameObject.Destroy(trans.Find("Image").gameObject);
                }
            }

            // Add background button onClick fxn
            int index = i;
            obj.GetComponent<Button>().onClick.AddListener(() => {
                Sound.Play(tap_something);
                themes.ChangeBackground(index);
                Settings.SaveSettings();
            });

            // Add unlockable script
            if (back.ID.Length <= 3 || back.cost == 0) { continue; }
            Unlockable unlock = obj.AddComponent<Unlockable>();
            unlock.ID = back.ID;
            unlock.unlockName = themes.backgrounds[i].name + " Background";
            unlock.unlockDescription = back.info;
            unlock.Setup(back.iapTier, back.cost);
        }

        return backParent.GetComponent<GridEditor>().ResetSizes();
    }

    // Just destroys all the children of a GameObject
    static void DestroyChildrenOf(Transform obj) {
        foreach (Transform child in obj) {
            Object.Destroy(child.gameObject);
        }
    }
}
