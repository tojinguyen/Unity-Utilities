using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Ads.Examples
{
    /// <summary>
    /// Simple example showing basic AdMob integration
    /// Copy this script and modify it for your game's needs
    /// </summary>
    public class SimpleAdExample : MonoBehaviour
    {
        [Header("Ad Timing")]
        [SerializeField] private float interstitialCooldown = 30f; // Minimum time between interstitials
        [SerializeField] private int gameActionsForInterstitial = 5; // Show interstitial every X actions
        
        private float lastInterstitialTime;
        private int gameActionCount;
        
        private async void Start()
        {
            // Initialize ads when the game starts
            await InitializeAds();
            
            // Show banner ad
            ShowBanner();
        }
        
        private async UniTask InitializeAds()
        {
            Debug.Log("Initializing ads...");
            await AdService.InitializeAsync();
            Debug.Log("Ads initialized!");
        }
        
        public void ShowBanner()
        {
            // Show banner at the bottom of the screen
            AdService.ShowBanner(AdPosition.Bottom);
        }
        
        public void HideBanner()
        {
            AdService.HideBanner();
        }
        
        /// <summary>
        /// Call this when the player completes a level, dies, or performs any significant action
        /// </summary>
        public void OnGameAction()
        {
            gameActionCount++;
            
            // Show interstitial ad every few actions, with cooldown
            if (gameActionCount >= gameActionsForInterstitial && 
                Time.time - lastInterstitialTime > interstitialCooldown)
            {
                ShowInterstitialAd().Forget();
            }
        }
        
        public async UniTaskVoid ShowInterstitialAd()
        {
            // Make sure ad is ready
            if (!AdService.IsInterstitialReady())
            {
                Debug.Log("Loading interstitial ad...");
                bool loaded = await AdService.LoadInterstitialAsync();
                if (!loaded)
                {
                    Debug.Log("Failed to load interstitial ad");
                    return;
                }
            }
            
            // Show the ad
            bool shown = await AdService.ShowInterstitialAsync();
            if (shown)
            {
                lastInterstitialTime = Time.time;
                gameActionCount = 0;
                Debug.Log("Interstitial ad shown");
            }
        }
        
        /// <summary>
        /// Call this when the player wants to watch an ad for a reward
        /// </summary>
        public async void WatchRewardedAd()
        {
            // Make sure ad is ready
            if (!AdService.IsRewardedReady())
            {
                Debug.Log("Loading rewarded ad...");
                bool loaded = await AdService.LoadRewardedAsync();
                if (!loaded)
                {
                    Debug.Log("No rewarded ad available");
                    // Show message to player that ad is not available
                    return;
                }
            }
            
            // Show the ad and wait for result
            var result = await AdService.ShowRewardedAsync();
            
            if (result.Success)
            {
                // Give reward to player
                GiveRewardToPlayer();
                Debug.Log($"Player earned reward: {result.Reward.Type} x{result.Reward.Amount}");
            }
            else
            {
                Debug.Log("Player didn't complete the ad");
                // Optional: Show message that they need to watch the full ad
            }
        }
        
        private void GiveRewardToPlayer()
        {
            // Example rewards - modify for your game
            // GameManager.Instance.AddCoins(100);
            // GameManager.Instance.AddExtraLife();
            // GameManager.Instance.UnlockBonus();
            
            Debug.Log("Reward given to player!");
        }
        
        /// <summary>
        /// Example: Show rewarded ad for extra coins
        /// </summary>
        public async void WatchAdForCoins()
        {
            var result = await AdService.ShowRewardedAsync();
            if (result.Success)
            {
                // Give coins based on reward amount
                int coinsToGive = (int)(result.Reward.Amount * 10); // Convert reward to coins
                Debug.Log($"Player earned {coinsToGive} coins!");
                
                // Add coins to player's account
                // PlayerData.AddCoins(coinsToGive);
            }
        }
        
        /// <summary>
        /// Example: Show rewarded ad for extra life
        /// </summary>
        public async void WatchAdForExtraLife()
        {
            var result = await AdService.ShowRewardedAsync();
            if (result.Success)
            {
                Debug.Log("Player earned an extra life!");
                
                // Give extra life
                // GameManager.Instance.AddLife();
            }
        }
        
        /// <summary>
        /// Example: Show rewarded ad to continue after game over
        /// </summary>
        public async void WatchAdToContinue()
        {
            var result = await AdService.ShowRewardedAsync();
            if (result.Success)
            {
                Debug.Log("Player can continue playing!");
                
                // Continue the game
                // GameManager.Instance.ContinueGame();
            }
            else
            {
                Debug.Log("Player chose not to continue");
                // Show game over screen
                // GameManager.Instance.ShowGameOver();
            }
        }
    }
}
