using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


// Structure to load, save, and retreive a saved level
public static class SavedLevels
{
    // ----- CLASSES FOR SAVING AND LOADING -----

    [System.Serializable]
    public class SavedLevelsClass {
        [SerializeField] public List<SavedLevelClass> savedLevels;
        public SavedLevelsClass() {
            savedLevels = new List<SavedLevelClass>();
        }
    }

    [System.Serializable]
    public class SavedLevelClass : SavedLevel {
        // Progress level
        [SerializeField] new public List<SavedLevelProgressRow> progress;

        // Edit numbers at each position
        [SerializeField] public EditNumbersClass editNumbersClass;

        public SavedLevelClass(SavedLevel savedLevel) : base(savedLevel) {
            // Set progress list
            this.progress = new List<SavedLevelProgressRow>();
            if (savedLevel.progress != null) { 
                foreach (var row in savedLevel.progress) {
                    SavedLevelProgressRow progRow = new SavedLevelProgressRow();
                    foreach (var col in row) {
                        progRow.row.Add(col);
                    }
                    this.progress.Add(progRow);
                }
            }

            // Set EditNumbersClass
            this.editNumbersClass = new EditNumbersClass(savedLevel.editNumbers);

            Debug.Log("Created EditNumbersClass in SavedLevelClass constructor:\n" + this.editNumbersClass.ToString());
        }
    }

    [System.Serializable]
    public class SavedLevelProgressRow {
        [SerializeField] public List<int> row;
        public SavedLevelProgressRow() {
            row = new List<int>();
        }
    }

    [System.Serializable]
    public class EditNumbersClass {
        [SerializeField] public List<EditsTuple> edits;
        public EditNumbersClass(EditsClass editsClass) {
            string log = "Creating EditNumbersClass from EditsClass:\n";

            edits = new List<EditsTuple>();
            foreach (var pair in editsClass.edits) {
                edits.Add(new EditsTuple(pair.Key.Item1, pair.Key.Item2, new EditsList(pair.Value)));

                log += $" -- Adding pair at ({pair.Key.Item1}, {pair.Key.Item2}):  ";
                foreach (int num in pair.Value) {
                    log += num + " ";
                }
            }

            Debug.Log(log);
        }
        public override string ToString() {
            string str = "EditNumbersClass: \n";
            int count = 1;
            foreach (EditsTuple tuple in edits) {
                str += $"({count}) -- " + tuple.ToString() + "\n";
                ++count;
            }
            return str;
        }
    }

    [System.Serializable]
    public class EditsTuple {
        [SerializeField] public int x, y;
        [SerializeField] public EditsList editsList;
        public EditsTuple(int x, int y, EditsList editsList) {
            this.x = x;  this.y = y;  this.editsList = editsList;
        }
        public override string ToString() {
            string str = $"at ({x}, {y}):  {editsList.ToString()}";
            return str;
        }
    }

    [System.Serializable]
    public class EditsList {
        [SerializeField] public List<int> editNums;
        public EditsList(List<int> edits) {
            editNums = new List<int>();
            editNums = edits;
        }
        public override string ToString() {
            string str = "{";
            for (int i = 0; i < editNums.Count; ++i) {
                str += editNums[i].ToString();
                if (i != editNums.Count-1) {
                    str += ", ";
                } else {
                    str += "}";
                }
            }
            return str;
        }
    }


    // ----- Variables -----
    static List<SavedLevel> savedLevels = null;
    static string path = "";


    // ----- ACCESS -----
    public static List<SavedLevel> GetSavedLevels() {
        return savedLevels;
    }

    public static string Path() {
        return path;
    }

    public static new string ToString() {
        if (savedLevels == null) { return "Saved levels is null!\n"; }
        string str = "----- SAVED LEVELS: -----\n";
        int count = 1;
        foreach (SavedLevel level in savedLevels) {
            str += "-- Level " + count + ":\n";
            str += level.ToString() + "\n";
            ++count;
        }
        str += "----- ----- ----- -----";
        return str;
    }


    // ----- METHODS -----

    // Add SavedLevel to <savedLevels>
    public static void Add(SavedLevel level) {
        SavedLevel match = Find(level);
        if (match != null) {
            Debug.Log("Match found. Updating it");
            match = level;
        } else {
            if (!CheckSavedLevels()) { return; }
            Debug.Log("SAVING LEVEL:\n" + level.ToString());
            savedLevels.Add(level);
        }
        Save();
    }

    // Find specific box and board type combo
    public static List<SavedLevel> Find(BoardSize board, BoxSize box) {
        // Check saved levels
        if (!CheckSavedLevels()) { return null; }

        List<SavedLevel> lvls = new List<SavedLevel>();
        foreach (SavedLevel lvl in savedLevels) {
            if (lvl.boardSize == board && lvl.boxSize == box) {
                lvls.Add(lvl);
            }
        }
        if (lvls.Count == 0) {
            lvls = null;
        }
        return lvls;
    }

