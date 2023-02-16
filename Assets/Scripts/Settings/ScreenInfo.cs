using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using OnDemandRendering = UnityEngine.Rendering.OnDemandRendering;

public static class Resolution
{
    // ----- Variables
    public static int width;
    public static int height;


    // ----- Methods

    // Used to set the resolution based on inputted ratio of Screen size
    public static void SetResolution() {

        float screenRatio = (float)Screen.width / (float)Screen.height;

        int pixels = Screen.height * Screen.width;
        float newPixels = Settings.RenderScale * (float)pixels;

        width  = (int) MathF.Sqrt(screenRatio * newPixels);
        height = (int)(MathF.Sqrt(screenRatio * newPixels) / screenRatio);
        Screen.SetResolution(width, height, Screen.fullScreenMode);
    }
    public static void SetResolution(float ratio) {
        if (ratio <= 0.1f || ratio >= 3f) {
            Debug.LogWarning("Resolution scale must be withing 0.1 and 3");
            return;
        }

        Settings.RenderScale = ratio;
        SetResolution();
    }

    // Set resolution to RenderScale setting
    public static void Start() {
        //Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
        Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.ExclusiveFullScreen);
        
        //float ratio = Settings.RenderScale;
        //SetResolution(ratio);
    }
}


public static class Framerate
{
    // ----- Variables
    public static int FPS;


    // ----- Methods

    // Set the target framerate
    public static void SetFramerate() {
        int x = Screen.currentResolution.width;
        int y = Screen.currentResolution.height;
        Screen.SetResolution(x, y, Screen.fullScreenMode, Settings.TargetFrameRate);

        // 30 FPS
        if (Settings.TargetFrameRate <= 30) {
            Application.targetFrameRate = 60;
            //Screen.SetResolution(x, y, Screen.fullScreenMode, 60);
            OnDemandRendering.renderFrameInterval = (int)(60f / (float)Settings.TargetFrameRate);
        }
        // 45 FPS
        else if (Settings.TargetFrameRate < 60) {
            Application.targetFrameRate = Settings.TargetFrameRate;
            //Screen.SetResolution(x, y, Screen.fullScreenMode, Settings.TargetFrameRate);
            OnDemandRendering.renderFrameInterval = (int)(60f / (float)Settings.TargetFrameRate);
        }
        // 60+ FPS
        else {
            Application.targetFrameRate = Settings.TargetFrameRate;
            //Screen.SetResolution(x, y, Screen.fullScreenMode, Settings.TargetFrameRate);
            OnDemandRendering.renderFrameInterval = 1;
        }
    }
    public static void SetFramerate(int FPS) {
        Settings.TargetFrameRate = FPS;
        SetFramerate();
    }

    // Set target framerate based on its setting
    public static void Start() {
        SetFramerate();
    }
}