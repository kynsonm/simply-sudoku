using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class StatisticsManager : MonoBehaviour
{
    [System.Serializable]
    public enum StatisticSection {
        Completion, Actions, Game_Time, Levels, Dates
    }

    [System.Serializable]
    public enum StatisticVariable {
        // Completion
        levelsCompleted, percentageCompleted, awardsGotten, percentageAwards,
        // Tracking actions
        numbersPlaced, numbersErased, editsMade, undoneActions, redoneActions, hintsUsed,
        // Tracking time
        daysPlayed, totalTimePlayed, totalGameTime,
        // Levels stuff
        averageCompletionTime, quickestCompletion, mostCompletesInaDay, averageActionsUsed,
        // Dates
        dateStarted, lastPlayed
    }

    // For creating stuff
    [System.Serializable]
    public class StatisticToMake {
        public StatisticSection section;
        public StatisticVariable var;
        public string title;
        public string suffix;
        [TextArea(1, 5)]
        public string infoText;
    }


    // ----- VARIABLES -----
    [SerializeField] Transform StatisticsHolder;
    [SerializeField] GameObject StatisticTitle;
    [SerializeField] GameObject StatisticsObject;
    [SerializeField] float StatisticHeightDivider;
    [Space(10f)]
    [SerializeField] LookType titleLook;
    [SerializeField] LookType amountLook;
    [Space(10f)]
    [SerializeField] List<StatisticToMake> statisticsToMake;
    

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Statistics.Load();
        Statistics.lastPlayed = System.DateTime.Now;
        StartCoroutine(Save());
        StartCoroutine(MakeObjects());
        CreateTimers();
    }

    // Creates statistics timers if necessary
    void CreateTimers() {
        // Start the total time counter
        if (GameObject.Find("Total Time Played") == null) {
            GameObject newObject = new GameObject();
            GameObject timerObj = Instantiate(newObject);
            timerObj.name = "Total Time Played";
            DontDestroyOnLoad(timerObj);
            StatisticsTimer timer = timerObj.AddComponent<StatisticsTimer>();
            GameObject.Destroy(newObject);

            // Total time action
            UnityAction<float> a = (float value) => {
                Statistics.totalTimePlayed += value;
                timer.UpdateText(Statistics.totalTimePlayed);
            };
            timer.StartTimer(a, 1f);
        }

        // Start the game time counter
        if (GameObject.Find("Total Game Time") == null) {
            GameObject newObject = new GameObject();
            GameObject timerObj = Instantiate(newObject);
            timerObj.name = "Total Game Time";
            DontDestroyOnLoad(timerObj);
            StatisticsTimer timer = timerObj.AddComponent<StatisticsTimer>();
            GameObject.Destroy(newObject);

            // Game timer action
            UnityAction<float> a = (float value) => {
                if (!GameManager.isCompleted && SceneLoader.CurrentScene == SceneLoader.Scene.Game) {
                    Statistics.totalGameTime += value;
                }
                if (Statistics.totalGameTime != value) {
                    timer.UpdateText(Statistics.totalGameTime);
                }
            };
            timer.StartTimer(a, 1f);
        }
    }

    // Makes all statistics areas, hard coded by each <StatisticSection>
    IEnumerator MakeObjects() {
        StatisticHeightDivider = (StatisticHeightDivider < 1f) ? 1f : StatisticHeightDivider;

        if (StatisticsHolder == null) {
            Debug.Log("Statistics Holder is null!");
            yield break;
        }

        // Destroy what was there and clear the lists
        foreach (Transform trans in StatisticsHolder) {
            GameObject.Destroy(trans.gameObject);
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Statistics.Load();
        
        // Make each object by section
        MakeStatisticSection(StatisticSection.Completion);
        MakeStatisticSection(StatisticSection.Actions);
        MakeStatisticSection(StatisticSection.Levels);
        MakeStatisticSection(StatisticSection.Game_Time);
        MakeStatisticSection(StatisticSection.Dates);

        // Set size of content
        float size = 0f;
        foreach (Transform trans in StatisticsHolder) {
            RectTransform rect = trans.GetComponent<RectTransform>();
            size += rect.sizeDelta.y;
        }
        VerticalLayoutGroup vert = StatisticsHolder.GetComponent<VerticalLayoutGroup>();
        size += vert.padding.top + vert.padding.bottom + vert.spacing * (StatisticsHolder.transform.childCount - 1);
        StatisticsHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, size);
    }

    // Creates statistics menu
    void MakeStatisticSection(StatisticSection section) {
        // Create the title and set size
        GameObject title = Instantiate(StatisticTitle, StatisticsHolder.transform);
        title.name = section.ToString() + " Achs";
        RectTransform rect = title.gameObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 0.8f * (Screen.height / StatisticHeightDivider));

        float barPos = 0.1f * rect.sizeDelta.y;
        float barSize = 0.1f * rect.sizeDelta.y;
        float titleSize = rect.sizeDelta.y - barSize - barPos;

        // Set its text
        TMP_Text text = title.GetComponentInChildren<TMP_Text>();
        text.text = section.ToString().Replace('_', ' ');
        text.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, titleSize);
        text.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

        // Set the bar
        RectTransform barRect = title.transform.Find("Bar").GetComponent<RectTransform>();
        barRect.sizeDelta = new Vector2(0f, barSize);
        barRect.anchoredPosition = new Vector2(0f, barPos);

        // Create the statistics in that section
        List<StatisticToMake> stats = StatisticsBySection(section);
        GameObject lastStatMade = null;
        foreach (StatisticToMake stat in stats) {
            lastStatMade = MakeStatistic(stat);
        }
        lastStatMade.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 1.15f * (Screen.height / StatisticHeightDivider));
    }

    GameObject MakeStatistic(StatisticToMake stat) {
        // Make the object
        GameObject obj = GameObject.Instantiate(StatisticsObject, StatisticsHolder);
        obj.name = stat.title;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, Screen.height / StatisticHeightDivider);

        // ----- Title
        Transform statText = obj.transform.Find("Text");
        string title = stat.title + ":";
        statText.Find("Title").GetComponent<TMP_Text>().text = title;

        // ----- Amount

        string value = (typeof(Statistics).GetField(stat.var.ToString()).GetValue(null)).ToString();

        // Specific text for percentage completed
        if (stat.var == StatisticVariable.percentageCompleted) {
            float perc = 10000f * Statistics.percentageCompleted;
            perc = Mathf.Round(perc);
            perc /= 100f;
            value = perc.ToString();
        }
        // Text for awards gotten
        if (stat.var == StatisticVariable.awardsGotten) {
            Statistics.awardsGotten = Achievements.NumberOfAchievementsCompleted();
            value = Statistics.awardsGotten.ToString();
        }
        // Specific text for percentage awards completed
        if (stat.var == StatisticVariable.percentageAwards) {
            Statistics.awardsGotten = Achievements.NumberOfAchievementsCompleted();
            Statistics.percentageAwards = (float)Statistics.awardsGotten / (float)Achievements.allAchievements.Count;
            float perc = 10000f * Statistics.percentageAwards;
            perc = Mathf.Round(perc);
            perc /= 100f;
            Debug.Log("statPerc == " + Statistics.percentageAwards + "  and  PERCENTAGE == " + perc);
            value = perc.ToString();
        }
        // Text for time statistics
        if (stat.var == StatisticVariable.averageCompletionTime) {
            float time = 0f;
            int count = 0;
            foreach (var level in SavedLevels.GetSavedLevels()) {
                if (!level.isCompleted) { continue; }
                time += level.completedTime;
                ++count;
            }
            if (count == 0) {
                value = "No levels completed yet!";
            } else {
                Statistics.averageCompletionTime = time / (float)count;
                value = FormatTime(Statistics.averageCompletionTime);
                stat.suffix = "";
            }
        }
        if (stat.var == StatisticVariable.quickestCompletion) {
            value = FormatTime(Statistics.quickestCompletion);
        }

        // Set timer texts
        if (stat.var == StatisticVariable.totalGameTime) {
            GameObject timerObj = GameObject.Find("Total Game Time");
            StatisticsTimer timer = timerObj.GetComponent<StatisticsTimer>();
            TMP_Text text = statText.Find("Info").GetComponentInChildren<TMP_Text>();
            text.gameObject.name = "Total Game Time";
            timer.SetTimerText(text);
        }
        else if (stat.var == StatisticVariable.totalTimePlayed) {
            GameObject timerObj = GameObject.Find("Total Time Played");
            StatisticsTimer timer = timerObj.GetComponent<StatisticsTimer>();
            TMP_Text text = statText.Find("Info").GetComponentInChildren<TMP_Text>();
            text.gameObject.name = "Total Time Played";
            timer.SetTimerText(text);
        }
        else {
            value += " <i>" + stat.suffix;
            statText.Find("Info").GetComponentInChildren<TMP_Text>().text = value;
        }

        // ----- Info message
        InfoButton info = obj.GetComponentInChildren<InfoButton>();
        info.message = stat.infoText;

        return obj;
    }

    // Returns a list of statistics depending on the inputted seciont
    List<StatisticToMake> StatisticsBySection(StatisticSection section) {
        List<StatisticToMake> stats = new List<StatisticToMake>();
        foreach (StatisticToMake stat in statisticsToMake) {
            if (stat.section == section) {
                stats.Add(stat);
            }
        }
        return stats;
    }

    // Takes in time float and returns a formatted string
    string FormatTime(float seconds) {
        Debug.Log("FORMATTING TIME: seconds = " + seconds);

        // Make sure seconds is valid
        if (seconds == float.NaN) { return "NaN"; }
        if (seconds <= 0f) { return "0"; }
        if (seconds >= 99999f) { return "Too High!"; }

        string s = "";

        TimeSpan time = TimeSpan.FromSeconds(seconds);

        if (time.Days > 0) {
            s += time.Days.ToString() + " <i>days</i>, ";
        }
        if (time.Hours > 0) {
            s += time.Hours.ToString() + " <i>hours</i>, ";
        }
        if (time.Minutes > 0) {
            s += time.Minutes.ToString() + " <i>minutes</i>, ";
        }
        string dec = "." + (time.Milliseconds / 100);

        s += time.Seconds + dec + " <i>seconds</i>";

        return s;
    }

    // Save statistics every so often
    IEnumerator Save() {
        AchievementManager manager = GameObject.FindObjectOfType<AchievementManager>();
        while (true) {
            yield return new WaitForSeconds(2f);
            Statistics.Save();
        }
    }
}
