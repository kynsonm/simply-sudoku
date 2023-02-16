using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MeanTextSize : MonoBehaviour
{
    public class TextGroup {
        public TMP_Text text;
        public TextTheme theme;
        public int lastChildCount;
        public TextGroup(TMP_Text text_in, TextTheme theme_in) {
            text = text_in;
            theme = theme_in;
            lastChildCount = text.transform.childCount;
        }
    }

    // Variables
    [SerializeField] List<GameObject> MeanTextSizeParents;
    [SerializeField] List<int> ParentChildCounts;
    [SerializeField] List<string> NamesToSkip;
    List<TextGroup> texts;
    float meanTextSize;

    // Monobehaviour stuff
    void Start() {
        GetTexts();
    }
    void OnEnable() {
        StopAllCoroutines();
        if (!CheckTexts()) {
            StartCoroutine(FullReset());
        }
        StartCoroutine(KeepUpdating());
    }
    IEnumerator KeepUpdating() {
        while (true) {
            yield return new WaitForSeconds(0.5f);
            if (!CheckTexts()) {
                StartCoroutine(FullReset());
            }
        }
    }


    // ----- METHODS -----

    IEnumerator FullReset() {
        GetTexts();
        ResetTextSize();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        FindMeanTextSize();
        yield return new WaitForEndOfFrame();
        SetTextSize();
    }

    // Reset text sizes to get max
    void ResetTextSize() {
        foreach (var pair in texts) {
            pair.theme.updateTextSize = false;
            pair.theme.usingSameTextSize = true;
            pair.text.fontSizeMax = 10000f;
        }
    }

    // Find mean text size of
    void FindMeanTextSize() {
        if (!CheckTexts()) { return; }
        float sum = 0f;
        int count = 0;
        foreach (var pair in texts) {
            sum += (pair.theme.MaxTextRatio * pair.text.fontSize);
            ++count;
        }
        meanTextSize = sum / (float)count;
    }

    void SetTextSize() {
        foreach (var pair in texts) {
            pair.theme.updateTextSize = false;
            pair.theme.usingSameTextSize = true;
            pair.text.fontSizeMax = meanTextSize;
        }
    }


    // ----- UTILITIES -----

    // Get all the texts
    void GetTexts() {
        texts = new List<TextGroup>();
        ParentChildCounts = new List<int>();
        foreach (GameObject obj in MeanTextSizeParents) {
            ParentChildCounts.Add(obj.transform.childCount);
            GetTextsRecursive(obj.transform);
        }
    }
    void GetTextsRecursive(Transform parent) {
        // Check if it needs to be skipped
        if (NamesToSkip.Contains(parent.name)) { return; }
        // Add it if necessary
        TMP_Text text = parent.GetComponent<TMP_Text>();
        TextTheme theme = parent.GetComponent<TextTheme>();
        if (text != null && theme != null) {
            texts.Add(new TextGroup(text, theme));
        }
        // Add all children
        foreach (Transform child in parent) {
            GetTextsRecursive(child);
        }
    }

    // Check all the texts
    bool CheckTexts() {
        // If no texts, return false
        if (texts == null) { return false; }
        // Check if parent child counts are right
        for (int i = 0; i < MeanTextSizeParents.Count; ++i) {
            if (MeanTextSizeParents[i].transform.childCount != ParentChildCounts[i]) {
                Debug.Log("MTS: Child counts don't match -- Resetting");
                return false;
            }
        }
        // Check if anything is null... thats it?
        foreach (var pair in texts) {
            if (pair.text == null) { return false; }
            if (pair.theme == null) { return false; }
            if (pair.lastChildCount != pair.text.transform.childCount) { return false; }
        }
        return true;
    }

    // yeah idk it just does what it says
    string parText(TMP_Text text) {
        string name = text.gameObject.name;
        string parent = text.gameObject.transform.parent.name;
        return "{" + parent + " --> " + name + "}";
    }
}
