using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Dictates an event that happened
// Used for undoing and redoing actions
// Members:
//   x and y coordinates
//   GameActionType that happened
//   Number that was added/removed (if needed)
public class GameAction
{
    public int x;
    public int y;
    public GameActionType type;
    public List<int> nums;
    public List<int> prevNums;
    public bool isEdit;

    // Constructors
    public GameAction(int inX, int inY, GameActionType inType) {
        x = inX;
        y = inY;
        type = inType;
        nums = null;
        prevNums = null;
    }
    public GameAction(int inX, int inY, int inNum) {
        x = inX;
        y = inY;
        nums = new List<int>();
        nums.Add(inNum);
        prevNums = null;
    }
    public GameAction(int inX, int inY) {
        x = inX;
        y = inY;
        nums = null;
        prevNums = null;
    }

    // Gets an action to add to Redo or Undo for undid/redid action
    // TODO: Finish this
    public GameAction Reverse() {
        GameAction reverse = new GameAction(x, y);
        reverse.isEdit = isEdit;

        switch (type)
        {
        case GameActionType.Set: {
            if (isEdit) {
                reverse.type = GameActionType.Add;
            } else {
                reverse.type = GameActionType.Set;
            }
            reverse.nums = prevNums;
            reverse.prevNums = nums;
            break;
        }

        case GameActionType.Erase: {
            if (isEdit) {
                reverse.type = GameActionType.Add;
                reverse.isEdit = false;
            } else {
                reverse.type = GameActionType.Set;
            }
            reverse.nums = prevNums;
            reverse.prevNums = new List<int>();
            reverse.prevNums.Add(0);
            break;
        }

        case GameActionType.Add: {
            reverse.type = GameActionType.Remove;
            reverse.nums = nums;
            if (isEdit) {

            } else {
                reverse.prevNums = prevNums;
            }
            break;
        }

        case GameActionType.Remove: {
            reverse.type = GameActionType.Add;
            reverse.nums = nums;
            if (isEdit) {

            } else {
                reverse.prevNums = prevNums;
            }
            break;
        }
        }
        return reverse;
    }
}

public enum GameActionType
{
    Set,
    Erase,
    Add,
    Remove
}