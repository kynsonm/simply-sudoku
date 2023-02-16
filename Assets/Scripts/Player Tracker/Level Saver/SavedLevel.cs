using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Used to save DateTime to JSON file
[System.Serializable]
public class DateTimeUtility {
    // Actual DateTime
    public DateTime dateTime { get; }

    // DateTime converted to binary (long)
    [SerializeField]
    public long dateTimeLong;

    // Constructor
    public DateTimeUtility(DateTime dateTime_in) {
        dateTime = dateTime_in;
        dateTimeLong = dateTime.ToBinary();
    }
}

// Used to save edit numbers ig
[System.Serializable]
public class EditsClass {
    public Dictionary<Tuple<int,int>, List<int>> edits;

    // --- Methods
    // Returns edits at specific location
    public List<int> EditsAtPosition(int x, int y) {
        if (edits.ContainsKey(new Tuple<int, int>(x, y))) {
            return edits[new Tuple<int, int>(x, y)];
        } else {
            return null;
        }
    }
    // Returns if edits at postion (x, y) exist
    public bool Contains(int x, int y) {
        return edits.ContainsKey(new Tuple<int, int>(x, y));
    }

    // --- Constructor(s)
    // Default
    public EditsClass() { edits = new Dictionary<Tuple<int, int>, List<int>>(); }
    // From game board (when doing SavedLevels saving)
    public EditsClass(Board gameBoard) {
        string log = "Creating EditsClass from GameBoard:\n";

        edits = new Dictionary<Tuple<int, int>, List<int>>();
        for (int i = 0; i < gameBoard.Rows(); ++i) {
            for (int j = 0; j < gameBoard.Cols(); ++j) {
                if (!gameBoard.Edit(i, j)) { continue; }
                edits.Add(new Tuple<int, int>(i, j), gameBoard.Edits(i, j));

                log += $" -- Adding ({i}, {j}) edits:  ";
                foreach (int num in gameBoard.Edits(i, j)) {
                    log += num + " ";
                }
            }
        }

        Debug.Log(log);
    }
    // From EditNumbersClass (when doing SavedLevels loading)
    public EditsClass(SavedLevels.SavedLevelClass savedLevelClass) {
        // If no edits are saved
        if (savedLevelClass.editNumbersClass == null) {
            edits = new Dictionary<Tuple<int, int>, List<int>>();
            return;
        }

        SavedLevels.EditNumbersClass editNumbersClass = savedLevelClass.editNumbersClass;
        edits = new Dictionary<Tuple<int, int>, List<int>>();
        // For every tuple class in input
        if (editNumbersClass.edits == null) { return; }
        foreach (var tup in editNumbersClass.edits) {
            // Add it to <edits>
            int x = tup.x,  y = tup.y;
            List<int> editNums = tup.editsList.editNums;
            edits.Add(new Tuple<int, int>(x, y), editNums);
        }
    }
}


// Structure to save completed & in progress levels
[System.Serializable]
public class SavedLevel
{
    // ----- Variables ------

    // Identifiers
    public BoardSize boardSize;    // Size of the board
    public BoxSize boxSize;        // Size of the box
    public Difficulty difficulty;  // Difficulty of the level
    public int levelNumber;        // Level number

    // Completed variables
    public bool isCompleted;        // Whether it has been completed or not
    public float completedTime;     // Time it took to complete the level
    public int completedNumActions; // How many actions it took to complete the level
    public DateTimeUtility completedDate;  // Date it was completed

    // Progress variables
    public List<List<int>> progress;// Current progress on the level
    public EditsClass editNumbers;  // Edit numbers at each position
    public float time;              // Time they have taken so far
    public int progressNumActions;  // How many actions taken so far
    public DateTimeUtility date;    // Date they last played the level


    // ----- Methods -----

    public void Clear(List<List<int>> empty) {
        progress = LevelInfo.Copy(empty);
        time = 0f;
    }


    // ----- Constructors -----

