using UnityEngine;
using UnityEngine.Events;
using Ad;

public static class Ads
{
    // ----- VARIABLES -----
    static AdManager adManager;
    public static InterstitialAd interstitialAd;
    public static RewardedAd rewardedAd;
    public static BannerAd bannerAd;

    // ----- SETUP -----

    // Get adManager
    public static void SetVars() {
        adManager = GameObject.FindObjectOfType<AdManager>();
    }

    // Check vars
    public static bool CheckVars() {
        bool allGood = true;
        
        // Check adManager
        if (adManager == null) {
            SetVars();
            if (adManager == null) {
                Debug.LogError("Ads: Ad Manager is null!");
                allGood = false;
            }
        }

        return allGood;
    }


    // ----- METHODS -----

    public static void InterstitialAd() {
        if (!CheckVars()) { return; }
        adManager.ShowInterstitialAd();
    }

    public static void RewardedAd(UnityAction rewardedAction) {
        if (!CheckVars()) { return; }
        adManager.ShowRewardedAd(rewardedAction);
    }
    public static void RewardedAd() {
        if (!CheckVars()) { return; }
        adManager.ShowRewardedAd();
    }
}
