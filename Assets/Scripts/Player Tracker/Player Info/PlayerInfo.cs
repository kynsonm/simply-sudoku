using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerInfo
{
    public static string PlayerName = "";
    public static int Coins;
    public static int XP = -1;
    public static int Level = -1;

    public static bool PurchaseHasBeenMade = false;


    public static void Load() {
        if (!PlayerPrefs.HasKey(nameof(PlayerName))) {
            FirstRun();
        }

        PlayerName = PlayerPrefs.GetString(nameof(PlayerName));
        Coins = PlayerPrefs.GetInt(nameof(Coins));
        XP = PlayerPrefs.GetInt(nameof(XP));
        Level = PlayerPrefs.GetInt(nameof(Level));
        PurchaseHasBeenMade = PlayerPrefs.GetInt(nameof(PurchaseHasBeenMade)) == 1;
    }

    public static void Save() {
        PlayerPrefs.SetString(nameof(PlayerName), PlayerName);
        PlayerPrefs.SetInt(nameof(Coins), Coins);
        PlayerPrefs.SetInt(nameof(XP), XP);
        PlayerPrefs.SetInt(nameof(Level), Level);
        PlayerPrefs.SetInt(nameof(PurchaseHasBeenMade), PurchaseHasBeenMade ? 1 : 0);
    }

    public static void FirstRun() {
        PlayerName = "Your name here...";
        Coins = 0;
        XP = 0;
        Level = 0;
        Save();
    }
}
