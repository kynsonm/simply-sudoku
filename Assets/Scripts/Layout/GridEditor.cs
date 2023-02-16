using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]

public class GridEditor : MonoBehaviour
{
    // Useful variables
    public GameObject GridLayoutHolder;
    public List<GameObject> Objects = new List<GameObject>();

    [Space(10f)]
    [SerializeField] bool fitToWidth;
    [SerializeField] float cellHeightMultiplier;
    float lastHeightMultiplier;
    bool lastFitToWidth;
    int lastColumnCount;

    [Space(10f)]
    public Vector2 VertPaddingMultiplier;
    Vector2 lastVertMult = new Vector2();
    public float HorPaddingMultiplier;
    float lastHorMult = 0f;
    public Vector2 SpacingDividers;
    Vector2 lastSpacingDivs;
    Vector2 lastSpacingSize;

    int ignoreCount = 0;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Method to replace Update
    IEnumerator UpdateGridEditor() {
        while (true) {
            yield return new WaitForSeconds(0.5f);

            // Get this object if layout holder is null
            if (GridLayoutHolder == null) {
                GridLayoutHolder = gameObject;
            }

            // If child count changes, reset the variables w/ new children
            if (GridLayoutHolder.transform.childCount != Objects.Count + ignoreCount) {
                ResetSizes();
            }

            if (lastSpacingSize.x > 10000f || lastSpacingSize.y > 10000f) {
                SpacingDividers = new Vector2(0.1f, 0.1f);
                ResetSizes();
            }

            // Updating padding
            if (VertPaddingMultiplier != lastVertMult || HorPaddingMultiplier != lastHorMult
                || SpacingDividers != lastSpacingDivs) {

                ResetSizes();
                lastVertMult = VertPaddingMultiplier;
                lastHorMult = HorPaddingMultiplier;
                lastSpacingDivs = new Vector2(SpacingDividers.x, SpacingDividers.y);
            }

            // Update cell sizes taking into account new padding stuff
            if (fitToWidth != lastFitToWidth || cellHeightMultiplier != lastHeightMultiplier) {
                lastFitToWidth = fitToWidth;
                lastHeightMultiplier = cellHeightMultiplier;
                if (!fitToWidth) { continue; }

                ResetSizes();
            }
        }
    }

    public float ResetSizes() {
        // Checking stuff
        if (GridLayoutHolder == null) {
            GridLayoutHolder = this.gameObject;
        }
        if (lastSpacingSize.x > 10000f || lastSpacingSize.y > 10000f) {
            SpacingDividers = new Vector2(0.1f, 0.1f);
            ResetSizes();
        }

        // Do the important things
        AddAllElements();
        UpdateSidePadding();
        UpdateCellSizing();
        float size = SetRectSize();

        // Update vars
        lastVertMult = VertPaddingMultiplier;
        lastHorMult = HorPaddingMultiplier;
        lastSpacingDivs = new Vector2(SpacingDividers.x, SpacingDividers.y);
        lastFitToWidth = fitToWidth;
        lastHeightMultiplier = cellHeightMultiplier;

        // Return size
        return size;
    }

    float SetRectSize() {
        GridLayoutGroup grid = GridLayoutHolder.GetComponent<GridLayoutGroup>();
        float size = 0f;
        int rows = Mathf.CeilToInt((float)Objects.Count / (float)grid.constraintCount);
        size += rows * (grid.spacing.y + grid.cellSize.y);
        size += grid.padding.top + grid.padding.bottom;

        //Debug.Log($"Rows == {rows} at rowSize == {grid.spacing.y + grid.cellSize.y}");

        GridLayoutHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, size);
        return size;
    }

    // Adds all children of GridLayoutHolder to Objects
    void AddAllElements() {
        // Resets Objects and Sizes
        Objects.Clear();
        ignoreCount = 0;

        // For all children
        foreach (Transform child in GridLayoutHolder.transform) {
            LayoutElement layout = child.gameObject.GetComponent<LayoutElement>();
        
            // Otherwize, increase ignoreCount
            if (layout != null) {
                if (layout.ignoreLayout) {
                    ++ignoreCount;
                } else {
                    Objects.Add(child.gameObject);
                }
            } else {
                Objects.Add(child.gameObject);
            }
        }
    }

    // Updates the size of the cells to fit to the object width
    void UpdateCellSizing() {
        GridLayoutGroup grid = GridLayoutHolder.GetComponent<GridLayoutGroup>();
        cellHeightMultiplier = (cellHeightMultiplier <= 0f) ? 1f : cellHeightMultiplier;

        int col = grid.constraintCount;
        float space = grid.spacing.x;
        space *= col-1;

        float width = GridLayoutHolder.GetComponent<RectTransform>().rect.width;

        width -= (space + grid.padding.left + grid.padding.right);

        float newSize = width / (float)col;

        grid.cellSize = new Vector2(newSize, cellHeightMultiplier * newSize);
    }

    // Updates side padding size, depending on screen width
    void UpdateSidePadding() {
        GridLayoutGroup grid = GridLayoutHolder.GetComponent<GridLayoutGroup>();
        
        int pad = (int)(Screen.height * HorPaddingMultiplier);
        grid.padding.left = pad;
        grid.padding.right = pad;
        
        Vector2Int vertPad = new Vector2Int();
        vertPad.x = (int)(Screen.width * VertPaddingMultiplier.x);
        vertPad.y = (int)(Screen.width * VertPaddingMultiplier.y);
        grid.padding.top = vertPad.x;
        grid.padding.bottom = vertPad.y;

        if (SpacingDividers.x < 1f || SpacingDividers.y < 1f) {
            grid.spacing = new Vector2(0, 0);
        } else {
            grid.spacing = new Vector2(Screen.width / SpacingDividers.x, Screen.width / SpacingDividers.y);
        }
        lastSpacingSize = grid.spacing;

        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.gameObject.GetComponentInChildren<RectTransform>());
    }
}
