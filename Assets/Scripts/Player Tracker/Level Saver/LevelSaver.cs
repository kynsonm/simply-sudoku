using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class LevelSaver : MonoBehaviour
{
    [SerializeField] string SaveFileName;
    string path;

    [SerializeField] List<SavedLevel> savedLevels;
    [SerializeField] public SavedLevels.SavedLevelsClass savedLevelsClass;

    // Start is called before the first frame update
    void Awake()
    {
        path = Application.persistentDataPath + "/" + SaveFileName;
        SavedLevels.Path(path);
        SavedLevels.Load();
        StartCoroutine(CheckSavedLevels());

        savedLevels = SavedLevels.GetSavedLevels();
    }

    // Check if save file is ok
    // Check if saved levels is not null
    IEnumerator CheckSavedLevels() {
        while (true) {
            if (SavedLevels.GetSavedLevels() == null) {
                Debug.Log("NO SAVED LEVELS!");
                SavedLevels.Path(path);
                SavedLevels.Load();
            }
            yield return new WaitForSeconds(5f);
        }
    }

    public void CLEAR_SAVED_LEVELS() {
        SavedLevels.CLEAR();
    }
}
