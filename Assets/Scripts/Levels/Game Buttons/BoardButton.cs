using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using static SoundClip;

// WIP / Test implementation of "using"
using Level = System.Collections.Generic.List<System.Collections.Generic.List<int>>;

[SerializeField]
public class Board
{
    // The actual game board!
    List<List<BoardButton>> board;
    public SavedLevel savedLevel;
    Level empty;      // Empty board
    Level solution;   // Solution
    Level progress;   // In-progress level

    // Level info
    BoardSize boardSize;
    BoxSize boxSize;
    Difficulty difficulty;
    int levelNum;

    // Selected stuff
    public bool buttonIsSelected;
    public Vector2Int SelectedCoords;


    // ----- ACCESS & LEVEL INFO -----

    public Level InProgressLevel() {
        return progress;
    }

    public void Save() {
        Debug.Log("Progress ==\n" + LevelInfo.BoardToString(progress));
        savedLevel.progress = new Level(progress);
        savedLevel.editNumbers = new EditsClass(this);
        savedLevel.date = new DateTimeUtility(DateTime.Now);
        savedLevel.time = GameManager.GameTime;
        Debug.Log("Gametime == " + GameManager.GameTime);
        SavedLevels.Add(savedLevel);
    }

    public void Complete() {
        // Get coins for first completion
        if (!savedLevel.isCompleted) {
            PlayerInfo.Coins += 10;
        }

        // Update completed time
        if (savedLevel.completedTime == -1 || savedLevel.completedTime == 0) {
            savedLevel.completedTime = GameManager.GameTime;
        } else {
            savedLevel.completedTime = MathF.Min(GameManager.GameTime, savedLevel.completedTime);
        }

        savedLevel.progress = null;
        savedLevel.date = new DateTimeUtility(DateTime.FromBinary(long.MinValue));
        savedLevel.time = 0f;

        savedLevel.isCompleted = true;
        savedLevel.completedDate = new DateTimeUtility(DateTime.Now);

        // Add to saved levels and increase achievement progress
        SavedLevels.Add(savedLevel);
        Achievements.LevelCompleted(boardSize, boxSize);
    }
    

    public void Success() {
        foreach (var col in board) {
            foreach (var but in col) {
                but.isOriginal = true;
            }
        }
    }


    // ----- METHODS -----

    // Resets the game to the original empty level
    public void Reset() {
        empty = LevelInfo.Copy(LevelInfo.EmptyLevel);

        for (int i = 0; i < empty.Count; ++i) {
            for (int j = 0; j < empty[i].Count; ++j) {
                int num = empty[i][j];
                board[i][j].Set(num);
            }
        }

        savedLevel.Clear(empty);
        progress = savedLevel.progress;
        GameManager.isClear = true;

        GameManager.GameTime = 0f;
        GameObject.FindObjectOfType<GameUIManager>().ResetTimeVariables();

        Save();
    }

    // Checks if there are any
    public bool isClear() {
        for (int i = 0; i < Rows(); ++i) {
            for (int j = 0; j < Cols(); ++j) {
                if (board[i][j].isEdit()) {
                    return false;
                }
                if (board[i][j].Is() != empty[i][j]) {
                    return false;
                }
            }
        }
        return true;
    }

    // Pauses the game
    // Turns all the buttons & text off, and changes the color stuff
    public void Pause() {
        if (GameManager.IsPaused) { return; }
        Uninteractable();
        MonobehaviourTween tween = GameObject.FindObjectOfType<MonobehaviourTween>();
        tween.FadeBoardButtons(board, false);

        Sound.Play(pause_button);
    }
    // Resumes the game
    // Turns all the colors back to normal
    public void Resume() {
        if (!GameManager.IsPaused) { return; }
        Interactable();
        MonobehaviourTween tween = GameObject.FindObjectOfType<MonobehaviourTween>();
        tween.FadeBoardButtons(board, true);

        Sound.Play(play_button);
    }

    // See if the solution matches current board
    public bool Check() {
        for (int i = 0; i < Rows(); ++i) {
            for (int j = 0; j < Cols(); ++j) {
                if (board[i][j].Is() != solution[i][j]) {
                    return false;
                }
            }
        }
        return true;
    }

