using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using TMPro;
using UnityEngine.UI;
using static SoundClip;


// ----- Achievement base class
[System.Serializable]
public class Achievement
{
    // Info
    public string name;
    public string info;

    [HideInInspector] public int progress;
    public int targetNumber;
    
    public int xp;
    public int reward;

    [HideInInspector] public bool completed;
    [HideInInspector] public bool collected;
    public string achievedText;

    // Looks
    public string iconText;

    // Backend
    [HideInInspector] public long ID;
    [HideInInspector] public GameObject achievementObject;

    // Testing constructor
    public Achievement() {
        name = "Achievement Name";
        info = "Get this achievement by doing... nothing!";
        progress = 5;
        targetNumber = 10;
        xp = 200;
        reward = 500;
        completed = false;
        collected = false;
        achievedText = "Theres nothing better than getting a reward for doing less than the bare minimum";
        iconText = "No";
    }
}


// ----- Class for level-specific achievements
[System.Serializable]
public class BoardAchievement : Achievement
{
    public BoardSize boardSize;
    public BoxSize boxSize;
}


// ----- HOLDS ALL COMPLETED ACHIEVEMENTS
[System.Serializable]
public class CompletedAchievements {
    // Actual saved achievements
    [SerializeField] public List<Achievement> achievements;

    public CompletedAchievements() {
        achievements = new List<Achievement>();
    }
}


public static class Achievements
{
    // Achievements
    static AchievementManager achievementManager;
    public static List<Achievement> allAchievements;
    public static CompletedAchievements completedAchievements;
    static Dictionary<long,Achievement> achievementSearch;

    // Tracking -- Using PlayerPrefs
    public static int DaysPlayed;
    public static DateTime LastPlayed;
    public static int LevelsCompleted;


    // ----- Completion, Collection -----

    // Completes an achievement
    public static void CompleteAchievement(Achievement achievement) {
        if (!achievement.completed) {
            GameObject.FindObjectOfType<AchievementPopup>().Popup(achievement);
            Sound.Play(award_available);
        }
        achievement.completed = true;

        achievementManager.SetUpAchievementObject(achievement);

        if (!completedAchievements.achievements.Contains(achievement)) {
            completedAchievements.achievements.Add(achievement);
        }

        Save();

        Debug.Log($"Completed achievement: {achievement.name}, {achievement.ID}, compl? = {achievement.completed}");
    }

    // Collects an achievement -- xp and coins
    public static void CollectAchievement(Achievement achievement) {
        CompleteAchievement(achievement);
        achievement.collected = true;
        Sound.Play(award_collection);

        achievementManager.SetUpAchievementObject(achievement);

        PlayerInfo.XP += achievement.xp;
        PlayerInfo.Coins += achievement.reward;

        int currentLevel = PlayerInfo.Level;
        PlayerInfo.Level = LevelCurve.LevelAtXP(PlayerInfo.XP);
        if (PlayerInfo.Level != currentLevel) {
            CheckPlayerLevelAchievements();
        }
        PlayerInfo.Save();

        // Reset texts of these areas (ex. XP, level, etc)
        PlayerInfoScript playerInfo = GameObject.FindObjectOfType<PlayerInfoScript>();
        if (playerInfo != null) {
            playerInfo.SetTexts();
        }

        Save();
    }

    // Check each achievements progress value vs its target value
    // If it is <= target value, complete the achievement
    public static void DoubleCheckProgresses() {
        foreach (Achievement ach in allAchievements) {
            if (ach.progress >= ach.targetNumber && !ach.completed) {
                ach.completed = true;
                achievementManager.SetUpAchievementObject(ach);
                if (!completedAchievements.achievements.Contains(ach)) {
                    completedAchievements.achievements.Add(ach);
                }
                Save();
            }
        }
    }


    // ----- TRACKING METHODS -----

    // Check current level for completed level achievement
    public static void CheckPlayerLevelAchievements() {
        if (!checkAchievementManager()) { return; }
        foreach (Achievement ach in achievementManager.PlayerLevelAchievements) {
            if (ach.completed) { continue; }
            
            ach.progress = PlayerInfo.Level;
            if (PlayerInfo.Level < ach.targetNumber) { continue; }

            CompleteAchievement(ach);
            achievementManager.SetUpAchievementObject(ach);
        }
    }

    // Check for days played achievements
    public static void CheckDaysPlayedAchievements() {
        if (!checkAchievementManager()) { return; }
        foreach (Achievement ach in achievementManager.DaysPlayed) {
            if (ach.completed) { continue; }
            
            ach.progress = DaysPlayed;
            if (DaysPlayed < ach.targetNumber) { continue; }

            CompleteAchievement(ach);
            achievementManager.SetUpAchievementObject(ach);
        }
    }

    // When a level is completed, do stuff idk
    public static void LevelCompleted(BoardSize board, BoxSize box) {
        if (!checkAchievementManager()) { return; }
        ++LevelsCompleted;

        // Check all completed levels
        foreach (Achievement ach in achievementManager.LevelAchievements) {
            ach.progress = LevelsCompleted;
            if (ach.progress >= ach.targetNumber && !ach.completed) {
                CompleteAchievement(ach);
            }
        }

        // Check levels of this board type
        foreach (BoardAchievement ach in achievementManager.BoardsCompleted) {
            if (ach.boardSize == board && ach.boxSize == box) {
                ++ach.progress;
                if (ach.progress >= ach.targetNumber && !ach.completed) {
                    CompleteAchievement(ach);
                }
                break;
            }
        }

        Save();
    }


    // ----- Basic information retrieval -----

