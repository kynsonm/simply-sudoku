using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using static SoundClip;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] GameObject SceneChangePrefab;
    [SerializeField] Sprite circle;
    [SerializeField] LeanTweenType easeType;

    void Start() {
        if (SceneLoader.CurrentScene == SceneLoader.Scene.invalid) {
            OutOfSplashScreen();
        }
    }

    public void OutOfSplashScreen() {
        // Stop if anything is wrong
        if (!CheckVars()) {
            return;
        }

        SceneLoader.CurrentScene = SceneLoader.Scene.MainMenu;

        // Get important vars
        Vector3 pos = new Vector3(Screen.width/2, Screen.height/2);
        float start = 2.5f * MaxEdge(pos);
        float end = 0f;

        GameObject canv = Instantiate(SceneChangePrefab);
        DontDestroyOnLoad(canv);

        // Set up initial circle size
        GameObject circ = canv.transform.Find("Circle").gameObject;
        circ.GetComponent<Image>().sprite = circle;
        circ.GetComponent<ImageTheme>().lookType = LookType.background;
        RectTransform rect = circ.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(start, start);

        // And tween the scale
        FadeAudioLevels(true, 1f);
        LeanTween.value(circ, start, end, 1f)
        .setDelay(1f)
        .setEase(easeType)
        .setOnStart(() => {
            Sound.Play(scene_change_close);
        })
        .setOnUpdate((float value) => {
            rect.sizeDelta = new Vector2(value, value);
        })
        .setOnComplete(() => {
            GameObject.Destroy(canv);
        });
    }

    public bool ChangeScene(int nextScene, GameObject refObject) {
        return ChangeScene(nextScene, refObject, 1f);
    }
    public bool ChangeScene(int nextScene, GameObject refObject, float speedMultiplier) {
        // Change scene
        ActuallyChangeTheScene(nextScene, refObject.GetComponent<RectTransform>().position, speedMultiplier);
        return true;
    }
    public bool ChangeScene(int nextScene, Vector3 refPosition) {
        ActuallyChangeTheScene(nextScene, refPosition, 1f);
        return true;
    }
    void ActuallyChangeTheScene(int nextScene, Vector3 refPosition, float speedMultiplier) {
        Sound.Play(scene_change_open);

        // Turn off interactability of all buttons in the scene, just to be sure
        foreach (Button butt in GameObject.FindObjectsOfType<Button>()) {
            butt.interactable = false;
        }

        // Create transition object
        GameObject canv = Instantiate(SceneChangePrefab);
        DontDestroyOnLoad(canv);
        
        // Set position and scale of the circle
        GameObject circ = canv.transform.Find("Circle").gameObject;
        circ.GetComponent<Image>().sprite = circle;
        SetColor(circ.GetComponent<ImageTheme>());
        RectTransform rect = circ.GetComponent<RectTransform>();
        rect.position = refPosition;

        // Find the max radius needed for the circle
        float maxEdge = MaxEdge(refPosition);
        // Tween scale
        // Stop and load next scene when circle covers the entire screen
        float time = speedMultiplier * Settings.AnimSpeedMultiplier;
        FadeAudioLevels(false, time);
        LeanTween.value(circ, 0f, maxEdge, time)
        .setEase(easeType)
        .setOnUpdate((float value) => {
            rect.sizeDelta = new Vector2(2.5f * value, 2.5f * value);
        })
        .setOnComplete(() => {
            // Clear SameTextSize's
            TextSizeStatic.CLEAR_ALL();

            // Cancel tweens, load next scene
            LeanTween.cancel(circ);
            LeanTween.cancelAll();
            SceneManager.LoadScene(nextScene);
            SceneLoader.CurrentScene = (SceneLoader.Scene)(nextScene);

            float delay = ((SceneLoader.Scene)nextScene == SceneLoader.Scene.MainMenu) ? 0.2f : 0f;

            // Tween out the circle
            rect.position = new Vector3(Screen.width/2, Screen.height/2, rect.position.z);
            LeanTween.value(circ, rect.sizeDelta.x, 0f, 1f)
            .setDelay(delay)
            .setOnStart(() => {
                FadeAudioLevels(true, 1f);
            })
            .setEase(easeType)
            .setOnUpdate((float value) => {
                rect.sizeDelta = new Vector2(value, value);
                Application.targetFrameRate = Settings.TargetFrameRate;
            })
            .setOnComplete(() => {
                GameObject.Destroy(canv);

                // Restarts SameTextSizes
                TextSizeStatic.Construct();

                Application.targetFrameRate = Settings.TargetFrameRate;
            });
        });
    }

    // Returns the largest amount that the circle's size needs to be
    //   in order to cover the entire screen
    float MaxEdge(Vector3 pos) {
        // Initialize vars
        Vector2 point = new Vector2(pos.x, pos.y);
        Vector2 corner;
        float dis;

        // Top left
        corner = new Vector2(0f, Screen.height);
        dis = Vector2.Distance(point, corner);

        // Top right
        corner = new Vector2(Screen.width, Screen.height);
        dis = Mathf.Max(dis, Vector2.Distance(point, corner));

        // Bottom left
        corner = new Vector2(0f, 0f);
        dis = Mathf.Max(dis, Vector2.Distance(point, corner));

        // Bottom right
        corner = new Vector2(Screen.width, 0f);
        dis = Mathf.Max(dis, Vector2.Distance(point, corner));

        return dis;
    }
    float MaxEdge(Vector2 pos) {
        return MaxEdge(new Vector3(pos.x, pos.y, 0f));
    }



    // ----- UTILITIES -----

    // Tweens in or out global volume multiplier
    void FadeAudioLevels(bool fadeIn, float time) {
        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 1f : 0f;

        Sound.GlobalVolumeMultiplier = start;
        LeanTween.value(start, end, time)
        .setOnUpdate((float value) => {
            Sound.GlobalVolumeMultiplier = value;
        });
    }

    // Sets the circle to a random color
    void SetColor(ImageTheme theme) {
        if (theme == null) {
            Debug.Log("No image theme found");
        }

        int rand = Random.Range(0, 8);
        theme.UseColor = (rand >= 5);

        switch (rand) {
            case 0:
                theme.lookType = LookType.UI_main;
                break;
            case 1:
                theme.lookType = LookType.UI_accent;
                break;
            case 2:
                theme.lookType = LookType.UI_background;
                break;
            case 3:
                theme.lookType = LookType.background;
                break;
            case 4:
                theme.color = WhichColor.Color1;
                break;
            case 5:
                theme.color = WhichColor.Color2;
                break;
            case 6:
                theme.color = WhichColor.Color3;
                break;
            case 7:
                theme.color = WhichColor.Color4;
                break;
            default:
                Debug.Log("No case associated with this number: " + rand);
                theme.UseColor = false;
                theme.lookType = LookType.UI_background;
                break;
        }
    }

    // Reuturns whether everything is good or not
    bool CheckVars() {
        bool allGood = true;
        if (SceneChangePrefab == null) {
            Debug.Log("No scene change prefab present");
            allGood = false;
        }
        if (circle == null) {
            Debug.Log("Circle sprite is null");
            allGood = false;
        }
        return allGood;
    }
}
