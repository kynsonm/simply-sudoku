using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    // For referencing GameManager
    public GameObject GameManagerObject;

    // Top Bar stuff
    [Space(10f)]
    public GameObject TitleObject;
    TMP_Text titleText;

    // Game board stuff
    // ???


    // ----- Monobehaviour Stuff -----

    // Start is called before the first frame update
    void Start()
    {
        // Initial variable states
        Time.timeScale = 1.0f;
        startTime = Time.time;
        currTime = Time.time;

        GetObjects();
        SetTitleText();
        StartCoroutine(UpdateTimer());
        UpdateGameUI();
    }

    // Update is called once per frame
    void UpdateGameUI()
    {
        StartCoroutine(UpdateGameUIEnum());
    }
    IEnumerator UpdateGameUIEnum() {
        while (!timerEnumRunning) {
            StartCoroutine(UpdateTimer());
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
    }
    // ----- monobehaviour stuff -----


    // ----- Real stuff -----

    // Set the title text w/ Type and difficulty
    void SetTitleText() {
        string title = "";

        // Board size
        Vector2Int boardSize = new Vector2Int(LevelInfo.BoardSizeToWidth(), LevelInfo.BoardSizeToHeight());
        if (boardSize.x == 9 && boardSize.y == 9) {
            title += "Standard";
        } else {
            title += boardSize.x.ToString() + "x" + boardSize.y.ToString();
        }

        string diff = LevelInfo.DifficultyToString(LevelInfo.CurrentDifficulty);

        title += " - " + diff;

        titleText.text = title;
    }


    // Timer stuff
    [Space(10f)]
    public GameObject TimerObject;
    TMP_Text timerText;
    [SerializeField] int numSpacing, colonSpacing;
    int lastNumSpacing, lastColonSpacing;
    bool timerEnumRunning;
    bool lastIsPaused;
    float startTime;    // Start time when the scene was loaded
    float pausedTime;   // Time when the game was paused
    float pausedTotal;  // Total amount of time the game has been paused
    float currTime;     // Current time
    public float SavedLevelTimeOffset;

    public void ResetTimeVariables() {
        StopAllCoroutines();

        Time.timeScale = 1.0f;
        startTime = Time.time;
        currTime = Time.time;

        SavedLevelTimeOffset = 0f;

        StartCoroutine(UpdateTimer());
    }

    // Run the timers
    IEnumerator UpdateTimer() {
        timerEnumRunning = true;
        pausedTotal = 0;
        pausedTime = 0;

        // Get spacing strings
        float multiplier = GetMultiplier();
        Debug.Log("Multiplier == " + multiplier);
        string numSpacingStr = "<mspace=" + multiplier * numSpacing + ">";
        string colSpacingStr = "<mspace=" + multiplier * colonSpacing + ">";

        float deltaSum = 0;
        while (true) {
            // Set current time
            currTime = Time.time;

            // If timer is not active, dont do anything
            if (GameManager.IsPaused) {
                if (!lastIsPaused) { pausedTime = currTime; }
                lastIsPaused = true;

                yield return new WaitForEndOfFrame();
                continue;
            }
            if (pausedTime > 0f) {
                pausedTotal += currTime - pausedTime;
                pausedTime = 0f;
            }
            lastIsPaused = false;

            // Only update text every 0.2 seconds
            deltaSum += Time.deltaTime;
            if (deltaSum < 0.2f) {
                yield return new WaitForEndOfFrame();
                continue;
            }
            deltaSum = 0f;

            multiplier = GetMultiplier();
            numSpacingStr = "<mspace=" + multiplier * numSpacing + ">";
            colSpacingStr = "<mspace=" + multiplier * colonSpacing + ">";

            // Update timer text
            //   into format of:  mm:ss or hh:mm:ss
            string str = "";
            float totalTime = (currTime - startTime) - pausedTotal + SavedLevelTimeOffset;
            GameManager.GameTime = totalTime;
            TimeSpan span = TimeSpan.FromSeconds(totalTime);
            // Hours
            if (span.Hours > 0) {
                str += numSpacingStr;
                str += span.Hours;
                /*if (span.Hours > 9) {
                    str += span.Hours;
                } else {
                    str += "0" + span.Hours;
                }*/
                str += colSpacingStr;
                str += ":";
            }
            // Minutes
            str += numSpacingStr;
            str += span.Minutes;
            str += colSpacingStr + ":";
            str += numSpacingStr;
            // Seconds
            if (span.Seconds > 9) {
                str += span.Seconds;
            } else {
                str += "0" + span.Seconds;
            }

            if (Settings.ShowTimer) {
                timerText.text = str;
            } else {
                timerText.text = "";
            }

            yield return new WaitForEndOfFrame();
        }
    }


    // ----- Utilities -----

    float GetMultiplier() {
        float font = timerText.fontSize;
        float refFont = 136f;
        float ratio = font / refFont;
        //ratio = MathF.Sqrt(ratio);
        //ratio = MathF.Sqrt(ratio);

        float screen = (Screen.width + Screen.height) / 2f;
        float screenRef = (3040f + 1440f) / 2f;
        float screenRatio = screen / screenRef;

        return ratio * screenRatio;
    }

    // Get objects needed in other scripts
    void GetObjects() {
        // Get game manager object (this object) if null
        if (GameManagerObject == null) {
            GameManagerObject = gameObject;
        }

        // Get title text
        if (titleText == null) {
            titleText = TitleObject.GetComponent<TMP_Text>();
            if (titleText == null) {
                Debug.Log("Got title text from child");
                titleText = TitleObject.GetComponentInChildren<TMP_Text>();
            }
            if (titleText == null) {
                Debug.Log("No TMP_Text on title object or in children!");
            }
        }

        // Get timer text
        if (timerText == null) {
            timerText = TimerObject.GetComponent<TMP_Text>();
            if (timerText == null) {
                Debug.Log("Got timer text from child");
                timerText = TimerObject.GetComponentInChildren<TMP_Text>();
            }
            if (timerText == null) {
                Debug.Log("No TMP_Text on timer object or in children!");
            }
        }
    }
}
