using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]

// Dimensions of the entire board
[System.Serializable]
[SerializeField]
public enum BoardSize
{
    _9x9,
    _12x9,
    _9x6, _9x4,
    _8x8, _8x6, _8x4,
    _6x6, _6x4,
    _4x4,
    invalid
}

// Dimensions of just the (smaller) boxes
[System.Serializable]
[SerializeField]
public enum BoxSize
{
    _3x3, _4x2, _3x2, _2x2,
    invalid
}

// Selected difficulty
[System.Serializable]
[SerializeField]
public enum Difficulty
{
    simp, easy, med, hard, expert, expertplus,
    invalid
}


public static class LevelInfo
{
    // Usable level file that the selected file comes from
    public static TextAsset LevelFile;
    public static int LevelNumber;

    // Full file path to load a level file with
    static string FilePath;

    // Variables that specify important info
    public static BoardSize CurrentBoardSize;
    public static BoxSize CurrentBoxSize;
    public static Difficulty CurrentDifficulty;
    public static bool levelSelected = false;

    // The actual game board stuff
    public static List<List<int>> EmptyLevel;
    public static List<List<int>> Solution;
    private static Vector2Int boardDimensions;
    private static Vector2Int boxDimensions;


    public static List<List<int>> Copy(List<List<int>> tmp) {
        List<List<int>> copy = new List<List<int>>();

        for (int i = 0; i < tmp.Count; ++i) {
            List<int> row = new List<int>();
            for (int j = 0; j < tmp[i].Count; ++j) {
                int num = tmp[i][j];
                row.Add(num);
            }
            copy.Add(row);
        }

        return copy;
    }

    public static void GetBoards() {
        if (LevelFile == null) {
            Debug.Log("No board to get (invalid level file)");
            return;
        }

        // Set each level to all 0s
        EmptyLevel = new List<List<int>>();
        Solution = new List<List<int>>();
        for (int i = 0; i < boardDimensions.y; ++i) {
            List<int> row = new List<int>();
            for (int j = 0; j < boardDimensions.x; ++j) {
                row.Add(0);
            }
            EmptyLevel.Add(row);
        }
        for (int i = 0; i < boardDimensions.y; ++i) {
            List<int> row = new List<int>();
            for (int j = 0; j < boardDimensions.x; ++j) {
                row.Add(0);
            }
            Solution.Add(row);
        }

        // String of the entire level
        string all = LevelFile.text;
        // Number of indexes in one ENTIRE level (w/ solution)
        int scale = (2*BoardSizeToHeight()) * (2*BoardSizeToWidth()+1) + 3;
        // Number of indexes in one board (ex. JUST empty or solution)
        int levelSize = BoardSizeToHeight() * (2 * BoardSizeToWidth()+1);

        // Starting index
        int index = LevelNumber * scale;
        int startIndex = index;
        // Max index, in case it is out of range
        int maxIndex = all.Length;
        if (index >= maxIndex-10) {
            Debug.Log("Level index " + index + " is out of bounds for number " + LevelNumber);
            --LevelNumber;
            GetBoards();
            return;
        }
        maxIndex = (LevelNumber+1) * scale;

        // For indexing <Grid>
        int x = 0, y = 0;
        int width = BoardSizeToWidth();
        int height = BoardSizeToHeight();
        
        // For every index in the level
        int count = 0;
        bool emptyLevel = true;
        while (index < maxIndex) {

            // Find the right number for this character
            int num;
            switch (all[index])
            {
                case 'A':  num = 10;  break;
                case 'B':  num = 11;  break;
                case 'C':  num = 12;  break;
                case '\n':
                case ' ': 
                    num = -1; 
                    break;
                default: {
                    string temp = "" + all[index];
                    if (!int.TryParse(temp, out num)) {
                        Debug.Log("This character is invalid lmao");
                    }
                    break;
                }
            }
            ++index;
            
            // Stop if space or newline
            if (num == -1) { continue; }

            if (emptyLevel) {
                EmptyLevel[x][y] = num;
            } else {
                Solution[x][y] = num;
            }

            // Update grid coordinates
            ++y;
            if (y >= width) {
                ++x;
                y = 0;
            }
            if (x >= height) {
                emptyLevel = false;
                x = 0;
                y = 0;
            }

            // Stop if count is too high (shouldnt ever happen but JIC)
            ++count;
            if (count > 2000) {
                Debug.Log("Count got too high (something went wrong)");
                break;
            }
        }
    }

