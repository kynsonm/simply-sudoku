using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditButtons : MonoBehaviour
{
    [SerializeField] Transform EditButtonsHolder;
    [SerializeField] GameObject GameNumberPrefab;
    [SerializeField] GameObject VerticalLayoutObject;

    [Space(10f)]
    [SerializeField] float originalSize;
    [SerializeField] float sizeMultiplierWhenOn;
    int vertEditorIndex = -1;

    public static bool UpdatedEditButtons = false;

    // Start is called before the first frame update
    void Awake() { OnEnable(); }
    void OnEnable() {
        if (!CheckObjects()) { return; }
        SetUpNumbers();
        SetUp();
    }

    // Edit number button press
    public void EditButtonPress(int number) {
        GameManager.EditMode = true;
        GameManager.NumberButtonPress(number);
        GameManager.EditMode = false;
    }

    // Turn edit numbers on or off
    public void Activate() {
        if (!CheckObjects()) { return; }
        Settings.ShowEditNumbers = !Settings.ShowEditNumbers;
        SetUp();
        UpdatedEditButtons = true;
    }
    
    void SetUp() {
        // If in main menu (not in game)
        if (EditButtonsHolder == null || VerticalLayoutObject.GetComponent<VerticalEditor>() == null) {
            return;
        }
        GameObject editNumbers = EditButtonsHolder.gameObject;
        VerticalEditor vert = VerticalLayoutObject.GetComponent<VerticalEditor>();
        // Need to turn them on, set up the numbers, and set new area size
        if (Settings.ShowEditNumbers) {
            editNumbers.SetActive(true);
            SetUpNumbers();
            vert.Sizes[vertEditorIndex] = sizeMultiplierWhenOn * originalSize;
            vert.UpdateVertEditor();
        }
        // Need to turn them off and reset area size
        else {
            editNumbers.SetActive(false);
            vert.Sizes[vertEditorIndex] = originalSize;
            vert.UpdateVertEditor();
        }

        // Update tile sizing 
        GameBoardCreator creator = GameObject.FindObjectOfType<GameBoardCreator>();
        if (creator != null) {
            creator.ResetBoardSizing();
        }
    }

    // Create each number
    public void SetUpNumbers() {
        // Destroy buttons already there
        foreach (Transform child in EditButtonsHolder) {
            GameObject.Destroy(child.gameObject);
        }
        // Create numbers
        int num = Mathf.Max(LevelInfo.BoardSizeToHeight(), LevelInfo.BoardSizeToWidth());
        for (int i = 0; i < num; ++i) {
            // Create obj and set name
            GameObject obj = Instantiate(GameNumberPrefab, EditButtonsHolder);
            obj.name = i + " Edit Number";
            // Set its text
            TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
            text.text = (i+1).ToString();
            // Set text color
            TextTheme theme = obj.GetComponentInChildren<TextTheme>();
            theme.lookType = LookType.text_accent;
            theme.Reset();
            // Set button onClick
            Button butt = obj.GetComponent<Button>();
            int index = i+1;
            butt.onClick.RemoveAllListeners();
            butt.onClick.AddListener(() => EditButtonPress(index));
        }
    }

    // Sets everything up
    bool CheckObjects() {
        // Check edit buttons holder
        if (EditButtonsHolder == null) {
            Debug.LogWarning("EditButtons: NO EDIT BUTTON HOLDER");
            return false;
        }
        // Check vertical layout object
        if (VerticalLayoutObject == null) {
            Debug.LogWarning("EditButtons: NO VERTICAL LAYOUT OBJECT");
            return false;
        }
        // Check vertical editor index
        if (vertEditorIndex == -1) {
            VerticalEditor vert = VerticalLayoutObject.GetComponent<VerticalEditor>();
            for (int i = 0; i < vert.Objects.Count; ++i) {
                if (vert.Objects[i].name == "Numbers and Buttons") {
                    vertEditorIndex = i;
                }
            }
            if (vertEditorIndex == -1) {
                Debug.LogWarning("EditButtons: NO NUMBERS AND BUTTONS NAME");
                return false;
            }
        }
        // Check original size
        if (originalSize <= 100f) {
            Debug.LogWarning("EditButtons: ORIGINAL SIZE NOT SET");
            return false;
        }
        sizeMultiplierWhenOn = (sizeMultiplierWhenOn <= 0.1f) ? 0.1f : sizeMultiplierWhenOn;
        return true;
    }
}
