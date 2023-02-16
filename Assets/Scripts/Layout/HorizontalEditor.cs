using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Want script happening all the time
[ExecuteInEditMode]

public class HorizontalEditor : MonoBehaviour
{
    // Useful variables
    public GameObject HorizontalLayoutHolder;
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
        if (HorizontalLayoutHolder == null) {
            HorizontalLayoutHolder = this.gameObject;
        }
        Objects = new List<GameObject>();
        AddAllElements();
        UpdateSidePadding();
    }

    private void OnEnable() {
        StopAllCoroutines();
        StartCoroutine(UpdateHorEditorEnum());
    }

    // DEBUG ONLY -- REMOVE THIS IN RELEASE PLS
    float time = 0f;
    void Update() {
        if (time < 0.5f) {
            time += Time.deltaTime;
            return;
        }
        time = 0f;
        UpdateHorEditor();
    }

    // Update is called once per frame
    void UpdateHorEditor() {
        if (!gameObject.activeInHierarchy) { return; }
        // Get this object if layout holder is null
        if (HorizontalLayoutHolder == null) {
            HorizontalLayoutHolder = gameObject;
        }

        // If child count changes, reset the variables w/ new children
        if (HorizontalLayoutHolder.transform.childCount != Objects.Count + ignoreCount) {
            AddAllElements();
        }

        if (lastSpacingSize > 10000f) {
            SpacingDivider = 0.01f;
            UpdateSidePadding();
        }

        // Makes sure that all widths are set to respective Sizes
        // TODO: Make this not depend on GetComponent<>
        CheckAndUpdate();

        // Updating padding
        if (VertPaddingMultiplier != lastVertMult || HorPaddingMultiplier != lastHorMult
            || SpacingDivider != lastSpacingDiv) {
            
            UpdateSidePadding();
            lastVertMult = VertPaddingMultiplier;
            lastHorMult = HorPaddingMultiplier;
            lastSpacingDiv = SpacingDivider;
        }
    }
    IEnumerator UpdateHorEditorEnum()
    {
        while (true) {
            yield return new WaitForSeconds(0.5f);
            UpdateHorEditor();
        }
    }

    // Adds all children of HorizontalLayoutHolder to Objects
    void AddAllElements() {
        // Resets Objects and Sizes
        Objects.Clear();
        Sizes.Clear();
        ignoreCount = 0;

        // For all children
        foreach (Transform child in HorizontalLayoutHolder.transform) {
            LayoutElement layout = child.gameObject.GetComponent<LayoutElement>();
            
            // If objects does not have a layout element, give it one!
            if (layout == null) {
                //Debug.Log("Horizontal: Not good gameobject in " + child.gameObject.name);
                child.gameObject.AddComponent<LayoutElement>();
                layout = child.gameObject.GetComponent<LayoutElement>();
                layout.ignoreLayout = false;
            }

            // If ignoreLayout is off, add this object to Objects and its size to Sizes
            if (!layout.ignoreLayout) {
                // If preferredWidth is off, its set to -1 (but we want it on!)
                if (layout.preferredWidth == -1) {
                    layout.preferredWidth = 0;
                }
                Objects.Add(child.gameObject);
                Sizes.Add(layout.preferredWidth);
            }
            // Otherwize, increase ignoreCount
            else {
                ++ignoreCount;
            }
        }
    }

    // Checks that each Object's preferred width is set to Size
    void CheckAndUpdate() {
        //Debug.Log("Horizontal update: Objects count == " + Objects.Count);
        int count = 0;
        for (int i = 0; i < Objects.Count; ++i) {
            if (Objects[i] == null) { continue; }
            ++count;
            if (Objects[i].GetComponent<LayoutElement>().preferredWidth != Sizes[i]) {
                Objects[i].GetComponent<LayoutElement>().preferredWidth = Sizes[i];
            }
        }
        //Debug.Log("Horizontal update: Objects updated: " + count);
    }

    // Updates side padding size, depending on screen width
    void UpdateSidePadding() {
        HorizontalLayoutGroup hor = HorizontalLayoutHolder.GetComponent<HorizontalLayoutGroup>();
        
        int pad = (int)(Screen.width * HorPaddingMultiplier);
        hor.padding.left = pad;
        hor.padding.right = pad;
        
        Vector2Int horPad = new Vector2Int();
        horPad.x = (int)(Screen.height * VertPaddingMultiplier.x);
        horPad.y = (int)(Screen.height * VertPaddingMultiplier.y);
        hor.padding.top = horPad.x;
        hor.padding.bottom = horPad.y;

        if (SpacingDivider < 1f) {
            hor.spacing = 0;
        } else {
            hor.spacing = Screen.width / SpacingDivider;
        }
        lastSpacingSize = hor.spacing;

        LayoutRebuilder.ForceRebuildLayoutImmediate(hor.gameObject.GetComponentInChildren<RectTransform>());
    }
}