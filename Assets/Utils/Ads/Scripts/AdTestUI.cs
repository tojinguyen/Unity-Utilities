using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Ads
{
    /// <summary>
    /// Example script showing how to use the AdService
    /// Attach this to a GameObject with UI buttons for testing
    /// </summary>
    public class AdTestUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button initializeButton;
        [SerializeField] private Button showBannerButton;
        [SerializeField] private Button hideBannerButton;
        [SerializeField] private Button showInterstitialButton;
        [SerializeField] private Button showRewardedButton;
        [SerializeField] private Text statusText;
        
        [Header("Settings")]
        [SerializeField] private AdPosition bannerPosition = AdPosition.Bottom;
        
        private void Start()
        {
            SetupButtons();
            SubscribeToAdEvents();
            UpdateStatus("Ad system not initialized");
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromAdEvents();
        }
        
        private void SetupButtons()
        {
            if (initializeButton)
                initializeButton.onClick.AddListener(() => InitializeAds().Forget());
                
            if (showBannerButton)
                showBannerButton.onClick.AddListener(ShowBanner);
                
            if (hideBannerButton)
                hideBannerButton.onClick.AddListener(HideBanner);
                
            if (showInterstitialButton)
                showInterstitialButton.onClick.AddListener(() => ShowInterstitial().Forget());
                
            if (showRewardedButton)
                showRewardedButton.onClick.AddListener(() => ShowRewarded().Forget());
        }
        
        private void SubscribeToAdEvents()
        {
            AdService.SubscribeToEvents(
                onInitialized: OnAdSystemInitialized,
                onAdLoaded: OnAdLoaded,
                onAdFailedToLoad: OnAdFailedToLoad,
                onAdShown: OnAdShown,
                onAdClosed: OnAdClosed,
                onAdFailedToShow: OnAdFailedToShow,
                onRewardEarned: OnRewardEarned
            );
        }
        
        private void UnsubscribeFromAdEvents()
        {
            AdService.UnsubscribeFromEvents(
                onInitialized: OnAdSystemInitialized,
                onAdLoaded: OnAdLoaded,
                onAdFailedToLoad: OnAdFailedToLoad,
                onAdShown: OnAdShown,
                onAdClosed: OnAdClosed,
                onAdFailedToShow: OnAdFailedToShow,
                onRewardEarned: OnRewardEarned
            );
        }
        
        #region Button Actions
        
        private async UniTaskVoid InitializeAds()
        {
            UpdateStatus("Initializing ads...");
            await AdService.InitializeAsync();
        }
        
        private void ShowBanner()
        {
            AdService.ShowBanner(bannerPosition);
            UpdateStatus("Showing banner ad");
        }
        
        private void HideBanner()
        {
            AdService.HideBanner();
            UpdateStatus("Banner ad hidden");
        }
        
        private async UniTaskVoid ShowInterstitial()
        {
            if (!AdService.IsInterstitialReady())
            {
                UpdateStatus("Loading interstitial ad...");
                bool loaded = await AdService.LoadInterstitialAsync();
                if (!loaded)
                {
                    UpdateStatus("Failed to load interstitial ad");
                    return;
                }
            }
            
            UpdateStatus("Showing interstitial ad...");
            bool shown = await AdService.ShowInterstitialAsync();
            UpdateStatus(shown ? "Interstitial ad completed" : "Failed to show interstitial ad");
        }
        
        private async UniTaskVoid ShowRewarded()
        {
            if (!AdService.IsRewardedReady())
            {
                UpdateStatus("Loading rewarded ad...");
                bool loaded = await AdService.LoadRewardedAsync();
                if (!loaded)
                {
                    UpdateStatus("Failed to load rewarded ad");
                    return;
                }
            }
            
            UpdateStatus("Showing rewarded ad...");
            var result = await AdService.ShowRewardedAsync();
            
            if (result.Success)
            {
                UpdateStatus($"Reward earned: {result.Reward.Type} x{result.Reward.Amount}");
            }
            else
            {
                UpdateStatus($"Rewarded ad failed: {result.ErrorMessage}");
            }
        }
        
        #endregion
        
        #region Ad Event Handlers
        
        private void OnAdSystemInitialized()
        {
            UpdateStatus("Ad system initialized successfully");
        }
        
        private void OnAdLoaded(AdType adType)
        {
            UpdateStatus($"{adType} ad loaded");
        }
        
        private void OnAdFailedToLoad(AdType adType, string error)
        {
            UpdateStatus($"{adType} ad failed to load: {error}");
        }
        
        private void OnAdShown(AdType adType)
        {
            UpdateStatus($"{adType} ad shown");
        }
        
        private void OnAdClosed(AdType adType)
        {
            UpdateStatus($"{adType} ad closed");
        }
        
        private void OnAdFailedToShow(AdType adType, string error)
        {
            UpdateStatus($"{adType} ad failed to show: {error}");
        }
        
        private void OnRewardEarned(AdReward reward)
        {
            UpdateStatus($"Reward earned: {reward.Type} x{reward.Amount}");
        }
        
        #endregion
        
        private void UpdateStatus(string message)
        {
            if (statusText)
            {
                statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            }
            
            Debug.Log($"[AdTestUI] {message}");
        }
    }
}
