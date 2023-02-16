using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SoundClip;

public class LevelSelectMenu : MonoBehaviour
{
    // Main menu info - Just to keep scripts sorted really
    [SerializeField] LevelMenuType menuType;
    public GameObject MainMenuManagerObject;
    [SerializeField] GameObject MenuObject;
    MenuButtons closeMenuButtons;
    [Space(10f)]

    // Scroll rect stuff
    public GameObject ScrollRectObject;
    PinchableScrollRect scrollRect;
    public GameObject ScrollRectContent;
    MenuStyle menuSelected;
    [Space(10f)]

    // Board size menu info
    public GameObject BoardSizeMenu;
    [Space(10f)]

    // Difficulty menu info
    public GameObject DifficultyMenu;
    public GameObject DiffButtonPrefab;
    [Space(10f)]

    // Level menu info
    public GameObject LevelMenu;
    public GameObject LevelButtonPrefab;
    [Space(10f)]

    // Width multipliers for all children in these menus
    public float boardWidthMult;
    public float diffWidthMult;
    [Space(10f)]

    // Child & sizing info
    public float childSizeDivider;
    float childSize;
    [SerializeField] List<GameObject> Children = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        if (childSizeDivider == 0) {
            childSizeDivider = 1;
        }
        if (closeMenuButtons == null) {
            closeMenuButtons = MainMenuManagerObject.GetComponent<MenuButtons>();
        }

        BoardSizeMenu.SetActive(true);
        SetScrollContent(MenuStyle.board);

        if (scrollRect == null) {
            scrollRect = ScrollRectObject.GetComponent<PinchableScrollRect>();
        }

