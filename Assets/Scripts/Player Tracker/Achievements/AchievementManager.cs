using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class AchievementManager : MonoBehaviour
{
    // Achievement objects
    [SerializeField] GameObject AchievementCanvas;
    [SerializeField] GameObject AchievementAreaPrefab;
    [SerializeField] GameObject AchievementPrefab;
    [SerializeField] Transform AchievementParent;
    [SerializeField] GameObject PlayerInfoObject;

    [Space(10f)]
    [SerializeField] float TitleDivider;
    float titleSize;
    [SerializeField] float AchievementHeightDivider;
    float achievementHeight;
    //float descriptionTextSize = -1f;

    // Achievements
    [Space(10f)]
    public List<Achievement> LevelAchievements;
    public List<Achievement> PlayerLevelAchievements;
    public List<BoardAchievement> BoardsCompleted;
    public List<Achievement> DaysPlayed;
    public List<Achievement> OtherAchievements;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (AchievementCanvas == null) { yield break; }

        StartCoroutine(TurnOffAchievmentCanvas());
        //StartCoroutine(ActivateLayoutGroups(AchievementParent, true, true));

        // Load all achievements and set them up
        yield return new WaitForEndOfFrame();
        GiveBoardAchievementsNames(BoardsCompleted);
        Achievements.Load();
        SetupAllAchievements();

        // After 2 frames for dimensions and sizes to work themselves out,
        //   create all of the achievement objects
        yield return new WaitForEndOfFrame();
        CreateAchievements();

        // Save achievemetns every so often
        StartCoroutine(AchievementSaveEnum());

        // Log all achievements
        Debug.Log(Achievements.allAchievementsLog());

        // Give days played achievements after a few seconds
        yield return new WaitForSeconds(2f);
        Achievements.CheckDaysPlayedAchievements();
        Achievements.DoubleCheckProgresses();
    }
    IEnumerator AchievementSaveEnum() {
        while (true) {
            yield return new WaitForSeconds(5f);
            Achievements.Save();
        }
    }
    IEnumerator TurnOffAchievmentCanvas() {
        /*
        // Remove the onClick of the menuButton associated with award menu
        MenuButtons buttons = GameObject.FindObjectOfType<MenuButtons>(true);
        Button achButton = buttons.children[2].GetComponentInChildren<Button>(true);

        Button.ButtonClickedEvent onClick = new Button.ButtonClickedEvent();
        onClick = achButton.onClick;
        achButton.onClick.RemoveAllListeners();
        */

        // Move the menu to the right while things get set up
        RectTransform achMenu = AchievementCanvas.transform.Find("Awards Menu").GetComponent<RectTransform>();
        achMenu.anchoredPosition = new Vector2(achMenu.anchoredPosition.x + Screen.width, achMenu.anchoredPosition.y);

        // Wait certain amount frames
        for (int i = 0; i < 6; ++i) {
            AchievementCanvas.SetActive(true);
            yield return new WaitForEndOfFrame();
        }

        // Move it back and turn the canvas off
        SetContentSize();
        AchievementCanvas.SetActive(false);
        achMenu.anchoredPosition = new Vector2(achMenu.anchoredPosition.x - Screen.width, achMenu.anchoredPosition.y);
    }


    // Create all achievements ever made in existence (wow!)
    void CreateAchievements() {
        if (AchievementParent == null) { return; }

        // Destroy whats already there
        foreach (Transform trans in AchievementParent) {
            GameObject.Destroy(trans.gameObject);
        }

        float size = (float)(Screen.width + Screen.height) / 2f;
        titleSize = size / TitleDivider;
        achievementHeight = size / AchievementHeightDivider;

        // Create all the objects
        CreateAchievements(LevelAchievements, "Levels Completed");
        CreateAchievements(PlayerLevelAchievements, "Player Level");
        CreateAchievements(BoardsCompleted, "Level Types Completed");
        CreateAchievements(DaysPlayed, "Days Played (In A Row!)");
        //CreateAchievements(OtherAchievements, "Other Achievements");

        StartCoroutine(SetTitleTextSizes());
        //SetContentSize();
    }

    void SetContentSize() {
        RectTransform rect = AchievementParent.GetComponent<RectTransform>();
        VerticalLayoutGroup vert = AchievementParent.GetComponent<VerticalLayoutGroup>();

        float size = vert.padding.top + vert.padding.bottom;
        size += (AchievementParent.childCount /* - 1 */) * vert.spacing;

        foreach (Transform child in AchievementParent) {
            RectTransform childRect = child.GetComponent<RectTransform>();
            size += childRect.sizeDelta.y;
        }

        rect.sizeDelta = new Vector2(0f, size);
    }

    // Creates an achievement area and returns the content to create achievements under
    Transform CreateAchievementArea(string title, int numAchievements) {
        // Create the area
        GameObject obj = Instantiate(AchievementAreaPrefab, AchievementParent);
        obj.name = title + " Ach Area";
        Transform trans = obj.transform;

        // Set title and size
        TMP_Text titleText = trans.GetComponentInChildren<TMP_Text>();
        titleText.text = title;
        RectTransform titleRect = titleText.gameObject.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(0f, titleSize);

        // Set achievement area size
        RectTransform achAreaRect = trans.Find("Achievements").GetComponent<RectTransform>();
        VerticalLayoutGroup vert = achAreaRect.gameObject.GetComponent<VerticalLayoutGroup>();
        float achAreaSize = (float)numAchievements * achievementHeight;
        achAreaSize += (float)(numAchievements-1) * vert.spacing;
        achAreaSize += vert.padding.top + vert.padding.bottom;
        achAreaRect.sizeDelta = new Vector2(0f, achAreaSize);

        // Set total area size
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, titleSize + achAreaSize);

        // Return content
        return trans.Find("Achievements");
    }

    // Entire section creation
    void CreateAchievements(List<Achievement> achievements, string sectionName) {
        Transform parent = CreateAchievementArea(sectionName, achievements.Count);
        foreach (Achievement ach in achievements) {
            CreateAchievement(parent, ach);
        }
    }
    void CreateAchievements(List<BoardAchievement> achievements, string sectionName) {
        Transform parent = CreateAchievementArea(sectionName, achievements.Count);
        string x = "\U000000D7";
        string d = "\U00002013";
        foreach (BoardAchievement ach in achievements) {
            string board = ach.boardSize.ToString().Remove(0, 1).Replace("x", x);
            string box = ach.boxSize.ToString().Remove(0, 1).Replace("x", x);
            string name = $"{board} {d} [{box}]";
            ach.iconText = $"{board}\n[{box}]";
            CreateAchievement(parent, ach, name);
        }
    }

    public void SetupBoardAchievements() {
        string x = "\U000000D7";
        string d = "\U00002013";
        foreach (BoardAchievement ach in BoardsCompleted) {
            string board = ach.boardSize.ToString().Remove(0, 1).Replace("x", x);
            string box = ach.boxSize.ToString().Remove(0, 1).Replace("x", x);
            string name = $"{board} {d} [{box}]";
            ach.name = name;
        }
    }

    // Individual achievement creation
    void CreateAchievement(Transform parent, Achievement ach) { CreateAchievement(parent, ach, ach.name); }
    void CreateAchievement(Transform parent, Achievement ach, string name) {
        GameObject obj = Instantiate(AchievementPrefab, parent);
        ach.achievementObject = obj;
        obj.name = name;
        SetUpAchievementObject(ach);
    }
    public void SetUpAchievementObject(Achievement ach) {
        if (ach.achievementObject == null) {
            Debug.LogWarning("Achievement object is null! On " + ach.name);
            return;
        }
        Transform trans = ach.achievementObject.transform;
        trans.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, achievementHeight);

        // Set icon info
        Transform imageArea = trans.Find("Image");
        TMP_Text iconText = imageArea.Find("Icon").GetComponentInChildren<TMP_Text>();
        iconText.text = ach.iconText;
        if (!ach.completed) {
            imageArea.Find("Icon").Find("Off").gameObject.SetActive(true);
        } else {
            imageArea.Find("Icon").Find("Off").gameObject.SetActive(false);
        }

        // Set progress stuff
        Transform progressArea = imageArea.Find("Progress");
        if (ach.collected || ach.progress == 0) {
            foreach (Transform child in progressArea) { child.gameObject.SetActive(false); }
        } else {
            Slider progress = progressArea.GetComponent<Slider>();
            progress.maxValue = ach.targetNumber;
            progress.value = Mathf.Max(ach.progress, (int)(progress.maxValue * 0.1f));
            progressArea.GetComponentInChildren<TMP_Text>().text = $"{ach.progress} / {ach.targetNumber}";
        }

        // Set text stuff
        StartCoroutine(SetupTextArea(ach));

        // Set xp and coins info if necessary
        Transform rewardsArea = trans.Find("Text").Find("Reward");
        if (ach.collected) {
            foreach (Transform child in rewardsArea) { child.gameObject.SetActive(false); }
        } else {
            string pad = "<#00000000>...</color>";
            //string pad = "";
            rewardsArea.Find("XP").GetComponent<TMP_Text>().text = $"{ach.xp} XP {pad}";
            rewardsArea.Find("Coins").GetComponent<TMP_Text>().text = $"{pad} {ach.reward} Coins";
        }

        // Set collect button
        Button collect = trans.GetComponentInChildren<Button>(true);
        if (!ach.collected && ach.completed) {
            StartCoroutine(SetupCollectButton(ach, collect));
        } else {
            collect.gameObject.SetActive(false);
        }

        //StartCoroutine(ActivateLayoutGroups(ach.achievementObject.transform, true, false));
    }


    // ----- ACHIEVEMENT SETUP -----
    
    // Does all the intial setup necessary
    void SetupAllAchievements() {
        SetProgress(LevelAchievements, Achievements.LevelsCompleted);
        SetProgress(BoardsCompleted);
        SetProgress(DaysPlayed, Achievements.DaysPlayed);
        SetProgress(PlayerLevelAchievements, PlayerInfo.Level);
    }

    // Sets progress of each achievement int the list to the given value
    void SetProgress(List<Achievement> achievements, int progressValue) {
        foreach (Achievement ach in achievements) {
            ach.progress = progressValue;
        }
    }
    void SetProgress(List<BoardAchievement> achievements) {
        foreach (BoardAchievement ach in achievements) {
            ach.progress = SavedLevels.NumberOfLevelsCompleted(ach.boardSize, ach.boxSize);
        }
    }

    // Setup text area (this is messy to do in the method, so do it here instead)
    IEnumerator SetupTextArea(Achievement ach) {
        // Set texts
        Transform trans = ach.achievementObject.transform;
        Transform textArea = trans.Find("Text");
        TMP_Text title = textArea.Find("Title").GetComponent<TMP_Text>();
        title.text = ach.name;
        TMP_Text description = textArea.Find("Description").GetComponent<TMP_Text>();
        TextTheme theme = description.gameObject.GetComponent<TextTheme>();
        if (ach.completed || ach.collected) {
            description.text = ach.achievedText;
            theme.IsItalic = true;
        } else {
            description.text = ach.info;
        }
        theme.Reset();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Set each text area size
        float size1 = achievementHeight / 3.75f;
        float size2 = achievementHeight / 5f;

        RectTransform titleRect = title.gameObject.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(0f, size1);
        title.fontSizeMax = 10000f;

        RectTransform descRect = description.gameObject.GetComponent<RectTransform>();
        RectTransform rewRect = textArea.Find("Reward").GetComponent<RectTransform>();
        if (ach.collected) {
            descRect.sizeDelta = new Vector2(0f, achievementHeight - size1);
            rewRect.sizeDelta = new Vector2(0f, 0f);
        } else {
            float numLines = (float)Mathf.Min(description.textInfo.lineCount, 2);
            descRect.sizeDelta = new Vector2(0f, numLines * size1);
            rewRect.sizeDelta = new Vector2(0f, size2);
        }

        // Set text size for description
        yield return new WaitForEndOfFrame();
        title.fontSizeMax = 10000f;
        yield return new WaitForEndOfFrame();
        description.fontSizeMax = title.fontSize;
    }
    // Setup collect button (this is annoying to do in the method, so do it here instad)
    IEnumerator SetupCollectButton(Achievement ach, Button collectButton) {
        // Set button click
        collectButton.gameObject.SetActive(true);
        collectButton.onClick.RemoveAllListeners();
        collectButton.onClick.AddListener(() => {
            Achievements.CollectAchievement(ach);
            collectButton.gameObject.SetActive(false);
        });

        yield return new WaitForEndOfFrame();

        // Setup sizing
        RectTransform rect = collectButton.gameObject.GetComponent<RectTransform>();
        float pad = 0.25f * Mathf.Min(rect.rect.width, rect.rect.height);
        RectTransform areaRect = collectButton.transform.Find("Active Area").GetComponent<RectTransform>();
        RectTransformOffset.All(areaRect, pad);
        RectTransform blurRect = areaRect.transform.Find("Blur").GetComponent<RectTransform>();
        RectTransformOffset.All(blurRect, -2f * pad);
    }

    // ----- TEXT SIZING -----

    // Makes sure each title has the same text size
    // Also sets divider bar size and position
    IEnumerator SetTitleTextSizes() {
        // First, set the font size to as high as it can be, then wait for size to refresh
        foreach (Transform area in AchievementParent) {
            TMP_Text text = area.GetComponentInChildren<TMP_Text>();
            text.fontSizeMax = 10000f;
        }
        yield return new WaitForEndOfFrame();

        // Then, find minimum font size
        float minTextSize = float.MaxValue;
        foreach (Transform area in AchievementParent) {
            TMP_Text text = area.GetComponentInChildren<TMP_Text>();
            if (text.fontSize < minTextSize) { minTextSize = text.fontSize; }
        }

        // Next, set the font size
        foreach (Transform area in AchievementParent) {
            TMP_Text text = area.GetComponentInChildren<TMP_Text>();
            text.fontSizeMax = minTextSize;
        }
        yield return new WaitForEndOfFrame();

        // Setup divider size and position
        foreach (Transform area in AchievementParent) {
            RectTransform dividerRect = area.Find("Divider").GetComponent<RectTransform>();
            dividerRect.sizeDelta = new Vector2(0f, 0.065f * titleSize);
            float pos = area.GetComponentInChildren<TMP_Text>().textBounds.size.y + 0.08f * titleSize;
            dividerRect.anchoredPosition = new Vector2(0f, -pos);
        }
    }


    // ----- UTILITIES -----

    // Returns every achievement
    public List<Achievement> AllAchievements() {
        List<Achievement> achs = new List<Achievement>();
        foreach (var ach in LevelAchievements)       { achs.Add(ach); }
        foreach (var ach in BoardsCompleted)         { achs.Add(ach); }
        foreach (var ach in DaysPlayed)              { achs.Add(ach); }
        foreach (var ach in PlayerLevelAchievements) { achs.Add(ach); }
        foreach (var ach in OtherAchievements)       { achs.Add(ach); }
        return achs;
    }

    // Gives names to each board achievement
    void GiveBoardAchievementsNames(List<BoardAchievement> achs) {
        foreach (BoardAchievement ach in achs) {
            if (ach.name != "" || ach.name.Length > 2) { continue; }
            string board = ach.boardSize.ToString().Remove(0, 1);
            string box = ach.boxSize.ToString().Remove(0, 1);
            ach.name = board + " - [" + box + "]";
            ach.info = "Complete every level of every difficulty in the " + ach.name + " set";
        }
    }

    // Resets all achievements and progression and shit
    public void RESET_ALL_ACHIEVEMENTS() {
        Achievements.RESET_ACHIEVEMENTS();
    }
    public void COMPLETE_ALL_ACHEIVEMENTS() {
        Achievements.COMPLETE_ALL();
    }

    // Turns off all layout groups after a bit to reduce lag
    IEnumerator ActivateLayoutGroups(Transform parent, bool turnOff, bool wait) {
        // Make sure everything is on
        VerticalLayoutGroup vert = parent.parent.GetComponent<VerticalLayoutGroup>();
        if (vert != null) {
            vert.enabled = true;
            //vert.childControlWidth = false;
        }

        foreach (Transform child in parent) {
            ActivateLayoutRecursive(child, false);
        }
        // Wait for things to set up
        for (int i = 0; i < 5; ++i) {
            if (wait) {
                yield return new WaitForSeconds(1);
            }
        }
        // Start the recursion
        foreach (Transform child in parent) {
            ActivateLayoutRecursive(child, turnOff);
        }

        if (vert != null) {
            yield return new WaitForEndOfFrame();
            //vert.childControlWidth = true;
            yield return new WaitForEndOfFrame();
            //vert.enabled = !turnOff;
        }
    }
    void ActivateLayoutRecursive(Transform parent, bool turnOff) {
        // Turn off any layout groups
        VerticalLayoutGroup vert = parent.GetComponent<VerticalLayoutGroup>();
        if (vert != null) { vert.enabled = !turnOff; }
        HorizontalLayoutGroup hor = parent.GetComponent<HorizontalLayoutGroup>();
        if (hor != null)  { hor.enabled = !turnOff; }
        GridLayoutGroup grid = parent.GetComponent<GridLayoutGroup>();
        if (grid != null) { grid.enabled = !turnOff; }

        // Repeat for its children
        foreach (Transform child in parent) {
            ActivateLayoutRecursive(child, turnOff);
        }
    }
}
