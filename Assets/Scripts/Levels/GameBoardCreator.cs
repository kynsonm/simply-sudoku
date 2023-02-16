using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameBoardCreator : MonoBehaviour
{
    public GameObject BoardArea;
    public GameObject BoardHolder;
    RectTransform boardRect;
    public GameObject TilePrefab;

    public float MaxWidthMultipler;
    public float MaxHeightMultiplier;

    public Sprite BoardBord, BoxBord, TileBord, Tile;

    Vector2Int boardSize = new Vector2Int();
    Vector2Int boxSize = new Vector2Int();

    float tileSize;

    [SerializeField]
    public static List<List<GameObject>> Grid = new List<List<GameObject>>();


    // Set each number depending on the text asset & level number
    void SetNumbers() {
        Debug.Log(LevelInfo.BoardToString(true));
        Debug.Log(LevelInfo.BoardToString(false));

        //List<List<int>> numbers = new List<List<int>>(LevelInfo.EmptyLevel);
        List<List<int>> numbers = LevelInfo.Copy(LevelInfo.EmptyLevel);

        // TODO: Update colors

        for (int x = 0; x < numbers.Count; ++x) {
            for (int y = 0; y < numbers[x].Count; ++y) {
                // Set name
                Grid[x][y].gameObject.name = "(" + x + ", " + y + ") Button";

                // Set text component
                if (numbers[x][y] == 0) {
                    Grid[x][y].GetComponentInChildren<TMP_Text>().text = "";
                } else {
                    Grid[x][y].GetComponentInChildren<TMP_Text>().text = numbers[x][y].ToString();
                }

                // Set button component
                int i = x, j = y;
                Grid[x][y].GetComponent<Button>().onClick.AddListener(
                    delegate { GameManager.BoardButtonPress(i, j); }
                );
            }
        }
    }

    // Set each border sprite (^ , < , down, >)
    //      Depending on board and box sizes
    void SetBorderSprites() {
        for (int i = 0; i < Grid.Count; ++i) {
            for (int j = 0; j < Grid[i].Count; ++j) {
                // Get images
                Transform trans = Grid[i][j].transform;
                Image top = trans.Find("Top Border").GetComponent<Image>();
                Image bot = trans.Find("Bottom Border").GetComponent<Image>();
                Image left = trans.Find("Left Border").GetComponent<Image>();
                Image right = trans.Find("Right Border").GetComponent<Image>();

                // Set board borders
                if (i == 0)              { top.sprite = BoardBord; }
                if (i == Grid.Count - 1) { bot.sprite = BoardBord; }
                if (j == 0)              { left.sprite = BoardBord; }
                if (j == Grid[i].Count-1){ right.sprite = BoardBord; }

                // Set box borders
                if (i % boxSize.y == 0 && i != 0)   { top.sprite = BoxBord; }
                if (i % boxSize.y == (boxSize.y-1) && i != Grid.Count-1)    { bot.sprite = BoxBord; }
                if (j % boxSize.x == 0 && j != 0)   { left.sprite = BoxBord; }
                if (j % boxSize.x == (boxSize.x-1) && j != Grid[i].Count-1) { right.sprite = BoxBord; }
            }
        }
    }

    // Instantiate each Tile prefab & set parent to BoardHolder
    //   AND add each created gameobject to <Grid>
    void CreateObjects() {
        foreach (List<GameObject> row in Grid) {
            foreach (GameObject obj in row) {
                Destroy(obj);
            }
        }

        Grid.Clear();
        for (int y = 0; y < boardSize.y; ++y) {
            List<GameObject> row = new List<GameObject>();
            for (int x = 0; x < boardSize.x; ++x) {
                GameObject obj = Instantiate(TilePrefab);
                obj.GetComponentInChildren<TMP_Text>().fontSizeMax = 1000;

                obj.transform.SetParent(BoardHolder.transform);
                row.Add(obj);

                // Make sure scale is right
                obj.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            }

            Grid.Add(row);
        }

        // Debug stuff
        for (int i = 0; i < Grid.Count; ++i) {
            for (int j = 0; j < Grid[i].Count; ++j) {
                Grid[i][j].GetComponentInChildren<TMP_Text>().text = " (" + i.ToString() + ", " + j.ToString() + ") ";
            }
        }
    }

    // Set the grid element to that on the <BoardHolder> gameobject
    //      Also set cell size and constraint count (depending on board size)
    void SetGridElement() {
        GridLayoutGroup grid = BoardHolder.GetComponent<GridLayoutGroup>();
        if (grid == null) {
            Debug.LogError("NO GRID ELEMENT!!");
        }

        grid.cellSize = new Vector2(tileSize, tileSize);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = boardSize.x;
    }

    // Set board dimensions, depending on <Board Area> dimensions
    //   Uses ratio of board height and box width to determine "largest" dimension
    // Also sets <tileSize> to fit neatlly within the area
    //   If it goes past the max tile size, resize everything depending on max tile size
    void SetBoardDimensions() {
        // Set size of the board
        Vector2 BoardAreaRectSize = BoardArea.GetComponent<RectTransform>().rect.size;
        float ts1 = (MaxHeightMultiplier * BoardAreaRectSize.y) / (float)boardSize.y;
        float ts2 = (MaxWidthMultipler * BoardAreaRectSize.x) / (float)boardSize.x;
        tileSize = (ts1 < ts2) ? ts1 : ts2;
        boardRect.sizeDelta = new Vector2((float)boardSize.x*tileSize, (float)boardSize.y*tileSize);

        // Set tile size, for grid layout
        if (tileSize > Screen.height / 10) {
            Debug.Log("Tile size is greater than max size");
            tileSize = (float)Screen.height / 10f;
            boardRect.sizeDelta = new Vector2(boardSize.x * tileSize, boardSize.y * tileSize);
        }

        boardRect.anchoredPosition = new Vector2(0f, 0f);
    }

    // Updates game board dimensions and tile size
    // Used if game board dimensions change for whatever reason
    public void ResetBoardSizing() {
        StartCoroutine(ResetBoardSizingEnum());
    }
    IEnumerator ResetBoardSizingEnum() {
        yield return new WaitForEndOfFrame();
        SetBoardDimensions();
        SetGridElement();
    }

    // Gets board and box sizes depending on the selected (Current) level
    void ReadLevelInfo() {
        string str;
        int num;

        // Get <boardSize>
        str = "" + LevelInfo.CurrentBoardSize.ToString()[1];
        int.TryParse(str, out num);
        boardSize.x = num;
        str = "" + LevelInfo.CurrentBoardSize.ToString()[3];
        int.TryParse(str, out num);
        boardSize.y = num;
        if (LevelInfo.CurrentBoardSize == BoardSize._12x9) {
            boardSize = new Vector2Int(12, 9);
        }

        // Get <boxSize>
        str = "" + LevelInfo.CurrentBoxSize.ToString()[1];
        int.TryParse(str, out num);
        boxSize.x = num;
        str = "" + LevelInfo.CurrentBoxSize.ToString()[3];
        int.TryParse(str, out num);
        boxSize.y = num;

        // Check if dimensions need to be swapped
        if (boardSize.x != boardSize.y) {
            // Swap board size
            int temp = boardSize.y;
            boardSize.y = boardSize.x;
            boardSize.x = temp;
            // Swap box sizes
            temp = boxSize.y;
            boxSize.y = boxSize.x;
            boxSize.x = temp;

            Debug.LogWarning("SWAPPING BOARD AND BOX TO " + boardSize.ToString() + " and " + boxSize.ToString());
        }
    }

    // Creates a level depending on selected level in <LevelInfo>
    IEnumerator CreateLevel() {
        yield return new WaitForEndOfFrame();
        
        LevelInfo.SetLevel();
        ReadLevelInfo();

        SetBoardDimensions();
        SetGridElement();
        CreateObjects();
        SetBorderSprites();
        SetNumbers();

        GameManager.GameBoard = new Board(Grid, LevelInfo.EmptyLevel);
        GameManager.GameBoard.ResetThemes();
        GameManager.isClear = GameManager.GameBoard.isClear();
        GameManager.GameBoard.Resume();
    }

    // BASIC STUFF

    // Start is called before the first frame update
    void Start()
    {
        GetBoardHolder();
        if (!LevelInfo.levelSelected) {
            Debug.Log("No level selected. Getting default");
            LevelInfo.SetLevel(BoardSize._9x9, BoxSize._3x3, Difficulty.hard, 2);
        }
        StartCoroutine(CreateLevel());
    }

    // Sets BoardHolder to this object if its not defined
    //      Also sets <boardRect> depending on <BoardHolder>
    void GetBoardHolder() {
        if (BoardHolder == null) {
            BoardHolder = gameObject;
            boardRect = BoardHolder.GetComponent<RectTransform>();
        }
        if (boardRect == null) {
            boardRect = BoardHolder.GetComponent<RectTransform>();
        }
    }

    // For debug purposes
    public override string ToString() {
        string str = "";
        str += "Board size: " + boardSize.ToString();
        str += "\nBox size: " + boxSize.ToString();
        return str;
    }
}
