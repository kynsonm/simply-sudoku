using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Used in SameTextSize monobehaviour
// For each text object, it holds the TMP_Text, the text theme, its ChildCount, and object (rect) size
[System.Serializable]
public class SameTextSizeClass
{
    // Text
    public TMP_Text text;
    public TextTheme theme;

    // Transform
    [SerializeField] Transform transform;
    [SerializeField] int childCount;

    // Rect
    RectTransform rect;
    [SerializeField] Vector2 objectSize;


    // Check transform's child count with saved one
    public bool CheckChildren() {
        if (transform == null) { return false; }
        return (childCount == transform.childCount);
    }

    // Checks rect's rect size compared to objectSize
    public bool CheckSize() {
        bool sizeGood = (rect.rect.width == objectSize.x && rect.rect.height == objectSize.y);
        if (!sizeGood) {
            objectSize = new Vector2(rect.rect.width, rect.rect.height);
        }
        return sizeGood;
    }

    // Constructor
    public SameTextSizeClass(Transform transformIn) {
        // Check inputted transform
        transform = transformIn;
        if (transform == null) {
            Debug.LogWarning($"SameTextSizeClass: Transform is null!");
            return;
        }

        // Debugging info
        string name = transform.gameObject.name;
        string parName = transform.parent.gameObject.name;
        string log = "(" + parName + " --> " + name + ")";

        // Set variables
        rect = transform.GetComponent<RectTransform>();
        childCount = transform.childCount;
        objectSize = new Vector2(rect.rect.width, rect.rect.height);

        // Check text on the object
        text = transform.GetComponent<TMP_Text>();
        if (transform == null) {
            Debug.LogWarning($"SameTextSizeClass: {log}: TMP_Text is null!");
            return;
        }

        // Check text theme on the object
        theme = transform.GetComponent<TextTheme>();
        if (theme == null) {
            Debug.LogWarning($"SameTextSizeClass: {log}: TextTheme is null!");
            return;
        }
        theme.usingSameTextSize = true;
    }
}

public class SameTextSize : MonoBehaviour
{
    // Parent objects to check for same text size
    public Transform parent;
    public List<Transform> otherParents;
    public float textSizeMultiplier = 1f;   // Number to multiply the min found value by
    public bool UPDATE_TEXT = false;        // Update the text right away (DEBUG)
    public List<string> parentNamesToSkip = new List<string>();   // Skip parents with these names

    // Variables that keep information
    //[HideInInspector]
    public List<SameTextSizeClass> texts = null;
    [HideInInspector] public bool isActive = false;

    // For tracking
    bool firstRun = true;
    bool gettingTexts = false;


    // ----- METHODS -----

    // Returns whether this script needs to update text sizes or not
    public bool NeedsToUpdate() {
        // Need to update for the first time
        if (firstRun) {
            firstRun = false;
            return true;
        }

        // If we had to get new children
        if (!CheckChildren()) {
            return true;
        }

        // Check sizes
        bool needsUpdate = false;
        foreach (SameTextSizeClass text in texts) {
            // Checks if saved size is same as current size
            // Resets saved size if not, and returns false
            if (!text.CheckSize()) {
                needsUpdate = true;
            }
        }
        return needsUpdate;
    }

    // Checks if every saved child count is the same as the transform's child count
    //   If so, do nothing!
    //   If not, it resets <texts>
    // Returns whether it had to be reset or not
    public bool CheckChildren() {
        if (texts == null) {
            TextSizeStatic.Reset(this);
            return false;
        }
        if (texts.Count == 0 && parent.childCount != 0) {
            TextSizeStatic.Reset(this);
            return false;
        }

        bool allGood = true;
        foreach (SameTextSizeClass text in texts) {
            if (!text.CheckChildren()) {
                allGood = false;
                break;
            }
        }
        if (!allGood) {
            TextSizeStatic.Reset(this);
        }
        return allGood;
    }

    public void Reset() {
        gettingTexts = true;
        TextSizeStatic.GetTexts(this);
        gettingTexts = false;
        UPDATE_TEXT = true;
    }


    // ----- MONOBEHAVIOUR STUFF -----

    // Even if object is off
    private void Awake() {
        // Get parent
        if (parent == null) {
            parent = gameObject.transform;
        }

        gettingTexts = true;
        TextSizeStatic.GetTexts(this);
        gettingTexts = false;
    }

    // Before first frame
    private void Start() {
        // Get parent
        if (parent == null) {
            parent = gameObject.transform;
        }

        // Idk, just to be sure
        if (texts == null || texts.Count == 0) {
            if (!gettingTexts) {
                TextSizeStatic.GetTexts(this);
            }
        }
        // An object was deleted?
        foreach (SameTextSizeClass text in texts) {
            if (text.text == null && !gettingTexts) {
                TextSizeStatic.GetTexts(this);
                return;
            }
        }
    }
    // -------------------------------
}
