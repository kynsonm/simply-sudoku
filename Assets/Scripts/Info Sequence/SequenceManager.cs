using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using static SoundClip;

// Information for each step of a sequence
[System.Serializable]
public class SequenceStep
{
    // ----- General info
    public string name;     // Just an identifier in the editor
    public GameObject obj;  // Object to highlight
    public List<GameObject> otherObjs;  // Objects to highlight on top of this one
    public bool clickButton = false; // If the object has button component, click it

    [TextArea(minLines:3, maxLines:5)]
    public string message;  // To put in the info popup
}


public class SequenceManager : MonoBehaviour
{
    // ----- VARIABLES -----

    [SerializeField] List<SequenceStep> Sequence;

    [Space(10f)]
    [SerializeField] GameObject SequenceCanvas;
    [SerializeField] LeanTweenType easeCurve;
    [SerializeField] float maskPaddingDivider;
    [SerializeField] float moveTime;

    RectTransform mask;
    float sequenceStartPPU;
    int sequenceStep = -1;


    // ----- SEQUENCE STUFF -----

    // Does the sequence
    public void StartSequence() {
        // Get the mask object
        mask = SequenceCanvas.transform.Find("Mask").GetComponent<RectTransform>();
        sequenceStartPPU = mask.GetComponent<ImageTheme>().PPUMultiplier;

        // Get its canvas group, or add one
        CanvasGroup canv = SequenceCanvas.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = SequenceCanvas.AddComponent<CanvasGroup>();
        }

        // Tween fade
        LeanTween.value(SequenceCanvas, 0f, 1f, Settings.AnimSpeedMultiplier)
        .setEase(easeCurve)
        .setOnStart(() => {
            SequenceCanvas.SetActive(true);
            ActivateCanvases(false);
        })
        .setOnUpdate((float value) => {
            canv.alpha = value;
        });

