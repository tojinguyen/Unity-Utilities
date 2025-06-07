using System;
using UnityEngine;

namespace TirexGame.Utils.IAP
{
    [CreateAssetMenu(fileName = "IAPConfig", menuName = "TirexGame/IAP/IAP Config")]
    public class IAPConfig : ScriptableObject
    {
        [Header("General Settings")]
        [SerializeField] private bool enableTestMode = true;
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool enableReceiptValidation = false;
        
        [Header("Validation Settings")]
        [SerializeField] private string serverValidationUrl = "";
        [SerializeField] private int validationTimeoutSeconds = 10;
        
        [Header("Products")]
        [SerializeField] private ProductDefinition[] products = new ProductDefinition[0];
        
        public bool EnableTestMode => enableTestMode;
        public bool EnableLogging => enableLogging;
        public bool EnableReceiptValidation => enableReceiptValidation;
        public string ServerValidationUrl => serverValidationUrl;
        public int ValidationTimeoutSeconds => validationTimeoutSeconds;
        public ProductDefinition[] Products => products;
        
        [System.Serializable]
        public class ProductDefinition
        {
            [Header("Product Information")]
            public string productId = "";
            public ProductType type = ProductType.Consumable;
            
            [Header("Store-specific IDs (Optional)")]
            public string googlePlayId = "";
            public string appleAppStoreId = "";
            
            [Header("Configuration")]
            public bool enablePayout = true;
            
            public string GetStoreSpecificId()
            {
#if UNITY_ANDROID
                return !string.IsNullOrEmpty(googlePlayId) ? googlePlayId : productId;
#elif UNITY_IOS
                return !string.IsNullOrEmpty(appleAppStoreId) ? appleAppStoreId : productId;
#else
                return productId;
#endif
            }
        }
        
        private void OnValidate()
        {
            // Ensure product IDs are not empty
            for (int i = 0; i < products.Length; i++)
            {
                if (string.IsNullOrEmpty(products[i].productId))
                {
                    products[i].productId = $"product_{i}";
                }
            }
        }
        
        public ProductDefinition GetProduct(string productId)
        {
            foreach (var product in products)
            {
                if (product.productId == productId)
                {
                    return product;
                }
            }
            return null;
        }
        
        public bool HasProduct(string productId)
        {
            return GetProduct(productId) != null;
        }
    }
}
