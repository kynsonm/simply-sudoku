using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[ExecuteInEditMode]

public class BoardTypeButton : MonoBehaviour
{
    public GameObject LevelSelectMenuObject;
    LevelSelectMenu menuInfo;

    public BoardSize boardSize;
    public BoxSize boxSize;
    public List<Difficulty> difficultiesToMake;
    TMP_Text text;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        SetUpButton();
    }

    // Returns if anything is wrong
    // Otherwise, set its name and onClick
    void SetUpButton() {
        // Return if objects are null
        if (menuInfo == null) {
            menuInfo = LevelSelectMenuObject.GetComponent<LevelSelectMenu>();
            if (menuInfo == null) {
                Debug.Log("BoardTypeButton: menuInfo is null");
                return;
            }
        }
        if (text == null) {
            text = gameObject.GetComponentInChildren<TMP_Text>();
            if (text == null) {
                Debug.Log("BoardTypeButton: text is null");
                return;
            }
        }
        // Set it up
        SetName();
        SetOnClick();
    }

    void SetOnClick() {
        Button butt = gameObject.GetComponent<Button>();
        butt.onClick.AddListener( delegate {
            menuInfo.SelectBoardType(this);
        });
    }

    void SetName() {
        string content = "";
        string title = "";

        // If it is a normal level
        if (boardSize == BoardSize._9x9 && boxSize == BoxSize._3x3) {
            title = "Standard Sudoku";
            content = title;
        } 
        // Otherwise, it is a weird board
        else {
            title += LevelInfo.BoardSizeToWidth(boardSize);
            content += LevelInfo.BoardSizeToWidth(boardSize);

            title += "x";
            content += "\U000000D7";

            title += LevelInfo.BoardSizeToHeight(boardSize);
            content += LevelInfo.BoardSizeToHeight(boardSize);

            title += " - ";
            content += " \U00002014 ";

            Vector2Int box = LevelInfo.BoxSizeVector(boxSize, boardSize);
            title += "[" + box.x + "x" + box.y + "]";
            content += "[" + box.x + "\U000000D7" + box.y + "]";
        }

        gameObject.name = title;
        text.text = content;
    }
}
