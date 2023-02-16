using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using static SoundClip;

public static class GameManager
{
    public static Board GameBoard;

    public static bool SelectMode = true;
    public static bool EditMode = false;
    public static bool EraseMode = false;

    public static bool IsPaused;

    public static List<GameAction> ActionsBack = new List<GameAction>();
    public static List<GameAction> ActionsForward = new List<GameAction>();

    public static float GameTime;

    public static bool isClear = true;
    public static bool isCompleted = false;


    // Load game scene with currently selected level
    public static void LoadGame(GameObject refObject) {
        SceneLoader.LoadScene(SceneLoader.Scene.Game, refObject);
    }


    // Does something depending on what index was pressed
    public static void BoardButtonPress(int x, int y) {
        bool original = GameBoard.Ori(x, y);
        int num = GameBoard.Num(x, y);

        if (IsPaused) {
            Debug.Log("Game is paused");
            return;
        }
        
        if (EraseMode) {
            if (!original) {
                GameAction action = new GameAction(x, y, GameActionType.Erase);
                if (GameBoard.Edit(x, y)) {
                    action.isEdit = true;
                    action.prevNums = new List<int>(GameBoard[x,y].Edits());
                } else {
                    if (GameBoard.Zero(x, y)) { return; }
                    action.isEdit = false;
                    action.prevNums = new List<int>();
                    action.prevNums.Add(GameBoard.Num(x, y));
                }
                action.nums = new List<int>();
                action.nums.Add(0);

                DoAction(action);
            }
        }
        if (SelectMode) {
            if (GameBoard.buttonIsSelected &&
                GameBoard.SelectedCoords.x == x && GameBoard.SelectedCoords.y == y) {

                GameBoard.Unselect();
                //Sound.Play(deselect_spot);
                Sound.PlayBackward(tap_something);
            } else {
                GameBoard.Select(x, y);
                //Sound.Play(select_spot);
                Sound.Play(tap_something);
            }
        }
    }


    // Does a given action
    // Adds it to the ActionsBack list (for undoes) if specified
    public static void DoAction(GameAction action, bool addToList) {
        int x = action.x, y = action.y;
        if (GameBoard.Ori(x, y)) {
            Debug.Log("This is an original number, discarding DoAction");
            return;
        }

        isClear = false;
        bool makeSound = addToList;

        if (addToList) {
            ActionsBack.Add(action);
            if (ActionsForward.Count > 0) {
                ActionsForward.Clear();
            }
        }

        ++GameBoard.savedLevel.progressNumActions;
        Debug.Log($"Saved Level Actions: Prog = {GameBoard.savedLevel.progressNumActions} and Comp = {GameBoard.savedLevel.completedNumActions}");

        switch (action.type)
        {
        case GameActionType.Add: {
            if (action.isEdit) {
                foreach (int i in action.nums) {
                    if (!GameBoard.Contains(x, y, i)) {
                        GameBoard.Add(x, y, i);
                    }
                }
            } else {
                GameBoard.Set(x, y, action.nums);
            }
            ++Statistics.editsMade;
            Sound.Play(edit_button, makeSound);
            break;
        }

        case GameActionType.Remove: {
            if (action.isEdit) {
                foreach (int i in action.nums) {
                    GameBoard.Remove(x, y, i);
                }
            } else {
                GameBoard.Set(x, y, action.prevNums[0] );
            }
            Sound.Play(erase_button, makeSound);
            break;
        }

        case GameActionType.Erase: {
            GameBoard.Set(x, y, 0);
            ++Statistics.numbersErased;
            Sound.Play(erase_button, makeSound);
            goto default;
        }

        case GameActionType.Set: {
            GameBoard.Set(x, y, action.nums[0]);
            ++Statistics.numbersPlaced;
            Sound.Play(number_button, makeSound);
            break;
        }

        default: {
            Debug.Log("No case associated with this ActionType");
            break;
        }
        }

        isCompleted = GameBoard.Check();
        if (isCompleted) {
            CompleteLevel();
        }
    }
    // Does a given action and adds it to ActionsBack list (for undoes)
    public static void DoAction(GameAction action) {
        DoAction(action, true);
    }