        // Set the step
        sequenceStep = 0;
        NextStep();
    }

    // Goes to the next step
    public void NextStep() {
        // If menu is tweening, wait and call it again
        if (mask.gameObject.LeanIsTweening() && !enumRunning) {
            StartCoroutine(NextStepEnum());
            return;
        }
        // NextStep() is waiting to start
        if (enumRunning) { return; }

        // Stop sequence if it's done
        if (sequenceStep == Sequence.Count) {
            Exit();
            return;
        }

        // Move the mask to that spot
        SequenceStep step = Sequence[sequenceStep];
        ++sequenceStep;
        RectTransform rect = null;
        bool centerInfo = false;

        // If clickButton is true, click it and continue to the next step
        if (step.clickButton == true) {
            ClickButton(step);
            return;
        }

        // If step obj is null (and no otherObjs on the step), just center it
        if (step.obj == null) {
            if (step.otherObjs == null || step.otherObjs.Count == 0) {
                centerInfo = true;
            }
        } else {
            rect = step.obj.GetComponent<RectTransform>();
        }

        Sound.Play(tap_something);

        // Setup vars
        ImageTheme maskImg = mask.gameObject.GetComponent<ImageTheme>();
        ImageTheme img = mask.transform.Find("Image").GetComponent<ImageTheme>();
        float startPPU = maskImg.PPUMultiplier;
        float startSize = (mask.rect.width + mask.rect.height) / 2f;
        moveTime = (moveTime <= 0.1f) ? 0.1f : moveTime;

        LeanTween.cancel(mask.gameObject);

        // Tween it
        LeanTween.value(mask.gameObject, 0f, 1f, moveTime)
        .setEase(easeCurve)
        .setOnStart(() => {
            if (Info.isOpen()) { Info.CloseInfo(); }
        })
        .setOnUpdate((float value) => {
            // Update size and position of mask
            List<SequenceObject> objs = GetSequenceObjects(step, rect);
            Vector2 size = mask.sizeDelta + value * (GetStepSize(objs) - mask.sizeDelta);
            mask.sizeDelta = size;
            mask.position = (Vector2)mask.position + value * (CenteredPosition(objs) - (Vector2)mask.position);

            // Update PPU multiplier for mask and image
            float newPPU = startPPU * (((size.x + size.y) / 2f) / startSize);
            newPPU = (newPPU <= 1f) ? 1f : newPPU;
            maskImg.PPUMultiplier = newPPU;
            img.PPUMultiplier = newPPU;
        })
        .setOnComplete(() => {
            List<SequenceObject> objs = GetSequenceObjects(step, rect);
            mask.sizeDelta = GetStepSize(objs);
            mask.localScale = new Vector3(1f, 1f, 1f);
            Info.CreateInfo(mask.gameObject, step.message, centerInfo);
        });
    }

    bool enumRunning = false;
    IEnumerator NextStepEnum() {
        enumRunning = true;
        for (int i = 0; i < 5; ++i) {
            yield return new WaitForEndOfFrame();
        }
        enumRunning = false;
        NextStep();
    }

    // Stops a sequence where it is
    public void Exit() {
        // Reset sequence step index;
        sequenceStep = -1;

        // Tween fade
        CanvasGroup canv = SequenceCanvas.GetComponent<CanvasGroup>();
        LeanTween.value(SequenceCanvas, 1f, 0f, Settings.AnimSpeedMultiplier)
        .setEase(easeCurve)
        .setOnUpdate((float value) => {
            canv.alpha = value;
        })
        .setOnComplete(() => {
            SequenceCanvas.SetActive(false);
            ActivateCanvases(true);

            mask.GetComponent<ImageTheme>().PPUMultiplier = sequenceStartPPU;
            mask.transform.Find("Image").GetComponent<ImageTheme>().PPUMultiplier = sequenceStartPPU;
        });
    }


    // ----- FINDING POSITIONS AND SIZES -----

    // For saving centered position and size of an object
    class SequenceObject {
        public Vector2 size;
        public Vector2 pos;

        public SequenceObject(GameObject obj) {
            RectTransform rect = obj.GetComponent<RectTransform>();
            size = new Vector2(rect.rect.width, rect.rect.height);

            // Get centered posititon
            pos = new Vector2(rect.position.x, rect.position.y);
            if (rect.pivot.x != 0.5f) {
                pos.x += size.x * (-rect.pivot.x + 0.5f);
            }
            if (rect.pivot.y != 0.5f) {
                pos.y += size.y * (-rect.pivot.y + 0.5f);
            }
        }
        public SequenceObject(Vector2 sizeIn, Vector2 posIn) {
            size = sizeIn;
            pos = posIn;
        }
    }

    // Returns list of all sequence objects depending on the current step and step rect
    List<SequenceObject> GetSequenceObjects(SequenceStep step, RectTransform objRect) {
        List<SequenceObject> objs = new List<SequenceObject>();
        // Add objRect if it isnt null
        if (objRect != null) {
            objs.Add(new SequenceObject(objRect.gameObject));
        }
        // Add the other objects if they arent null
        if (step.otherObjs != null && step.otherObjs.Count != 0) {
            foreach (GameObject obj in step.otherObjs) {
                objs.Add(new SequenceObject(obj));
            }
        }
        // If both are null, then do default values
        if (objs.Count == 0) {
            objs.Add(new SequenceObject(new Vector2(Screen.width/1.25f, Screen.height/7f), new Vector2(Screen.width/2, Screen.height/2)));
        }
        return objs;
    }

    // Gets the centered position of a RectTransform
    // Takes into account pivot position
    Vector2 CenteredPosition(List<SequenceObject> objs) {
        // Get all x and y values
        List<float> x = new List<float>();
        List<float> y = new List<float>();
        foreach (SequenceObject obj in objs) {
            x.Add(obj.pos.x);
            y.Add(obj.pos.y);
        }
        // Get mins and maxes
        float xMin = x.Min(), xMax = x.Max();
        float yMin = y.Min(), yMax = y.Max();
        return new Vector2((xMin+xMax)/2f, (yMin+yMax)/2f);
    }

    // Gets the combined size of all objects in the step
    Vector2 GetStepSize(List<SequenceObject> objs) {
        SequenceObject minX = objs[0], maxX = objs[0];
        SequenceObject minY = objs[0], maxY = objs[0];
        // Get min and max for x and y
        foreach (SequenceObject obj in objs) {
            if (obj.pos.x < minX.pos.x) { minX = obj; }
            if (obj.pos.x > maxX.pos.x) { maxX = obj; }
            if (obj.pos.y < minY.pos.y) { minY = obj; }
            if (obj.pos.y > maxY.pos.y) { maxY = obj; }
        }
        // Calculate combined size
        Vector2 size = new Vector2();
        size.x = (maxX.pos.x + maxX.size.x/2f) - (minX.pos.x - minX.size.x/2f);
        size.y = (maxY.pos.y + maxY.size.y/2f) - (minY.pos.y - minY.size.y/2f);
        // Add padding
        maskPaddingDivider = (maskPaddingDivider <= 2f) ? 2f : maskPaddingDivider;
        float pad = (1f / maskPaddingDivider) * (float)Mathf.Min(Screen.width, Screen.height);
        size += new Vector2(pad, pad);
        return size;
    }


    // ----- UTILITIES -----

    // Pushes the button held in the step, then does the next step
    void ClickButton(SequenceStep step) {
        // Get the button object
        GameObject obj = step.obj;
        if (obj == null) {
            Debug.LogWarning("Object on clickButton step is null!");
            return;
        }

        // Get the button
        Button butt = obj.GetComponent<Button>();
        if (butt == null) {
            Debug.LogWarning("Button on clickButton step is null!");
            return;
        }

        // Push the button, do next step
        butt.onClick.Invoke();
        NextStep();
    }

    // Make a rect out of nothing
    // For the start of the sequence
    RectTransform MakeRect() {
        GameObject obj = new GameObject("Nothing", typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect = obj.GetComponent<RectTransform>();
        rect.position = new Vector3(Screen.width/2, Screen.height/2, mask.position.z);
        rect.sizeDelta = new Vector2(Screen.width/1.25f, Screen.height/7f);
        rect.ForceUpdateRectTransforms();
        return rect;
    }

    // Turn every canvas in the scene either on or off
    void ActivateCanvases(bool turnOn) {
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>(true);
        foreach (Canvas canv in canvases) {
            GameObject obj = canv.gameObject;

            // Skip sequence canvas
            if (obj.name == SequenceCanvas.name) { continue; }

            // Add canvas group if it doesnt have one
            CanvasGroup group = obj.GetComponent<CanvasGroup>();
            if (group == null) {
                group = obj.AddComponent<CanvasGroup>();
            }

            // And turn it off
            //group.interactable = turnOn;
            group.blocksRaycasts = turnOn;
        }
    }


    // ----- MONOBEHAVIOUR STUFF -----

    // Start is called before the first frame update
    IEnumerator Start()
    {
        SequenceCanvas.SetActive(true);
        yield return new WaitForEndOfFrame();
        SequenceCanvas.SetActive(false);
    }
}