    public void Uninteractable() {
        Unselect();

        // Get board holder
        CanvasGroup canv = board[0][0].buttonObject.transform.parent.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = board[0][0].buttonObject.transform.parent.gameObject.AddComponent<CanvasGroup>();
        }

        canv.interactable = false;
        canv.blocksRaycasts = false;
    }
    public void Interactable() {
        // Get board holder
        CanvasGroup canv = board[0][0].buttonObject.transform.parent.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = board[0][0].buttonObject.transform.parent.gameObject.AddComponent<CanvasGroup>();
        }
        
        canv.interactable = true;
        canv.blocksRaycasts = true;
    }

    // Returns whether coordinates are erasable or not
    // AKA: We don't wanna erase original numbers
    public bool Erasable(int x, int y) {
        return (!Is(x, y, 0) || Edit(x, y)) && !Ori(x, y);
    }

    public bool SelectedIsSolution() {
        return Ori(SelectedCoords.x, SelectedCoords.y);
    }

    public void MakeSelectedSolution() {
        int x = SelectedCoords.x;
        int y = SelectedCoords.y;
        int num = LevelInfo.Solution[x][y];
        LevelInfo.EmptyLevel[x][y] = num;
        Set(x, y, num);
        board[x][y].isOriginal = true;
        board[x][y].ResetTextTheme();
        Null(x, y);
        Check();
        Save();
    }

    // Get number of rows and columns
    public int Rows()       { return board.Count; }
    public int Cols()       { return board[0].Count; }
    public int Cols(int x)  { return board[x].Count; }

    // Get objects from coordinates
    public TMP_Text Text(int x, int y)     { return board[x][y].numberText; }
    public ImageTheme Theme(int x, int y)  { return board[x][y].imageTheme; }


    // ----- NUMBERS -----

    public void Set(int x, int y, int num)  {
        board[x][y].Set(num);
        progress[x][y] = num;
    }
    public int Num(int x, int y)            { return board[x][y].Is(); }
    public bool Zero(int x, int y)          { return Is(x, y, 0); }
    public bool Is(int x, int y, int num)   { return board[x][y].Is(num); }
    public bool Ori(int x, int y)           { return board[x][y].isOriginal; }


    // ----- EDITS -----

    public bool Edit(int x, int y)                  { return board[x][y].isEdit(); }
    public List<int> Edits(int x, int y)            { return board[x][y].Edits(); }
    public bool Contains(int x, int y, int num)     { return board[x][y].Contains(num); }
    public void Set(int x, int y, List<int> edits)  {
        board[x][y].SetEdit(edits);
        progress[x][y] = 0;
    }
    public void Add(int x, int y, int num)          {
        board[x][y].Add(num);
        progress[x][y] = 0;
    }
    public void Remove(int x, int y, int num)       {
        board[x][y].Remove(num);
        progress[x][y] = 0;
    }
    public void Null(int x, int y)                  { board[x][y].Null(); }
    public void Sort(int x, int y)                  { board[x][y].Sort(); }
    public void Sort() {
        for (int x = 0; x < board.Count; ++x) {
            for (int y = 0; y < board[x].Count; ++y) {
                Sort(x, y);
            }
        }
    }


    // ----- SELECTION -----

    // Sets selected button coordinates
    public void Select(int x, int y) {
        Unselect();
        SelectedCoords = new Vector2Int(x, y);
        buttonIsSelected = true;
        ChangeTileColors(true);
    }
    // Unselects selected button
    public void Unselect() {
        if (buttonIsSelected) {
            ChangeTileColors(false);
        }
        SelectedCoords = new Vector2Int(-1, -1);
        buttonIsSelected = false;
    }
    // Changes row and column and box to selected colors
    void ChangeTileColors(bool isSelecting) {
        ImageTheme theme;
        int x = SelectedCoords.x, y = SelectedCoords.y;

        if (!isSelecting) {
            for (int i = 0; i < Rows(); ++i) {
                for (int j = 0; j < Cols(i); ++j) {
                    theme = Theme(i, j);
                    if (theme == null) { return; }
                    board[i][j].ResetTextTheme();
                    if (board[i][j].isHalf && Settings.AlternateGameBoardColors) {
                        theme.Half(Color.black, 0.2f);
                    } else {
                        theme.StopHalf();
                    }
                    theme.Reset();
                }
            }
            return;
        }

        // Set halved looktype we want to use
        // And ratio of the new color to old color
        LookType half = LookType.UI_accent;
        float ratio = 0.333f;

        // Only highlight selected tile if this setting is off
        if (!Settings.HighlightTiles) {
            theme = Theme(x, y);
            if (theme == null) { return; }
            theme.Half(half, 0.5f);
            return;
        }

        // Change in row
        for (int i = 0; i < LevelInfo.BoardSizeToWidth(); ++i) {
            // Grid[x][i]
            theme = Theme(x, i);
            if (theme == null) { return; }
            theme.Half(half, ratio);
        }

        // Change in column
        for (int j = 0; j < LevelInfo.BoardSizeToHeight(); ++j) {
            // Grid[j][y]
            theme = Theme(j, y);
            if (theme == null) { return; }
            theme.Half(half, ratio);
        }

        // Change in box
        Vector2Int size = LevelInfo.BoxSizeVector();
        if (Rows() > Cols() || size.x != size.y) {
            size = new Vector2Int(size.y, size.x);
        }
        int startX = x - (x % size.x);
        int endX = startX + size.x;
        int startY = y - (y % size.y);
        int endY = startY + size.y;

        for (int i = startX; i < endX; ++i) {
            for (int j = startY; j < endY; ++j) {
                // Grid[i][j]
                theme = Theme(i, j);
                if (theme == null) { return; }
                theme.Half(half, ratio);
            }
        }

        // Change tile itself
        theme = Theme(x, y);
        if (theme == null) { return; }
        theme.Half(half, 0.5f);
    }


    // ----- CONSTRUCTOR -----
    public Board(List<List<GameObject>> objs, Level _nums) {
        Level nums = LevelInfo.Copy(_nums);

        // Check if dimensions match
        board = new List<List<BoardButton>>();
        if (objs.Count != nums.Count) {
            Debug.Log("Dimensions of rows don't match");
            board = null;
            return;
        }
        if (objs[0].Count != nums[0].Count) {
            Debug.Log("Dimensions of cols don't match");
            board = null;
            return;
        }

        // Set it up
        for (int i = 0; i < objs.Count; ++i) {
            // Create the column
            List<BoardButton> column = new List<BoardButton>();
            for (int j = 0; j < objs[i].Count; ++j) {
                BoardButton butt = new BoardButton(objs[i][j], nums[i][j]);
                butt.isHalf = checkHalf(i, j);
                butt.ResetTextTheme();
                column.Add(butt);
            }
            board.Add(column);
        }

        // Set solution and empty board
        empty = LevelInfo.Copy(LevelInfo.EmptyLevel);
        solution = LevelInfo.Copy(LevelInfo.Solution);
        progress = LevelInfo.Copy(empty);

        if (empty.Count != board.Count || empty[0].Count != board[0].Count) {
            Debug.Log("Dimensions of empty level and game board don't match");
        }
        if (solution.Count != board.Count || solution[0].Count != board[0].Count) {
            Debug.Log("Dimensions of solution level and game board don't match");
        }

        // Set variables
        boardSize = LevelInfo.CurrentBoardSize;
        boxSize = LevelInfo.CurrentBoxSize;
        difficulty = LevelInfo.CurrentDifficulty;
        levelNum = LevelInfo.LevelNumber;

        // Check for saved levels

        Debug.Log(SavedLevels.ToString());

        SavedLevel match = SavedLevels.Find(boardSize, boxSize, difficulty, levelNum);
        if (match != null) {
            string log = "Getting saved level from match:\n";

            Debug.Log("----- Match found:\n" + match.ToString());

            savedLevel = match;
            if (match.progress != null) {
                progress = new Level(match.progress);
                log += "Match's progress is not null, Copying from it";
            } else {
                progress = LevelInfo.Copy(empty);
                log += "Match's progress is null, Copying from empty level";
            }

            Debug.Log(log);
        } else {
            Debug.Log("No saved level for this level. Creating a new one");
            savedLevel = new SavedLevel(boardSize, boxSize, difficulty, levelNum);
        }
        GameObject.FindObjectOfType<GameUIManager>().SavedLevelTimeOffset = savedLevel.time;

        // Check progress boards
        if (progress != null && progress.Count != 0) {
            // Set it up from progress level
            for (int i = 0; i < board.Count; ++i) {
                for (int j = 0; j < board[i].Count; ++j) {
                    board[i][j].Set( progress[i][j] );
                }
            }
        } else {
            progress = new Level();
            progress = LevelInfo.Copy(LevelInfo.EmptyLevel);
            Debug.Log("Progress level is null");
        }

        // Set edits
        if (match == null) { return; }
        if (match.editNumbers == null) { return; }
        foreach (var pair in match.editNumbers.edits) {
            int x = pair.Key.Item1;
            int y = pair.Key.Item2;
            this.Set(x, y, pair.Value);
        }
    }

    // Returns whether the coordinate is supposed to be halved or not
    bool checkHalf(int x, int y) {
        // Get vars
        int boxSizeX = LevelInfo.BoxSizeVector().x;
        int boxSizeY = LevelInfo.BoxSizeVector().y;
        if (LevelInfo.BoardSizeToWidth() != LevelInfo.BoardSizeToHeight() || boxSizeX != boxSizeY) {
            int temp = boxSizeX;
            boxSizeX = boxSizeY;
            boxSizeY = temp;
        }
        int xBoxNum = (x / boxSizeX) + 1;
        int yBoxNum = (y / boxSizeY) + 1;

        // if odd row, need even col
        if (yBoxNum % 2 == 1) {
            if (xBoxNum % 2 == 0) {
                return true;
            }
        }
        // if even row, need odd col
        else {
            if (xBoxNum % 2 == 1) {
                return true;
            }
        }
        return false;
    }

    // Yeah idk how to describe this 
    public void ResetThemes() {
        ChangeTileColors(false);
        if (buttonIsSelected) {
            ChangeTileColors(true);
        }
        foreach (var row in board) {
            foreach (var butt in row) {
                butt.ResetTextTheme();
            }
        }
    }


    // ----- INDEXER -----
    public BoardButton this[int i, int j] {
        get { return board[i][j]; }
        set { board[i][j] = value; }
    }
}