    // Does an action given the number pressed and current state of the game
    public static void NumberButtonPress(int number) {
        if (GameBoard.buttonIsSelected && !EraseMode) {
            GameAction action = new GameAction(GameBoard.SelectedCoords.x, GameBoard.SelectedCoords.y);
            action.nums = new List<int>();
            action.prevNums = new List<int>();
            
            // Add or remove it from edit board 
            int x = GameBoard.SelectedCoords.x, y = GameBoard.SelectedCoords.y;
            // If edit mode is on
            if (EditMode) {
                // If there are already numbers in EditNums
                if (GameBoard.Edit(x, y)) {
                    // If those numbers contain this number, remove it
                    if (GameBoard.Contains(x, y, number)) {
                        action.type = GameActionType.Remove;
                    }
                    // Otherwise, add it
                    else {
                        action.type = GameActionType.Add;
                    }
                    // Number added/removed is this number
                    // w/ no previous numbers
                    // AND these are all edit numbers
                    action.nums.Add(number);
                    action.isEdit = true;
                }
                // Otherwise, we are replacing a board number
                else {
                    // In other words, we are adding inputted number
                    // w/ previous number to what was on the board
                    // AND replacing a board number (isEdit is false)
                    action.type = GameActionType.Add;
                    action.nums.Add(number);
                    action.prevNums.Add(GameBoard.Num(x, y));
                    action.isEdit = false;
                }
            }
            // Othersise, edit mode is off and we are just setting spot to number inputted
            else {
                if (GameBoard.Is(x, y, number)) {
                    return;
                }
                action.type = GameActionType.Set;
                action.nums.Add(number);
                action.prevNums.Add( GameBoard.Num(x, y) );
                action.isEdit = false;
            }

            DoAction(action);
        }
    }

    public static void EditNumberPress(int number) {
        EditNumberPress(number, false);
    }
    public static void EditNumberPress(int number, bool editMode_override) {
        if (GameBoard.buttonIsSelected) {
            GameAction action = new GameAction(GameBoard.SelectedCoords.x, GameBoard.SelectedCoords.y);
            action.nums = new List<int>();
            action.prevNums = new List<int>();
            
            // Add or remove it from edit board 
            int x = GameBoard.SelectedCoords.x, y = GameBoard.SelectedCoords.y;
            // If edit mode is on
            if (EditMode || editMode_override) {
                // If there are already numbers in EditNums
                if (GameBoard.Edit(x, y)) {
                    // If those numbers contain this number, remove it
                    if (GameBoard.Contains(x, y, number)) {
                        action.type = GameActionType.Remove;
                    }
                    // Otherwise, add it
                    else {
                        action.type = GameActionType.Add;
                    }
                    // Number added/removed is this number
                    // w/ no previous numbers
                    // AND these are all edit numbers
                    action.nums.Add(number);
                    action.isEdit = true;
                }
                // Otherwise, we are replacing a board number
                else {
                    // In other words, we are adding inputted number
                    // w/ previous number to what was on the board
                    // AND replacing a board number (isEdit is false)
                    action.type = GameActionType.Add;
                    action.nums.Add(number);
                    action.prevNums.Add(GameBoard.Num(x, y));
                    action.isEdit = false;
                }
            }
            else {
                return;
            }

            DoAction(action);
        }
    }

    // If any of the game buttons are pressed
    public static void EraseButtonPress() {
        EraseButtonPress(true);
    }
    public static void EraseButtonPress(bool okToSetEraseMode) {
        Sound.Play(erase_button);
        
        if (GameBoard.buttonIsSelected) {
            int x = GameBoard.SelectedCoords.x, y = GameBoard.SelectedCoords.y;
            if (GameBoard.Erasable(x, y)) {
                GameAction action = new GameAction(x, y, GameActionType.Erase);
                if (GameBoard.Edit(x, y)) {
                    action.isEdit = true;
                    action.prevNums = new List<int>(GameBoard.Edits(x, y));
                } else {
                    if (GameBoard.Zero(x, y)) { return; }
                    action.isEdit = false;
                    action.prevNums = new List<int>();
                    action.prevNums.Add( GameBoard.Num(x, y) );
                }
                action.nums = new List<int>();
                action.nums.Add(0);
                DoAction(action);
            }
        }
        else if (okToSetEraseMode) {
            SetMode(GameMode.EraseMode, !EraseMode);
        }
    }
    public static void EditButtonPress() {
        SetMode(GameMode.EditMode, !EditMode);
        Sound.Play(edit_button);
    }


