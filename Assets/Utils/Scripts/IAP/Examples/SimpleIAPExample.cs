using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.IAP.Examples
{
    /// <summary>
    /// Simple example showing basic IAP integration
    /// Copy this script and modify it for your game's needs
    /// </summary>
    public class SimpleIAPExample : MonoBehaviour
    {
        [Header("Product IDs")]
        [SerializeField] private string removeAdsProductId = "remove_ads";
        [SerializeField] private string premiumCurrencyProductId = "premium_currency_100";
        [SerializeField] private string vipSubscriptionProductId = "vip_subscription";
        
        [Header("UI References")]
        [SerializeField] private GameObject removeAdsButton;
        [SerializeField] private GameObject premiumShopButton;
        [SerializeField] private GameObject subscriptionButton;
        
        private bool hasRemovedAds = false;
        
        private async void Start()
        {
            await InitializeIAP();
        }
        
        private async UniTask InitializeIAP()
        {
            // Subscribe to IAP events
            IAPService.SubscribeToEvents(
                onInitialized: OnIAPReady,
                onInitializationFailed: OnIAPFailed,
                onPurchaseCompleted: OnPurchaseCompleted,
                onPurchaseFailed: OnPurchaseFailed,
                onPurchasesRestored: OnPurchasesRestored
            );
            
            // Initialize IAP
            await IAPService.InitializeAsync();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            IAPService.UnsubscribeFromEvents(
                onInitialized: OnIAPReady,
                onInitializationFailed: OnIAPFailed,
                onPurchaseCompleted: OnPurchaseCompleted,
                onPurchaseFailed: OnPurchaseFailed,
                onPurchasesRestored: OnPurchasesRestored
            );
        }
        
        #region Public Methods (Call from UI)
        
        public void PurchaseRemoveAds()
        {
            if (hasRemovedAds)
            {
                Debug.Log("Ads already removed!");
                return;
            }
            
            PurchaseProduct(removeAdsProductId).Forget();
        }
        
        public void PurchasePremiumCurrency()
        {
            PurchaseProduct(premiumCurrencyProductId).Forget();
        }
        
        public void PurchaseVIPSubscription()
        {
            PurchaseProduct(vipSubscriptionProductId).Forget();
        }
        
        public void RestorePurchases()
        {
            RestorePreviousPurchases().Forget();
        }
        
        #endregion
        
        #region Purchase Logic
        
        private async UniTaskVoid PurchaseProduct(string productId)
        {
            if (!IAPService.IsInitialized)
            {
                Debug.LogWarning("IAP not initialized yet");
                return;
            }
            
            if (!IAPService.IsProductAvailable(productId))
            {
                Debug.LogWarning($"Product not available: {productId}");
                return;
            }
            
            Debug.Log($"Purchasing: {productId}");
            
            var result = await IAPService.PurchaseAsync(productId);
            
            if (result.Success)
            {
                Debug.Log($"Purchase successful: {productId}");
                ProcessSuccessfulPurchase(result);
            }
            else
            {
                Debug.LogError($"Purchase failed: {result.ErrorMessage}");
                ShowPurchaseFailedMessage(result.ErrorMessage);
            }
        }
        
        private async UniTaskVoid RestorePreviousPurchases()
        {
            if (!IAPService.IsInitialized)
            {
                Debug.LogWarning("IAP not initialized yet");
                return;
            }
            
            Debug.Log("Restoring purchases...");
            
            var result = await IAPService.RestorePurchasesAsync();
            
            if (result.Success)
            {
                Debug.Log($"Restored {result.RestoredPurchases.Length} purchases");
                
                foreach (var purchase in result.RestoredPurchases)
                {
                    ProcessSuccessfulPurchase(purchase);
                }
            }
            else
            {
                Debug.LogError($"Restore failed: {result.ErrorMessage}");
            }
        }
        
        #endregion
        
        #region Purchase Processing
        
        private void ProcessSuccessfulPurchase(PurchaseResult purchase)
        {
            switch (purchase.ProductId)
            {
                case var id when id == removeAdsProductId:
                    ProcessRemoveAdsPurchase();
                    break;
                    
                case var id when id == premiumCurrencyProductId:
                    ProcessPremiumCurrencyPurchase();
                    break;
                    
                case var id when id == vipSubscriptionProductId:
                    ProcessVIPSubscriptionPurchase();
                    break;
                    
                default:
                    Debug.Log($"Unknown product purchased: {purchase.ProductId}");
                    break;
            }
            
            // Save purchase state (implement your save system here)
            SavePurchaseState();
        }
        
        private void ProcessRemoveAdsPurchase()
        {
            hasRemovedAds = true;
            
            // Hide ads throughout the game
            // DisableAds();
            
            // Update UI
            if (removeAdsButton != null)
                removeAdsButton.SetActive(false);
                
            Debug.Log("Ads removed successfully!");
        }
        
        private void ProcessPremiumCurrencyPurchase()
        {
            // Give player premium currency
            int currencyAmount = 100; // Based on product
            // AddPremiumCurrency(currencyAmount);
            
            Debug.Log($"Added {currencyAmount} premium currency!");
        }
        
        private void ProcessVIPSubscriptionPurchase()
        {
            // Activate VIP benefits
            // ActivateVIPBenefits();
            
            Debug.Log("VIP subscription activated!");
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnIAPReady()
        {
            Debug.Log("IAP system ready!");
            
            // Update UI based on available products
            UpdateProductUI();
            
            // Check for existing purchases
            CheckExistingPurchases();
        }
        
        private void OnIAPFailed(string error)
        {
            Debug.LogError($"IAP initialization failed: {error}");
        }
        
        private void OnPurchaseCompleted(PurchaseResult result)
        {
            Debug.Log($"Purchase completed: {result.ProductId}");
        }
        
        private void OnPurchaseFailed(string productId, string error)
        {
            Debug.LogError($"Purchase failed: {productId} - {error}");
            ShowPurchaseFailedMessage(error);
        }
        
        private void OnPurchasesRestored(RestoreResult result)
        {
            Debug.Log($"Purchases restored: {result.RestoredPurchases.Length} items");
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateProductUI()
        {
            // Update button text with prices
            var removeAdsProduct = IAPService.GetProductInfo(removeAdsProductId);
            if (removeAdsProduct.IsAvailable && removeAdsButton != null)
            {
                // Update button text: "Remove Ads - $2.99"
                var buttonText = removeAdsButton.GetComponentInChildren<UnityEngine.UI.Text>();
                if (buttonText != null)
                {
                    buttonText.text = $"Remove Ads - {removeAdsProduct.Price}";
                }
            }
            
            var currencyProduct = IAPService.GetProductInfo(premiumCurrencyProductId);
            if (currencyProduct.IsAvailable && premiumShopButton != null)
            {
                var buttonText = premiumShopButton.GetComponentInChildren<UnityEngine.UI.Text>();
                if (buttonText != null)
                {
                    buttonText.text = $"100 Gems - {currencyProduct.Price}";
                }
            }
            
            var subscriptionProduct = IAPService.GetProductInfo(vipSubscriptionProductId);
            if (subscriptionProduct.IsAvailable && subscriptionButton != null)
            {
                var buttonText = subscriptionButton.GetComponentInChildren<UnityEngine.UI.Text>();
                if (buttonText != null)
                {
                    buttonText.text = $"VIP Monthly - {subscriptionProduct.Price}";
                }
            }
        }
        
        private void CheckExistingPurchases()
        {
            // Check if user already owns non-consumable products
            // This would typically be done by checking your save data
            // or validating receipts
            
            if (HasPurchasedProduct(removeAdsProductId))
            {
                ProcessRemoveAdsPurchase();
            }
        }
        
        private void ShowPurchaseFailedMessage(string error)
        {
            // Show user-friendly error message
            Debug.Log($"Purchase could not be completed: {error}");
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool HasPurchasedProduct(string productId)
        {
            // Implement your logic to check if product was previously purchased
            // This could check PlayerPrefs, a save file, or validate with server
            return PlayerPrefs.GetInt($"purchased_{productId}", 0) == 1;
        }
        
        private void SavePurchaseState()
        {
            // Save purchase state to persistent storage
            if (hasRemovedAds)
            {
                PlayerPrefs.SetInt($"purchased_{removeAdsProductId}", 1);
            }
            
            PlayerPrefs.Save();
        }
        
        #endregion
    }
}
