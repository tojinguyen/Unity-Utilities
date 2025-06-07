using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.Ads
{
    public interface IAdManager
    {
        bool IsInitialized { get; }
        
        UniTask InitializeAsync();
        
        // Banner Ads
        void ShowBanner(AdPosition position = AdPosition.Bottom);
        void HideBanner();
        void DestroyBanner();
        
        // Interstitial Ads
        UniTask<bool> LoadInterstitialAsync();
        bool IsInterstitialReady();
        UniTask<bool> ShowInterstitialAsync();
        
        // Rewarded Ads
        UniTask<bool> LoadRewardedAsync();
        bool IsRewardedReady();
        UniTask<AdResult> ShowRewardedAsync();
        
        // Events
        event Action OnInitialized;
        event Action<AdType> OnAdLoaded;
        event Action<AdType, string> OnAdFailedToLoad;
        event Action<AdType> OnAdShown;
        event Action<AdType> OnAdClosed;
        event Action<AdType, string> OnAdFailedToShow;
        event Action<AdReward> OnRewardEarned;
    }
    
    public enum AdType
    {
        Banner,
        Interstitial,
        Rewarded
    }
    
    public enum AdPosition
    {
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }
    
    public struct AdResult
    {
        public bool Success;
        public AdReward Reward;
        public string ErrorMessage;
        
        public AdResult(bool success, AdReward reward = default, string errorMessage = "")
        {
            Success = success;
            Reward = reward;
            ErrorMessage = errorMessage;
        }
    }
    
    public struct AdReward
    {
        public string Type;
        public double Amount;
        
        public AdReward(string type, double amount)
        {
            Type = type;
            Amount = amount;
        }
    }
}
