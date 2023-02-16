using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class PopUp
{
    // Object to instantiate popup under
    public static Transform PopUpHolder;
    static readonly string defaultTitle = "Default Title";
    static readonly string defaultInfo = "Info stuff goes here blah blah blah boob peep peepee poopoo bob nanner";


    // Call CreatePopUp on the popup script holder canvas
    public static GameObject CreatePopUp(string title, string info) {
        if (!CheckObjects()) { return null; }

        PopUpHolder.gameObject.SetActive(true);
        PopUpScript popUp = PopUpHolder.GetComponent<PopUpScript>();
        return popUp.CreatePopUp(title, info);
    }
    public static GameObject CreatePopUp(string title, string info, bool turnOffBlur) {
        if (!CheckObjects()) { return null; }

        PopUpHolder.gameObject.SetActive(true);
        PopUpScript popUp = PopUpHolder.GetComponent<PopUpScript>();
        return popUp.CreatePopUp(title, info, turnOffBlur);
    }
    // Create popUp with default info (for debug purposes)
    public static GameObject CreatePopUp() {
        return CreatePopUp(defaultTitle, defaultInfo);
    }


    // Close popUp with specified onComplete action
    public static void ClosePopUp(UnityAction onComplete) {
        if (!CheckObjects()) { return; }
        PopUpHolder.GetComponent<PopUpScript>().ClosePopUp(onComplete);
    }
    // Or close with no onComplete
    public static void ClosePopUp() {
        ClosePopUp(null);
    }


    // Sets PopUpHolder to inputted object
    public static void SetPopUpHolder(GameObject obj) {
        PopUpHolder = obj.transform;
    }


    // Makes sure everything is good (hopefully)
    static bool CheckObjects() {
        if (PopUpHolder == null) {
            Debug.Log("PopUpHoldler is null");
            return false;
        }
        if (PopUpHolder.GetComponent<PopUpScript>() == null) {
            Debug.Log("PopUpHolder script is null");
            return false;
        }
        return true;
    }
}
