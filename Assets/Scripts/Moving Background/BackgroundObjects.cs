using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

// Base class for a floating object in the background
public class FloatingObject
{
    //  Variables

    public float size;

    public Vector3 startPos;
    public Vector3 endPos;
    public bool movingOnX;

    public float speed;
    public Vector3 angle;
    public bool rotateX, rotateY, rotateZ;
    public float angleSpeed;


    // Constructor
    public FloatingObject(BackgroundAnimator backgroundAnimator) {
        SetVars(backgroundAnimator);
    }


    // Sets theme stuff for shared variables
    public void SetupPrefab(GameObject prefab) {
        // Create gameobject & assign parent
        prefab.transform.position = startPos;

        // Set initial rotation
        prefab.transform.localEulerAngles = angle;

        // Set size
        RectTransform rect = prefab.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);
    }

    // Sets color, alpha,
    // startPos, endPos, curPos
    public void SetVars(BackgroundAnimator backgroundAnimator) {
        // Object size
        if (Background.minSizeMultiplier >= 1f || Background.maxSizeMultiplier >= 1f) {
            Background.minSizeMultiplier = 1f;
            Background.maxSizeMultiplier = 1f;
        }
        float maxDim = MathF.Max(Screen.height, Screen.width);
        size = UnityEngine.Random.Range(
            maxDim * Background.minSizeMultiplier,
            maxDim * Background.maxSizeMultiplier
        );

        // Speeds and angle
        speed = UnityEngine.Random.Range(backgroundAnimator.MinSpeed, backgroundAnimator.MaxSpeed);
        angle = new Vector3(
            UnityEngine.Random.Range(0f, Background.maxStartXAngle),
            UnityEngine.Random.Range(0f, Background.maxStartYAngle),
            UnityEngine.Random.Range(0f, Background.maxStartZAngle)
        );
        angleSpeed = UnityEngine.Random.Range(backgroundAnimator.MinAngleSpeed, backgroundAnimator.MaxAngleSpeed);

        // Start and ending position
        float zPos = backgroundAnimator.FloatingObjectHolder.transform.parent.GetComponent<Canvas>().planeDistance;
        startPos = new Vector3(0, 0, zPos);
        endPos = new Vector3(0, 0, zPos);
        CheckAndReset();

        // Randomly chooses to cross along the x or y axis (hor or vert)
        int temp = UnityEngine.Random.Range(0, 2);
        // Randomly chooses to start on one side of the screen or the other
        int temp2 = UnityEngine.Random.Range(0, 2);
        int count = 0;
        if (temp == 0) {
            // Cross screen horizontally
            movingOnX = true;
            while (!backgroundAnimator.ValidPosition(startPos.y, false)) {
                startPos.y = UnityEngine.Random.Range(100, Screen.height-100);
                ++count;
                if (count > 100) { break; }
            }
            endPos.y = startPos.y;
            if (temp2 == 0) {
                // Start on left side, end on right
                startPos.x = -1 * size;
                endPos.x = Screen.width + size;
            } else {
                // Start on right side, end on left
                startPos.x = Screen.width + size;
                endPos.x = -1 * size;
            }
        } else {
            // Cross screen vertically
            movingOnX = false;
            while (!backgroundAnimator.ValidPosition(startPos.x, true)) {
                startPos.x = UnityEngine.Random.Range(100, Screen.width-100);
                ++count;
                if (count > 100) { break; }
            }
            endPos.x = startPos.x;
            if (temp2 == 0) {
                // Start at bottom, end at top
                startPos.y = -1 * size;
                endPos.y = Screen.height + size;
            } else {
                // Start at top, end at bottom
                startPos.y = Screen.height + size;
                endPos.y = -1 * size;
            }
        }
    }

    void CheckAndReset() {
        List<bool> onX = BackgroundAnimator.onX, onY = BackgroundAnimator.onY;

        if (onX == null || onY == null) { return; }

        bool reset = true;
        for (int i = 0; i < onX.Count; ++i) {
            if (onX[i] == false) { reset = false; }
        }
        if (reset) {
            for (int i = 0; i < onX.Count; ++i) {
                onX[i] = false;
            }
        }

        reset = true;
        for (int i = 0; i < onY.Count; ++i) {
            if (onY[i] == false) { reset = false; }
        }
        if (reset) {
            for (int i = 0; i < onY.Count; ++i) {
                onY[i] = false;
            }
        }
    }
}

// Class used for when a floating object is an image
public class FloatingImage : FloatingObject
{
    public Sprite sprite;

    public FloatingImage(BackgroundAnimator backgroundAnimator, GameObject prefab) : base(backgroundAnimator)
    {
        //int index = UnityEngine.Random.Range(0, Background.sprites.Count);
        //sprite = Background.sprites[index];

        sprite = Background.GetRandomSprite();

        // Setup shared variables
        this.SetupPrefab(prefab);

        // Setup image-specific stuff

        // Delete text
        GameObject obj = prefab.GetComponentInChildren<TMP_Text>().gameObject;
        GameObject.Destroy(obj);

        // Set rotation
        rotateX = Background.rotateX;
        rotateY = Background.rotateY;
        rotateZ = Background.rotateZ;
        
        // Set image sprite
        Image img = prefab.GetComponentInChildren<Image>();
        img.sprite = sprite;

        // Set color, if needed
        ImageTheme theme = prefab.GetComponentInChildren<ImageTheme>();
        if (Background.useThemeColors) {
            Array values = Enum.GetValues(typeof(WhichColor));
            WhichColor color = (WhichColor)UnityEngine.Random.Range(0, values.Length);

            theme.Half(color, Background.themeColorPercentage);
            theme.Reset();
        }
        else if (Background.useThemeLookTypes) {
            Array values = Enum.GetValues(typeof(LookType));
            LookType look = (LookType)UnityEngine.Random.Range(0, values.Length);

            theme.updateColor = true;
            theme.lookType = look;
            theme.Reset();
        }
    }
}

// Class that holds info or the numbers that float across the background
// ex. the number it is, its size, color, speed, etc
// Gets these variables randomly
public class FloatingNumber : FloatingObject
{
    public int number;

    public FloatingNumber(BackgroundAnimator backgroundAnimator, GameObject prefab) : base(backgroundAnimator)
    {
        // Actual number it is
        number = UnityEngine.Random.Range(0, 10);

        // Determines whether to rotate around X and Y, as well as Z, or not
        int temp = UnityEngine.Random.Range(0, 100);
        if (temp > 80) {
            rotateX = true;
            rotateY = true;
        }
        rotateZ = true;

        // Setup shared variables
        this.SetupPrefab(prefab);

        // Setup number-specific stuff

        // Delete image
        GameObject obj = prefab.GetComponentInChildren<Image>().gameObject;
        GameObject.Destroy(obj);

        // Set text
        TMP_Text text = prefab.GetComponentInChildren<TMP_Text>();
        text.text = number.ToString();

        // Determines what color it should be, depending on the theme
        TextTheme theme = prefab.GetComponentInChildren<TextTheme>();
        temp = UnityEngine.Random.Range(0, 100);
        if (temp > 75)                    { theme.lookType = LookType.text_accent; }
        else if (temp > 50 && temp <= 75) { theme.lookType = LookType.text_background; }
        else                              { theme.lookType = LookType.text_main; }
        theme.Reset();
    }
}