using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.Ads
{
    public static class AdService
    {
        private static IAdManager _adManager;
        
        private static IAdManager AdManager
        {
            get
            {
                if (_adManager == null)
                    _adManager = TirexGame.Utils.Ads.AdManager.Instance;
                return _adManager;
            }
        }
        
        #region Initialization
        
        /// <summary>
        /// Initialize the ad system
        /// </summary>
        public static UniTask InitializeAsync()
        {
            return AdManager.InitializeAsync();
        }
        
        /// <summary>
        /// Check if the ad system is initialized
        /// </summary>
        public static bool IsInitialized => AdManager.IsInitialized;
        
        #endregion
        
        #region Banner Ads
        
        /// <summary>
        /// Show banner ad at specified position
        /// </summary>
        public static void ShowBanner(AdPosition position = AdPosition.Bottom)
        {
            AdManager.ShowBanner(position);
        }
        
        /// <summary>
        /// Hide banner ad
        /// </summary>
        public static void HideBanner()
        {
            AdManager.HideBanner();
        }
        
        /// <summary>
        /// Destroy banner ad
        /// </summary>
        public static void DestroyBanner()
        {
            AdManager.DestroyBanner();
        }
        
        #endregion
        
        #region Interstitial Ads
        
        /// <summary>
        /// Load interstitial ad
        /// </summary>
        public static UniTask<bool> LoadInterstitialAsync()
        {
            return AdManager.LoadInterstitialAsync();
        }
        
        /// <summary>
        /// Check if interstitial ad is ready to show
        /// </summary>
        public static bool IsInterstitialReady()
        {
            return AdManager.IsInterstitialReady();
        }
        
        /// <summary>
        /// Show interstitial ad
        /// </summary>
        public static UniTask<bool> ShowInterstitialAsync()
        {
            return AdManager.ShowInterstitialAsync();
        }
        
        #endregion
        
        #region Rewarded Ads
        
        /// <summary>
        /// Load rewarded ad
        /// </summary>
        public static UniTask<bool> LoadRewardedAsync()
        {
            return AdManager.LoadRewardedAsync();
        }
        
        /// <summary>
        /// Check if rewarded ad is ready to show
        /// </summary>
        public static bool IsRewardedReady()
        {
            return AdManager.IsRewardedReady();
        }
        
        /// <summary>
        /// Show rewarded ad and wait for result
        /// </summary>
        public static UniTask<AdResult> ShowRewardedAsync()
        {
            return AdManager.ShowRewardedAsync();
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Subscribe to ad events
        /// </summary>
        public static void SubscribeToEvents(
            Action onInitialized = null,
            Action<AdType> onAdLoaded = null,
            Action<AdType, string> onAdFailedToLoad = null,
            Action<AdType> onAdShown = null,
            Action<AdType> onAdClosed = null,
            Action<AdType, string> onAdFailedToShow = null,
            Action<AdReward> onRewardEarned = null)
        {
            if (onInitialized != null)
                AdManager.OnInitialized += onInitialized;
            if (onAdLoaded != null)
                AdManager.OnAdLoaded += onAdLoaded;
            if (onAdFailedToLoad != null)
                AdManager.OnAdFailedToLoad += onAdFailedToLoad;
            if (onAdShown != null)
                AdManager.OnAdShown += onAdShown;
            if (onAdClosed != null)
                AdManager.OnAdClosed += onAdClosed;
            if (onAdFailedToShow != null)
                AdManager.OnAdFailedToShow += onAdFailedToShow;
            if (onRewardEarned != null)
                AdManager.OnRewardEarned += onRewardEarned;
        }
        
        /// <summary>
        /// Unsubscribe from ad events
        /// </summary>
        public static void UnsubscribeFromEvents(
            Action onInitialized = null,
            Action<AdType> onAdLoaded = null,
            Action<AdType, string> onAdFailedToLoad = null,
            Action<AdType> onAdShown = null,
            Action<AdType> onAdClosed = null,
            Action<AdType, string> onAdFailedToShow = null,
            Action<AdReward> onRewardEarned = null)
        {
            if (onInitialized != null)
                AdManager.OnInitialized -= onInitialized;
            if (onAdLoaded != null)
                AdManager.OnAdLoaded -= onAdLoaded;
            if (onAdFailedToLoad != null)
                AdManager.OnAdFailedToLoad -= onAdFailedToLoad;
            if (onAdShown != null)
                AdManager.OnAdShown -= onAdShown;
            if (onAdClosed != null)
                AdManager.OnAdClosed -= onAdClosed;
            if (onAdFailedToShow != null)
                AdManager.OnAdFailedToShow -= onAdFailedToShow;
            if (onRewardEarned != null)
                AdManager.OnRewardEarned -= onRewardEarned;
        }
        
        #endregion
    }
}
