using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]

public static class TextSizeStatic
{
    // ----- VARIABLES -----

    public static TextSizeManager textSizeManager;


    // ----- METHODS -----

    // Clears all SameTextSizes in textSizeManager
    // For changing scenes really
    public static void CLEAR_ALL() {
        textSizeManager.allTextSizes.Clear();
        textSizeManager = null;
    }

    public static void Construct() {
        textSizeManager = GameObject.FindObjectOfType<TextSizeManager>();
        if (textSizeManager == null) {
            Debug.LogError("Could not find textSizeManager!");
        }
    }

    // Clear texts in textScript and get them again
    public static void Reset(SameTextSize textScript) {
        Debug.LogWarning("Resetting SameTextSize " + textScript.gameObject.gameObject);
        foreach (SameTextSizeClass text in textScript.texts) {
            if (text.theme == null) { continue; }
            text.theme.usingSameTextSize = false;
        }
        textScript.texts.Clear();
        GetTexts(textScript);
    }

    // Clear all stuff in textSizeManager
    public static void Reset() {
        textSizeManager.allTextSizes.Clear();
        foreach (SameTextSize size in GameObject.FindObjectsOfType<SameTextSize>(true)) {
            size.Reset();
        }
    }

    // Gets a lsit of TMP_Texts from 
    public static void GetTexts(SameTextSize textScript) {
        //Debug.Log("GETTING TEXTS FOR " + textScript.gameObject.name);

        List<SameTextSizeClass> texts = new List<SameTextSizeClass>();

        // Get texts from parent
        FindText(textScript.parent, texts, textScript);
        
        // Get texts from other parents
        if (textScript.otherParents != null && textScript.otherParents.Count != 0) {
            foreach (Transform parent in textScript.otherParents) {
                FindText(parent, texts, textScript);
            }
        }

        // Set it in the script
        textScript.texts = texts;
        if (textSizeManager != null) {
            textSizeManager.ResetSize(textScript);
        }

        textScript.UPDATE_TEXT = true;
    }
    // Recursive check for texts
    static void FindText(Transform parent, List<SameTextSizeClass> texts, SameTextSize textScript) {
        if (parent == null) {
            //Debug.LogWarning("Parent is null, returning");
            return;
        }
        // If this name is a name to skip, skip it!
        if (textScript.parentNamesToSkip == null || textScript.parentNamesToSkip.Count == 0) {
            //Debug.Log("Parent names to skip is null or is empty");
        }
        else if (textScript.parentNamesToSkip.Contains(parent.name)) {
            //Debug.Log("Same name == " + parent.name);
            return;
        }
        // Add this text if it exists
        TMP_Text text = parent.GetComponent<TMP_Text>();
        if (text != null) {
            texts.Add(new SameTextSizeClass(text.gameObject.transform));
        }
        // Search children
        foreach (Transform child in parent) {
            FindText(child, texts, textScript);
        }
    }

    // Constructor
    static TextSizeStatic() {
        textSizeManager = GameObject.FindObjectOfType<TextSizeManager>();
        if (textSizeManager == null) {
            Debug.LogWarning("Text size manager is null!!");
        }
    }
}