    public static int NumberOfLevels(BoardSize board, BoxSize box, Difficulty diff) {
        string path = FindLevelPath(board, box, diff);
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null) {
            Debug.Log("NumberOfLevels: Could not find level file with path: " + path);
            return -1;
        }
        // All levels
        string full = textAsset.text;
        // Number of indexes per level
        int scale = (2*BoardSizeToHeight(board)) * (2*BoardSizeToWidth(board)+1) + 3;
        // Max index in full level file
        int maxIndex = full.Length;

        return maxIndex / scale;
    }


    public static void SetLevel(BoardSize boardSize, BoxSize boxSize, Difficulty difficulty, int number) {
        CurrentBoardSize = boardSize;
        CurrentBoxSize = boxSize;
        CurrentDifficulty = difficulty;
        LevelNumber = number;

        boardDimensions.x = BoardSizeToWidth();
        boardDimensions.y = BoardSizeToHeight();

        SetLevel();
    }
    public static void SetLevel() {
        SetLevelFile();

        if (LevelFile == null) {
            Debug.Log("Could not find level file with path: " + FilePath);
            return;
        }

        Debug.Log("Getting boards for level: " + FilePath);
        GetBoards();
        
        GameManager.ActionsBack = new List<GameAction>();
        GameManager.ActionsForward = new List<GameAction>();
        GameManager.isCompleted = false;
        
        levelSelected = true;
    }

    public static bool NextLevel() {
        ++LevelNumber;
        SetLevel();
        if (LevelFile == null) {
            return false;
        } else {
            return true;
        }
    }


    static void SetLevelFile() {
        FindLevelPath();
        LevelFile = Resources.Load<TextAsset>(FilePath);
    }

    // Turn the variables into a usable string
    static void FindLevelPath() {
        FilePath = "Levels/";
        // If it is a regular level, do something different
        if (CurrentBoardSize == BoardSize._9x9 && CurrentBoxSize == BoxSize._3x3) {
            FilePath += "Normal";
            FindLevelPathNormal();
        }
        else {
            FilePath += "Weird";
            FindLevelPathWeird();
        }
    }
    static string FindLevelPath(BoardSize board, BoxSize box, Difficulty diff) {
        string str = "Levels/";
        // If it is a regular level, do something different
        if (board == BoardSize._9x9 && box == BoxSize._3x3) {
            str += "Normal";
            str += FindLevelPathNormal(diff);
        }
        else {
            str += "Weird";
            str += FindLevelPathWeird(board, box, diff);
        }
        return str;
    }

    // Finds level name given that it is a normal 9x9, 3x3 board
    //   (since they have different names)
    static void FindLevelPathNormal() {
        string name = "/";
        switch (CurrentDifficulty)
        {
            case Difficulty.easy:       name += "Easy";         break;
            case Difficulty.med:        name += "Medium";       break;
            case Difficulty.hard:       name += "Hard";         break;
            case Difficulty.expert:     name += "Expert";       break;
            case Difficulty.expertplus: name += "ExpertPlus";   break;
            default:
                Debug.Log("Invalid normal level difficulty: " + CurrentDifficulty.ToString());
                break;
        }

        name += "Levels";

        FilePath += name;
    }
    static string FindLevelPathNormal(Difficulty diff) {
        string str = "/";
        switch (diff)
        {
            case Difficulty.easy:       str += "Easy";         break;
            case Difficulty.med:        str += "Medium";       break;
            case Difficulty.hard:       str += "Hard";         break;
            case Difficulty.expert:     str += "Expert";       break;
            case Difficulty.expertplus: str += "ExpertPlus";   break;
            default:
                Debug.Log("Invalid normal level difficulty: " + CurrentDifficulty.ToString());
                break;
        }
        str += "Levels";
        return str;
    }

    // Finds level name given that it is a weird level
    //  (since it is systematic given box/board size)
    static void FindLevelPathWeird() {
        string name = "/";

        // Get box size to string
        // ex. "/2x2"
        string temp = CurrentBoxSize.ToString();
        temp = temp.Remove(0, 1);
        name += temp;

        // Get board size to string
        // ex. "/2x2(4x4)"
        temp = CurrentBoardSize.ToString();
        temp = temp.Remove(0, 1);
        name += ("(" + temp + ")");

        // Continue folder name
        FilePath += name;

        // Get difficulty to string
        // ex. "/2x2(4x4)easy"
        temp = CurrentDifficulty.ToString();
        name += temp;

        FilePath += name;
    }
    static string FindLevelPathWeird(BoardSize board, BoxSize box, Difficulty diff) {
        string str = "/";
        string temp;
        // "/2x2"
        temp = box.ToString();
        temp = temp.Remove(0, 1);
        str += temp;
        // "/2x2(4x4)"
        temp = board.ToString();
        temp = temp.Remove(0, 1);
        str += "(" + temp + ")";
        // "/2x2(4x4)/2x2(4x4)easy"
        str += str;
        str += diff.ToString();
        return str;
    }


    public static string LevelInfoString() {
        string str = "";
        str += BoardSizeString();
        str += " " + "\U00002013" + " ";
        str += BoxSizeString();
        return str;
    }
    public static string BoardSizeString() {
        string str = "";
        str += BoardSizeToWidth() + "\U000000D7" + BoardSizeToHeight();
        return str;
    }
    public static string BoxSizeString() {
        string str = "";
        str += "[" + boxDimensions.x + "\U000000D7" + boxDimensions.y + "]";
        return str;
    }

    // Using y axis of BoardSize
    public static int BoardSizeToHeight() {
        return BoardSizeToHeight(CurrentBoardSize);
    }
    public static int BoardSizeToHeight(BoardSize size) {
        switch (size)
        {
            case BoardSize._12x9:
                return 12;
            case BoardSize._9x9:
            case BoardSize._9x6:
            case BoardSize._9x4:
                return 9;
            case BoardSize._8x8:
            case BoardSize._8x6:
            case BoardSize._8x4:
                return 8;
            case BoardSize._6x4:
            case BoardSize._6x6:
                return 6;
            case BoardSize._4x4:
                return 4;
            default:
                Debug.Log("No height for this board size");
                return 0;
        }
    }

    // Using x axis of BoardSize
    public static int BoardSizeToWidth() {
        return BoardSizeToWidth(CurrentBoardSize);
    }
    public static int BoardSizeToWidth(BoardSize size) {
        switch (size)
        {
            case BoardSize._12x9:
            case BoardSize._9x9:
                return 9;
            case BoardSize._8x8:
                return 8;
            case BoardSize._9x6:
            case BoardSize._8x6:
            case BoardSize._6x6:
                return 6;
            case BoardSize._9x4:
            case BoardSize._8x4:
            case BoardSize._6x4:
            case BoardSize._4x4:
                return 4;
        }
        Debug.Log("No width for this board size");
        return 0;
    }

    // Using y axis of BoxSize
    public static Vector2Int BoxSizeVector() {
        return BoxSizeVector(CurrentBoxSize, CurrentBoardSize);
    }
    public static Vector2Int BoxSizeVector(BoxSize boxSize, BoardSize boardSize) {
        if (boxSize == BoxSize._3x3) { return new Vector2Int(3, 3); }
        if (boxSize == BoxSize._2x2) { return new Vector2Int(2, 2); }

        int h = BoardSizeToHeight(boardSize);
        int w = BoardSizeToWidth(boardSize);
        // Box dimensions are also flipped
        if (h > w) {
            switch (boxSize) {
                case BoxSize._3x2:  return new Vector2Int(2, 3);
                case BoxSize._4x2:  return new Vector2Int(2, 4);
            }
        }
        switch (boxSize) {
            case BoxSize._3x2:  return new Vector2Int(3, 2);
            case BoxSize._4x2:  return new Vector2Int(4, 2);
        }
        Debug.Log("No BoxSize case for this box size");
        return new Vector2Int(-1, -1);
    }

    static string DifficultyToString() {
        return DifficultyToString(CurrentDifficulty);
    }
    public static string DifficultyToString(Difficulty diff) {
        switch (diff)
        {
            case Difficulty.simp:       return "Simple";
            case Difficulty.easy:       return "Easy";
            case Difficulty.med:        return "Medium";
            case Difficulty.hard:       return "Hard";
            case Difficulty.expert:     return "Expert";
            case Difficulty.expertplus: return "Expert+";
        }
        Debug.Log("Invalid level difficulty: " + diff.ToString());
        return "Invalid";
    }

    public static string BoardToString(bool printEmpty) {
        if (printEmpty) {
            return "Empty level:\n" + BoardToString(EmptyLevel);
        } else {
            return "Solution:\n" + BoardToString(Solution);
        }
    }
    public static string BoardToString(List<List<int>> level) {
        if (level == null) { return "Inputted level is null!\n"; }
        string str = "";
        for (int i = 0; i < level.Count; ++i) {
            for (int j = 0; j < level[i].Count; ++j) {
                str += (level[i][j] + " ");
            }
            str += "\n";
        }
        return str;
    }
}
