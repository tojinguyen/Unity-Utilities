using System;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.IAP
{
    public interface IIAPManager
    {
        bool IsInitialized { get; }
        
        UniTask InitializeAsync();
        
        // Purchase Methods
        UniTask<PurchaseResult> PurchaseAsync(string productId);
        UniTask<RestoreResult> RestorePurchasesAsync();
        
        // Product Info
        bool IsProductAvailable(string productId);
        ProductInfo GetProductInfo(string productId);
        ProductInfo[] GetAllProducts();
        
        // Validation
        UniTask<ValidationResult> ValidatePurchaseAsync(string receipt, string productId);
        
        // Events
        event Action OnIAPInitialized;
        event Action<string> OnInitializationFailed;
        event Action<PurchaseResult> OnPurchaseCompleted;
        event Action<string, string> OnIAPPurchaseFailed;
        event Action<RestoreResult> OnPurchasesRestored;
        event Action<string> OnPurchaseRestoreFailed;
    }
    
    public enum ProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }
    
    public enum PurchaseState
    {
        Purchased,
        Failed,
        Restored,
        Deferred,
        Cancelled
    }
    
    public struct PurchaseResult
    {
        public bool Success;
        public string ProductId;
        public string TransactionId;
        public string Receipt;
        public PurchaseState State;
        public string ErrorMessage;
        
        public PurchaseResult(bool success, string productId, string transactionId = "", 
                            string receipt = "", PurchaseState state = PurchaseState.Failed, 
                            string errorMessage = "")
        {
            Success = success;
            ProductId = productId;
            TransactionId = transactionId;
            Receipt = receipt;
            State = state;
            ErrorMessage = errorMessage;
        }
    }
    
    public struct RestoreResult
    {
        public bool Success;
        public PurchaseResult[] RestoredPurchases;
        public string ErrorMessage;
        
        public RestoreResult(bool success, PurchaseResult[] restoredPurchases = null, string errorMessage = "")
        {
            Success = success;
            RestoredPurchases = restoredPurchases ?? new PurchaseResult[0];
            ErrorMessage = errorMessage;
        }
    }
    
    public struct ValidationResult
    {
        public bool IsValid;
        public string ProductId;
        public string ErrorMessage;
        
        public ValidationResult(bool isValid, string productId, string errorMessage = "")
        {
            IsValid = isValid;
            ProductId = productId;
            ErrorMessage = errorMessage;
        }
    }
    
    public struct ProductInfo
    {
        public string ProductId;
        public string Title;
        public string Description;
        public string Price;
        public string PriceCurrencyCode;
        public decimal PriceDecimal;
        public ProductType Type;
        public bool IsAvailable;
        
        public ProductInfo(string productId, string title, string description, string price, 
                          string priceCurrencyCode, decimal priceDecimal, ProductType type, bool isAvailable)
        {
            ProductId = productId;
            Title = title;
            Description = description;
            Price = price;
            PriceCurrencyCode = priceCurrencyCode;
            PriceDecimal = priceDecimal;
            Type = type;
            IsAvailable = isAvailable;
        }
    }
}
