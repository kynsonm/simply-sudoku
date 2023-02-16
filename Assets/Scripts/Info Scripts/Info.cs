using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Static class so that we can call it from anywhere
public static class Info
{
    // Objects I care about
    public static GameObject InfoScriptObject;
    public static InfoScript infoScript;

    // Create info dialogue
    public static void CreateInfo(GameObject obj, string message) {
        CreateInfo(obj, message, false);
    }

    public static void CreateInfo(GameObject obj, string message, bool centerIt) {
        if (!CheckObjects()) {
            Debug.LogError("Objects on Info class are not right");
            return;
        }
        infoScript.CreateInfo(obj, message, centerIt);
    }

    // Close info dialogue
    public static void CloseInfo() {
        if (!CheckObjects()) {
            Debug.LogError("Objects on Info class are not right");
            return;
        }
        infoScript.CloseInfo();
    }

    // Returns whether an info is open or not
    public static bool isOpen() {
        return infoScript.infoOpen;
    }

    // Returns true if everything is good
    // Returns false if something isnt right
    static bool CheckObjects() {
        if (InfoScriptObject == null) {
            Debug.Log("Info script object is null");
            return false;
        }
        if (infoScript == null) {
            infoScript = InfoScriptObject.GetComponent<InfoScript>();
            if (infoScript == null) {
                Debug.Log("Info script is null");
                return false;
            }
        }
        return true;
    }
}
