using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Mediation;
using UnityEngine;

namespace Ad
{
    public class InterstitialAd : IDisposable
    {
        IInterstitialAd ad;
        string adUnitId;
        string gameId;

        public bool isLoaded() {
            if (ad == null) { return false; }
            return !(ad.AdState == AdState.Unloaded);
        }

        public async Task InitServices(string adUnitId_in, string gameID_in)
        {
            adUnitId = adUnitId_in;
            gameId = gameID_in;

            try
            {
                InitializationOptions initializationOptions = new InitializationOptions();
                initializationOptions.SetGameId(gameId);
                await UnityServices.InitializeAsync(initializationOptions);

                InitializationComplete();
            }
            catch (Exception e)
            {
                InitializationFailed(e);
            }
        }

        public void SetupAd()
        {
            //Create
            ad = MediationService.Instance.CreateInterstitialAd(adUnitId);

            //Subscribe to events
            ad.OnClosed += AdClosed;
            ad.OnClicked += AdClicked;
            ad.OnLoaded += AdLoaded;
            ad.OnFailedLoad += AdFailedLoad;
            
            // Impression Event
            MediationService.Instance.ImpressionEventPublisher.OnImpression += ImpressionEvent;
        }

        public void Dispose() => ad?.Dispose();

        
        public async void ShowAd()
        {
            Debug.Log("INTERSTITIAL: SHOWING INTERSTITIAL AD: Ad loaded? " + ad.AdState.ToString());

            if (ad.AdState == AdState.Unloaded) {
                await LoadAd();
                Debug.LogWarning("Ad is now loaded?: " + (ad.AdState == AdState.Loaded));
            }

            if (ad.AdState == AdState.Loaded)
            {
                try
                {
                    InterstitialAdShowOptions showOptions = new InterstitialAdShowOptions();
                    showOptions.AutoReload = true;
                    await ad.ShowAsync(showOptions);
                    AdShown();
                }
                catch (ShowFailedException e)
                {
                    AdFailedShow(e);
                }
            }
        }

        async void InitializationComplete()
        {
            SetupAd();
            await LoadAd();
        }

        async public Task LoadAd()
        {
            try
            {
                await ad.LoadAsync();
            }
            catch (LoadFailedException)
            {
                // We will handle the failure in the OnFailedLoad callback
            }
        }

        void InitializationFailed(Exception e)
        {
            Debug.LogWarning("INTERSTITIAL: Initialization Failed: " + e.Message);
        }

        void AdLoaded(object sender, EventArgs e)
        {
            Debug.Log("INTERSTITIAL: Ad loaded");
        }

        void AdFailedLoad(object sender, LoadErrorEventArgs e)
        {
            Debug.LogWarning("INTERSTITIAL: Failed to load ad");
            Debug.LogWarning("INTERSTITIAL: " + e.Message);
        }
        
        void AdShown()
        {
            Debug.Log("INTERSTITIAL: Ad shown!");
        }
        
        void AdClosed(object sender, EventArgs e)
        {
            Debug.Log("INTERSTITIAL: Ad has closed");
            // Execute logic after an ad has been closed.

            SuccessScreen successScreen = GameObject.FindObjectOfType<SuccessScreen>();
            if (successScreen != null) {
                successScreen.DoQueudAction();
            }

            GameObject.FindObjectOfType<UnlockManager>().EndPurchase();
        }

        void AdClicked(object sender, EventArgs e)
        {
            Debug.Log("INTERSTITIAL: Ad has been clicked");
            // Execute logic after an ad has been clicked.
        }
        
        void AdFailedShow(ShowFailedException e)
        {
            Debug.LogWarning("INTERSTITIAL: " + e.Message);
        }

        void ImpressionEvent(object sender, ImpressionEventArgs args)
        {
            var impressionData = args.ImpressionData != null ? JsonUtility.ToJson(args.ImpressionData, true) : "null";
            Debug.Log("IINTERSTITIAL: mpression event from ad unit id " + args.AdUnitId + " " + impressionData);
        }
        
    }
}