    public SavedLevel(BoardSize board, BoxSize box, Difficulty diff, int level, bool completed, float compTime,
    /**/              DateTime compDate, List<List<int>> prog, float curTime, DateTime curDate) {
        boardSize = board;
        boxSize = box;
        difficulty = diff;
        levelNumber = level;

        isCompleted = completed;
        completedTime = compTime;
        completedDate = new DateTimeUtility(compDate);

        time = curTime;
        date = new DateTimeUtility(curDate);

        if (prog == null) {
            progress = null;
        } else {
            progress = new List<List<int>>(prog);
        }

        editNumbers = new EditsClass();
    }
    public SavedLevel(SavedLevel savedLevel_in) : this(
        savedLevel_in.boardSize, savedLevel_in.boxSize, savedLevel_in.difficulty, savedLevel_in.levelNumber,
        savedLevel_in.isCompleted, savedLevel_in.completedTime, savedLevel_in.completedDate.dateTime, 
        savedLevel_in.progress, savedLevel_in.time, savedLevel_in.date.dateTime)
    {
        editNumbers = savedLevel_in.editNumbers;
    }
    public SavedLevel(SavedLevels.SavedLevelClass savedLevel_in) : this(
        savedLevel_in.boardSize, savedLevel_in.boxSize, savedLevel_in.difficulty, savedLevel_in.levelNumber,
        savedLevel_in.isCompleted, savedLevel_in.completedTime, savedLevel_in.completedDate.dateTime, 
        new List<List<int>>(), savedLevel_in.time, savedLevel_in.date.dateTime)
    {
        // Set progress from SavedLevelClass
        this.progress = new List<List<int>>();
        foreach (var row in savedLevel_in.progress) {
            List<int> progRow = new List<int>();
            foreach (var col in row.row) {
                progRow.Add(col);
            }
            this.progress.Add(progRow);
        }
        // Set edits from SavedLevelClass
        this.editNumbers = new EditsClass(savedLevel_in);
    }

    // Creates saved level based on inputted info
    public SavedLevel(BoardSize board, BoxSize box, Difficulty diff, int level) {
        // Identifiers
        boardSize = board;
        boxSize = box;
        difficulty = diff;
        levelNumber = level;

        // Completed vars
        isCompleted = false;
        completedTime = -1f;
        completedNumActions = 0;
        completedDate = new DateTimeUtility(DateTime.FromBinary(long.MinValue));

        // Progress vars
        progress = null;
        editNumbers = new EditsClass();
        time = 0f;
        progressNumActions = 0;
        date = new DateTimeUtility(DateTime.Now);
    }


    // ----- UTILITIES -----

    bool CheckWithSolution(List<List<int>> progress, List<List<int>> solution) {
        // Check if null
        if (progress == null) { return false; }
        // Check each spot
        for (int i = 0; i < solution.Count; ++i) {
            for (int j = 0; j < solution[i].Count; ++j) {
                if (solution[i][j] != progress[i][j]) { return false; }
            }
        }
        // Otherwise, its all good
        return true;
    }

    public override string ToString() {
        string s = "";
        s += "Board: " + boardSize.ToString() + "\n";
        s += "Box:   " + boxSize.ToString()   + "\n";
        s += "Difficulty:  " + difficulty.ToString() + "\n";
        s += "Level Num: " + levelNumber + "\n\n";

        s += "Completed?: " + isCompleted + "\n";
        s += "Complete Time: " + completedTime + "\n";
        s += "Complete Num Actions: " + completedNumActions + "\n";
        s += "Complete Date: " + completedDate.dateTime.ToString("{0:MM/dd/yy HH:mm:ss zzz}") + "\n\n";

        s += "Prog Time: " + time + "\n";
        s += "Prog Num Actions: " + progressNumActions + "\n";
        s += "Prog Date:  " + date.dateTime.ToString("MM/dd/yy HH:mm:ss zzz") + "\n\n";
        if (progress != null) {
            s += LevelInfo.BoardToString(progress);
        } else {
            s += "Progress is null\n";
        }
        return s;
    }
}