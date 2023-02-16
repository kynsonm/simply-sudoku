using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Statistics
{
    // ----- STATISTIC VARIABLES -----

    // Completion
    public static int levelsCompleted;
    public static float percentageCompleted;
    public static int awardsGotten;
    public static float percentageAwards;

    // Tracking actions
    public static int numbersPlaced;
    public static int numbersErased;
    public static int editsMade;
    public static int undoneActions;
    public static int redoneActions;
    public static int hintsUsed;

    // Tracking time
    public static int daysPlayed;
    public static float totalTimePlayed;
    public static float totalGameTime;

    // Levels
    public static float averageCompletionTime;
    public static int averageActionsUsed;
    public static float quickestCompletion;
    public static int mostCompletesInaDay;

    // Dates
    public static DateTime dateStarted;
    public static DateTime lastPlayed;


    // ----- HELPER VARIALBES -----

    public static int currentGameActions;
    public static int gamesCompletedToday;


    // ----- SAVE & LOAD -----

    // Load and calculate all the variables
    public static void Load() {
        // First time app runs
        if (!PlayerPrefs.HasKey(nameof(levelsCompleted))) {
            FirstRun();
        }
        // Wait a sec if not ready yet
        else if (SavedLevels.GetSavedLevels() == null) {
            SavedLevels.Load();
        }

        Achievements.Load();

        // Completion
        levelsCompleted = PlayerPrefs.GetInt(nameof(levelsCompleted));
        percentageCompleted = (float)levelsCompleted / 3750f;
        awardsGotten = Achievements.NumberOfAchievementsCompleted();
        int numAwards = Achievements.allAchievements.Count;
        percentageAwards = (float)awardsGotten / (float)numAwards;

        // Tracking actions
        numbersPlaced = PlayerPrefs.GetInt(nameof(numbersPlaced));
        numbersErased = PlayerPrefs.GetInt(nameof(numbersErased));
        editsMade = PlayerPrefs.GetInt(nameof(editsMade));
        undoneActions = PlayerPrefs.GetInt(nameof(undoneActions));
        redoneActions = PlayerPrefs.GetInt(nameof(redoneActions));
        hintsUsed = PlayerPrefs.GetInt(nameof(hintsUsed));

        // Tracking time
        daysPlayed = PlayerPrefs.GetInt(nameof(daysPlayed));
        totalTimePlayed = PlayerPrefs.GetFloat(nameof(totalTimePlayed));
        totalGameTime = PlayerPrefs.GetFloat(nameof(totalGameTime));
        quickestCompletion = PlayerPrefs.GetFloat(nameof(quickestCompletion));
        mostCompletesInaDay = PlayerPrefs.GetInt(nameof(mostCompletesInaDay));

        // Dates
        long.TryParse(PlayerPrefs.GetString(nameof(dateStarted)), out long temp);
        dateStarted = DateTime.FromBinary(temp);
        long.TryParse(PlayerPrefs.GetString(nameof(lastPlayed)), out temp);
        lastPlayed = DateTime.FromBinary(temp);

        // Get averages
        List<SavedLevel> levels = SavedLevels.GetSavedLevels();
        float totalTime = 0f;
        int numCompleted = 0;
        int numActions = 0;
        foreach (SavedLevel lvl in levels) {
            if (!lvl.isCompleted) { continue; }
            totalTime += lvl.completedTime;
            numActions += lvl.completedNumActions;
            ++numCompleted;
        }
        averageCompletionTime = (numCompleted == 0) ? 0f : totalTime / numCompleted;
        averageActionsUsed = (numCompleted == 0) ? 0 : numActions / numCompleted;

        // Update days played
        if (lastPlayed.Date < DateTime.Now.Date) {
            gamesCompletedToday = 0;
            ++daysPlayed;
            ++Achievements.DaysPlayed;
        } else {
            gamesCompletedToday = PlayerPrefs.GetInt(nameof(gamesCompletedToday));
        }
    }

    // Save all the trackers that matter
    public static void Save() {
        // Completion
        PlayerPrefs.SetInt(nameof(levelsCompleted), levelsCompleted);

        // Tracking actions
        PlayerPrefs.SetInt(nameof(numbersPlaced), numbersPlaced);
        PlayerPrefs.SetInt(nameof(numbersErased), numbersErased);
        PlayerPrefs.SetInt(nameof(editsMade), editsMade);
        PlayerPrefs.SetInt(nameof(undoneActions), undoneActions);
        PlayerPrefs.SetInt(nameof(redoneActions), redoneActions);
        PlayerPrefs.SetInt(nameof(hintsUsed), hintsUsed);

        // Tracking time
        PlayerPrefs.SetInt(nameof(daysPlayed), daysPlayed);
        PlayerPrefs.SetFloat(nameof(totalTimePlayed), totalTimePlayed);
        PlayerPrefs.SetFloat(nameof(totalGameTime), totalGameTime);
        PlayerPrefs.SetInt(nameof(mostCompletesInaDay), mostCompletesInaDay);
        PlayerPrefs.SetFloat(nameof(quickestCompletion), quickestCompletion);

        // Levels
        PlayerPrefs.SetInt(nameof(averageActionsUsed), averageActionsUsed);

        // Dates
        PlayerPrefs.SetString(nameof(dateStarted), dateStarted.ToBinary().ToString());
        PlayerPrefs.SetString(nameof(lastPlayed), lastPlayed.ToBinary().ToString());

        // Helpers
        PlayerPrefs.SetInt(nameof(gamesCompletedToday), gamesCompletedToday);
    }


    // ----- UTILITIES -----

    static void FirstRun() {
        // Completion
        PlayerPrefs.SetInt(nameof(levelsCompleted), 0);
        PlayerPrefs.SetInt(nameof(awardsGotten), 0);

        // Tracking actions
        PlayerPrefs.SetInt(nameof(numbersPlaced), 0);
        PlayerPrefs.SetInt(nameof(numbersErased), 0);
        PlayerPrefs.SetInt(nameof(editsMade), 0);
        PlayerPrefs.SetInt(nameof(undoneActions), 0);
        PlayerPrefs.SetInt(nameof(redoneActions), 0);
        PlayerPrefs.SetInt(nameof(hintsUsed), 0);

        // Tracking time
        PlayerPrefs.SetInt(nameof(daysPlayed), 1);
        PlayerPrefs.SetFloat(nameof(totalTimePlayed), 0f);
        PlayerPrefs.SetFloat(nameof(totalGameTime), 0f);
        PlayerPrefs.SetInt(nameof(mostCompletesInaDay), 0);
        PlayerPrefs.SetFloat(nameof(quickestCompletion), -1f);

        // Dates
        PlayerPrefs.SetString(nameof(dateStarted), DateTime.Now.ToBinary().ToString());
        PlayerPrefs.SetString(nameof(lastPlayed), DateTime.Now.ToBinary().ToString());
    }

    public static void ResetStatistics() {
        // Tracking actions
        PlayerPrefs.SetInt(nameof(numbersPlaced), 0);
        PlayerPrefs.SetInt(nameof(numbersErased), 0);
        PlayerPrefs.SetInt(nameof(editsMade), 0);
        PlayerPrefs.SetInt(nameof(undoneActions), 0);
        PlayerPrefs.SetInt(nameof(redoneActions), 0);
        PlayerPrefs.SetInt(nameof(hintsUsed), 0);

        // Tracking time
        PlayerPrefs.SetInt(nameof(mostCompletesInaDay), 0);
    }

    public static void RESET_ALL_STATISTICS() {
        FirstRun();
    }
}
