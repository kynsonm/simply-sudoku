using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameInfo : MonoBehaviour
{
    // ----- VARIABLES -----
    [SerializeField] GameObject TilePrefab;

    [Space(10f)]
    [SerializeField] GameObject InfoCanvas;
    [SerializeField] GameObject InfoContent;
    [SerializeField] List<GameObject> MakeUninteractable;

    [Space(10f)]
    [SerializeField] RectTransform TitleSizeReference;
    [SerializeField] RectTransform TopSizeReference;
    [SerializeField] RectTransform BottomSizeReference;

    [Space(10f)]
    [SerializeField] GameObject BoardHolder;
    [SerializeField] GameObject BoxHolder;
    [SerializeField] GameObject ColHolder, RowHolder;

    [Space(10f)]
    [SerializeField] float titleBarDivider;


    GameObject activateInfoButton;
    GameBoardCreator gameBoardCreator;


    // Runs even when object is off
    private void Awake()
    {
        gameBoardCreator = GameObject.FindObjectOfType<GameBoardCreator>(true);
        activateInfoButton = GameObject.FindObjectOfType<TopButtons>(true).Info;
        StartCoroutine(TurnOffInfo());
        SetUp();
    }
    IEnumerator TurnOffInfo() {
        InfoCanvas.SetActive(true);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        SetMenuSize(InfoCanvas.transform.Find("Menu").GetComponent<RectTransform>());
        yield return new WaitForEndOfFrame();
        InfoCanvas.SetActive(false);
    }

    // Creates all the tiles and sets up all the infos
    void SetUp() {
        SetUpTitles();
        CreateAllTiles();
        SetUpAllInfos();
    }


    // Checks whether info is open or not
    bool lastOpen = false;
    public bool isOpen() {
        return lastOpen;
    }


    // ----- INFO TEXTS -----

    // Sets up all the infos
    void SetUpAllInfos() {
        SetRowInfo();
        SetColInfo();
        SetBoxInfo();
    }

    // Sets row info
    void SetRowInfo() {
        TMP_Text text = RowHolder.transform.parent.Find("Info").GetComponent<TMP_Text>();
        string str = "This is one row of the board. Each number in a row must be <i>unique</i>!";
        str += " In other words, there cannot be any repeated numbers in one row";
        if (LevelInfo.BoardSizeToHeight() > LevelInfo.BoardSizeToWidth()) {
            str += "\nIn this level, not <i>every</i> number will show up in each row";
        }
        //text.text = str;
        text.text = CheckString(str);
    }

    // Set column info
    void SetColInfo() {
        TMP_Text text = ColHolder.transform.parent.Find("Info").GetComponent<TMP_Text>();
        string str = "This is one column of the board. Each number in a column must also be <i>unique</i>.";
        if (LevelInfo.BoardSizeToHeight() > LevelInfo.BoardSizeToWidth()) {
            str += " Unlike in a row, however, each ";
        } else {
            str += " Like in a row, every ";
        }
        str += "number appears in each column";
        //text.text = str;
        text.text = CheckString(str);
    }

    // Set box info
    void SetBoxInfo() {
        TMP_Text text = BoxHolder.transform.parent.Find("Info").GetComponent<TMP_Text>();
        string str = "This is one box of the board. Each number in a box must also be <i>unique</i>.\n";

        Vector2Int boxSize = LevelInfo.BoxSizeVector();
        int tilesPerBox = boxSize.x * boxSize.y;

        // If JUST unique nums in box
        if (tilesPerBox < LevelInfo.BoardSizeToHeight()) {
            // If JUST unique numbers in row
            if (LevelInfo.BoardSizeToHeight() > LevelInfo.BoardSizeToWidth()) {
                str += "Like in a row, ";
            }
            // Each number in row
            else {
                str += "However, ";
            }
            str += "not <i>every</i> number will show up in each box";
        }
        // Each number in a box
        else {
            // If JUST unique numbers in row
            if (LevelInfo.BoardSizeToHeight() > LevelInfo.BoardSizeToWidth()) {
                str += "Like in each column, ";
            }
            // Each number in row
            else {
                str += "Like in each row/column, ";
            }
            str += "every number will appear in each box";
        }
        //text.text = str;
        text.text = CheckString(str);
    }

    string CheckString(string stringIn) {
        string col1, col2, col3, col4;
        col1 = "<color=#" + ColorUtility.ToHtmlStringRGBA(Theme.color1) + ">";
        col2 = "<color=#" + ColorUtility.ToHtmlStringRGBA(Theme.color2) + ">";
        col3 = "<color=#" + ColorUtility.ToHtmlStringRGBA(Theme.color3) + ">";
        col4 = "<color=#" + ColorUtility.ToHtmlStringRGBA(Theme.color4) + ">";
        string endCol = "</color>";

        string str = stringIn;
        str = str.Replace("row", col1 + "row" + endCol);
        str = str.Replace("column", col2 + "column" + endCol);
        str = str.Replace("box", col3 + "box" + endCol);

        return str;
    }


    // ----- TILE SETUP -----

    // Creates all the stuff
    void CreateAllTiles() {
        SetUpTiles(BoardHolder, TileType.board);
        SetUpTiles(BoxHolder, TileType.box);
        SetUpTiles(ColHolder, TileType.column);
        SetUpTiles(RowHolder, TileType.row);
    }

    // Decides how to set up the tile sprites in SetUpTiles()
    enum TileType {
        board, row, column, box
    }

    void SetUpTiles(GameObject holder, TileType tileType) {
        StartCoroutine(SetUpTilesEnum(holder, tileType));
    }
    IEnumerator SetUpTilesEnum(GameObject holder, TileType tileType) {
        // Wait for rects to rect
        yield return new WaitForEndOfFrame();

        // Get vars
        RectTransform rect = holder.GetComponent<RectTransform>();
        float rectWidth = rect.rect.width;
        float rectHeight = rect.rect.height;

        // How many tiles to make vertically
        int height = 1;
        if (tileType == TileType.board || tileType == TileType.column) {
            height = LevelInfo.BoardSizeToHeight();
        }
        else if (tileType == TileType.box) {
            height = LevelInfo.BoxSizeVector().y;
        }

        // How many tiles to make horizontally
        int width = 1;
        if (tileType == TileType.board || tileType == TileType.row) {
            width = LevelInfo.BoardSizeToWidth();
        }
        else if (tileType == TileType.box) {
            width = LevelInfo.BoxSizeVector().x;
        }

        // Make all the objects
        Transform trans = holder.transform;
        for (int i = 0; i < height; ++i) {
            for (int j = 0; j < width; ++j) {
                GameObject obj = Instantiate(TilePrefab, trans);
                obj.GetComponentInChildren<TMP_Text>(true).text = "";
                SetTileSprites(obj, tileType, i, j);
            }
        }

        // Horizontal layout group
        if (tileType == TileType.row) {
            HorizontalLayoutGroup hor = holder.GetComponent<HorizontalLayoutGroup>();
            hor.childControlHeight = true;
            yield return new WaitForEndOfFrame();
            hor.childControlHeight = false;

            float size = trans.GetChild(0).GetComponent<RectTransform>().rect.width;
            foreach (Transform child in trans) {
                RectTransform childRect = child.GetComponent<RectTransform>();
                childRect.sizeDelta = new Vector2(childRect.sizeDelta.x, size);
            }
        }
        // Vertical layout group
        else if (tileType == TileType.column) {
            VerticalLayoutGroup hor = holder.GetComponent<VerticalLayoutGroup>();
            hor.childControlWidth = true;
            yield return new WaitForEndOfFrame();
            hor.childControlWidth = false;

            float size = trans.GetChild(0).GetComponent<RectTransform>().rect.height;
            foreach (Transform child in trans) {
                RectTransform childRect = child.GetComponent<RectTransform>();
                childRect.sizeDelta = new Vector2(size, childRect.sizeDelta.y);
            }
        }
        // Grid layout group
        else {
            float size = Mathf.Min(rectWidth / (float)width, rectHeight / (float)height);
            GridLayoutGroup grid = holder.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = width;
            grid.cellSize = new Vector2(size, size);
            
            yield return new WaitForEndOfFrame();

            HorizontalEditor parVert = grid.transform.parent.GetComponent<HorizontalEditor>();
            parVert.Sizes[0] = 0f;
        }
    }

    // Changes border sprites depending on current row and column
    void SetTileSprites(GameObject tile, TileType tileType, int row, int col) {
        // Make sure its good
        if (gameBoardCreator == null) {
            gameBoardCreator = GameObject.FindObjectOfType<GameBoardCreator>(true);
            if (gameBoardCreator == null) {
                Debug.LogError("GameBoardCreator is null!");
                return;
            }
        }

        // Get objects
        Transform trans = tile.transform;
        Image top = trans.Find("Top Border").GetComponent<Image>();
        Image bot = trans.Find("Bottom Border").GetComponent<Image>();
        Image left = trans.Find("Left Border").GetComponent<Image>();
        Image right = trans.Find("Right Border").GetComponent<Image>();

        Vector2Int boxSize = LevelInfo.BoxSizeVector();
        if (LevelInfo.BoardSizeToHeight() > LevelInfo.BoardSizeToWidth()) {
            boxSize = new Vector2Int(boxSize.y, boxSize.x);
        }

        // Board borders
        if (tileType != TileType.box) {
            if (row == 0) { top.sprite = gameBoardCreator.BoardBord; }
            if (row == LevelInfo.BoardSizeToHeight()-1) { bot.sprite = gameBoardCreator.BoardBord; }
            if (col == 0) { left.sprite = gameBoardCreator.BoardBord; }
            if (col == LevelInfo.BoardSizeToWidth()-1) { right.sprite = gameBoardCreator.BoardBord; }
        } else {
            if (row == 0) { top.sprite = gameBoardCreator.BoxBord; }
            if (row == boxSize.y-1) { bot.sprite = gameBoardCreator.BoxBord; }
            if (col == 0) { left.sprite = gameBoardCreator.BoxBord; }
            if (col == boxSize.x-1) { right.sprite = gameBoardCreator.BoxBord; }
        }

        // Box borders
        if (tileType == TileType.board) {
            if (row+1 % boxSize.y == 0) { bot.sprite = gameBoardCreator.BoxBord; }
            if (row % boxSize.y == 0 && row != 0) { top.sprite = gameBoardCreator.BoxBord; }
            if (col+1 % boxSize.x == 0) { right.sprite = gameBoardCreator.BoxBord; }
            if (col % boxSize.y == 0 && col != 0) { left.sprite = gameBoardCreator.BoxBord; }
        }
    }


    // ----- TITLES -----

    // Set the bar and text sizes for each title in the content
    void SetUpTitles() {
        // Individual titles within content
        titleBarDivider = (titleBarDivider <= 2f) ? 2f : titleBarDivider;
        foreach (Transform child in InfoContent.transform) {
            // Skip objects w/out "Title" in their name
            if (!child.name.Contains("Title")) { continue; }

            // Title rect
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.ForceUpdateRectTransforms();
            float size = rect.rect.height;

            // Children rects
            RectTransform text = child.Find("Text").GetComponent<RectTransform>();
            RectTransform bar = child.Find("Bar").GetComponent<RectTransform>();

            // Set sizes
            bar.sizeDelta = new Vector2(0f, size / titleBarDivider);
            text.sizeDelta = new Vector2(0f, size - (size / titleBarDivider));
        }
    }


    // ----- TURN STUFF ON/OFF -----

    public bool lastGamePaused = false;

    // Turns the canvas on/off
    // Returns whether anything happened
    public bool TurnOn() {
        if (InfoCanvas.activeInHierarchy) { return false; }
        if (InfoCanvas.transform.Find("Menu").gameObject.LeanIsTweening()) {
            return false;
        }

        SetUpAllInfos();
        
        CheckGameSettings();
        ActivateCanvas(true);
        MoveMenu(true);
        lastGamePaused = GameManager.IsPaused;
        if (!GameManager.IsPaused) {
            GameManager.GameBoard.Pause();
        }
        lastOpen = true;
        return true;
    }
    public bool TurnOff() { return TurnOff(false); }
    public bool TurnOff(bool tweenOverride) {
        if (!InfoCanvas.activeInHierarchy) { return false; }
        if (tweenOverride) { }
        else if (InfoCanvas.transform.Find("Menu").gameObject.LeanIsTweening()) {
            return false;
        }

        CheckGameSettings();
        ActivateCanvas(false);
        MoveMenu(false);
        lastOpen = false;
        return true;
    }
    public bool Activate() {
        if (InfoCanvas.activeInHierarchy) { return TurnOff(); }
        else { return TurnOn(); }
    }
    public void TurnOn(int identifier) { TurnOn(); }
    public void TurnOff(int identifier) { TurnOff(); }
    public void Activate(int identifier) { Activate(); }

    // Turn info canvas on/off
    void ActivateCanvas(bool turnOn) {
        // Turn the canvas on/off
        if (turnOn) { InfoCanvas.SetActive(true); }
        // Turn interactable and blocks raycasts either on or off
        foreach (GameObject obj in MakeUninteractable) {
            CanvasGroup canv = GetCanvasGroup(obj);
            canv.ignoreParentGroups = turnOn;
            canv.blocksRaycasts = !turnOn;
            canv.interactable = !turnOn;
        }
    }

    // Check GameSettings being open
    void CheckGameSettings() {
        GameSettings settings = GameObject.FindObjectOfType<GameSettings>(true);
        if (!settings.gameObject.activeInHierarchy) { return; }

        if (settings.IsOpened()) {
            settings.Close(true);
            settings.lastGamePaused = true;
        }
    }

    // ----- UTILITIES -----

    // Gets canvas group attatched to obj, or adds one
    CanvasGroup GetCanvasGroup(GameObject obj) {
        CanvasGroup canv = obj.GetComponent<CanvasGroup>();
        if (canv == null) {
            canv = obj.AddComponent<CanvasGroup>();
        }
        return canv;
    }

    // Opens or closes the menu from/to position of button
    void MoveMenu(bool isOpening) {
        GameObject menu = InfoCanvas.transform.Find("Menu").gameObject;

        if (menu.LeanIsTweening()) { return; }

        // Get rects
        RectTransform rect = menu.GetComponent<RectTransform>();
        Vector3 endPos = new Vector3(rect.position.x, rect.position.y, rect.position.z);
        RectTransform butt = activateInfoButton.GetComponent<RectTransform>();

        // Get start and end position
        Vector2 start = isOpening ? butt.position : rect.position;
        Vector2 end = isOpening ? rect.position : butt.position;
        Vector2 posOnEnd = new Vector2(0f, rect.anchoredPosition.y);
        rect.position = start;

        // Get canvas group
        CanvasGroup canv = GetCanvasGroup(menu);

        float scaleStart = rect.localScale.x;

        // Move from start to end
        // Fade & scale in/out
        LeanTween.value(menu, 0f, 1f, Settings.AnimSpeedMultiplier)
        .setEase(LeanTweenType.easeInOutCubic)
        .setOnStart(() => {
            if (isOpening) { InfoCanvas.SetActive(true); }
        })
        .setOnUpdate((float value) => {
            // Set position
            rect.position = start + value * (end - start);

            // Set scale and alpha
            float scale = isOpening ? value : (1f - value);
            canv.alpha = Mathf.Pow(scale, 0.5f);

            Vector3 newScale = new Vector3(scale, scale, 1f);
            if (!isOpening) {
                float temp = scaleStart * scale;
                newScale = new Vector3(temp, temp, 1f);
            }
            rect.localScale = newScale;
        })
        .setOnComplete(() => {
            rect.position = endPos;
            if (!isOpening) {
                InfoCanvas.SetActive(false);
                if (!lastGamePaused) {
                    GameManager.GameBoard.Resume();
                }
            } else {
                SetMenuSize(rect);
            }
        });
    }

    // Set position/dimensions of the menu and title
    void SetMenuSize(RectTransform menuRect) {
        // Menu size
        float position = TopSizeReference.anchoredPosition.y;
        float size = TopSizeReference.position.y - BottomSizeReference.position.y;
        menuRect.sizeDelta = new Vector2(0f, size);
        menuRect.anchoredPosition = new Vector2(0f, position);

        // Menu title/bar size
        float titleSize = TitleSizeReference.GetComponent<RectTransform>().rect.height;
        RectTransform titleRect = InfoCanvas.transform.Find("Menu").Find("Title").GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(0f, titleSize);
        titleRect.anchoredPosition = new Vector2(0f, 0f);

        // Scroll view size
        float scrollSize = menuRect.sizeDelta.y - titleSize;
        RectTransform scroll = menuRect.transform.Find("Scroll View").GetComponent<RectTransform>();
        scroll.sizeDelta = new Vector2(0f, scrollSize);
        scroll.anchoredPosition = new Vector2(0f, 0f);
    }
}
