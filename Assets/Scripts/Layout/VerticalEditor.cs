using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Want script happening all the time
[ExecuteInEditMode]

public class VerticalEditor : MonoBehaviour
{
    // Useful variables
    public GameObject VerticalLayoutHolder;
    public List<GameObject> Objects = new List<GameObject>();
    public List<float> Sizes = new List<float>();

    public Vector2 VertPaddingMultiplier;
    Vector2 lastVertMult = new Vector2();
    public float HorPaddingMultiplier;
    float lastHorMult = 0f;
    public float SpacingDivider;
    float lastSpacingDiv;
    float lastSpacingSize;

    int ignoreCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (VerticalLayoutHolder == null) {
            VerticalLayoutHolder = this.gameObject;
        }
        Objects = new List<GameObject>();
        AddAllElements();
        UpdateSidePadding();
    }

    private void OnEnable() {
        StopAllCoroutines();
        StartCoroutine(UpdateVertEditorEnum());
    }

    // DEBUG ONLY -- REMOVE THIS IN RELEASE PLS
    float time = 0f;
    void Update() {
        if (time < 0.5f) {
            time += Time.deltaTime;
            return;
        }
        time = 0f;
        UpdateVertEditor();
    }

    // Edit enum
    IEnumerator UpdateVertEditorEnum() {
        while (true) {
            yield return new WaitForSeconds(0.5f);
            UpdateVertEditor();
        }
    }

    // Update is called once per frame
    public void UpdateVertEditor()
    {
        if (!gameObject.activeInHierarchy) { return; }
        // Get this object if layout holder is null
        if (VerticalLayoutHolder == null) {
            VerticalLayoutHolder = gameObject;
        }

        // If child count changes, reset the variables w/ new children
        if (VerticalLayoutHolder.transform.childCount != Objects.Count + ignoreCount) {
            AddAllElements();
        }

        if (lastSpacingSize > 10000f) {
            SpacingDivider = 0.01f;
            UpdateSidePadding();
        }

        // Makes sure that all heights are set to respective Sizes
        CheckAndUpdate();

        // Updating padding
        if (VertPaddingMultiplier != lastVertMult || HorPaddingMultiplier != lastHorMult
            || lastSpacingDiv != SpacingDivider) {
            UpdateSidePadding();
            lastVertMult = VertPaddingMultiplier;
            lastHorMult = HorPaddingMultiplier;
            lastSpacingDiv = SpacingDivider;
        }
    }

    // Adds all children of VerticalLayoutHolder to Objects
    void AddAllElements() {
        // Resets Objects and Sizes
        Objects.Clear();
        Sizes.Clear();
        ignoreCount = 0;

        // For all children
        foreach (Transform child in VerticalLayoutHolder.transform) {
            LayoutElement layout = child.gameObject.GetComponent<LayoutElement>();
            
            // If objects does not have a layout element, give it one!
            if (layout == null) {
                //Debug.Log("Vertical: Not good gameobject in " + child.gameObject.name);
                child.gameObject.AddComponent<LayoutElement>();
                layout = child.gameObject.GetComponent<LayoutElement>();
                layout.ignoreLayout = false;
            }

            // If ignoreLayout is off, add this object to Objects and its size to Sizes
            if (!layout.ignoreLayout) {
                // If preferredHeight is off, its set to -1 (but we want it on!)
                if (layout.preferredHeight == -1) {
                    layout.preferredHeight = 0;
                }
                Objects.Add(child.gameObject);
                Sizes.Add(layout.preferredHeight);
            }
            // Otherwize, increase ignoreCount
            else {
                ++ignoreCount;
            }
        }
    }

    // Checks that each Object's preferred height is set to Size
    void CheckAndUpdate() {
        for (int i = 0; i < Objects.Count; ++i) {
            if (Objects[i] == null) { continue; }
            if (Objects[i].GetComponent<LayoutElement>().preferredHeight != Sizes[i]) {
                Objects[i].GetComponent<LayoutElement>().preferredHeight = Sizes[i];
            }
        }
    }

    // Updates side padding size, depending on screen width
    void UpdateSidePadding() {
        VerticalLayoutGroup vert = VerticalLayoutHolder.GetComponent<VerticalLayoutGroup>();
        
        int pad = (int)(Screen.width * HorPaddingMultiplier);
        vert.padding.left = pad;
        vert.padding.right = pad;
        
        Vector2Int vertPad = new Vector2Int();
        vertPad.x = (int)(Screen.height * VertPaddingMultiplier.x);
        vertPad.y = (int)(Screen.height * VertPaddingMultiplier.y);
        vert.padding.top = vertPad.x;
        vert.padding.bottom = vertPad.y;

        if (SpacingDivider < 1f) {
            vert.spacing = 0;
        } else {
            vert.spacing = Screen.width / SpacingDivider;
        }
        lastSpacingSize = vert.spacing;

        LayoutRebuilder.ForceRebuildLayoutImmediate(vert.gameObject.GetComponentInChildren<RectTransform>());
    }
}