using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class BackgroundAnimator : MonoBehaviour
{
    public GameObject FloatingObjectHolder;
    public GameObject ObjectPrefab;
    public List<GameObject> MovingBackgroundObjects;
    public List<GameObject> StaticBackgroundObjects;

    [Space(10f)]
    public int NumberAtStart;
    public float SpawnInterval;

    [Space(10f)]
    public int MinSpeed;
    public int MaxSpeed;
    public int MinAngleSpeed, MaxAngleSpeed;

    [Space(10f)]
    public float SpeedMultiplier;
    public float XAngleTimeMultiplier, YAngleTimeMultiplier, ZAngleTimeMultiplier;
    public bool ContinueCreating;

    [Space(10f)]
    public int numOnX, numOnY;

    int CreateAndDeleteOffset;

    [HideInInspector] public static List<bool> onX, onY;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        // Turn it on or off depending on settings
        ActivateBackground(Settings.BackgroundActive);

        // Increase the size of the background to account for blur edges
        RectTransform rect = gameObject.transform.Find("Static Background").GetComponent<RectTransform>();
        float size = 20f;
        rect.offsetMin = new Vector2(-size, -size);
        rect.offsetMax = new Vector2(size, size);

        // Create onX and onY lists for spacing out objects on each axis
        onX = new List<bool>(numOnX);
        for (int i = 0; i < numOnX; ++i) { onX.Add(false); }
        onY = new List<bool>(numOnY);
        for (int i = 0; i < numOnY; ++i) { onY.Add(false); }

        // Setup background saver, and start the process
        BackgroundObjectSaver.SetVariables(this);
        BackgroundObjectSaver.LoadObjects();
        StartMakingObjects();
    }


    // ----- MOVING OBJECTS -----

    // Stops all coroutines and starts making objects
    public void StartMakingObjects() {
        this.StopAllCoroutines();
        StartCoroutine(CreateObjects());
    }

    // Creates objects every <SpawnInterval>
    // Stops if <ContinueCreating> is turned false
    IEnumerator CreateObjects() {
        Transform hold = FloatingObjectHolder.transform;
        
        // Create all those at the start
        if (ContinueCreating) {
            int numObjects = BackgroundObjectSaver.NumberOfObjects();
            bool maxNumberReached = numObjects < NumberAtStart && numObjects < Background.maxNumber;

            for (int i = BackgroundObjectSaver.NumberOfObjects(); i < NumberAtStart; ++i) {
                numObjects = BackgroundObjectSaver.NumberOfObjects();
                if (numObjects > Background.maxNumber || numObjects > NumberAtStart) { break; }

                BackgroundObjectSaver.MakeObject(numObjects, NumberAtStart, false);
            }
        }

        // Create objects while its supposed to
        while (ContinueCreating) {
            // Make numbers while below max
            if (hold.childCount < Background.maxNumber) {
                MakeObject();
            }
            yield return new WaitForSeconds(SpawnInterval);
        }
    }

    // Makes an object
    // Starts it at a certain percentage into its path if specified by <percentageAlreadyDone>
    int cloneNumber = 0;
    void MakeObject() {
        MakeObject(0f, false);
    }
    public void MakeObject(float percentageAlreadyDone, bool tweenItIn) {
        // Return if we don't want things to be made
        if ((Background.sprites == null || Background.sprites.Count == 0) && !Background.areNumbers) {
            return;
        }

        // Make the prefab
        GameObject obj = Instantiate(ObjectPrefab, FloatingObjectHolder.transform);
        ++cloneNumber;
        obj.name = "Object " + cloneNumber;

        // Make either a number or image
        FloatingObject floatObj;
        if (Background.areNumbers) {
            floatObj = new FloatingNumber(this, obj);
        }
        else {
            floatObj = new FloatingImage(this, obj);
        }

        // Put the object somewhere in the middle of the screen if <percentageAlreadyDone>
        if (percentageAlreadyDone > 0.025f) {
            float start = floatObj.movingOnX ? floatObj.startPos.x : floatObj.startPos.y;
            float end = floatObj.movingOnX ? floatObj.endPos.x : floatObj.endPos.y;
            float dis = start + (percentageAlreadyDone * (end - start));

            if (floatObj.movingOnX) { floatObj.startPos.x = dis; }
            else { floatObj.startPos.y = dis; }

            floatObj.speed *= (1f - percentageAlreadyDone);

            obj.GetComponent<RectTransform>().position = floatObj.startPos;
        }

        if (tweenItIn) {
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.localScale = new Vector3(0f, 0f, rect.localScale.z);
            LeanTween.value(obj, 0f, 1f, 0.7f * Settings.AnimSpeedMultiplier)
            .setEase(LeanTweenType.easeOutBack)
            .setOnUpdate((float value) => {
                rect.localScale = new Vector3(value, value, rect.localScale.z);
            });
        }

        // Then move it
        MoveObject(obj, floatObj);

        // Add it to numberSaver list
        BackgroundObjectSaver.Add();
    }


    // Tween the number across the screen & rotation
    public void MoveObject(GameObject obj, FloatingObject info) {
        // Travel time
        float time = info.speed * SpeedMultiplier;
        
        // Tween position to end of the screen
        LeanTween.move(obj, info.endPos, time)
        .setOnStart(() => {
            obj.GetComponent<RectTransform>().position = info.startPos;
        })
        .setOnComplete( () => {
            LeanTween.cancel(obj);
            Destroy(obj);
            BackgroundObjectSaver.Remove();
        });

        // Get rotation var
        Vector3 mults = new Vector3(XAngleTimeMultiplier, YAngleTimeMultiplier, ZAngleTimeMultiplier);
        mults.x *= info.rotateX ? 1f : 0f;
        mults.y *= info.rotateY ? 1f : 0f;
        mults.z *= info.rotateZ ? 1f : 0f;
        mults /= info.angleSpeed;

        mults.x *= Background.rotationSpeedMultX;
        mults.y *= Background.rotationSpeedMultY;
        mults.z *= Background.rotationSpeedMultZ;

        // Tween rotation
        LeanTween.value(obj, 0f, 1f, 1f)
        .setOnStart(() => {
            obj.transform.localEulerAngles = info.angle;
        })
        .setOnUpdate((float value) => {
            info.angle += mults;
            obj.transform.localEulerAngles = info.angle;
        })
        .setLoopCount(int.MaxValue);
    }


    // ----- TURNING ON AND OFF -----

    // Turns on or off the background, depending on the start its already in
    // Returns the new state of the background (on or off)
    public bool ActivateBackground() {
        return ActivateBackground(!Settings.BackgroundActive);
    }
    bool ActivateBackground(bool turnOn) {
        // Turn on / off each moving object
        foreach (GameObject obj in MovingBackgroundObjects) {
            obj.SetActive(turnOn);
        }

        // Lean start/pause each object
        foreach (Transform num in FloatingObjectHolder.transform) {
            if (turnOn) { num.gameObject.LeanResume(); }
            else { num.gameObject.LeanPause(); }
        }

        // Turn off / on each static object
        foreach (GameObject obj in StaticBackgroundObjects) {
            obj.SetActive(!turnOn);
        }

        // Update isMoving depending on current state
        Settings.BackgroundActive = turnOn;
        ContinueCreating = turnOn;

        if (turnOn) {
            StopAllCoroutines();
            StartCoroutine(CreateObjects());
        }
        return turnOn;
    }


    // ----- UTILITIES -----

    // Checks onX or onY lists to see if there are any objects already in this "lane"
    // Returns true if an object can be made at this position
    public bool ValidPosition(float pos, bool useX) {
        if (pos == 0) { return false; }
        if (onX == null || onY == null) { return true; }
        // onX == moving vertically
        // onY == moving horizontally
        if (useX) {
            int index = (int)pos / (Screen.width/numOnX);
            if (onX[index] == false) {
                onX[index] = true;
                return true;
            }
        } else {
            int index = (int)pos / (Screen.height/numOnY);
            if (onY[index] == false) {
                onY[index] = true;
                return true;
            }
        }
        return false;
    }
}