using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Ads.Examples
{
    public class SimpleAdExample : MonoBehaviour
    {
        [Header("Ad Timing")]
        [SerializeField] private float interstitialCooldown = 30f; 
        [SerializeField] private int gameActionsForInterstitial = 5; 
        
        private float _lastInterstitialTime;
        private int _gameActionCount;
        
        private async void Start()
        {
            try
            {
                // Initialize ads when the game starts
                await InitializeAds();
            
                // Show banner ad
                ShowBanner();
            }
            catch (Exception e)
            {
                ConsoleLogger.LogError("Error initializing ads: " + e.Message);
            }
        }
        
        private async UniTask InitializeAds()
        {
            ConsoleLogger.Log("Initializing ads...");
            await AdService.InitializeAsync();
            ConsoleLogger.Log("Ads initialized!");
        }
        
        public void ShowBanner()
        {
            AdService.ShowBanner(AdPosition.Bottom);
        }
        
        public void HideBanner()
        {
            AdService.HideBanner();
        }
        
        public void OnGameAction()
        {
            _gameActionCount++;
            
            if (_gameActionCount >= gameActionsForInterstitial && 
                Time.time - _lastInterstitialTime > interstitialCooldown)
            {
                ShowInterstitialAd().Forget();
            }
        }
        
        public async UniTaskVoid ShowInterstitialAd()
        {
            if (!AdService.IsInterstitialReady())
            {
                ConsoleLogger.Log("Loading interstitial ad...");
                bool loaded = await AdService.LoadInterstitialAsync();
                if (!loaded)
                {
                    ConsoleLogger.Log("Failed to load interstitial ad");
                    return;
                }
            }
            
            var shown = await AdService.ShowInterstitialAsync();
            if (shown)
            {
                _lastInterstitialTime = Time.time;
                _gameActionCount = 0;
                ConsoleLogger.Log("Interstitial ad shown");
            }
        }
   
        public async void WatchRewardedAd()
        {
            // Make sure ad is ready
            if (!AdService.IsRewardedReady())
            {
                ConsoleLogger.Log("Loading rewarded ad...");
                var loaded = await AdService.LoadRewardedAsync();
                if (!loaded)
                {
                    ConsoleLogger.Log("No rewarded ad available");
                    return;
                }
            }
            
            var result = await AdService.ShowRewardedAsync();
            
            if (result.Success)
            {
                GiveRewardToPlayer();
                ConsoleLogger.Log($"Player earned reward: {result.Reward.Type} x{result.Reward.Amount}");
            }
            else
            {
                ConsoleLogger.Log("Player didn't complete the ad");
            }
        }
        
        private void GiveRewardToPlayer()
        {
            ConsoleLogger.Log("Reward given to player!");
        }

        public async void WatchAdForCoins()
        {
            var result = await AdService.ShowRewardedAsync();
            if (result.Success)
            {
                var coinsToGive = (int)(result.Reward.Amount * 10); 
                ConsoleLogger.Log($"Player earned {coinsToGive} coins!");
            }
        }
  
        public async void WatchAdForExtraLife()
        {
            var result = await AdService.ShowRewardedAsync();
            if (result.Success)
            {
                ConsoleLogger.Log("Player earned an extra life!");
            }
        }
  
        public async void WatchAdToContinue()
        {
            var result = await AdService.ShowRewardedAsync();
            if (result.Success)
            {
                ConsoleLogger.Log("Player can continue playing!");
                
                // Continue the game
                // GameManager.Instance.ContinueGame();
            }
            else
            {
                ConsoleLogger.Log("Player chose not to continue");
                // Show game over screen
                // GameManager.Instance.ShowGameOver();
            }
        }
    }
}
