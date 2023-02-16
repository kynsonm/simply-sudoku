using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Events;

public class SuccessScreen : MonoBehaviour
{
    // ----- VARIABLES -----

    [SerializeField] GameObject SuccessCanvas;
    CanvasGroup successCanvasGroup;
    [SerializeField] List<GameObject> MakeUninteractable;
    [SerializeField] GameObject BlurCanvas;
    CanvasGroup blurCanvasGroup;
    [SerializeField] TMP_Text InfoText;

    [Space(10f)]
    [SerializeField] float halfColorRatio;

    [Space(10f)]
    [SerializeField] float FadeTime;
    [SerializeField] LeanTweenType FadeEaseCurve;

    bool screenOn;


    // ----- SETUP -----
    // Start is called before the first frame update
    IEnumerator Start()
    {
        SuccessCanvas.SetActive(true);
        while (!CheckVars()) {
            GetVars();
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        TurnOffCanvas();
    }
    // -------------------------------

    // ----- SUCCESS SCREEN INFO -----

    public void MakeSuccess() {
        if (screenOn) { return; }
        screenOn = true;

        //GameManager.CompleteLevel();
        MakeSuccess(LevelInfo.CurrentBoardSize, LevelInfo.CurrentBoxSize, LevelInfo.CurrentDifficulty,
        /**/        LevelInfo.LevelNumber+1, GameManager.GameTime);
    }

    public void MakeSuccess(BoardSize board, BoxSize box, Difficulty diff, int number, float time) {
        int height = LevelInfo.BoardSizeToHeight(board);
        int width = LevelInfo.BoardSizeToWidth(board);
        Vector2Int boxSize = LevelInfo.BoxSizeVector(box, board);
        string diffString = LevelInfo.DifficultyToString(diff).ToLower();

        string x = "\U000000D7";
        string d = "\U00002014";
        //string b = "\U00002022";
        string color = "<#" + ColorUtility.ToHtmlStringRGB(Theme.text_accent) + ">";
        string endC = "</color>";
        string levelInfo = width + x + height + " " + d + " [" + boxSize.x + x + boxSize.y + "]";

        string s = "";
        s += color + "<i>Congratulations!</i></color>\n";
        s += $"You completed the level and received {HalfColor(Theme.color4)}10 coins</color>!\n";
        s += "<line-height=25>\n</line-height>";
        s += "<align=left>";
        s += "It took you " + TimeToString(time);
        s += $" to complete {HalfBack()}<i>level {number}</i>{endC}";
        s += $" of {HalfBack()}<i>{levelInfo}</i>{endC}";
        s += $" on {HalfBack()}<i>{diffString}</i>{endC} difficulty";
    
        InfoText.text = s;

        InfoText.gameObject.GetComponent<TextTheme>().UpdateTextSize();

        TurnOnCanvas();
        StartCoroutine(UpdateTextSize());
    }

    IEnumerator UpdateTextSize() {
        string str = InfoText.text;
        int index = str.IndexOf("<align=left>");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        float fontSize = 0.85f * InfoText.fontSize;
        str = str.Insert(index, $"<size={fontSize}>");
        InfoText.text = str;
    }

    public void CloseSuccess() {
        TurnOffCanvas();
        screenOn = false;
    }

    void TurnOffCanvas() {
        TweenCanvas(false);
    }
    void TurnOnCanvas() {
        TweenCanvas(true);
    }   
    void TweenCanvas(bool turnOn) {
        float start = turnOn ? 0f : 1f;
        float end = turnOn ? 1f : 0f;

        Blur.BlurMaterial material = Blur.FindMaterial(Blur.BlurType.UI);

        LeanTween.value(successCanvasGroup.gameObject, start, end, FadeTime)
        .setEase(FadeEaseCurve)
        .setOnStart(() => {
            BlurCanvas.SetActive(true);
            SuccessCanvas.SetActive(true);
            successCanvasGroup.blocksRaycasts = turnOn;
            successCanvasGroup.interactable = turnOn;
        })
        .setOnUpdate((float value) => {
            successCanvasGroup.alpha = value;
            blurCanvasGroup.alpha = value;
            material.SetRadius(value);
        })
        .setOnComplete(() => {
            if (!turnOn) {
                BlurCanvas.SetActive(false);
                SuccessCanvas.SetActive(false);
            }
            if (turnOn) { GameManager.GameBoard.Uninteractable(); }
            else { GameManager.GameBoard.Interactable(); }

            material.SetRadius(end);
        });

        // Turn stuff on / off
        foreach (GameObject obj in MakeUninteractable) {
            CanvasGroup canv = obj.GetComponent<CanvasGroup>();
            if (canv == null) { canv = obj.AddComponent<CanvasGroup>(); }
            canv.blocksRaycasts = !turnOn;
            canv.interactable = !turnOn;
            canv.ignoreParentGroups = turnOn;
        }
    }

    public void NextLevel() {
        // Change scene object reference
        GameObject obj = GameObject.Find("Next Level Success");
        if (obj == null) {
            obj = InfoText.gameObject;
        }
        // Action taken when next level button is clicked
        UnityAction a = () => {
            if (!LevelInfo.NextLevel()) {
                GameManager.isClear = true;
                SceneLoader.LoadScene(SceneLoader.Scene.MainMenu, obj);
            } else {
                GameManager.isClear = true;
                SceneLoader.LoadScene(SceneLoader.Scene.Game, obj);
            }
        };
        // Do the action
        NextLevel(a);
    }
    public void NextLevel(UnityAction actionToDo) {
        // If ad available, set queued action instead
        if (AdManager.InterstitialAdReady()) {
            queuedAction = actionToDo;
            AdManager.DoGameInterstitialAd();
        } else {
            actionToDo.Invoke();
        }
        
        Debug.Log("Doing SuccessScreen action:\n" + actionToDo.ToString());
    }

    public void MainMenu() {
        GameObject obj = GameObject.Find("Success Next Level");
        if (obj == null) {
            obj = InfoText.gameObject;
        }

        // Action taken when main menu button is clicked
        UnityAction a = () => {
            GameManager.isClear = true;
            SceneLoader.LoadScene(SceneLoader.Scene.MainMenu, obj);
        };

        // If ad available, set queud action instead
        if (AdManager.InterstitialAdReady()) {
            queuedAction = a;
        } else {
            a.Invoke();
        }
    }

    public UnityAction queuedAction;
    public void SetQueuedAction(UnityAction action) {
        queuedAction = action;
    }
    public void DoQueudAction() {
        if (queuedAction == null) { return; }
        queuedAction.Invoke();
        queuedAction = null;
    }

    // ----- UTILITIES -----

    // Get important variables
    void GetVars() {
        successCanvasGroup = GetComponentRecursive<CanvasGroup>(SuccessCanvas);
        if (successCanvasGroup == null) {
            successCanvasGroup = SuccessCanvas.AddComponent<CanvasGroup>();
        }
        
        blurCanvasGroup = BlurCanvas.GetComponent<CanvasGroup>();

        CheckVars();
    }

    // Check if any vars don't exist
    bool CheckVars() {
        bool allGood = true;
        if (successCanvasGroup == null) {
            Debug.Log(nameof(successCanvasGroup) + " is null");
            allGood = false;
        }
        if (InfoText == null) {
            Debug.Log("Info text is null");
            allGood = false;
        }
        if (blurCanvasGroup == null) {
            Debug.Log("Blur canvas group is null");
            allGood = false;
        }
        return allGood;
    }

    string TimeToString(float time) {
        TimeSpan span = TimeSpan.FromSeconds(time);

        string hr = HalfColor(Theme.color1);
        string mn = HalfColor(Theme.color1);
        string sc = HalfColor(Theme.color1);
        //string mn = HalfColor(Theme.color3);
        //string sc = HalfColor(Theme.color2);
        string endC = "</color>";

        string str = "";

        // ex. "1 hour, 2 minutes and 12 seconds"
        if (span.Hours > 0) {
            str += hr + span.Hours + " hour";
            if (span.Hours > 1) { str += "s"; }
            str += endC;
            // "1 hour, 2 minutes and 12 seconds"
            if (span.Minutes > 0 && span.Seconds > 0) {
                str += ", ";
            }
            // "1 hour and 3 minutes" or "1 hour and 18 seconds"
            else {
                str += " and ";
            }
        }
        if (span.Minutes > 0) {
            str += mn + span.Minutes + " minute";
            if (span.Minutes > 1) { str += "s"; }
            str += endC;
            // ex. "4 minutes and 18 seconds"
            if (span.Seconds > 0) {
                str += " and ";
            }
        }
        // ex. "1 hour and 4 minutes" or "15 minutes and 4 seconds"
        if (span.Seconds > 0) {
            str += sc + span.Seconds;
            if (span.Minutes == 0 && span.Hours == 0) { str += "." + (span.Milliseconds/100); }
            str += " second";
            if (span.Seconds > 0) { str += "s"; }
            str += endC;
        }
        // ex. "14 minutes and 0 seconds" but not "1 hour and 3 minutes and 0 seconds"
        else if (span.Hours == 0) {
            str += $" and {sc}0 seconds{endC}";
        }

        return str;
    }

    // Gets component on object <obj> or any of its children
    T GetComponentRecursive<T>(GameObject obj) {
        T component = obj.GetComponent<T>();

        // If component is on this object, return it
        if (obj.GetComponent<T>() != null) {
            return component;
        }

        // If component is on any children, return it
        foreach (Transform transform in obj.transform) {
            component = GetComponentRecursive<T>(transform.gameObject);
            if (component != null) {
                return component;
            }
        }

        // Otherwise, return null (ideally)
        return component;
    }

    string HalfBack() {
        return HalfColor(Theme.text_accent);
    }
    string HalfColor(Color ofHalf) {
        Color half = ThemeController.Half(Theme.text_main, ofHalf, halfColorRatio);
        return "<#" + ColorUtility.ToHtmlStringRGB(half) + ">";
    }
}