    // Undoes the last action taken
    public static void Undo() {
        if (ActionsBack == null) {
            Debug.Log("ActionsBack is null");
            Sound.Play(tap_nothing);
            return;
        }
        if (ActionsBack.Count == 0) {
            Debug.Log("No actions to undo!");
            Sound.Play(tap_nothing);
            return;
        }

        Sound.Play(undo_button);

        GameAction toDo = ActionsBack[ActionsBack.Count-1].Reverse();
        ActionsForward.Add(toDo);
        ActionsBack.RemoveAt(ActionsBack.Count-1);

        ++Statistics.undoneActions;
        DoAction(toDo, false);
    }
    // Redoes the last action taken
    public static void Redo() {
        if (ActionsForward == null) {
            Debug.Log("ActionsForward is null");
            Sound.Play(tap_nothing);
            return;
        }
        if (ActionsForward.Count == 0) {
            Debug.Log("No actions to redo!");
            Sound.Play(tap_nothing);
            return;
        }

        Sound.Play(redo_button);

        GameAction toDo = ActionsForward[ActionsForward.Count-1].Reverse();
        ActionsBack.Add(toDo);
        ActionsForward.RemoveAt(ActionsForward.Count-1);

        ++Statistics.redoneActions;
        DoAction(toDo, false);
    }


    // Set current mode of the game
    // There are different cases for each mode
    static void SetMode(GameMode mode, bool turnOn) {
        if (turnOn) {
            switch (mode) {
                case GameMode.SelectMode:
                    SelectMode = true;  EditMode = false; EraseMode = false;
                    return;
                case GameMode.EditMode:
                    SelectMode = true;  EditMode = true;  EraseMode = false;
                    return;
                case GameMode.EraseMode:
                    SelectMode = false; EditMode = false; EraseMode = true;
                    return;
            }
        } else {
            switch (mode) {
                case GameMode.SelectMode:
                    SelectMode = false;
                    return;
                case GameMode.EditMode:
                    EditMode = false;
                    return;
                case GameMode.EraseMode:
                    EraseMode = false;
                    SelectMode = true;
                    return;
            }
        }
        Debug.Log("No case associated with this game mode");
    }

    // Everything that needs to happen when a level is completed
    public static void CompleteLevel() {
        playCompletionSound();

        // Check completion time
        if (GameTime < Statistics.quickestCompletion || Statistics.quickestCompletion <= 0f) {
            Statistics.quickestCompletion = GameTime;
        }

        // Convert game time to number of levels to increase count by
        int num = 1;
        TimeSpan gameTimeSpan = TimeSpan.FromSeconds(GameTime);
        if (gameTimeSpan.Minutes > 10) { ++num; }
        if (gameTimeSpan.Minutes > 30) { ++num; }
        if (gameTimeSpan.Hours >= 1) { ++num; }
        AdManager.LevelCompleted(num);

        // Update saved level numActions
        int progActs = GameBoard.savedLevel.progressNumActions;
        int compActs = GameBoard.savedLevel.completedNumActions;
        Debug.Log($"CompleteLevel: NUMBER OF ACTIONS: Prog = {progActs} and Comp = {compActs}");
        if (progActs < compActs || compActs == 0) {
            Debug.Log($"CompleteLevel: Replacing completed number of actions from {compActs} to {progActs}");
            GameBoard.savedLevel.completedNumActions = progActs;
        }
        GameBoard.savedLevel.progressNumActions = 0;
        Debug.Log("CompleteLevel: New level is\n" + GameBoard.savedLevel.ToString());

        // Update the game board
        isCompleted = true;
        GameBoard.Complete();

        // Start level complete process for various monobehaviours
        GameObject.FindObjectOfType<GameUIManager>(true).StopAllCoroutines();
        GameObject.FindObjectOfType<TopButtons>   (true).CompleteLevel();
        GameObject.FindObjectOfType<GameNumbers>  (true).CompleteLevel();
        GameObject.FindObjectOfType<GameButtons>  (true).CompleteLevel();
        GameObject.FindObjectOfType<SuccessScreen>(true).MakeSuccess();

        // Update statistics
        ++Statistics.levelsCompleted;
        ++Statistics.gamesCompletedToday;
        if (Statistics.gamesCompletedToday > Statistics.mostCompletesInaDay) {
            Statistics.mostCompletesInaDay = Statistics.gamesCompletedToday;
        }
    }

    // Depending on the time taken to complete the level, denoted by <GameTime>
    static void playCompletionSound() {
        // Play the funky success sound given <cutoff> % change of happening
        int cutoff = 5;
        if (UnityEngine.Random.Range(0, 100) < cutoff) {
            Sound.Play(success_funky);
            return;
        }

        Debug.Log("GameTime is: " + GameTime);

        float min = 60;
        float hour = 60 * min;

        if      (GameTime > hour)     { Sound.Play(success_large); }
        else if (GameTime > 15 * min) { Sound.Play(success_med); }
        else if (GameTime > 5 * min)  { Sound.Play(success_low); }
        else                          { Sound.Play(success_lowest); }
    }
}

public enum GameMode
{
    SelectMode,
    EditMode,
    EraseMode,
}