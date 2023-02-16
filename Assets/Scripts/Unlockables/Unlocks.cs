using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Unlocks
{
    // Holds all current unlocks
    static Dictionary<string, bool> unlocks = new Dictionary<string, bool>();

    public static void DEBUG_ResetAllUnlocks() {
        unlocks = new Dictionary<string, bool>();
        foreach (Unlockable unlock in GameObject.FindObjectsOfType<Unlockable>(true)) {
            unlock.isUnlocked = false;
            unlock.Setup();
            Save(unlock.ID, unlock.isUnlocked);
        }
        Save();
    }

    // Purchase a thingy
    public static void Unlock(string ID) {
        if (unlocks.ContainsKey(ID)) {
            Debug.Log("Unlocks cointains key ID: " + ID);
            unlocks[ID] = true;
        }
    }

    // Load saved unlocks
    public static void Load() {
        if (!System.IO.File.Exists(Application.persistentDataPath + "/Unlockables.json")) {
            return;
        }

        // Read json file
        string str = System.IO.File.ReadAllText(Application.persistentDataPath + "/Unlockables.json");
        UnlocksJSON unlockJSONs = JsonUtility.FromJson<UnlocksJSON>(str);
        if (unlockJSONs == null || unlockJSONs.unlockJSONs == null) {
            unlocks = new Dictionary<string, bool>();
        }

        // Reset unlocks
        unlocks = new Dictionary<string, bool>();

        // Get each unlock from <unlockJSONs>
        if (unlockJSONs == null) {
            unlockJSONs = new UnlocksJSON(new List<unlockJSON>());
        }
        if (unlockJSONs.unlockJSONs == null) {
            unlockJSONs.unlockJSONs = new List<unlockJSON>();
        }
        foreach (unlockJSON unlock in unlockJSONs.unlockJSONs) {
            unlocks.Add(unlock.ID, unlock.isUnlocked);
        }

        // Set each Unlockable's status
        var replace = GameObject.FindObjectsOfType<Unlockable>(true);
        foreach (Unlockable unl in replace) {
            if (unlocks.ContainsKey(unl.ID)) {
                unl.isUnlocked = unlocks[unl.ID];
                unl.Setup();
            }
        }
    }

    // Load a specific ID
    public static void Load(string ID) {

    }

    // Save all Unlockables in the scene
    public static void Save() {
        string log = "";

        // Save all unlockables in the scene
        foreach (Unlockable unlock in GameObject.FindObjectsOfType<Unlockable>(true)) {
            Save(unlock.ID, unlock.isUnlocked);
            log += $"-- Unlockable {unlock.ID}: {unlock.isUnlocked.ToString()}\n";
        }

        // Save all IAPPacks and CoinPacks in shop menu
        ShopMenu shopMenu = GameObject.FindObjectOfType<ShopMenu>();
        if (shopMenu != null) {
            foreach (IAPPack pack in shopMenu.IAPPacks()) {
                Save(pack.ID, pack.isUnlocked);
                log += $"-- IAPPack {pack.ID}: {pack.isUnlocked.ToString()}\n";
            }
            foreach (CoinPack pack in shopMenu.CoinPacks()) {
                Save(pack.ID, pack.isUnlocked);
                log += $"-- CoinPack {pack.ID}: {pack.isUnlocked.ToString()}\n";
            }
        }

        Debug.Log($"SAVING {unlocks.Count} UNLOCKABLES TO JSON:\n" + log);

        WriteToJSON();
    }

    // Save a specific Unlockable @ ID
    public static void Save(string ID, bool isUnlocked) {
        // Update its value
        if (unlocks.ContainsKey(ID)) {
            unlocks[ID] = isUnlocked;
        } else {
            unlocks.Add(ID, isUnlocked);
        }
    }

    // Write 
    static void WriteToJSON() {
        List<unlockJSON> unlockJSONs = new List<unlockJSON>();
        foreach (var unlock in unlocks) {
            unlockJSONs.Add(new unlockJSON(unlock.Key, unlock.Value));
        }

        UnlocksJSON toJSON = new UnlocksJSON(unlockJSONs);

        string str = JsonUtility.ToJson(toJSON);

        System.IO.File.WriteAllText(Application.persistentDataPath + "/Unlockables.json", str);
    }

    // Returns true if the Unlockable @ ID is unlocked
    public static bool isUnlocked(string ID) {
        if (unlocks.ContainsKey(ID)) {
            return unlocks[ID];
        }
        Debug.LogWarning($"Unlocks: {ID} -- No ID is associated with this unlock ID is saved!");
        return false;
    }

    // Returns whether ID exists in <unlocks>
    public static bool IDExists(string ID) {
        if (unlocks.ContainsKey(ID)) { return true; }
        return false;
    }


    // Class that holds a list of unlockJSON's
    //   Used in saving unlockables to the json save file
    [System.Serializable]
    class UnlocksJSON {
        [SerializeField] public List<unlockJSON> unlockJSONs;

        public UnlocksJSON(List<unlockJSON> unlocks_in) {
            unlockJSONs = new List<unlockJSON>(unlocks_in);
        }
    }

    // Class that holds the ID and isUnlocked of an Unlockable
    //   Used in saving unlockables to the json save file
    [System.Serializable]
    class unlockJSON {
        [SerializeField] public string ID;
        [SerializeField] public bool isUnlocked;

        public unlockJSON(string ID_in, bool isUnlocked_in) {
            ID = ID_in;
            isUnlocked = isUnlocked_in;
        }
    }
}
