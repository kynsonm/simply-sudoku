using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
/*

public class AchievementManager : MonoBehaviour
{
    // Achievement objects
    public GameObject AchievementAreaPrefab;
    public GameObject AchievementPrefab;
    public Transform AchievementParent;
    public GameObject PlayerInfoObject;

    [Space(5f)]
    public float TitleDivider;
    public float AchievementHeightDivider;

    // Saved Achievements
    CompletedAchievements completedAchievements = null;

    // Achievements
    [Space(5f)]
    public List<Achievement> LevelAchievements;
    public List<Achievement> PlayerLevelAchievements;
    public List<BoardAchievement> BoardsCompleted;
    public List<Achievement> DaysPlayed;
    public List<Achievement> OtherAchievements;

    // In game, this is a list of all the achievement areas made
    [HideInInspector]
    public List<GameObject> achAreas;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        Achievements.Start();
        Achievements.AchievementHeightDivider = AchievementHeightDivider;

        // Set ID's for each level
        SetIDs();

        // Set everything up
        Achievements.SetVars(this);

        LoadCompletedAchievements();
        StartCoroutine(SaveCompletedAchievementsEnum());
        SetCompletedAchievements();

        yield return new WaitForEndOfFrame();
        Achievements.Start();
        CreateAchievements();
    }


    // ----- Achievements information -----

    public List<Achievement> ALL() {
        List<Achievement> achs = new List<Achievement>();
        foreach (Achievement ach in LevelAchievements)       { achs.Add(ach); }
        foreach (Achievement ach in PlayerLevelAchievements) { achs.Add(ach); }
        foreach (Achievement ach in BoardsCompleted)         { achs.Add(ach); }
        foreach (Achievement ach in DaysPlayed)              { achs.Add(ach); }
        foreach (Achievement ach in OtherAchievements)       { achs.Add(ach); }
        return achs;
    }
    public int NumberOfAchievements() {
        int num;
        num  = LevelAchievements.Count;
        num += PlayerLevelAchievements.Count;
        num += BoardsCompleted.Count;
        num += DaysPlayed.Count;
        num += OtherAchievements.Count;
        return num;
    }
    public int NumberOfAchievementsGotten() {
        if (completedAchievements == null) { LoadCompletedAchievements(); }
        return completedAchievements.achievements.Count;
    }

    public void RESET_ACHIEVEMENTS() {
        Achievements.RESET_ACHIEVEMENTS();
        CreateAchievements();
    }


    // ----- SAVED LEVELS -----

    void SetCompletedAchievements() {
        foreach (Achievement ach in ALL()) {
            long id = ach.ID;
            if (completedAchievements.search.ContainsKey(id)) {
                Achievement comp = completedAchievements.search[id];
                ach.completed = comp.completed;
                ach.collected = comp.collected;
            }
        }
    }

    // Save all completed achievements into json file
    public void SaveCompletedAchievements() {
        string path = Application.persistentDataPath + "/SavedAchievements.json";
        string text = JsonUtility.ToJson(completedAchievements);
        File.WriteAllText(path, text);
    }
    IEnumerator SaveCompletedAchievementsEnum() {
        while (true) {
            yield return new WaitForSeconds(1f);
            SaveCompletedAchievements();
        }
    }

    // Load saved achievements from json file, if it exists
    public void LoadCompletedAchievements() {
        string path = Application.persistentDataPath + "/SavedAchievements.json";
        completedAchievements = new CompletedAchievements();
        // Check if json file exists, and read it if so
        if (File.Exists(path)) {
            string text = File.ReadAllText(path);
            completedAchievements = JsonUtility.FromJson<CompletedAchievements>(text);
        }
        // otherwise, create it w/ Save() call
        else {
            SaveCompletedAchievements();
        }

        // Add them all to <search>
        completedAchievements.search = new Dictionary<long, Achievement>();
        foreach (Achievement ach in completedAchievements.achievements) {
            ach.ID = Achievements.ID(ach);
            completedAchievements.search.Add(ach.ID, ach);
        }

        Debug.Log("--- GOT COMPLETED ACHIEVEMENS: ---\n" + completedAchievements.ToString());
    }

    // --------------------------------

    IEnumerator AchievementParentSize() {
        yield return new WaitForEndOfFrame();
        float size = 0f;
        foreach (Transform trans in AchievementParent.transform) {
            size += trans.GetComponent<RectTransform>().rect.height;
        }

        VerticalLayoutGroup vert = AchievementParent.GetComponent<VerticalLayoutGroup>();
        size += (float)(AchievementParent.transform.childCount - 1) * vert.spacing;
        size += vert.padding.top + vert.padding.bottom;
        AchievementParent.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, size);
    }

    void CreateAchievements() {
        if (AchievementParent == null) { return; }

        //StartCoroutine(TurnOffLayoutGroups(false, false));

        // Destroy whats already there
        foreach (Transform trans in AchievementParent) {
            GameObject.Destroy(trans.gameObject);
        }

        // Create all the objects
        CreateAchievements(LevelAchievements, "Levels Completed");
        CreateAchievements(BoardsCompleted, "Level Types Completed");
        CreateAchievements(DaysPlayed, "Days Played (In A Row!)");
        CreateAchievements(PlayerLevelAchievements, "Player Level");
        CreateAchievements(OtherAchievements, "Other Achievements");

        // Call SetChildDimensions() to set heights of everything
        SetAchievementDimensions();
        StartCoroutine(AchievementParentSize());
        SetTextSizes();

        //StartCoroutine(TurnOffLayoutGroups(true, true));
    }
    GameObject CreateAchievements(List<Achievement> achievements, string achievementsTitle) {
        if (AchievementParent == null) { return null; }

        // Set up the initial area
        GameObject achs = Instantiate(AchievementAreaPrefab, AchievementParent);
        achs.name = achievementsTitle + " Achs";
        achs.GetComponentInChildren<TMP_Text>().text = achievementsTitle;

        int NumLevelsCompleted = Achievements.NumLevelsCompleted();

        // TODO: SET ACH ICON DIMENSIONS TO BE SQUARE (SOMEWHERE)

        // Set up each level
        Transform holder = achs.transform.Find("Achievements");
        foreach (Achievement ach in achievements) {
            // Instantiate the achievement
            GameObject obj = Instantiate(AchievementPrefab, holder);
            ach.achievementObject = obj;

            // Set text stuff
            Transform textObj = obj.transform.Find("Text");
            TMP_Text title = textObj.Find("Title").GetComponent<TMP_Text>();
            title.text = ach.name;
            TMP_Text info = textObj.Find("Description").GetComponent<TMP_Text>();
            TextTheme theme = info.gameObject.GetComponent<TextTheme>();

            // Set icon stuff
            Transform image = obj.transform.Find("Image");
            Transform icon = image.Find("Icon");
            icon.GetComponentInChildren<TMP_Text>().text = ach.iconText;

            ImageTheme imgTheme = icon.GetComponentInChildren<ImageTheme>();
            if (ach.completed) {
                info.text = ach.achievedText;
                theme.IsItalic = true;
                imgTheme.gameObject.transform.parent.GetComponentInChildren<TextTheme>().Alpha = 1f;
                imgTheme.UseColor = false;
            } else {
                info.text = ach.info;
                theme.IsItalic = false;
                imgTheme.gameObject.transform.parent.GetComponentInChildren<TextTheme>().Alpha = 0.4f;
                imgTheme.UseColor = true;
                imgTheme.color = WhichColor.grey;
            }

            // Set progress stuff
            int max = ach.targetNumber;
            int prog = ach.progress;
            int off = (int)(0.16f * max);

            //bool low = prog < 0.1f * max || prog == 0 || max == 0;
            bool low = prog == 0 || max == 0;
            bool high = prog > max;
            if (low || high) {
                image.Find("Progress").gameObject.AddComponent<CanvasGroup>().alpha = 0f;
                continue;
            }

            Transform progress = image.Find("Progress");
            Slider slider = progress.GetComponent<Slider>();
            slider.maxValue = max;
            slider.value = prog + off;
            slider.interactable = false;

            TMP_Text text = progress.GetComponentInChildren<TMP_Text>();
            text.text = prog + " / " + max;
        }

        return achs;
    }
    GameObject CreateAchievements(List<BoardAchievement> boardAchs, string achievementsTitle) {
        if (AchievementParent == null) { return null; }

        // Set up the initial area
        GameObject achs = Instantiate(AchievementAreaPrefab, AchievementParent);
        achs.name = achievementsTitle + " Achs";
        achs.GetComponentInChildren<TMP_Text>().text = achievementsTitle;

        // Set up each level
        Transform holder = achs.transform.Find("Achievements");
        foreach (BoardAchievement ach in boardAchs) {
            // Instantiate the achievement
            GameObject obj = Instantiate(AchievementPrefab, holder);
            ach.achievementObject = obj;

            if (ach.name == "") {
                string m = "\U000000D7";
                string d = "\U00002013";
                int y = LevelInfo.BoardSizeToHeight(ach.boardSize);
                int x = LevelInfo.BoardSizeToWidth(ach.boardSize);
                Vector2 b = LevelInfo.BoxSizeVector(ach.boxSize, ach.boardSize);
                ach.name = x + m + y + " " + d + " [" + b.x + m + b.y + "]";
                ach.info = "Complete every level in the " + ach.name + " set";
                ach.iconText = b.x + m + b.y;
            }

            // Set text stuff
            Transform textObj = obj.transform.Find("Text");
            TMP_Text title = textObj.Find("Title").GetComponent<TMP_Text>();
            title.text = ach.name;
            TMP_Text info = textObj.Find("Description").GetComponent<TMP_Text>();
            TextTheme theme = info.gameObject.GetComponent<TextTheme>();

            // Set icon stuff
            Transform image = obj.transform.Find("Image");
            Transform icon = image.Find("Icon");
            icon.GetComponentInChildren<TMP_Text>().text = ach.iconText;

            ImageTheme imgTheme = icon.GetComponentInChildren<ImageTheme>();
            if (ach.completed) {
                info.text = ach.achievedText;
                theme.IsItalic = true;
                imgTheme.gameObject.transform.parent.GetComponentInChildren<TextTheme>().Alpha = 1f;
                imgTheme.UseColor = false;
            } else {
                info.text = ach.info;
                theme.IsItalic = false;
                imgTheme.gameObject.transform.parent.GetComponentInChildren<TextTheme>().Alpha = 0.4f;
                imgTheme.UseColor = true;
                imgTheme.color = WhichColor.grey;
            }

            // Set progress stuff
            int max = ach.targetNumber;
            int prog = ach.progress;
            int off = (prog < 0.16f * max) ? (int)(0.16f * max) : 0;

            //bool low = prog < 0.1f * max || prog == 0 || max == 0;
            bool low = prog == 0 || max == 0;
            bool high = prog > max;
            if (low || high) {
                image.Find("Progress").gameObject.AddComponent<CanvasGroup>().alpha = 0f;
                continue;
            }

            Transform progress = image.Find("Progress");
            Slider slider = progress.GetComponent<Slider>();
            slider.maxValue = max;
            slider.value = prog + off;
            slider.interactable = false;

            TMP_Text text = progress.GetComponentInChildren<TMP_Text>();
            text.text = prog + " / " + max;
        }

        return achs;
    }


    // Setup the grid or vertical layout group
    // Setup title size, total achievement area size, 
    void SetAchievementDimensions() {
        if (AchievementParent == null) { return; }

        TitleDivider = (TitleDivider <= 1f) ? 1f : TitleDivider;
        AchievementHeightDivider = (AchievementHeightDivider < 1f) ? 1f : AchievementHeightDivider;

        float titleHeight = Screen.height / TitleDivider;
        float height = Screen.height / AchievementHeightDivider;

        for (int i = 0; i < achAreas.Count; ++i) {
            // Title size
            RectTransform title = achAreas[i].transform.Find("Area Title").GetComponent<RectTransform>();
            title.sizeDelta = new Vector2(0f, titleHeight);

            // Each achievement size
            Transform holder = achAreas[i].transform.Find("Achievements");
            foreach (Transform ach in holder) {
                //ach.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, height);
            }

            VerticalLayoutGroup vert = holder.GetComponent<VerticalLayoutGroup>();
            //GridLayoutGroup vert = holder.GetComponent<GridLayoutGroup>();
            float spacing = (float)(holder.childCount-1) * vert.spacing;
            //float spacing = (float)(holder.childCount-1) * vert.spacing.y;
            float size = (height * holder.childCount) + spacing + vert.padding.top + vert.padding.bottom;

            // Content size and position of holder
            RectTransform holdRect = holder.GetComponent<RectTransform>();
            holdRect.sizeDelta = new Vector2(0f, size);
            holdRect.anchoredPosition = new Vector2(0f, 0f);

            // Content size of area
            achAreas[i].GetComponent<RectTransform>().sizeDelta = new Vector2(0f, size + titleHeight);
        }
    }


    // Set achievement ID's
    void SetIDs() {
        SetIDs(LevelAchievements);
        SetIDs(PlayerLevelAchievements);
        SetIDs(BoardsCompleted);
        SetIDs(DaysPlayed);
        SetIDs(OtherAchievements);
    }
    void SetIDs(List<Achievement> achievements) {
        foreach (Achievement ach in achievements) {
            ach.ID = Achievements.ID(ach);
        }
    }
    void SetIDs(List<BoardAchievement> achievements) {
        foreach (Achievement ach in achievements) {
            ach.ID = Achievements.ID(ach);
        }
    }


    // ----- SET TEXT SIZES -----

    public void SetTextSizes() {
        StartCoroutine(SetTextSizeEnum());
    }

    IEnumerator SetTextSizeEnum() {
        // If not active, wait a second and try again
        if (achAreas == null || achAreas.Count == 0 || !achAreas[0].activeInHierarchy) {
            yield return new WaitForSeconds(1f);
            StartCoroutine(SetTextSizeEnum());
            yield break;
        }

        // RESET TEXT SIZES
        foreach (GameObject area in achAreas) {
            // Set title to 1000f
            area.GetComponentInChildren<TMP_Text>().fontSizeMax = 1000f;
            // Ach title and description
            Transform holder = area.transform.Find("Achievements");
            foreach (Transform ach in holder) {
                ach.transform.Find("Text").Find("Title").GetComponent<TMP_Text>().fontSizeMax = 1000f;
                ach.transform.Find("Text").Find("Description").GetComponent<TMP_Text>().fontSizeMax = 1000f;
            }
        }

        yield return new WaitForEndOfFrame();

        float areaTitleMin = int.MaxValue;
        float titleMin = int.MaxValue;
        float descMin = int.MaxValue;

        // FIND LOWEST FONT SIZE
        foreach (GameObject area in achAreas) {
            // Check title text
            TMP_Text areaTitle = area.GetComponentInChildren<TMP_Text>();
            if (areaTitle.fontSize < areaTitleMin) {
                areaTitleMin = areaTitle.fontSize;
            }

            Transform holder = area.transform.Find("Achievements");
            foreach (Transform ach in holder) {
                // Check ach title text
                TMP_Text title = ach.transform.Find("Text").Find("Title").GetComponent<TMP_Text>();
                if (title.fontSize < titleMin) {
                    titleMin = title.fontSize;
                }

                // Check desc text
                TMP_Text desc = ach.transform.Find("Text").Find("Description").GetComponent<TMP_Text>();
                if (desc.fontSize < descMin) {
                    descMin = desc.fontSize;
                }
            }
        }

        // SET FONT SIZES
        foreach (GameObject area in achAreas) {
            // Set title to 1000f
            area.GetComponentInChildren<TMP_Text>().fontSizeMax = areaTitleMin;
            // Ach title and description
            Transform holder = area.transform.Find("Achievements");
            foreach (Transform ach in holder) {
                ach.transform.Find("Text").Find("Title").GetComponent<TMP_Text>().fontSizeMax = titleMin;
                ach.transform.Find("Text").Find("Description").GetComponent<TMP_Text>().fontSizeMax = descMin;
                ach.transform.Find("Image").Find("Icon").GetComponentInChildren<TextTheme>().ResetTextSize();
                ach.transform.Find("Image").Find("Progress").GetComponentInChildren<TextTheme>().ResetTextSize();
            }
        }

        yield return new WaitForEndOfFrame();

        foreach (GameObject area in achAreas) {
            // Divider size
            RectTransform divTrans = area.GetComponentInChildren<Image>().gameObject.GetComponent<RectTransform>();
            float titleTextSize = area.GetComponentInChildren<TMP_Text>().textBounds.extents.y;
            titleTextSize *= 2f;
            titleTextSize += Screen.height / 300f;
            divTrans.sizeDelta = new Vector2(0f, (float)Screen.height / 175f);
            divTrans.anchoredPosition = new Vector2(0f, -titleTextSize);
        }
    }


    // ----- UTILITIES -----

    // Turns off all layout groups after a bit to reduce lag
    IEnumerator TurnOffLayoutGroups(bool turnOff, bool wait) {
        // Wait for things to set up
        for (int i = 0; i < 100; ++i) {
            if (wait) {
                yield return new WaitForEndOfFrame();
            }
        }
        // Start the recursion
        foreach (Transform child in AchievementParent) {
            TurnOffLayoutRecursive(child, turnOff);
        }
    }
    void TurnOffLayoutRecursive(Transform parent, bool turnOff) {
        // Turn off any layout groups
        VerticalLayoutGroup vert = parent.GetComponent<VerticalLayoutGroup>();
        if (vert != null) { vert.enabled = !turnOff; }
        HorizontalLayoutGroup hor = parent.GetComponent<HorizontalLayoutGroup>();
        if (hor != null)  { hor.enabled = !turnOff; }
        GridLayoutGroup grid = parent.GetComponent<GridLayoutGroup>();
        if (grid != null) { grid.enabled = !turnOff; }

        // Repeat for its children
        foreach (Transform child in parent) {
            TurnOffLayoutRecursive(child, turnOff);
        }
    }
}
*/