[SerializeField]
public class BoardButton
{
    // Regular info
    int number;
    List<int> edits;
    public bool filled;
    public bool isOriginal;
    public bool isHalf;

    // Objects info
    public GameObject buttonObject;
    public Button button;
    public TMP_Text numberText;
    public TextTheme textTheme;
    public Image tile;
    public ImageTheme imageTheme;

    // Edits text info
    float originalMaxTextRatio = -1f;
    float originalTextSize = -1f;


    // ----- OTHERS -----

    public List<int> Edits() {
        return edits;
    }

    void TextColor() {
        // Original numbers are accented
        if (isOriginal) {
            textTheme.lookType = LookType.text_background;
            return;
        }

        // Edits are background
        // User numbers are main
        if (isEdit()) {
            textTheme.lookType = LookType.text_accent;
        } else {
            textTheme.lookType = LookType.text_main;
        }

        // Set the MaxTextRatio on textTheme if necessary
        // (aka it was an edit just before this)
        SetTextSizes();

        textTheme.Reset(false);
    }


    // ----- NUMBERS -----

    // Replace the number
    public void SetNumber(int num) {
        Clear();
        number = num;
        numberText.text = (num == 0) ? "" : number.ToString();
        numberText.fontSizeMax = 1000f;
        TextColor();
    }

