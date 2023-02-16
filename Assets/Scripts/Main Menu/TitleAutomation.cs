using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitleAutomation : MonoBehaviour
{
    [SerializeField] GameObject TitleObject;
    [SerializeField] float PauseBetweenChanges;
    [SerializeField] float WaitWhenChanged;
    [SerializeField] float ChangeInterval;

    [Space(10f)]
    [SerializeField] string padOne;
    [SerializeField] string padTwo;
    [SerializeField] string padThree;

    TMP_Text title;
    string originalTitle;
    List<string> titles;

    bool enumRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        GetTitleObject();
        GetTitle();
        originalTitle = title.text;
        FindTitles();
        UpdateTitleAutomation();
    }

    // Update is called once per frame
    void UpdateTitleAutomation()
    {
        StartCoroutine(UpdateTitleAutomationEnum());
    }
    IEnumerator UpdateTitleAutomationEnum() {
        while (true) {
            if (TitleObject == null) { GetTitleObject(); }
            if (title == null) { GetTitle(); }
            if (!enumRunning) {
                StartCoroutine(TitleEnum());
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    // Loops title tweening FOREVER!!!
    IEnumerator TitleEnum() {
        // Just wait in the beginning for things to appear normal
        enumRunning = true;
        title.text = titles[0];
        yield return new WaitForSeconds(PauseBetweenChanges);

        // Run all the time ;)
        while (true) {
            FindTitles();

            title.text = titles[0];

            // Choose a random character from the ones that are replacable (and replace it)
            for (int i = 1; i < titles.Count; ++i) {
                title.text = titles[i];
                yield return new WaitForSeconds(ChangeInterval);
            }

            // Wait before changing it back
            yield return new WaitForSeconds(WaitWhenChanged);

            // And turn the titles back sequentially
            for (int i = titles.Count-1; i > 0; --i) {
                title.text = titles[i];
                yield return new WaitForSeconds(ChangeInterval);
            }

            title.text = titles[0];

            // Wait before doing it again
            yield return new WaitForSeconds(PauseBetweenChanges);
        }
    }

    void FindTitles() {
        titles = new List<string>();
        string text = originalTitle;

        titles.Add(replacePadding(text));

        while (letterCanBeReplaced(text)) {
            char replace = randomCharToReplace();
            while (!text.Contains(replace)) {
                replace = randomCharToReplace();
            }
            string num = charToNum(replace).ToString();
            int index = (Random.Range(0, 2) == 0) ? text.LastIndexOf(replace) : text.IndexOf(replace);

            text = text.Remove(index, 1);
            text = text.Insert(index, num);
            
            string toAdd = replacePadding(text);
            titles.Add(toAdd);
        }
    }

    bool letterCanBeReplaced(string str) {
        if (str.Contains('S')) { return true; }
        if (str.Contains('l')) { return true; }
        if (str.Contains('o')) { return true; }
        return false;
    }

    char randomCharToReplace() {
        switch (Random.Range(0, 3)) {
            case 0: return 'S';
            case 1: return 'l';
            case 2: return 'o';
        }
        return '~';
    }

    char charToNum(char letter) {
        switch (letter) {
            case 'S': return '5';
            case 'l': return '1';
            case 'o': return '0';
        }
        return '~';
    }

    // ----- Utilities -----

    void GetTitleObject() {
        if (TitleObject == null) {
            Debug.Log("Getting title object from this objet");
            TitleObject = gameObject;
        }
    }

    void GetTitle() {
        if (title == null) {
            title = TitleObject.GetComponent<TMP_Text>();
            if (title == null) {
                Debug.Log("No text object on the title object");
            }
        }
    }

    string replacePadding(string str) {
        string copy = str;
        return copy.Replace("..", padTwo).Replace(".", padOne).Replace("_", padThree);
    }
}