    // Gets number of completed achievements
    public static int NumberOfAchievementsCompleted() {
        return completedAchievements.achievements.Count;
    }


    // ----- SAVING and LOADING -----

    // Save variables -- Tracking vars and <CompletedAchievements>
    public static void Save() {
        if (allAchievements == null) {
            if (checkAchievementManager()) {
                allAchievements = achievementManager.AllAchievements();
            }
            if (allAchievements == null) {
                Debug.LogWarning("All Achievements is STILL null???");
            }
        }

        // Tracking vars
        string suffix = "_achievements";
        PlayerPrefs.SetInt(nameof(DaysPlayed)+suffix, DaysPlayed);
        PlayerPrefs.SetString(nameof(LastPlayed)+suffix, LastPlayed.ToBinary().ToString());
        PlayerPrefs.SetInt(nameof(LevelsCompleted)+suffix, LevelsCompleted);

        // Completed levels
        string path = Application.persistentDataPath + "/SavedAchievements.json";
        string toSave = JsonUtility.ToJson(completedAchievements);
        File.WriteAllText(path, toSave);

        // Log it
        Debug.Log(allAchievementsLog() + "\n\n" + completedAchievementsLog());
    }
    static void FirstRun() {
        DaysPlayed = 1;
        LastPlayed = DateTime.Now;
        LevelsCompleted = 0;
        completedAchievements = new CompletedAchievements();
        Save();
    }

    // Load saved variables
    public static void Load() {
        if (!PlayerPrefs.HasKey("DaysPlayed_achievements")) { FirstRun(); }
        if (allAchievements == null) {
            if (checkAchievementManager()) {
                allAchievements = achievementManager.AllAchievements();
            }
            if (allAchievements == null) {
                Debug.LogWarning("All Achievements is STILL null???");
            }
        }

        // Tracking vars
        string suffix = "_achievements";
        DaysPlayed = PlayerPrefs.GetInt(nameof(DaysPlayed)+suffix);
        LastPlayed = DateTime.FromBinary( long.Parse(PlayerPrefs.GetString( nameof(LastPlayed)+suffix) ) );
        LevelsCompleted = PlayerPrefs.GetInt(nameof(LevelsCompleted)+suffix);

        // Check days played
        if (LastPlayed.Date < DateTime.Now.Date) {
            ++DaysPlayed;
            LastPlayed = DateTime.Now;
        }

        // Completed levels
        string path = Application.persistentDataPath + "/SavedAchievements.json";
        string text = File.ReadAllText(path);
        completedAchievements = JsonUtility.FromJson<CompletedAchievements>(text);

        if (!checkAchievementManager()) { return; }
        
        // Other vars
        achievementManager.SetupBoardAchievements();
        allAchievements = achievementManager.AllAchievements();
        achievementSearch = new Dictionary<long,Achievement>();
        foreach (Achievement ach in allAchievements) {
            // Set IDs for each achievement
            ach.ID = AchievementID(ach);

            // Add it to the search dictionary
            achievementSearch.Add(ach.ID, ach);
        }

        // Get which achievments are completed
        for (int i = 0; i < completedAchievements.achievements.Count; ++i) {
            Achievement ach = completedAchievements.achievements[i];
            Achievement match = achievementSearch[ach.ID];
            match.completed = ach.completed;
            match.collected = ach.collected;
            completedAchievements.achievements[i] = match;
        }
    }


    // ----- UTILITIES ----

    // displaying all achievements
    public static string allAchievementsLog() {
        string str = "----- ALL ACHIEVEMENTS: -----\n";
        foreach (Achievement ach in allAchievements) {
            str += $" -- {ach.name}, {ach.ID}, compl? = {ach.completed}, coll? = {ach.collected}\n";
        }
        return str;
    }
    // displaying completed achievements
    public static string completedAchievementsLog() {
        string str = "----- COMPLETED ACHIEVEMENTS: -----\n";
        foreach (Achievement ach in completedAchievements.achievements) {
            str += $" -- {ach.name}, {ach.ID}, compl? = {ach.completed}, coll? = {ach.collected}\n";
        }
        return str;
    }

    // Resets all variables
    public static void RESET_ACHIEVEMENTS() {
        foreach (Achievement ach in allAchievements) {
            ach.completed = false;
            ach.collected = false;
            ach.progress = 0;
        }

        completedAchievements = new CompletedAchievements();
        DaysPlayed = 1;
        LastPlayed = DateTime.Now;
        LevelsCompleted = 0;
        Save();

        PlayerInfo.FirstRun();
    }
    public static void COMPLETE_ALL() {
        completedAchievements = new CompletedAchievements();
        foreach (Achievement ach in allAchievements) {
            ach.completed = true;
            ach.collected = false;
            ach.progress = ach.targetNumber;
            completedAchievements.achievements.Add(ach);
        }
    }

    // Return true if achievement manager is found
    static bool checkAchievementManager() {
        if (achievementManager == null) {
            achievementManager = GameObject.FindObjectOfType<AchievementManager>();
            if (achievementManager == null) {
                Debug.LogWarning("Cant find achievement manager in scene!");
                return false;
            }
        }
        return true;
    }

    public static long AchievementID(Achievement ach) {
        long ID = 0;

        for (int i = 0; i < ach.name.Length; ++i) {
            ID += (i+1) * (int)ach.name[i];
        }

        ID *= 1 + ach.xp;
        ID *= 1 + ach.reward;

        return ID;
    }

    public static void SetVars(AchievementManager manager) {
        achievementManager = manager;
        if (achievementManager == null) {
            Debug.Log("Achievement manager is null!");
        }
    }
}