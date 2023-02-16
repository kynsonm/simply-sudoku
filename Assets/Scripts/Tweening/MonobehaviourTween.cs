using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis {
    x, y, z
}

public class MonobehaviourTween : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FPSCounter());
    }

    private List<int> data;
    private int dataSize = 20;
    IEnumerator FPSCounter() {
        yield return new WaitForEndOfFrame();
        data = new List<int>(dataSize);
        for (int i = 0; i < dataSize; ++i) {
            data.Add((int)(1f / Time.deltaTime));
        }

        int index = 0;
        while (true) {
            data[index] = (int)(1f / Time.deltaTime);
            ++index;
            if (index >= dataSize) {
                index = 0;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    float FPSAverage() {
        float sum = 0f;
        // If data is not right, correct it
        if (data == null || data.Count < dataSize) {
            Debug.Log("Data is bad! Resetting it");
            data = new List<int>(dataSize);
            for (int i = 0; i < dataSize; ++i) {
                data.Add((int)(1f / Time.deltaTime));
            }
        }
        // Find sum
        for (int i = 0; i < dataSize; ++i) {
            sum += data[i];
        }
        // Get average
        sum /= dataSize;
        return sum;
    }

    int numPerFrame(float interval) {
        float time = 1f / FPSAverage();
        int num = (int)(time / interval);
        num = (num <= 1) ? 1 : num;
        return num;
    }


    // ----- DO THE STUFF -----

    public LTDescr Move(GameObject obj, Axis axis, float start, float end) {
        RectTransform rect = obj.GetComponent<RectTransform>();

        Vector3 pos = rect.position;
        Vector3 endPos = rect.position;
        if (axis == Axis.x) {
            pos.x = start;
            endPos.x = end;
        }
        else if (axis == Axis.y) {
            pos.y = start;
            endPos.y = end;
        }
        else {
            pos.z = start;
            endPos.z = end;
        }
        rect.position = pos;

        LTDescr tween =  LeanTween.move(obj, endPos, Settings.AnimSpeedMultiplier)
        .setEase(LeanTweenType.easeInOutCubic);

        return tween;
    }

    public void FadeObject(GameObject obj, bool fadeIn) {
        CanvasGroup canv = obj.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = obj.AddComponent<CanvasGroup>();
        }
        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 1f : 0f;

        LeanTween.value(obj, start, end, 0.75f*Settings.AnimSpeedMultiplier)
        .setOnUpdate((float value) => {
            canv.alpha = value;
        })
        .setEase(LeanTweenType.easeInOutSine);
    }

    public void FadeBoardButtons(List<List<BoardButton>> board, bool fadeIn) {
        StartCoroutine(FadeBoardButtonsEnum(board, fadeIn));
    }
    IEnumerator FadeBoardButtonsEnum(List<List<BoardButton>> board, bool fadeIn) {
        // Important vars
        int rows = board.Count, cols = board[0].Count;
        float interval = 0.333f * (Settings.AnimSpeedMultiplier / (rows * cols));
        int perFrame = numPerFrame(interval);
        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 1f : 0f;

        if (!fadeIn) {
            GameManager.IsPaused = true;
        }

        // Do the thing
        int count = 0;
        foreach (var col in board) {
            foreach (var but in col) {
                // Get object and canvas group
                GameObject obj = but.numberText.gameObject;
                CanvasGroup group = obj.GetComponent<CanvasGroup>();
                if (group == null) {
                    group = obj.AddComponent<CanvasGroup>();
                }

                // Tween it
                LeanTween.value(obj, start, end, 0.333f * Settings.AnimSpeedMultiplier)
                .setOnUpdate((float value) => {
                    group.alpha = value;
                })
                .setEase(LeanTweenType.easeInOutSine);

                // Wait for the next one
                ++count;
                if (count == perFrame) {
                    yield return new WaitForSeconds(interval);
                    count = 0;
                }
            }
        }

        if (fadeIn) {
            GameManager.IsPaused = false;
        }
    }
}
