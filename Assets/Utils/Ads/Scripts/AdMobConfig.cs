using System;
using UnityEngine;

namespace TirexGame.Utils.Ads
{
    [CreateAssetMenu(fileName = "AdMobConfig", menuName = "TirexGame/Ads/AdMob Config")]
    public class AdMobConfig : ScriptableObject
    {
        [Header("App IDs")]
        [SerializeField] private string androidAppId = "";
        [SerializeField] private string iosAppId = "";
        
        [Header("Ad Unit IDs")]
        [SerializeField] private AdUnitIds androidAdUnitIds = new AdUnitIds();
        [SerializeField] private AdUnitIds iosAdUnitIds = new AdUnitIds();
        
        [Header("Settings")]
        [SerializeField] private bool enableTestMode = true;
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private int interstitialLoadRetryAttempts = 3;
        [SerializeField] private int rewardedLoadRetryAttempts = 3;
        [SerializeField] private float retryDelay = 2f;
        
        public string GetAppId()
        {
#if UNITY_ANDROID
            return androidAppId;
#elif UNITY_IOS
            return iosAppId;
#else
            return androidAppId;
#endif
        }
        
        public AdUnitIds GetAdUnitIds()
        {
#if UNITY_ANDROID
            return androidAdUnitIds;
#elif UNITY_IOS
            return iosAdUnitIds;
#else
            return androidAdUnitIds;
#endif
        }
        
        public bool EnableTestMode => enableTestMode;
        public bool EnableLogging => enableLogging;
        public int InterstitialLoadRetryAttempts => interstitialLoadRetryAttempts;
        public int RewardedLoadRetryAttempts => rewardedLoadRetryAttempts;
        public float RetryDelay => retryDelay;
        
    [System.Serializable]
    public class AdUnitIds
    {
        [Header("Banner")]
        public string bannerId = "";
        
        [Header("Interstitial")]
        public string interstitialId = "";
        
        [Header("Rewarded")]
        public string rewardedId = "";
        
        /// <summary>
        /// Clears all ad unit IDs
        /// </summary>
        public void ClearAll()
        {
            bannerId = string.Empty;
            interstitialId = string.Empty;
            rewardedId = string.Empty;
        }
        
        /// <summary>
        /// Sets test ad unit IDs for Android
        /// </summary>
        public void SetAndroidTestIds()
        {
            bannerId = "ca-app-pub-3940256099942544/6300978111";
            interstitialId = "ca-app-pub-3940256099942544/1033173712";
            rewardedId = "ca-app-pub-3940256099942544/5224354917";
        }
        
        /// <summary>
        /// Sets test ad unit IDs for iOS
        /// </summary>
        public void SetIOSTestIds()
        {
            bannerId = "ca-app-pub-3940256099942544/2934735716";
            interstitialId = "ca-app-pub-3940256099942544/4411468910";
            rewardedId = "ca-app-pub-3940256099942544/1712485313";
        }
    }        private void OnValidate()
        {
            // OnValidate method kept for future validation logic if needed
        }
    }
}
