using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameButtons : MonoBehaviour
{
    [SerializeField] float animationSpeed;
    [SerializeField] int loopCount;

    [Space(10f)]
    public GameObject EditButtonObject;
    [SerializeField] [Range(0f, 90f)] float editRotationAngle;
    [SerializeField] LeanTweenType editEaseIn, editEaseDur, editEaseOut;

    [Space(10f)]
    public GameObject EraseButtonObject;
    [SerializeField] [Range(0f, 90f)] float eraseMoveDistance;
    [SerializeField] LeanTweenType eraseEaseIn, eraseEaseDur, eraseEaseOut;

    [Space(10f)]
    public GameObject HintButtonObject;
    [SerializeField] [Range(0f, 90f)] float hintRotationAngle;
    [SerializeField] float hintScale;
    [SerializeField] LeanTweenType hintEaseCurve;

    [Space(10f)]
    [SerializeField] float undoRedoRotationAngle;
    [SerializeField] LeanTweenType undoRedoEaseCurve;


    // ----- Main stuff -----

    void Start() {
        SetButtonOnClicks();
    }

    void SetButtonOnClicks() {
        // Edit
        Button butt = EditButtonObject.GetComponentInChildren<Button>();
        if (butt == null) { Debug.Log("Edit button is null"); }
        GameObject edit = butt.gameObject;
        butt.onClick.RemoveAllListeners();
        butt.onClick.AddListener(delegate {
            EditButtonPress();
            StopCoroutine(EditSequence(edit));
            StartCoroutine(EditSequence(edit));
        });

        // Erase
        butt = EraseButtonObject.GetComponentInChildren<Button>();
        if (butt == null) { Debug.Log("Erase button is null"); }
        GameObject erase = butt.gameObject;
        butt.onClick.RemoveAllListeners();
        butt.onClick.AddListener(delegate {
            EraseButtonPress();
            StopCoroutine(EraseSequence(erase));
            StartCoroutine(EraseSequence(erase));
        });

        // Hint
        butt = HintButtonObject.GetComponentInChildren<Button>();
        if (butt == null) { Debug.Log("Hint button is null"); }
        GameObject hint = butt.gameObject;
        butt.onClick.RemoveAllListeners();
        butt.onClick.AddListener(delegate {
            HintButtonClick();
            StopCoroutine(HintSequence(hint));
            StartCoroutine(HintSequence(hint));
        });


        // Undo
        butt = EditButtonObject.transform.parent.Find("Undo").GetComponentInChildren<Button>();
        if (butt == null) { Debug.Log("Undo button is null"); }
        GameObject undo = butt.gameObject;
        butt.onClick.RemoveAllListeners();
        butt.onClick.AddListener(delegate {
            UndoButtonPress();
            StopCoroutine(UndoRedoSequence(undo, true));
            StartCoroutine(UndoRedoSequence(undo, true));
        });

        // Redo
        butt = EditButtonObject.transform.parent.Find("Redo").GetComponentInChildren<Button>();
        if (butt == null) { Debug.Log("Redo button is null"); }
        GameObject redo = butt.gameObject;
        butt.onClick.RemoveAllListeners();
        butt.onClick.AddListener( () => {
            RedoButtonPress();
            StopCoroutine(UndoRedoSequence(redo, false));
            StartCoroutine(UndoRedoSequence(redo, false));
        });
    }

    public void CompleteLevel() {
        EditButtonObject.GetComponentInChildren<Button>().interactable = false;
        EraseButtonObject.GetComponentInChildren<Button>().interactable = false;
        HintButtonObject.GetComponentInChildren<Button>().interactable = false;
        EditButtonObject.transform.parent.Find("Undo").GetComponentInChildren<Button>().interactable = false;
        EditButtonObject.transform.parent.Find("Redo").GetComponentInChildren<Button>().interactable = false;
    }


    // ----- What each button press does -----

    public void EditButtonPress() {
        GameManager.EditButtonPress();
        UpdateColors();
    }

    public void EraseButtonPress() {
        GameManager.EraseButtonPress();
        UpdateColors();
    }

    public void UndoButtonPress() {
        GameManager.Undo();
    }
    public void RedoButtonPress() {
        GameManager.Redo();
    }

    public void HintButtonClick() {
        GameObject.FindObjectOfType<GameHint>().HintButtonPress();
    }


    // ----- LeanTweens for each button -----

    void Reset(GameObject obj) {
        LeanTween.cancel(obj);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.rotation = new Quaternion(0f, 0f, 0f, 0f);
    }

    IEnumerator EditSequence(GameObject edit) {
        Reset(edit);
        edit.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);

        float time = animationSpeed / (float)loopCount;
        //time /= 3f;
        float timeHalf = time / 2f;
        float timeFourth = time / 4f;

        for (int i = 0; i < loopCount; ++i) {
            // Tween to the left
            LeanTween.rotateAroundLocal(edit, new Vector3(0, 0, 1), editRotationAngle, timeFourth)
            .setEase(editEaseIn)
            .setOnComplete(() => {
                // Tween to the right
                LeanTween.rotateAroundLocal(edit, new Vector3(0, 0, 1), -2f * editRotationAngle, timeHalf)
                .setEase(editEaseDur)
                .setOnComplete(() => {
                    // Tween back to the center
                    LeanTween.rotateAroundLocal(edit, new Vector3(0, 0, 1), editRotationAngle, timeFourth)
                    .setEase(editEaseOut)
                    .setOnComplete(() => {
                        edit.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);
                    });
                });
            });
            yield return new WaitForSeconds(time);
        }

        edit.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);
    }

    IEnumerator EraseSequence(GameObject erase) {
        Reset(erase);

        GameObject obj = erase.transform.GetChild(0).gameObject;
        LeanTween.cancel(obj);
        RectTransform rect = obj.GetComponent<RectTransform>();

        float time = animationSpeed / (float)loopCount;
        float timeHalf = time / 2f;
        float timeFourth = time / 4f;

        for (int i = 0; i < loopCount; ++i) {
            // Tween from start (0) to the right
            LeanTween.value(obj, 0f, eraseMoveDistance, timeFourth)
            .setEase(eraseEaseIn)
            .setOnStart(() => {
                rect.anchoredPosition = new Vector2(0f, 0f);
            })
            .setOnUpdate((float value) => {
                rect.anchoredPosition = new Vector2(value, 0f);
            })
            .setOnComplete(() => {
                // Tween from right to the left
                LeanTween.value(obj, eraseMoveDistance, -eraseMoveDistance, timeHalf)
                .setEase(eraseEaseDur)
                .setOnUpdate((float value) => {
                    rect.anchoredPosition = new Vector2(value, 0f);
                })
                .setOnComplete(() => {
                    // Tween from left to the center (0)
                    LeanTween.value(obj, -eraseMoveDistance, 0f, timeFourth)
                    .setEase(eraseEaseOut)
                    .setOnUpdate((float value) => {
                        rect.anchoredPosition = new Vector2(value, 0f);
                    })
                    .setOnComplete(() => {
                        rect.anchoredPosition = new Vector2(0f, 0f);
                    });
                });
            });
            yield return new WaitForSeconds(time);
        }
        rect.anchoredPosition = new Vector2(0f, 0f);
    }

    IEnumerator HintSequence(GameObject hint) {
        Reset(hint);
        hint.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        hint.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);

        float totalTime = 4f * animationSpeed;
        float time = totalTime / 4f;

        LeanTween.rotateAroundLocal(hint, new Vector3(0, 0, 1), -hintRotationAngle, time)
        .setEase(hintEaseCurve)
        .setOnComplete(() => {
            LeanTween.scale(hint, new Vector2(hintScale, hintScale), time)
            .setEase(hintEaseCurve)
            .setLoopPingPong(1)
            .setOnComplete(() => {
                LeanTween.rotateAroundLocal(hint, new Vector3(0, 0, 1), hintRotationAngle, time)
                .setEase(hintEaseCurve)
                .setOnComplete(() => {
                    hint.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    hint.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
                });
            });
        });

        yield return new WaitForSeconds(totalTime);

        hint.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        hint.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
    }

    IEnumerator UndoRedoSequence(GameObject undoRedo, bool isUndo) {
        Reset(undoRedo);
        undoRedo.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);

        float time = animationSpeed / (float)loopCount / 2f;
        float angle;
        if (isUndo) {
            angle = undoRedoRotationAngle;
        } else {
            angle = -undoRedoRotationAngle;
        }

        LeanTween.rotateAroundLocal(undoRedo, new Vector3(0, 0, 1), angle, time)
        .setLoopPingPong(loopCount)
        .setEase(undoRedoEaseCurve)
        .setOnComplete(() => {
            undoRedo.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
        });

        yield return new WaitForSeconds(animationSpeed);

        undoRedo.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
    }


    // ----- Update colors depending on if a mode is activated -----

    void UpdateColors() {
        // Edit button color
        ImageTheme theme1 = EditButtonObject.GetComponentInChildren<ImageTheme>();
        LookType end1;
        if (GameManager.EditMode) {
            end1 = LookType.UI_accent;
        } else {
            end1 = LookType.UI_main;
        }

        theme1.lookType = end1;
        theme1.Reset();

        // Erase button color
        ImageTheme theme2 = EraseButtonObject.GetComponentInChildren<ImageTheme>();
        LookType end2;
        if (GameManager.EraseMode) {
            end2 = LookType.UI_accent;
        } else {
            end2 = LookType.UI_main;
        }

        theme2.lookType = end2;
        theme2.Reset();
    }

    Vector3 GetDiff(Color a, Color b) {
        return new Vector3(b.r - a.r, b.g - a.g, b.b - a.b);
    }
}