    // Returns the number of levels completed for the given board and box sizes
    public static int NumberOfLevelsCompleted(BoardSize board, BoxSize box) {
        // Check saved levels
        if (!CheckSavedLevels()) { return 0; }
        int num = 0;
        foreach (SavedLevel lvl in savedLevels) {
            if (lvl.boardSize == board && lvl.boxSize == box) {
                ++num;
            }
        }
        return num;
    }

    // Search all SavedLevels in <saved> for matching
    // Returns all matches, or null if no matches
    public static SavedLevel Find(BoardSize board, BoxSize box, Difficulty diff, int levelNumber) {
        if (!CheckSavedLevels()) { return null; }
        SavedLevel found = null;
        foreach (SavedLevel level in savedLevels) {
            // Bools to keep track of each match
            bool boardMatch = level.boardSize == board || board == BoardSize.invalid;
            bool boxMatch = level.boxSize == box || box == BoxSize.invalid;
            bool diffMatch = level.difficulty == diff || diff == Difficulty.invalid;
            bool levelMatch = level.levelNumber == levelNumber || levelNumber == -1;
            // Add if all matches are true
            if (boardMatch && boardMatch && diffMatch && levelMatch) {
                if (found != null) {
                    Debug.Log("There is already a saved level for this level!");
                }
                found = level;
            }
        }
        // If no matches, return null
        return found;
    }
    public static SavedLevel Find(SavedLevel level) {
        return Find(level.boardSize, level.boxSize, level.difficulty, level.levelNumber);
    }

    // Returns true if savedLevels exists, false if not and it cannot be found 
    static bool CheckSavedLevels() {
        if (savedLevels == null) {
            Load();
            if (savedLevels == null) {
                Debug.LogWarning("Saved levels is STILL NULL");
                return false;
            }
        }
        return true;
    }


    // ----- SAVE and LOAD -----

    // Set file path to the inputted path
    public static void Path(string newPath) {
        path = newPath;
    }

    // Save every level in <savedLevels> to JSON
    public static void Save() {
        // Create the file if it doesn't exist
        if (savedLevels == null) {
            savedLevels = new List<SavedLevel>();
            File.WriteAllText(Application.persistentDataPath + "/SavedLevels.json", "");
        }

        // Create SavedLevelClass for each 
        SavedLevelsClass toJSON = new SavedLevelsClass();
        foreach (SavedLevel level in savedLevels) {
            toJSON.savedLevels.Add(new SavedLevelClass(level));
        }
        string str = JsonUtility.ToJson(toJSON);
        
        Debug.Log("JSON STRING WRITE:\n" + str);

        // Write to file
        File.WriteAllText(Application.persistentDataPath + "/SavedLevels.json", str);
    }

    // Load all saved levels from JSON
    public static void Load() {
        // Create new instance of levels
        SavedLevelsClass levels = new SavedLevelsClass();

        // Get string of json file
        if (!File.Exists(Application.persistentDataPath + "/SavedLevels.json")) {
            Debug.LogWarning("NO JSON FILE EXISTS FOR SAVED LEVELS");
            Save();
            return;
        }
        string str = File.ReadAllText(Application.persistentDataPath + "/SavedLevels.json");

        Debug.Log("JSON FILE READ:\n" + str);

        // Get list of saved levels from json string
        levels = JsonUtility.FromJson<SavedLevelsClass>(str);
        if (levels == null) { levels = new SavedLevelsClass(); }
        if (levels.savedLevels == null) { levels.savedLevels = new List<SavedLevelClass>(); }
        
        savedLevels = new List<SavedLevel>();
        foreach (SavedLevelClass level in levels.savedLevels) {
            SavedLevel savedLevel = new SavedLevel(level);
            savedLevels.Add(savedLevel);
            Debug.Log("----- GOT SAVED LEVEL:\n" + savedLevel.ToString());
        }

        // For debugging in the inspector
        if (GameObject.FindObjectOfType<LevelSaver>() != null) {
            GameObject.FindObjectOfType<LevelSaver>().savedLevelsClass = levels;
        }

        return;
    }

    // FOR DEBUG PURPOSES ONLY!
    // Clears all saved levels
    public static void CLEAR() {
        savedLevels = new List<SavedLevel>();
        File.WriteAllText(Application.persistentDataPath + "/SavedLevels.json", "");
    }


    // ----- CONSTRUCTOR(S) & UTILITIES -----

    static int TryParse(string line, int lineNum, string message) {
        int num = -1;
        if (!int.TryParse(line, out num)) {
            Debug.Log("Invalid " + message + " on line " + lineNum);
        }
        return num;
    }

    static List<int> GetRow(string line) {
        List<int> row = new List<int>();
        for (int i = 0; i < line.Length; ++i) {
            char c = line[i];
            // Skip if a space
            if (c == ' ' || c == '\n') { continue; }
            // Hardcode numbers > 9
            if (c == 'A') { row.Add(10);  continue; }
            if (c == 'B') { row.Add(11);  continue; }
            if (c == 'C') { row.Add(12);  continue; }
            
            // Get number into <num>
            string s = "" + c;
            int num = -1;
            if (!int.TryParse(s, out num)) {
                Debug.Log("Invalid number in line " + line);
            }
            // And add it
            row.Add(num);
        }
        if (row.Count == 0) {
            return null;
        }
        return row;
    }
}