    // Replace the number pt 2
    public void Set(int num) {
        SetNumber(num);
    }

    public int Is() {
        return number;
    }
    public bool Is(int num) {
        return (number == num) && (edits == null || Count() == 0);
    }


    // ----- EDITS -----

    public bool isEdit() {
        if (Edits() == null)    { return false; }
        if (Edits().Count == 0) { return false; }
        return true;
    }
    // Set text to edit numbers
    void UpdateEditText() {
        numberText.text = formatEditText();
        SetTextSizes();
    }
    // Set edits
    public void SetEdit(List<int> editsIn) {
        SetNumber(0);
        edits = new List<int>(editsIn);
        UpdateEditText();
        TextColor();
    }
    public bool Contains(int num) {
        return edits.Contains(num);
    }
    public void Add(int num) {
        if (Count() == 0) {
            SetNumber(0);
        }
        edits.Add(num);
        Sort();
        UpdateEditText();
        TextColor();
    }
    public bool Remove(int num) {
        bool happened = edits.Remove(num);
        if (Count() == 0) {
            SetNumber(0);
        }
        UpdateEditText();
        TextColor();
        return happened;
    }
    public void Sort()          { edits.Sort(); }
    public void Clear()         {
        if (edits != null) { edits.Clear(); }
        numberText.text = "";
        numberText.fontSizeMax = 1000f;
        TextColor();
    }
    public int  Count()         { return edits.Count; }
    public bool Empty()         { return edits.Count == 0; }
    public void Null()          {
        edits = null;
        TextColor();
    }