        GetChildren();
    }

    // ----- Menu Selection Stuff -----

    // Called when a BoardTypeButton is pressed
    // Sets content of scroll rect to difficulty - Specified by <type>
    public void SelectBoardType(BoardTypeButton type) {
        if (!CanChangeMenus()) { return; }

        Sound.Play(tap_something);

        // Destroy all previous difficulty buttons
        DestroyChildrenOf(DifficultyMenu);

        // Create new difficulty buttons for each difficulty of this board type
        foreach (Difficulty diff in type.difficultiesToMake) {
            // Instantiate it and set object name & text (level number)
            string str = LevelInfo.DifficultyToString(diff);
            if (diff == Difficulty.expertplus) {
                str = "Expert+";
            }

            // Instantiate it
            GameObject obj = Instantiate(DiffButtonPrefab, DifficultyMenu.transform);
            obj.name = str + " Button";
            // Set text
            obj.GetComponentInChildren<TMP_Text>().text = str + " Level";
            if (menuType != LevelMenuType.Quickplay) {
                obj.GetComponentInChildren<TMP_Text>().text += "s";
            }

            // Set the onClick function
            obj.GetComponent<Button>().onClick.AddListener(
            delegate {
                SelectDifficulty(type, diff, obj);
            });
        }

        // And center the difficulty menu
        SetScrollContent(MenuStyle.difficulty);
        MoveMenuLeft(BoardSizeMenu, DifficultyMenu);

        GetChildren();
        SetSizes();
    }

    // Called when a difficulty button is pressed
    // Sets content of scroll rect to level - Specified by the difficulty button
    // Makes buttons for each level present in this difficulty for this board/box size
    public void SelectDifficulty(BoardTypeButton type, Difficulty diff, GameObject refObject) {
        // Number of levels in this selected difficulty
        int numLevels = LevelInfo.NumberOfLevels(type.boardSize, type.boxSize, diff);

        // If this is quickplay, just select a level
        if (menuType == LevelMenuType.Quickplay) {
            int levelNum = Random.Range(0, numLevels);
            LevelInfo.SetLevel(type.boardSize, type.boxSize, diff, levelNum);
            GameManager.LoadGame(refObject);
            return;
        }

        Sound.Play(tap_something);

        // Destroy all previous level buttons
        DestroyChildrenOf(LevelMenu);

        // Create new level buttons for each level in this difficulty
        for (int i = 0; i < numLevels; ++i) {
            // Instantiate it and set object name & text (level number)
            GameObject obj = Instantiate(LevelButtonPrefab, LevelMenu.transform);
            obj.name = "Level " + i;
            obj.GetComponentInChildren<TMP_Text>().text = (i+1).ToString();

            int num = i;

            // Set color based on saved levels
            ImageTheme img = obj.GetComponent<ImageTheme>();
            SavedLevel match = SavedLevels.Find(type.boardSize, type.boxSize, diff, num);
            if (match != null) {
                if (match.isCompleted) {
                    img.lookType = LookType.UI_accent;
                } else {
                    img.Half(LookType.UI_accent, 0.5f);
                }
            }

            // Set the onClick function
            obj.GetComponent<Button>().onClick.AddListener(
            delegate {
                LeanTween.cancelAll();
                BackgroundAnimator anim = GameObject.FindObjectOfType<BackgroundAnimator>();
                if (anim.FloatingObjectHolder == null) {
                    Debug.Log("Number holder is null");
                }
                foreach (Transform child in anim.FloatingObjectHolder.transform) {
                    Object.Destroy(child.gameObject);
                }
                LevelInfo.SetLevel(type.boardSize, type.boxSize, diff, num);
                GameManager.LoadGame(obj);
            });
        }

        // And center the level menu
        SetScrollContent(MenuStyle.level);
        MoveMenuLeft(DifficultyMenu, LevelMenu);
        GetChildren();

        // Set grid layout sizing 
        GridLayoutGroup grid = LevelMenu.GetComponent<GridLayoutGroup>();
        float size = LevelMenu.transform.Find("Info Text").GetComponent<RectTransform>().rect.height;
        GridEditor gridEditor = grid.gameObject.GetComponent<GridEditor>();
        grid.padding.top = (int)size;
        gridEditor.VertPaddingMultiplier.x = size / (float)Screen.width;
        
        // Set grid child size and spacing size
        childSize = LevelMenu.GetComponent<RectTransform>().rect.width - (2f * Screen.width * gridEditor.HorPaddingMultiplier);
        childSize /= 5.5f;
        grid.cellSize = new Vector2(childSize, childSize);
        float spaceSize = childSize / 20f;
        grid.spacing = new Vector2(spaceSize, spaceSize);

        // Set size of Rect Scroll content
        RectTransform rect = LevelMenu.GetComponent<RectTransform>();
        Vector2 rectSize = rect.sizeDelta;
        rectSize.y = ((numLevels/5) + 2) * (childSize + spaceSize);
        rect.sizeDelta = rectSize;

        // Updating text sizes
        foreach (Transform child in LevelMenu.transform) {
            if (child.name == "Info Text" || child.name == "Background") { continue; }
            child.GetComponentInChildren<TextTheme>().UpdateTextSize();
        }
    }

    // Goes back one menu, depending on what menu is currently selected
    public void GoBack() {
        if (!CanChangeMenus()) { return; }

        switch (menuSelected) {
            // Close the whole menu ig
            case MenuStyle.board: {
                closeMenuButtons.CloseMenu();
                break;
            }
            // Go to back to board sizing buttons
            case MenuStyle.difficulty: {
                SetScrollContent(MenuStyle.board);
                MoveMenuRight(DifficultyMenu, BoardSizeMenu);
                break;
            }
            // Go back to difficulty buttons
            case MenuStyle.level: {
                SetScrollContent(MenuStyle.difficulty);
                MoveMenuRight(LevelMenu, DifficultyMenu);
                break;
            }
        }
    }


    bool boardInfoChanged = false, diffInfoChanged = false, lvlInfoChanged = false;

    // Change the content of the scroll rect to given object
    void SetScrollContent(MenuStyle menu) {
        if (!CanChangeMenus()) { return; }

        bool doInfoUpdate = false;

        menuSelected = menu;
        switch (menuSelected) {
            case MenuStyle.board:
                ScrollRectContent = BoardSizeMenu;
                if (!boardInfoChanged) {
                    doInfoUpdate = true; boardInfoChanged = true;
                }
                break;
            case MenuStyle.difficulty:
                ScrollRectContent = DifficultyMenu;
                if (!diffInfoChanged) {
                    doInfoUpdate = true; diffInfoChanged = true;
                }
                break;
            case MenuStyle.level:
                ScrollRectContent = LevelMenu;
                if (!lvlInfoChanged) {
                    doInfoUpdate = true; lvlInfoChanged = true;
                }
                break;
            default:
                Debug.Log("No menu selected!");
                break;
        }
        RectTransform rect = ScrollRectContent.GetComponent<RectTransform>();
        //rect.localPosition = new Vector3(0f, 0f, rect.localPosition.z);

        // Set size of the info text
        RectTransform infoRect = ScrollRectContent.transform.Find("Info Text").GetComponent<RectTransform>();
        if (doInfoUpdate) {
            float mult = (float)Screen.height / 3040f;
            //mult = Mathf.Sqrt(mult);
            infoRect.sizeDelta = new Vector2(infoRect.sizeDelta.x, infoRect.sizeDelta.y * mult);
        }
        
        // Set scroll rect content and make it active
        if (scrollRect == null) {
            scrollRect = ScrollRectObject.GetComponent<PinchableScrollRect>();
        }
        scrollRect.content = rect;

        ScrollRectContent.SetActive(true);
        scrollRect.content.gameObject.SetActive(true);
    }


    // ----- Moving menus around -----

    void MoveMenuLeft(GameObject main, GameObject right) {
        if (main.LeanIsTweening() || right.LeanIsTweening()) { return; }

        RectTransform mainRect = main.GetComponent<RectTransform>();
        RectTransform rightRect = right.GetComponent<RectTransform>();
        mainRect.anchoredPosition = new Vector2(mainRect.anchoredPosition.x, 0f);
        rightRect.anchoredPosition = new Vector2(rightRect.anchoredPosition.x, 0f);

        float mainStart = mainRect.localPosition.x;
        float mainEnd = -Screen.width;
        Tween(mainStart, mainEnd, main)
        .setOnComplete(() => {
            foreach (Transform child in ScrollRectContent.transform.parent) {
                if (child.name == ScrollRectContent.name) { continue; }
                child.gameObject.SetActive(false);
            }
        });

        Debug.Log("Moving menu from " + mainStart + " to " + mainEnd);

        float rightStart = Screen.width;
        float rightEnd = 0f;
        right.SetActive(true);
        Tween(rightStart, rightEnd, right);

        //Sound.Play(slide_menu);
    }

    void MoveMenuRight(GameObject main, GameObject left) {
        if (main.LeanIsTweening() || left.LeanIsTweening()) { return; }

        RectTransform mainRect = main.GetComponent<RectTransform>();
        RectTransform leftRect = left.GetComponent<RectTransform>();
        mainRect.anchoredPosition = new Vector2(mainRect.anchoredPosition.x, 0f);
        leftRect.anchoredPosition = new Vector2(leftRect.anchoredPosition.x, 0f);

        float mainStart = mainRect.localPosition.x;
        float mainEnd = Screen.width;
        Tween(mainStart, mainEnd, main)
        .setOnComplete(() => {
            foreach (Transform child in ScrollRectContent.transform.parent) {
                if (child.name == ScrollRectContent.name) { continue; }
                child.gameObject.SetActive(false);
            }
        });

        float leftStart = -Screen.width;
        float leftEnd = 0f;
        left.SetActive(true);
        Tween(leftStart, leftEnd, left);

        //Sound.Play(slide_menu);
    }

    LTDescr Tween(float start, float end, GameObject toMove) {
        toMove.SetActive(true);
        // Set starting position
        Vector3 pos = toMove.GetComponent<RectTransform>().localPosition;
        pos.x = start;
        toMove.GetComponent<RectTransform>().localPosition = pos;
        // Then tween it
        float time = 1f * Settings.AnimSpeedMultiplier;

        LTDescr tween =  LeanTween.moveLocalX(toMove, end, time)
        .setEase(LeanTweenType.easeInOutCubic)
        .setOnStart(() => {
            foreach (GameObject obj in closeMenuButtons.menus) {
                obj.transform.Find("Close Button").GetComponent<Button>().interactable = false;
            }
        })
        .setOnComplete(() => {
            foreach (GameObject obj in closeMenuButtons.menus) {
                obj.transform.Find("Close Button").GetComponent<Button>().interactable = true;
            }
        });

        return tween;
    }


    // ----- RANDOM -----

    // Make sure we're allowed to change menus
    // Aka check each menu for tweening
    bool CanChangeMenus() {
        bool canChange = true;
        if (BoardSizeMenu.LeanIsTweening() && BoardSizeMenu != null) {
            Debug.Log("Board size is tweening");
            canChange = false;
        }
        if (DifficultyMenu.LeanIsTweening() && DifficultyMenu != null) {
            Debug.Log("Difficulty menu is tweening");
            canChange = false;
        }
        if (LevelMenu.LeanIsTweening() && LevelMenu != null) {
            Debug.Log("Level menu is tweening");
            canChange = false;
        }
        if (MenuObject.LeanIsTweening() && MenuObject != null) {
            Debug.Log("Whole menu is tweening");
            canChange = false;
        }
        return canChange;
    }


    // ----- Children & Sizing Stuff -----

    // Get all children on the scroll rect content object
    void GetChildren() {
        // Reset children list
        if (Children == null) {
            Children = new List<GameObject>();
        } else {
            Children.Clear();
        }
        // Add each child to it
        foreach (Transform child in ScrollRectContent.transform) {
            Children.Add(child.gameObject);
        }
        // And set their sizes
        if (menuSelected != MenuStyle.level) {
            SetSizes();
        }
    }

    // Destroy all children of object
    void DestroyChildrenOf(GameObject obj) {
        Transform trans = obj.transform;
        foreach (Transform child in trans) {
            if (child.name == "Info Text" || obj.name == "Background") { continue; }
            GameObject.Destroy(child.gameObject);
        }
    }

    // Set sizes for each child
    void SetSizes() {
        RectTransform rect;
        Vector2 size;
        VerticalLayoutGroup layout = ScrollRectContent.GetComponent<VerticalLayoutGroup>();
        childSize = Screen.height / childSizeDivider;

        float holderWidth = ScrollRectContent.GetComponent<RectTransform>().rect.width;

        // Set each child object (except info text) to have specified height
        foreach (GameObject obj in Children) {
            // Dont update info text
            if (obj.name == "Info Text" || obj.name == "Background") { continue; }
            
            // Set all their sizes to child size
            size = new Vector2(holderWidth * diffWidthMult, childSize);
            obj.GetComponent<RectTransform>().sizeDelta = size;
        }
        
        StartCoroutine(SetTextSizes());

        // Find total size of content
        float contentSize = (Children.Count - 1) * childSize;
        contentSize += (Children.Count-1) * layout.spacing;
        contentSize += ScrollRectContent.transform.Find("Info Text").GetComponent<RectTransform>().sizeDelta.y;
        if (contentSize < ScrollRectObject.GetComponent<RectTransform>().sizeDelta.y) {
            contentSize = ScrollRectObject.GetComponent<RectTransform>().sizeDelta.y;
        }

        // Set its size
        rect = ScrollRectContent.GetComponent<RectTransform>();
        size = rect.sizeDelta;
        size.y = contentSize;
        rect.sizeDelta = size;

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
    }

    // Set sizes for each text object in the scroll rect content
    IEnumerator SetTextSizes() {
        yield return new WaitForEndOfFrame();
        // Set each child object (except info text) to have specified height
        foreach (GameObject obj in Children) {
            if (obj == null) { continue; }
            if (obj.name == "Info Text" || obj.name == "Background") { continue; }
            if (!obj.activeInHierarchy)  { continue; }
            obj.GetComponentInChildren<TextTheme>().UpdateTextSize();
        }
    }

    enum MenuStyle {
        board, difficulty, level
    }
}

public enum LevelMenuType {
    Play, Quickplay, TimeTrials
}