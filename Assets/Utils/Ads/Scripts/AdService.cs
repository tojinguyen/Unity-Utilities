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
        
        public static event Action OnInitialized
        {
            add => AdManager.OnInitialized += value;
            remove => AdManager.OnInitialized -= value;
        }
        
        public static event Action<AdType> OnAdLoaded
        {
            add => AdManager.OnAdLoaded += value;
            remove => AdManager.OnAdLoaded -= value;
        }
        
        public static event Action<AdType, string> OnAdFailedToLoad
        {
            add => AdManager.OnAdFailedToLoad += value;
            remove => AdManager.OnAdFailedToLoad -= value;
        }
        
        public static event Action<AdType> OnAdShown
        {
            add => AdManager.OnAdShown += value;
            remove => AdManager.OnAdShown -= value;
        }
        
        public static event Action<AdType> OnAdClosed
        {
            add => AdManager.OnAdClosed += value;
            remove => AdManager.OnAdClosed -= value;
        }
        
        public static event Action<AdType, string> OnAdFailedToShow
        {
            add => AdManager.OnAdFailedToShow += value;
            remove => AdManager.OnAdFailedToShow -= value;
        }
        
        public static event Action<AdReward> OnRewardEarned
        {
            add => AdManager.OnRewardEarned += value;
            remove => AdManager.OnRewardEarned -= value;
        }
        
        #endregion
    }
}
