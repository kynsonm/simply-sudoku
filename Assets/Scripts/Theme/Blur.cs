using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Blur.BlurType;

public static class Blur
{
    // Denotes which blur in the scene to tween
    public enum BlurType {
        UI = 0, Background = 1,
        Other = 2,
        invalid = 3
    }

    // Holds the material, its max radius, and what type of blur it is
    public class BlurMaterial {
        // Variables
        public Material material;
        public float radius;
        public BlurType blurType;

        // Constructor
        public BlurMaterial(Material material_in) {
            // Set material and radius
            material = material_in;
            if (material != null) {
                if (!material.HasFloat("_Radius")) {
                    Debug.LogWarning($"Material \"{material.name}\" doesn't have property _Radius");
                    radius = -1;
                } else {
                    radius = material.GetFloat("_Radius");
                }
            } else {
                radius = -1;
                blurType = BlurType.invalid;
                return;
            }

            // Get BlurType based on material's name
            if      (material.name.Contains("UI"))         { blurType = BlurType.UI; }
            else if (material.name.Contains("Background")) { blurType = BlurType.Background; }
            else if (material.name.Contains("Other"))      { blurType = BlurType.Other; }
            else                                           { blurType = BlurType.invalid; }
        }

        // Sets the material radius based on either (1) ratio of max radius or (2) a direct number
        public void SetRadius(float newRadius) {
            if (newRadius < 0 || newRadius > 2) {
                Debug.LogWarning("Radius is out of range :(  -->  " + newRadius);
                return;
            }
            material.SetFloat("_Radius", (int)(newRadius * radius));
        }
        public void SetRadius(float newRadius, bool is_direct_radius) {
            if (!is_direct_radius) {
                SetRadius(newRadius);
                return;
            }
            material.SetFloat("_Radius", (int)newRadius);
        }
    }

    // Variables
    static List<BlurMaterial> BlurMaterials;

    // Set the given blur to a certain value
    public static void SetAlpha(BlurType blurType, float alpha) {
        if (blurType == invalid) { return; }
        BlurMaterial material = FindMaterial(blurType);
        if (material == null || material.material == null) { return; }
        material.SetRadius(alpha);
    }

    // Fade the given blur type over <time> time
    public static void FadeIn(BlurType blurType, float time) {
        Fade(blurType, 0f, 1f, time, new UnityAction<float>((float value) => {}));
    }
    public static void FadeOut(BlurType blurType, float time) {
        Fade(blurType, 1f, 0f, time, new UnityAction<float>((float value) => {}));
    }
    public static void FadeIn(BlurType blurType, float time, UnityAction<float> invokeOnUpdate) {
        Fade(blurType, 0f, 1f, time, invokeOnUpdate);
    }
    public static void FadeOut(BlurType blurType, float time, UnityAction<float> invokeOnUpdate) {
        Fade(blurType, 1f, 0f, time, invokeOnUpdate);
    }
    public static void Fade(BlurType blurType, float start_alpha, float end_alpha, float time, UnityAction<float> invokeOnUpdate) {
        if (blurType == invalid) { return; }
        if (invokeOnUpdate == null) { invokeOnUpdate = new UnityAction<float>((float value) => {}); }

        BlurMaterial material = FindMaterial(blurType);
        if (material == null || material.material == null) { return; }

        LeanTween.value(start_alpha, end_alpha, time)
        .setEase(LeanTweenType.easeInOutSine)
        .setOnStart(() => {
            material.SetRadius(start_alpha);
        })
        .setOnUpdate((float value) => {
            material.SetRadius(value);
            invokeOnUpdate.Invoke(value);
        })
        .setOnComplete(() => {
            material.SetRadius(end_alpha);
        });
    }

    // Returns the material associated with the given BlurType
    public static BlurMaterial FindMaterial(BlurType blurType) {
        if (!checkMaterials()) { return null; }
        string blur = blurType.ToString();
        foreach (BlurMaterial mat in BlurMaterials) {
            if (mat.material.name.Contains(blur)) {
                return mat;
            }
        }
        return null;
    }

    // Gets all the Blur materials in the scene
    static bool checkMaterials() {
        if (BlurMaterials == null) {
            BlurMaterials = new List<BlurMaterial>();
        }

        if (BlurMaterials.Count != 0) {
            return true;
        }

        Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
        if (materials.Length == 0) { 
            return false;
        }

        foreach (Material mat in materials) {
            if (!mat.name.Contains("Blur")) { continue; }
            BlurMaterials.Add(new BlurMaterial(mat));
        }

        return BlurMaterials.Count != 0;
    }

    // Setup all the blur sht
    public static void SetupBlurMaxes(SettingsManager settings) {
        foreach (BlurMaterial material in BlurMaterials) {
            switch (material.blurType) {
            case BlurType.UI: { 
                material.radius = settings.UIBlurWeightMax;
                material.SetRadius(1f);
                break;
            }
            case BlurType.Background: { 
                material.radius = settings.BackgroundBlurWeightMax;
                material.SetRadius(Settings.BackgroundBlur);
                break;
            }
            case BlurType.Other: {
                material.radius = settings.OtherBlurWeightMax;
                material.SetRadius(1f);
                break;
            }
            default: material.radius = -1;  break;
            }
        }
    }
}