    // ----- Constructor & utilities -----

    // BoardButton constructor
    public BoardButton(GameObject BoardButtonHolder, int numberIn) {
        buttonObject = BoardButtonHolder;
        number = numberIn;
        edits = new List<int>();
        isHalf = false;

        button = buttonObject.GetComponent<Button>();
        numberText = buttonObject.GetComponentInChildren<TMP_Text>();
        textTheme = buttonObject.GetComponentInChildren<TextTheme>();
        tile = BoardButtonHolder.transform.Find("Tile Image").GetComponent<Image>();
        imageTheme = tile.gameObject.GetComponent<ImageTheme>();

        isOriginal = (number != 0);

        CheckObjects();
        TextColor();
    }

    // Check the objects to make sure its good
    bool CheckObjects() {
        bool allGood = true;
        allGood = allGood && Check(button, "Button");
        allGood = allGood && Check(numberText, "Number text");
        allGood = allGood && Check(tile, "Tile image");
        allGood = allGood && Check(imageTheme, "Tile image theme");
        return allGood;
    }
    bool Check(System.Object obj, string message) {
        if (obj == null) {
            Debug.Log(message + " is null");
            return false;
        }
        return true;
    }

    // Gets original max text ratio and text size
    void SetTextSizes() {
        // Get text info if necessary
        if ((originalMaxTextRatio <= 0f || originalTextSize <= 0f) && !isEdit()) {
            if (isEdit()) {
                Debug.Log("Something's not right and idk why");
                return;
            }
            originalTextSize = numberText.fontSize;
            originalMaxTextRatio = textTheme.MaxTextRatio;
        }

        // Set regular text size if necessary
        float mult = 0.75f;
        if (isEdit() && textTheme.MaxTextRatio != mult * originalMaxTextRatio) {
            textTheme.MaxTextRatio = mult * originalMaxTextRatio;
            numberText.fontSizeMax = mult * originalTextSize;
            return;
        }

        // Set edit text sizes if necessary
        if (!isEdit() && textTheme.MaxTextRatio != originalMaxTextRatio) {
            textTheme.MaxTextRatio = originalMaxTextRatio;
            numberText.fontSizeMax = originalTextSize;
        }
    }

    // Reset the theme
    public void ResetTextTheme() {
        if (imageTheme == null) { return; }
        if (isHalf && Settings.AlternateGameBoardColors) {
            imageTheme.Half();
        } else {
            TextColor();
            imageTheme.StopHalf();
            imageTheme.Reset();
        }
    }

    // Returns a string depending on the edit numbers
    // Formats it depending on certain stuff idk
    string formatEditText() {
        string str = "";
        if (edits == null || edits.Count == 0) { return ""; }

        bool oneNumber = true;
        int boxWidth = LevelInfo.BoxSizeVector().x;
        int numCount = 0;

        foreach (int num in edits) {
            str += num.ToString();
            if (num != edits[edits.Count-1] && numCount != boxWidth) {
                str += " ";
            }

            ++numCount;
            if (numCount >= boxWidth) {
                str += "\n";
                numCount = 0;
                oneNumber = false;
            }
        }

        oneNumber = oneNumber && numCount == 1;
        if (oneNumber) { return str; }

        bool addSpace = numCount != 0 && numCount < boxWidth;
        string prefix = addSpace ? "<#00000000>" : "";
        string suffix = addSpace ? "</color>" : "";
        str += prefix;
        for (int i = numCount; i != 0 && i <= boxWidth; ++i) {
            str += ".";
            if (i != boxWidth) { str += "."; }
        }
        str += suffix;

        return str;
    }
}
