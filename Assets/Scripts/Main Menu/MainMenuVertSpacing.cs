using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuVertSpacing : MonoBehaviour
{
    [SerializeField] GameObject VerticalLayoutEditorObject;
    [SerializeField] int IndexOfSpacing;
    [SerializeField] int IndexOfTitle;

    // Start is called before the first frame update
    void OnEnable() { Start(); }
    void Start()
    {
        StartCoroutine(SetNewSizes());
    }

    // Set its size
    IEnumerator SetNewSizes() {
        // Already have this accounted for
        if (Screen.width > Screen.height) { yield break; }

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Get the vertical editor
        VerticalEditor vert = VerticalLayoutEditorObject.GetComponent<VerticalEditor>();
        if (vert == null) {
            Debug.LogError("MainMenuVertSpacing: No VerticalEditor on the LayoutEditor object");
            yield break;
        }

        // Find new multiplier
        float ratio = (float)Screen.height / (float)Screen.width;
        float mult = 1f;

        if      (ratio < 0.5f)  { mult = 0f; }
        else if (ratio > 2.12f) { mult = 1f; }
        else {
            mult = (0.6211f * ratio) - 0.3106f;
        }

        mult = Mathf.Pow(mult, 0.25f);

        vert.Sizes[IndexOfTitle] *= mult;
        vert.Sizes[IndexOfSpacing] *= mult;

        yield return new WaitForEndOfFrame();
        ThemeController.ResetAll();
    }
}
