using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]

public class ScrollView : MonoBehaviour
{
    // Object that holds this scroll view
    public GameObject ScrollViewHolder;
    public GameObject ContentObject;

    // Base size for each object, depends on screen height
    public float BaseSize = 0f;

    [SerializeField] bool controlHeight = true;
    [SerializeField] bool controlWidth = true;

    // FOR EDITING --> Multiplier applied to BaseSize
    public float BaseSizeMultiplier;
    float lastBaseMult;

    // All the children gameObjects of scrollview
    [SerializeField] List<GameObject> children;

    public List<float> SizeMultipliers;
    List<float> prevSizeMults = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        lastBaseMult = BaseSizeMultiplier;
        GetThisObject();
        GetBaseSize();
        GetChildren();
        SetChildrenDimensions();
        UpdatePrevSizes();

        StartCoroutine(UpdateEnum(0.333f));
    }

    IEnumerator UpdateEnum(float time) {
        // Sets ScrollViewHolder to this object if it is null
        GetThisObject();
        // Gets base size, mostly just if it changes when editting
        GetBaseSize();
        // Get children objects if it changes
        if (ScrollViewHolder.transform.childCount != children.Count) {
            GetChildren();
            SetChildrenDimensions();
        }
        // Set dimensions of <Sizes> changes
        if (CheckSizes() || lastBaseMult != BaseSizeMultiplier) {
            SetChildrenDimensions();
            lastBaseMult = BaseSizeMultiplier;
        }

        yield return new WaitForSeconds(time);
        StartCoroutine(UpdateEnum(time));
    }

    // Set the height and width of each child
    public void SetChildrenDimensions() {
        if (!gameObject.activeSelf || !this.isActiveAndEnabled || !ContentObject.activeSelf)
        { return; }
        StartCoroutine(SetChildrenDimensionsEnum());
    }
    IEnumerator SetChildrenDimensionsEnum() {
        float content_size = 0;
        for (int i = 0; i < children.Count; ++i) {
            Vector2 size = new Vector2();
            size.x = ScrollViewHolder.GetComponent<RectTransform>().rect.width;
            size.y = SizeMultipliers[i] * BaseSize;

            if (!controlWidth) {
                size.x = children[i].GetComponent<RectTransform>().sizeDelta.x;
            }
            if (!controlHeight) {
                size.y = children[i].GetComponent<RectTransform>().sizeDelta.y;
            }

            if (size.x <= 0f || size.y <= 0f) {
                yield return new WaitForEndOfFrame();
                StartCoroutine(SetChildrenDimensionsEnum());
                yield break;
            }

            children[i].GetComponent<RectTransform>().sizeDelta = size;

            content_size += SizeMultipliers[i] * BaseSize;
            if (i != children.Count-1) {
                content_size += ContentObject.GetComponent<VerticalLayoutGroup>().spacing;
            }
        }

        content_size += ContentObject.GetComponent<VerticalLayoutGroup>().padding.bottom;
        content_size += ContentObject.GetComponent<VerticalLayoutGroup>().padding.top;

        Vector2 newSize = ContentObject.GetComponent<RectTransform>().sizeDelta;
        newSize.y = content_size;
        ContentObject.GetComponent<RectTransform>().sizeDelta = newSize;

        UpdatePrevSizes();
    }

    // Gets base height, depending on screen height
    void GetBaseSize() {
        BaseSize = BaseSizeMultiplier * Screen.height;
    }

    // Returns true if any of them are different
    // Returns false if they are all the same
    bool CheckSizes() {
        for (int i = 0; i < prevSizeMults.Count; ++i) {
            if (prevSizeMults[i] != SizeMultipliers[i]) {
                return true;
            }
        }
        return false;
    }

    void UpdatePrevSizes() {
        prevSizeMults = new List<float>(SizeMultipliers);
    }

    // Gets each child of ScrollViewHolder
    void GetChildren() {
        children.Clear();
        foreach (Transform child in ContentObject.transform) {
            children.Add(child.gameObject);
        }
        if (SizeMultipliers.Count < children.Count) {
            for (int i = 0; i < (children.Count - SizeMultipliers.Count); ++i) {
                SizeMultipliers.Add(1f);
            }
        }
    }

    // Sets ScrollViewHolder to this object if it is null
    void GetThisObject() {
        if (ScrollViewHolder == null) {
            ScrollViewHolder = gameObject;
        }
    }
}
