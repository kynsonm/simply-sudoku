using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [SerializeField] GameObject SettingsMenu;
    [SerializeField] GameObject SettingsButton;
    [SerializeField] List<GameObject> MakeUninteractable;
    [Space(10f)]
    [SerializeField] GameObject TitleRef;
    [SerializeField] GameObject BottomRef;
    [Space(10f)]
    [SerializeField] LeanTweenType easeCurve;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetDimensions(true));
    }

    bool lastOpenend = false;
    public bool IsOpened() {
        return lastOpenend;
    }

    // Either turn on or off, depending on context
    // Returns true if moving menu is successful
    public bool lastGamePaused = false;
    public bool Activate() {
        bool activated;
        if (SettingsMenu.activeInHierarchy) {
            activated = Close();
            if (activated && !lastGamePaused) { GameManager.GameBoard.Resume(); }
        }
        else {
            activated = Open();
            if (activated) {
                lastGamePaused = GameManager.IsPaused;
                if (!GameManager.IsPaused) { GameManager.GameBoard.Pause(); }
            }
        }
        return activated;
    }

    // Tween menu either in or out, and fade/scale it
    public bool Open() {
        return MoveMenu(true, false);
    }
    public bool Open(bool tweenOverride) {
        return MoveMenu(true, true);
    }
    public bool Close() {
        return MoveMenu(false, false);
    }
    public bool Close(bool tweenOverride) {
        return MoveMenu(false, true);
    }
    bool MoveMenu(bool isOpening, bool tweenOverride) {
        if (tweenOverride) { }
        else if (SettingsMenu.LeanIsTweening()) { return false; }

        CheckGameInfo();
        lastOpenend = isOpening;
        SettingsMenu.transform.parent.gameObject.SetActive(true);

        if (isOpening) { lastGamePaused = GameManager.IsPaused; }
        else if (EditButtons.UpdatedEditButtons) {
            GameManager.GameBoard.Save();
            Settings.SaveSettings();
            SceneLoader.LoadScene(SceneLoader.Scene.Game);
            EditButtons.UpdatedEditButtons = false;
        }

        // Get rects
        RectTransform rect = SettingsMenu.GetComponent<RectTransform>();
        Vector3 endPos = new Vector3(rect.position.x, rect.position.y, rect.position.z);
        RectTransform butt = SettingsButton.GetComponent<RectTransform>();

        // Get start and end position
        Vector2 start = isOpening ? butt.position : rect.position;
        Vector2 end = isOpening ? rect.position : butt.position;
        Vector2 posOnEnd = new Vector2(0f, rect.anchoredPosition.y);
        rect.position = start;

        // Get canvas group
        CanvasGroup canv = SettingsMenu.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = SettingsMenu.AddComponent<CanvasGroup>();
        }

        float scaleStart = rect.localScale.x;

        // Move from start to end
        // Fade & scale in/out
        LeanTween.value(SettingsMenu, 0f, 1f, Settings.AnimSpeedMultiplier)
        .setEase(easeCurve)
        .setOnUpdate((float value) => {
            // Set position
            rect.position = start + value * (end - start);

            // Set scale and alpha
            float scale = isOpening ? value : (1f - value);
            canv.alpha = Mathf.Pow(scale, 0.5f);

            Vector3 newScale = new Vector3(scale, scale, 1f);
            if (!isOpening) {
                float temp = scaleStart * scale;
                newScale = new Vector3(temp, temp, 1f);
            }
            rect.localScale = newScale;
        })
        .setOnComplete(() => {
            rect.position = endPos;
            if (!isOpening) {
                SettingsMenu.transform.parent.gameObject.SetActive(false);
                if (!lastGamePaused) { GameManager.GameBoard.Resume(); }
            } else {
                SetPosition();
                SetDimensions(false);
            }
            ActivateUninteractables(isOpening);
        });

        return true;
    }

    void SetPosition() {
        RectTransform rect = SettingsMenu.GetComponent<RectTransform>();
        RectTransform topRect = TitleRef.GetComponent<RectTransform>();
        float menuPos = -(Screen.height - topRect.position.y);
        rect.anchoredPosition = new Vector2(0f, menuPos);
    }

    IEnumerator SetDimensions(bool turnMenuOff) {
        SettingsMenu.transform.parent.gameObject.SetActive(true);

        // Wait for things go get rendered
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Get rects
        RectTransform rect = SettingsMenu.GetComponent<RectTransform>();
        RectTransform topRect = TitleRef.GetComponent<RectTransform>();
        RectTransform bottomRect = BottomRef.GetComponent<RectTransform>();

        // Get vars
        float titleSize = topRect.rect.height;
        float menuSize = topRect.position.y - bottomRect.position.y;
        float menuPos = -(Screen.height - topRect.position.y);
        Vector3 buttonPos = SettingsButton.GetComponent<RectTransform>().position;

        // Set menu stuff
        rect.sizeDelta = new Vector2(0f, menuSize);
        rect.anchoredPosition = new Vector2(0f, menuPos);

        // Set title stuff
        RectTransform title = SettingsMenu.transform.Find("Title").GetComponent<RectTransform>();
        title.sizeDelta = new Vector2(0f, titleSize);
        title.anchoredPosition = new Vector2(0f, 0f);

        // Set scroll stuff
        RectTransform scroll = SettingsMenu.transform.Find("Scroll View").GetComponent<RectTransform>();
        scroll.sizeDelta = new Vector2(0f, menuSize - titleSize);

        // Set bottom bar size
        RectTransform bottom = SettingsMenu.transform.Find("Bottom Bar").GetComponent<RectTransform>();
        bottom.sizeDelta = new Vector2(0f, menuSize / 100f);

        // Set close sizes
        RectTransform closeTop = SettingsMenu.transform.parent.Find("Close Top").GetComponent<RectTransform>();
        closeTop.sizeDelta = new Vector2(0f, Mathf.Abs(menuPos));
        closeTop.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        closeTop.gameObject.GetComponent<Button>().onClick.AddListener(() => {
            Close();
        });

        RectTransform closeBot = SettingsMenu.transform.parent.Find("Close Bottom").GetComponent<RectTransform>();
        closeBot.sizeDelta = new Vector2(0f, bottomRect.position.y);
        closeBot.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        closeBot.gameObject.GetComponent<Button>().onClick.AddListener(() => {
            Close();
        });

        yield return new WaitForEndOfFrame();
        SettingsMenu.transform.parent.gameObject.SetActive(!turnMenuOff);
    }

    // Turn them either on or off
    void ActivateUninteractables(bool turnOff) {
        foreach (GameObject obj in MakeUninteractable) {
            CanvasGroup canv = GetCanvasGroup(obj);
            canv.interactable = !turnOff;
            canv.blocksRaycasts = !turnOff;
        }
    }

    // Either returns canvas group on an object or returns the added component
    CanvasGroup GetCanvasGroup(GameObject obj) {
        CanvasGroup canv = obj.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = obj.AddComponent<CanvasGroup>();
        }
        return canv;
    }


    // Check GameInfo being open
    void CheckGameInfo() {
        GameInfo info = GameObject.FindObjectOfType<GameInfo>(true);
        if (!info.gameObject.activeInHierarchy) { return; }
        Debug.Log("GameInfo is open?: " + info.isOpen());
        if (info.isOpen()) {
            info.TurnOff(true);
            info.lastGamePaused = true;
        }
    }
}
