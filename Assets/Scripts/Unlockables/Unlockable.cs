using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// Tier for cost of an IAP
// 1 is cheapest, 5 is most expensive
//   Figure out actual prices later
public enum IAPTier {
    invalid, _1, _2, _3, _4, _5
}
public static class CostOfIAPTier {
    public static float CostOf(IAPTier tier) {
        switch (tier) {
            case IAPTier._1: return 0.99f;
            case IAPTier._2: return 1.99f;
            case IAPTier._3: return 3.99f;
            case IAPTier._4: return 4.99f;
            case IAPTier._5: return 24.99f;
            default: return -1f;
        }
    }
}


// Add this to any object that is unlockable
// FEATS:
//   Adds lock icon if it is locked
//   If it is clicked when locked, make purchase popUp
//   If conditions are met, "Yes" or "Ad" is available, etc
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(RectTransform))]
public class Unlockable : MonoBehaviour
{
    // ----- VARIABLES -----

    // General info
    public string unlockName;
    public string unlockDescription;

    // Button that this is dependent on
    Button button;
    GameObject buttonCopy = null;
    public string ID = "0";
    public bool isUnlocked = false;

    UnlockManager unlockManager;

    // Keep track of these things
    public bool isIAP = false;
    public bool isCoin = false;
    public bool isAd = true;

    // Cost of buying
    public IAPTier IAPCost = IAPTier.invalid;
    public int coinsAmount = -1;

    
    // Before first frame
    private IEnumerator Start() {
        yield return new WaitForEndOfFrame();
        unlockManager = GameObject.FindObjectOfType<UnlockManager>();
        if (!Setup()) {
            Debug.LogWarning("Unlockable: Unlockable object could not be set up!");
        }
    }


    // ----- SETUP -----

    // Setup() :
    //   Does all the setup, assuming each variables is already assigned and good to go
    //   ex. Replaces action, sets locked image if necessary, etc
    public bool Setup() {
        // yeah idk
        if (unlockManager == null) {
            unlockManager = GameObject.FindObjectOfType<UnlockManager>();
        }

        button = gameObject.GetComponent<Button>();

        // Return false if any conditions are invalid
        if (!CheckForButton()) { return false; }     // No button on object

        // Already purchased
        if (Unlocks.isUnlocked(ID)) { isUnlocked = true; }
        if (isUnlocked) {
            button.interactable = true;
            if (buttonCopy != null) {
                GameObject.Destroy(buttonCopy);
            }
            return true;
        }

        if (!isIAP && !isCoin && !isAd) { return false; }     // Nothing to buy
        if (isIAP && IAPCost == IAPTier.invalid) { return false; }     // isIAP but with no cost
        if (isCoin && coinsAmount == -1) { return false; }     // isCoin but with no cost

        // Find size we want the button to be
        RectTransform thisRect = gameObject.GetComponent<RectTransform>();
        float off = Mathf.Max(thisRect.rect.height, thisRect.rect.width);
        off *= 0.05f;

        // Make the locked button
        if (buttonCopy != null) {
            GameObject.Destroy(buttonCopy);
        }
        GameObject copy = Instantiate(unlockManager.lockedButtonPrefab, gameObject.transform);
        buttonCopy = copy;
        RectTransform rect = copy.transform.Find("Lock").GetComponent<RectTransform>();
        RectTransformOffset.All(rect, off);

        // Setup its button
        Button butt = copy.GetComponent<Button>();
        butt.onClick.RemoveAllListeners();
        butt.onClick.AddListener(() => { Click(); });

        // Make this button uninteractable
        button.interactable = false;

        return true;
    }

    // Setup(vars) :
    //   Assigns variables for this script
    //   Take in isIAP, isCoin, isAd, and their amounts

    // -- Just need an ad
    public void Setup(bool isAd_in) {
        Setup(false, false, true, IAPTier.invalid, -1);
    }

    // -- Need coins or ad
    // Assumes isIAP == false, isCoin == true
    public void Setup(bool isAd_in, int coinsAmount_in) {
        Setup(false, true, isAd_in, IAPTier.invalid, coinsAmount_in);
    }

    // -- Need coins or IAP
    // Assumes isAd == false, other 2 are true
    public void Setup(IAPTier tier, int coinsAmount_in) {
        if (tier == IAPTier.invalid) {
            Setup(false, coinsAmount_in);
        } else {
            Setup(true, true, false, tier, coinsAmount_in);
        }
    }

    // -- IAP only
    // Assumes isAd = false, isCoin = false;
    public void Setup(IAPTier tier) {
        Setup(true, false, false, tier, -1);
    }

    // -- Base case
    // Assigns each var
    void Setup(bool isIAP_in, bool isCoin_in, bool isAd_in, IAPTier tier, int coinsAmount_in) {
        // Its a required component, but checking anyway
        button = gameObject.GetComponent<Button>();
        if (button == null) {
            Debug.Log("Unlockable: NO BUTTON ON OBJECT");
        }
        // And assign inputted vars
        isIAP = isIAP_in;
        isCoin = isCoin_in;
        isAd = isAd_in;
        IAPCost = tier;
        coinsAmount = coinsAmount_in;

        Setup();
    }


    // ----- METHODS -----

    // Yes button is clicked
    public void Click() {
        Debug.Log("Unlockable button was clicked");
        unlockManager.StartPurchase(this);
    }

    // Unlock it
    // Does not check for cost or anything
    public void Unlock() {
        isUnlocked = true;
        // save purchase by id somehow
        Setup();

        // Seems a tad barbaric but gets the job done
        ShopMenu shop = GameObject.FindObjectOfType<ShopMenu>(true);
        if (shop != null) {
            // TODO: Implement this better
            Unlocks.Save();
            shop.SetupShopMenu();
        }
    }


    // ----- UTILITIES -----

    // Checks for button on the gameobject
    bool CheckForButton() {
        if (button == null) {
            Debug.Log("Unlockable: NO BUTTON ON OBJECT");
            return false;
        }
        return true;
    }


    // Enum for easy customizing of unlock menu
    public enum Type {
        Ad, Ad_Coin, Coin, IAP_Coin, IAP
    }
    public Type GetUnlockableType() {
        if (isIAP) {
            if (isCoin) { return Type.IAP_Coin; }
            else { return Type.IAP; }
        }
        if (isCoin) {
            if (isAd) { return Type.Ad_Coin; }
            else { return Type.Coin; }
        }
        return Type.Ad;
    }
}
