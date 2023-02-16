using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Mediation;
using UnityEngine;

namespace Ad
{
    public class BannerAd : IDisposable
    {
        IBannerAd ad;
        string adUnitId;
        string gameId;

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
            ad = MediationService.Instance.CreateBannerAd(
                adUnitId,
                BannerAdPredefinedSize.Banner.ToBannerAdSize(),
                BannerAdAnchor.TopCenter,
                Vector2.zero);

            //Subscribe to events
            ad.OnRefreshed += AdRefreshed;
            ad.OnClicked += AdClicked;
            ad.OnLoaded += AdLoaded;
            ad.OnFailedLoad += AdFailedLoad;
            
            // Impression Event
            MediationService.Instance.ImpressionEventPublisher.OnImpression += ImpressionEvent;
        }

        public void Dispose() => ad?.Dispose();

        
        async void InitializationComplete()
        {
            SetupAd();
            await LoadAd();
        }

        async Task LoadAd()
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
            Debug.LogWarning("BANNER: Initialization Failed: " + e.Message);
        }

        void AdLoaded(object sender, EventArgs e)
        {
            Debug.Log("BANNER: Ad loaded");
        }

        void AdFailedLoad(object sender, LoadErrorEventArgs e)
        {
            Debug.LogWarning("BANNER: Failed to load ad");
            Debug.LogWarning("BANNER: " + e.Message);
        }
        
        void AdRefreshed(object sender, LoadErrorEventArgs e)
        {
            Debug.Log("Refreshed ad");
            Debug.Log("BANNER: " + e.Message);
        }
        
        void AdClicked(object sender, EventArgs e)
        {
            Debug.Log("BANNER: Ad has been clicked");
            // Execute logic after an ad has been clicked.
        }
        
        void ImpressionEvent(object sender, ImpressionEventArgs args)
        {
            var impressionData = args.ImpressionData != null ? JsonUtility.ToJson(args.ImpressionData, true) : "null";
            Debug.Log("BANNER: Impression event from ad unit id " + args.AdUnitId + " " + impressionData);
        }
        
    }
}