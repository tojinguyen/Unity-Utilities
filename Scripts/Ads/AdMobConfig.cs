using System;
using UnityEngine;

namespace TirexGame.Utils.Ads
{
    [CreateAssetMenu(fileName = "AdMobConfig", menuName = "TirexGame/Ads/AdMob Config")]
    public class AdMobConfig : ScriptableObject
    {
        [Header("App IDs")]
        [SerializeField] private string androidAppId = "ca-app-pub-3940256099942544~3347511713"; // Test App ID
        [SerializeField] private string iosAppId = "ca-app-pub-3940256099942544~1458002511"; // Test App ID
        
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
            public string bannerId = "ca-app-pub-3940256099942544/6300978111"; // Test Banner ID
            
            [Header("Interstitial")]
            public string interstitialId = "ca-app-pub-3940256099942544/1033173712"; // Test Interstitial ID
            
            [Header("Rewarded")]
            public string rewardedId = "ca-app-pub-3940256099942544/5224354917"; // Test Rewarded ID
        }
        
        private void OnValidate()
        {
            // Ensure test IDs are set by default
            if (enableTestMode)
            {
                if (string.IsNullOrEmpty(androidAdUnitIds.bannerId))
                    androidAdUnitIds.bannerId = "ca-app-pub-3940256099942544/6300978111";
                if (string.IsNullOrEmpty(androidAdUnitIds.interstitialId))
                    androidAdUnitIds.interstitialId = "ca-app-pub-3940256099942544/1033173712";
                if (string.IsNullOrEmpty(androidAdUnitIds.rewardedId))
                    androidAdUnitIds.rewardedId = "ca-app-pub-3940256099942544/5224354917";
                    
                if (string.IsNullOrEmpty(iosAdUnitIds.bannerId))
                    iosAdUnitIds.bannerId = "ca-app-pub-3940256099942544/2934735716";
                if (string.IsNullOrEmpty(iosAdUnitIds.interstitialId))
                    iosAdUnitIds.interstitialId = "ca-app-pub-3940256099942544/4411468910";
                if (string.IsNullOrEmpty(iosAdUnitIds.rewardedId))
                    iosAdUnitIds.rewardedId = "ca-app-pub-3940256099942544/1712485313";
            }
        }
    }
}
