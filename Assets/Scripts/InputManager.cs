using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputUtility {
    public static bool Shift() {
        //return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
    public static bool Alt() {
        //return Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
        return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
    }
    public static bool Tab() {
        return Input.GetKeyDown(KeyCode.Tab);
        //return Input.GetKey(KeyCode.Tab);
    }
    public static bool Control() {
        //return Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }
    public static bool Number(int number) {
        int bar = (int)KeyCode.Alpha0;
        int pad = (int)KeyCode.Keypad0;
        return Input.GetKeyDown((KeyCode)(bar+number)) || Input.GetKeyDown((KeyCode)(pad+number));
    }
    public static bool Up() {
        return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
    }
    public static bool Down() {
        return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
    }
    public static bool Left() {
        return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
    }
    public static bool Right() {
        return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
    }
}

public class InputManager : MonoBehaviour
{
    bool inGame;

    // Start is called before the first frame update
    void Start()
    {
        inGame = SceneLoader.CurrentScene == SceneLoader.Scene.Game;
    }

    // Update is called once per frame
    void Update()
    {
        if (inGame) { CheckGameInputs(); }
        else { CheckMainMenuInputs(); }
    }

    // ----- IN GAME INPUTS -----

    bool checkGameEnumRunning = false;

    // Delay inputs if necessary
    IEnumerator CheckGameInputsEnum() {
        checkGameEnumRunning = true;
        yield return new WaitForSeconds(0.1f);
        checkGameEnumRunning = false;
    }

    // Start checking inputs if necessary
    void CheckGameInputs() {
        // Dont do anything if enum is running
        if (checkGameEnumRunning) { return; }

        // Used to turn on enum if an input is made
        bool inputMade = false;

        // Pause game on space key down
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (GameManager.IsPaused) {
                GameManager.GameBoard.Resume();
            } else {
                GameManager.GameBoard.Pause();
            }
            inputMade = true;
        }

        // Check these inputs
        bool shift = InputUtility.Shift();
        bool ctrl = InputUtility.Control();
        bool alt = InputUtility.Alt();

        // Numbers
        for (int i = 1; i < Mathf.Max(LevelInfo.BoardSizeToHeight(), LevelInfo.BoardSizeToWidth())+1; ++i) {
            if (InputUtility.Number(i)) {
                if (shift || ctrl || alt) {
                    GameManager.EditNumberPress(i, true);
                } else {
                    GameManager.NumberButtonPress(i);
                }
                inputMade = true;
            }
        }

        // Erase
        if (Input.GetKeyDown(KeyCode.Q)) {
            GameManager.EraseButtonPress(false);
            inputMade = true;
        }
        // Undo
        if (Input.GetKeyDown(KeyCode.E)) {
            GameManager.Undo();
            inputMade = true;
        }
        // Redo
        if (Input.GetKeyDown(KeyCode.R)) {
            GameManager.Redo();
            inputMade = true;
        }
        // Hint
        if (Input.GetKeyDown(KeyCode.F)) {
            GameObject.FindObjectOfType<GameHint>().HintButtonPress();
            inputMade = true;
        }

        // Edit mode
        if (Input.GetKeyDown(KeyCode.E) && (shift || ctrl || alt)) {
            GameManager.EditMode = true;
            GameManager.EraseMode = false;
            GameManager.SelectMode = true;
            inputMade = true;
        }
        // Erase mode
        if (Input.GetKeyDown(KeyCode.Q) && (shift || ctrl || alt)) {
            GameManager.EraseMode = true;
            GameManager.EditMode = false;
            GameManager.SelectMode = false;
            inputMade = true;
        }

        // Board info
        int boxX = LevelInfo.BoxSizeVector().x;
        int boxY = LevelInfo.BoxSizeVector().y;
        int boardX = LevelInfo.BoardSizeToHeight();
        int boardY = LevelInfo.BoardSizeToWidth();

        // Arrow keys
        bool selectMade = false;
        if (GameManager.GameBoard == null) { return; }
        int x = GameManager.GameBoard.SelectedCoords.x;
        int y = GameManager.GameBoard.SelectedCoords.y;
        if (InputUtility.Left()) {
            --y;
            if (y < 0) { y = boardY - 1; }
            inputMade = true;
            selectMade = true;
        }
        if (InputUtility.Right()) {
            ++y;
            if (y > boardY - 1) { y = 0; }
            inputMade = true;
            selectMade = true;
        }
        if (InputUtility.Up()) {
            --x;
            if (x < 0) { x = boardX - 1; }
            inputMade = true;
            selectMade = true;
        }
        if (InputUtility.Down()) {
            ++x;
            if (x > boardX - 1) { x = 0; }
            inputMade = true;
            selectMade = true;
        }

        // Tab
        if (InputUtility.Tab()) {
            // Previous box w/ shift click
            if (InputUtility.Shift()) {
                y -= boxX;
                if (y < 0) {
                    y += boardY;
                    x -= boxY;
                }
                if (x < 0) {
                    x += boardX;
                }
            }
            // Next box otherwise
            else {
                y += boxX;
                if (y >= boardY) {
                    y = y % boxX;
                    x += boxY;
                }
                if (x >= boardX) {
                    x = x % boardX;
                }
            }
            inputMade = true;
            selectMade = true;
        }

        // Select the new x and y
        if (!GameManager.GameBoard.buttonIsSelected && selectMade) {
            GameManager.GameBoard.Select(0, 0);
        }
        else if (x != GameManager.GameBoard.SelectedCoords.x || y != GameManager.GameBoard.SelectedCoords.y) {
            GameManager.GameBoard.Select(x, y);
        }

        // Start the delay coroutine
        if (inputMade) {
            StartCoroutine(CheckGameInputsEnum());
        }
    }


    // ----- MAIN MENU INPUTS -----

    // Start checking inputs if necessary
    void CheckMainMenuInputs() {

    }
    // Delay inputs if necessary
    IEnumerator CheckMainMenuInputsEnum() {

        yield break;
    }
}
