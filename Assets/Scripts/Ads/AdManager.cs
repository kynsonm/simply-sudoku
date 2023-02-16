using UnityEngine;
using System.Collections;
using Unity.Services.Core;
using UnityEngine.Events;
using Unity.Services.Mediation;
using Ad;

public class AdManager : MonoBehaviour
{
    // ----- VARIABLES -----

    // Setup vars
    [SerializeField] string androidGameID = "5016819";
    [SerializeField] string iOSGameID = "5016818";
    private string gameID;
    private string adUnitIDSuffix;

    // Whether to show each one
    [SerializeField] bool showBannerAd = true;
    [SerializeField] int levelsBeforeAd;
    public static int numLevelsCompleted = 0;

    // Ad vars
    Ad.InterstitialAd interstitialAd;
    /*  Doing this in Ads now
    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;
    BannerAd bannerAd;
    */


    // Before first frame
    private void Awake() {
        UnityServices.InitializeAsync();
        InitializeAds();
    }


    // ----- SETUP -----

    // Set up ads, and each ad type
    async public void InitializeAds() {
        // Get gameID
        bool isAndroid = (Application.platform == RuntimePlatform.Android);
        gameID = (isAndroid) ? androidGameID : iOSGameID;
        adUnitIDSuffix = (isAndroid) ? "_Android" : "_iOS";

        // Interstitial setup
        if (interstitialAd == null) {
            interstitialAd = new Ad.InterstitialAd();
            await interstitialAd.InitServices(("Interstitial" + adUnitIDSuffix), gameID);
            interstitialAd.SetupAd();
        }

        // Rewarded setup
        if (Ads.rewardedAd == null) {
            Ads.rewardedAd = new Ad.RewardedAd();
            await Ads.rewardedAd.InitServices(("Rewarded" + adUnitIDSuffix), gameID);
            Ads.rewardedAd.SetupAd();
        }

        // Banner setup
        if (showBannerAd) {
            Ads.bannerAd = new Ad.BannerAd();
            await Ads.bannerAd.InitServices(("Banner" + adUnitIDSuffix), gameID);
            Ads.bannerAd.SetupAd();
        } else {
            Ads.bannerAd = null;
        }

        StartCoroutine(LoadAds());
    }

    // Load them while theyre not loaded
    private IEnumerator LoadAds() {
        // Load interstitial
        while (!interstitialAd.isLoaded()) {
            System.Threading.Tasks.Task task = interstitialAd.LoadAd();
            yield return new WaitForSeconds(1f);
        }
        // Load rewarded
        while (!Ads.rewardedAd.isLoaded()) {
            System.Threading.Tasks.Task task = Ads.rewardedAd.LoadAd();
            yield return new WaitForSeconds(1f);
        }
    }


    // ----- SHOW ADS -----

    // Check how many levels were completed and show an ad if necessary
    public static void LevelCompleted() { LevelCompleted(1); }
    public static void LevelCompleted(int amountToAddToLvlCount) {
        numLevelsCompleted += amountToAddToLvlCount;
    }
    public static void DoGameInterstitialAd() {
        // Get an instance of an ad manager
        AdManager adManager = GameObject.FindObjectOfType<AdManager>();
        if (adManager == null) { return; }

        // Get the cap of lvls before and ad
        int cap = adManager.levelsBeforeAd;
        cap = (cap == 0) ? 5 : cap;

        // Check if number of levels completed is above the cap
        if (numLevelsCompleted >= cap) {
            numLevelsCompleted = 0;
            adManager.ShowInterstitialAd();
        }
    }
    public static bool InterstitialAdReady() {
        // Get an instance of an ad manager
        AdManager adManager = GameObject.FindObjectOfType<AdManager>();
        if (adManager == null) { return false; }

        // Ge the cap of lvls before and ad
        int cap = adManager.levelsBeforeAd;
        cap = (cap == 0) ? 5 : cap;

        // Check if number of levels completed is above the cap
        if (numLevelsCompleted >= cap) {
            return true;
        }
        return false;
    }

    // Show interstitial ad
    public void ShowInterstitialAd() {
        interstitialAd.ShowAd();
    }

    // Show awarded ad
    public void ShowRewardedAd() {
        Ads.rewardedAd.ShowAd();
    }
    public void ShowRewardedAd(UnityAction rewardedAction) {
        RewardClass.SetAction(rewardedAction);
        Ads.rewardedAd.ShowAd();
    }
}
