using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]

public static class SceneLoader
{
    [SerializeField]
    public enum Scene {
        MainMenu = 0,
        Game = 1,
        DEBUG = 2,
        invalid = -1
    }

    public static Scene CurrentScene = Scene.invalid;

    public static void LoadScene(Scene scene, GameObject refObject) {
        LoadScene(scene, refObject, 1f);
    }
    public static void LoadScene(Scene scene, GameObject refObject, float speedMultiplier) {
        // Find main manager (that has SceneChanger component)
        GameObject manager = GameObject.Find("MainManager");
        if (manager == null) {
            manager = GameObject.Find("Main Manager");
            if (manager == null) {
                Debug.Log("Could not find Main Manager");
                return;
            }
        }

        // Get SceneChanger component from it
        SceneChanger changer = manager.GetComponent<SceneChanger>();
        if (changer == null) {
            Debug.Log("Could not find scene changer component on main manager");
            return;
        }

        // Cancel all tweens
        LeanTween.cancelAll();

        // Load next scene
        bool allGood = changer.ChangeScene((int)scene, refObject, speedMultiplier);
        if (allGood) {
            CurrentScene = scene;
        }
    }
    public static void LoadScene(Scene scene) {
        // Save all player info
        Settings.SaveSettings();
        PlayerInfo.Save();
        Statistics.Save();
        Achievements.Save();

        // Find main manager (that has SceneChanger component)
        GameObject manager = GameObject.Find("MainManager");
        if (manager == null) {
            manager = GameObject.Find("Main Manager");
            if (manager == null) {
                Debug.Log("Could not find Main Manager");
                SceneManager.LoadScene((int)scene);
                return;
            }
        }

        // Get SceneChanger component from it
        SceneChanger changer = manager.GetComponent<SceneChanger>();
        if (changer == null) {
            Debug.Log("Could not find scene changer component on main manager");
            SceneManager.LoadScene((int)scene);
            return;
        }

        // Cancel all tweens
        LeanTween.cancelAll();

        // Load next scene
        Vector3 pos = new Vector3(Screen.width/2f, Screen.height/2f, 100f);
        changer.ChangeScene((int)scene, pos);
    }
}
