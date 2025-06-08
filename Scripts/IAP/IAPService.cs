using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.IAP
{
    /// <summary>
    /// Static service class for easy IAP access throughout the application
    /// </summary>
    public static class IAPService
    {
        private static IIAPManager Manager => IAPManager.Instance;
        
        #region Properties
        
        public static bool IsInitialized => Manager?.IsInitialized ?? false;
        
        #endregion
        
        #region Initialization
        
        public static async UniTask InitializeAsync()
        {
            if (Manager != null)
            {
                await Manager.InitializeAsync();
            }
        }
        
        #endregion
        
        #region Purchase Methods
        
        public static async UniTask<PurchaseResult> PurchaseAsync(string productId)
        {
            if (Manager != null)
            {
                return await Manager.PurchaseAsync(productId);
            }
            return new PurchaseResult(false, productId, errorMessage: "IAPManager not available");
        }
        
        public static async UniTask<RestoreResult> RestorePurchasesAsync()
        {
            if (Manager != null)
            {
                return await Manager.RestorePurchasesAsync();
            }
            return new RestoreResult(false, errorMessage: "IAPManager not available");
        }
        
        #endregion
        
        #region Product Info
        
        public static bool IsProductAvailable(string productId)
        {
            return Manager?.IsProductAvailable(productId) ?? false;
        }
        
        public static ProductInfo GetProductInfo(string productId)
        {
            if (Manager != null)
            {
                return Manager.GetProductInfo(productId);
            }
            return new ProductInfo(productId, "", "", "", "", 0, ProductType.Consumable, false);
        }
        
        public static ProductInfo[] GetAllProducts()
        {
            return Manager?.GetAllProducts() ?? new ProductInfo[0];
        }
        
        #endregion
        
        #region Validation
        
        public static async UniTask<ValidationResult> ValidatePurchaseAsync(string receipt, string productId)
        {
            if (Manager != null)
            {
                return await Manager.ValidatePurchaseAsync(receipt, productId);
            }
            return new ValidationResult(false, productId, "IAPManager not available");
        }
        
        #endregion
        
        #region Event Subscription
        
        public static void SubscribeToEvents(
            System.Action onInitialized = null,
            System.Action<string> onInitializationFailed = null,
            System.Action<PurchaseResult> onPurchaseCompleted = null,
            System.Action<string, string> onPurchaseFailed = null,
            System.Action<RestoreResult> onPurchasesRestored = null,
            System.Action<string> onPurchaseRestoreFailed = null)
        {
            if (Manager == null) return;
            
            if (onInitialized != null)
                Manager.OnIAPInitialized += onInitialized;
            if (onInitializationFailed != null)
                Manager.OnInitializationFailed += onInitializationFailed;
            if (onPurchaseCompleted != null)
                Manager.OnPurchaseCompleted += onPurchaseCompleted;
            if (onPurchaseFailed != null)
                Manager.OnIAPPurchaseFailed += onPurchaseFailed;
            if (onPurchasesRestored != null)
                Manager.OnPurchasesRestored += onPurchasesRestored;
            if (onPurchaseRestoreFailed != null)
                Manager.OnPurchaseRestoreFailed += onPurchaseRestoreFailed;
        }
        
        public static void UnsubscribeFromEvents(
            System.Action onInitialized = null,
            System.Action<string> onInitializationFailed = null,
            System.Action<PurchaseResult> onPurchaseCompleted = null,
            System.Action<string, string> onPurchaseFailed = null,
            System.Action<RestoreResult> onPurchasesRestored = null,
            System.Action<string> onPurchaseRestoreFailed = null)
        {
            if (Manager == null) return;
            
            if (onInitialized != null)
                Manager.OnIAPInitialized -= onInitialized;
            if (onInitializationFailed != null)
                Manager.OnInitializationFailed -= onInitializationFailed;
            if (onPurchaseCompleted != null)
                Manager.OnPurchaseCompleted -= onPurchaseCompleted;
            if (onPurchaseFailed != null)
                Manager.OnIAPPurchaseFailed -= onPurchaseFailed;
            if (onPurchasesRestored != null)
                Manager.OnPurchasesRestored -= onPurchasesRestored;
            if (onPurchaseRestoreFailed != null)
                Manager.OnPurchaseRestoreFailed -= onPurchaseRestoreFailed;
        }
        
        #endregion
    }
}
