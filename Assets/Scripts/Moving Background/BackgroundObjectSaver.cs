using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public static class BackgroundObjectSaver
{
    // ----- VARIABLES -----
    static int numberOfObjects = 0;
    static GameObject numberPrefab;
    static BackgroundAnimator backgroundAnimator;


    // ----- METHODS -----

    public static void Add() {
        ++numberOfObjects;
    }
    public static void Remove() {
        --numberOfObjects;
    }
    public static int NumberOfObjects() {
        return numberOfObjects;
    }


    // Set prefab
    public static void SetVariables(BackgroundAnimator backAnimator) {
        backgroundAnimator = backAnimator;
        numberPrefab = backAnimator.ObjectPrefab;
    }

    // Load numbers in and tween them
    public static void LoadObjects() {
        LoadObjects(false);
    }
    public static void LoadObjects(bool tweenThemInAndOut) {
        // Return if we don't wanna load any objects
        if (!CheckObjects()) { return; }
        if (numberOfObjects <= 2) { return; }

        // Get list of children objects on <holder>
        Transform holder = backgroundAnimator.FloatingObjectHolder.transform;
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in holder) {
            children.Add(child.gameObject);
        }

        bool dontMakeObjects = (Background.sprites == null || Background.sprites.Count == 0) && !Background.areNumbers;

        // Initialize variables
        int toMake = (int)MathF.Min(numberOfObjects, Background.maxNumber); 
        int made = 0;
        numberOfObjects = 0;

        float time = 0.7f * Settings.AnimSpeedMultiplier;
        float interval = time / toMake;

        // Destroy each child of <holder>
        for (int i = 0; i < children.Count; ++i) {
            // Get objects, and destroy it if !doTween
            GameObject obj = children[i];
            if (!tweenThemInAndOut) {
                GameObject.Destroy(obj);
                if (!dontMakeObjects) {
                    made = MakeObject(made, toMake, tweenThemInAndOut);
                }
                continue;
            }
            RectTransform rect = obj.GetComponent<RectTransform>();

            // Tween it out, destroy when it ends
            LeanTween.value(obj, 1f, 0f, time)
            .setEase(LeanTweenType.easeInBack)
            .setDelay(i * interval)
            .setOnUpdate((float value) => {
                rect.localScale = new Vector3(value, value, rect.localScale.z);
            })
            .setOnComplete(() => {
                GameObject.Destroy(obj);
                if (!dontMakeObjects) {
                    made = MakeObject(made, toMake, tweenThemInAndOut);
                }
            });
        }
    }

    public static int MakeObject(int alreadyMade, int toMake, bool tweenThemInAndOut) {
        if (alreadyMade >= toMake) { return alreadyMade; }
        if (backgroundAnimator == null) {
            bool allGood = CheckObjects();
            if (allGood) {
                Debug.LogWarning("BackgroundObjectSaver: MakeObject() -- Can't find background animator :(");
                return alreadyMade;
            }
        }

        float perc = UnityEngine.Random.Range(0f, 0.9f);
        backgroundAnimator.MakeObject(perc, tweenThemInAndOut);
        ++numberOfObjects;

        ++alreadyMade;
        return alreadyMade;
    }


    // ----- UTILITIES -----

    static bool CheckObjects() {
        bool allGood = true;

        if (backgroundAnimator == null) {
            backgroundAnimator = GameObject.FindObjectOfType<BackgroundAnimator>(true);
            if (backgroundAnimator == null) {
                Debug.Log("Could not find the background animator script?");
                allGood = false;
            }
        }

        if (numberPrefab == null) {
            if (backgroundAnimator != null) {
                numberPrefab = backgroundAnimator.ObjectPrefab;
            } else {
                backgroundAnimator = GameObject.FindObjectOfType<BackgroundAnimator>(true);
                if (backgroundAnimator != null) {
                    numberPrefab = backgroundAnimator.ObjectPrefab;
                }
            }

            if (numberPrefab == null) {
                Debug.Log("Theres no floating number prefab?");
                allGood = false;
            }
        }

        return allGood;
    }
}
