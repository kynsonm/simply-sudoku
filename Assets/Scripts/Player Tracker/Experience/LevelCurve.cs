using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoundClip;

public static class LevelCurve
{
    // ----- VARIABLES

    // Basic
    static LevelCurveManager manager;
    static List<LevelCurveManager.LevelXP> XPperLevel;

    // Tracking
    static int playerXP = -1;
    static int playerLevel = -1;


    // ----- MEHODS -----


    // LEVELS

    // Returns level given an xp input
    public static int LevelAtXP(int xp) { return LevelAtXP(xp, false); }
    public static int LevelAtXP(int xp, bool setPlayerLevel) {
        int curXP = xp;
        int lvl = 0;
        for (int i = 1; i < XPperLevel.Count; ++i) {
            int lvlXP = XPperLevel[i].xp;
            if (curXP - lvlXP < 0) { break; }
            curXP -= lvlXP;
            ++lvl;
        }
        if (setPlayerLevel) {
            if (lvl > PlayerInfo.Level) {
                Sound.Play(level_up);
            }
            PlayerInfo.Level = lvl;
        }
        return lvl;
    }

    // Returns level given an xp input
    static int Level(int xp) {
        int curXP = xp;
        playerXP = curXP;

        int lvl = 0;

        for (int i = 1; i < XPperLevel.Count; ++i) {
            int lvlXP = XPperLevel[i].xp;
            if (xp - lvlXP < 0) { break; }
            xp -= lvlXP;
            ++lvl;
        }

        if (lvl > PlayerInfo.Level) {
            Sound.Play(level_up);
        }
        playerLevel = lvl;
        PlayerInfo.Level = playerLevel;
        return lvl;
    }

    // Returns current level based on playerXP
    public static int CurrentLevel() {
        UpdateVars(PlayerInfo.XP);
        // If level has already been calculated, return it
        if (playerXP == PlayerInfo.XP && playerLevel != -1) {
            return playerLevel;
        }
        // Otherwise, return newly calculated level
        return Level(PlayerInfo.XP);
    }


    // XP

    // Returns how much xp TOTAL is needed for the next level
    public static int TotalNextLevelXP() {
        UpdateVars(PlayerInfo.XP);
        int total = 0;
        for (int i = 0; i <= PlayerInfo.Level+1 && i < XPperLevel.Count; ++i) {
            total += XPperLevel[i].xp;
        }
        return total;
    }

    // Returns the TOTAL xp they currently have
    public static int TotalCurrentXP() {
        UpdateVars(PlayerInfo.XP);
        return PlayerInfo.XP;
    }

    // Returns how much xp is needed for the next level
    public static int NextLevelXP() {
        UpdateVars(PlayerInfo.XP);
        return XPperLevel[playerLevel+1].xp;
    }

    // Returns the xp they currently have
    public static int CurrentXP() {
        // Cehck player info has loaded
        if (PlayerInfo.XP == -1) {
            PlayerInfo.Load();
        }
        UpdateVars(PlayerInfo.XP);
        // Do the thing
        playerXP = PlayerInfo.XP;
        int xp = playerXP;
        foreach (var levelXP in XPperLevel) {
            if (xp - levelXP.xp < 0) { break; }
            xp -= levelXP.xp;
        }
        return xp;
    }


    // ----- UTILITIES -----

    static void UpdateVars(int xp) {
        playerXP = xp;
        playerLevel = Level(playerXP);
    }

    static LevelCurve() {
        GetManager();
    }

    static void GetManager() {
        manager = GameObject.FindObjectOfType<LevelCurveManager>();
        if (manager == null) {
            Debug.Log("Could not find manager");
        } else {
            XPperLevel = manager.XPperLevel;
        }
    }

    static bool CheckManager() {
        return (manager == null || XPperLevel == null);
    }
}
