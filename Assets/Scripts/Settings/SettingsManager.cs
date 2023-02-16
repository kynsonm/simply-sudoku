using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SettingsManager : MonoBehaviour
{
    // Blur radii vars
    public float BackgroundBlurWeightMax;
    public float UIBlurWeightMax;
    public float OtherBlurWeightMax;


    // ----- Awake and Start
    private void Awake() {
        Settings.LoadSettings();
        Resolution.Start();
        Framerate.Start();
    }
    private IEnumerator Start() 
    {
        yield return new WaitForEndOfFrame();
        Settings.LoadSettings();
        LoadSettings();
        StartCoroutine(SaveSettingsEnum());
    }


    // ----- Load and Save settings

    void LoadSettings() {
        Themes.themeIndex = Settings.ThemeIndex;
        Themes.fontIndex = Settings.FontIndex;

        Blur.SetupBlurMaxes(this);
        Framerate.SetFramerate();
    }

    IEnumerator SaveSettingsEnum() {
        while (true) {
            Settings.SaveSettings();
            yield return new WaitForSeconds(5f);
        }
    }
}
