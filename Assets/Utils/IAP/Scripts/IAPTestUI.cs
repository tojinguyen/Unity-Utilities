using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace TirexGame.Utils.IAP
{
    /// <summary>
    /// Test UI for IAP functionality - similar to AdTestUI
    /// </summary>
    public class IAPTestUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button initializeButton;
        [SerializeField] private Button refreshProductsButton;
        [SerializeField] private Button restorePurchasesButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Transform productListParent;
        [SerializeField] private GameObject productButtonPrefab;
        
        [Header("Test Products")]
        [SerializeField] private string[] testProductIds = { "test_consumable", "test_nonconsumable", "test_subscription" };
        
        private List<GameObject> _productButtons = new List<GameObject>();
        
        private void Start()
        {
            SetupUI();
            UpdateStatus("IAP Test UI Ready");
            
            // Subscribe to IAP events
            IAPService.SubscribeToEvents(
                onInitialized: OnIAPInitialized,
                onInitializationFailed: OnIAPInitializationFailed,
                onPurchaseCompleted: OnPurchaseCompleted,
                onPurchaseFailed: OnPurchaseFailed,
                onPurchasesRestored: OnPurchasesRestored,
                onPurchaseRestoreFailed: OnPurchaseRestoreFailed
            );
            
            // Check if already initialized
            if (IAPService.IsInitialized)
            {
                OnIAPInitialized();
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            IAPService.UnsubscribeFromEvents(
                onInitialized: OnIAPInitialized,
                onInitializationFailed: OnIAPInitializationFailed,
                onPurchaseCompleted: OnPurchaseCompleted,
                onPurchaseFailed: OnPurchaseFailed,
                onPurchasesRestored: OnPurchasesRestored,
                onPurchaseRestoreFailed: OnPurchaseRestoreFailed
            );
        }
        
        private void SetupUI()
        {
            if (initializeButton)
                initializeButton.onClick.AddListener(() => InitializeIAP().Forget());
                
            if (refreshProductsButton)
                refreshProductsButton.onClick.AddListener(RefreshProducts);
                
            if (restorePurchasesButton)
                restorePurchasesButton.onClick.AddListener(() => RestorePurchases().Forget());
        }
        
        private async UniTaskVoid InitializeIAP()
        {
            UpdateStatus("Initializing IAP...");
            await IAPService.InitializeAsync();
        }
        
        private void RefreshProducts()
        {
            if (!IAPService.IsInitialized)
            {
                UpdateStatus("IAP not initialized");
                return;
            }
            
            ClearProductButtons();
            
            var products = IAPService.GetAllProducts();
            
            if (products.Length == 0)
            {
                // Show test products if no products are available
                foreach (var productId in testProductIds)
                {
                    CreateProductButton(productId, $"Test Product ({productId})", "Test", ProductType.Consumable, false);
                }
                UpdateStatus($"No products available. Showing {testProductIds.Length} test products.");
            }
            else
            {
                foreach (var product in products)
                {
                    CreateProductButton(product.ProductId, product.Title, product.Price, product.Type, product.IsAvailable);
                }
                UpdateStatus($"Loaded {products.Length} products");
            }
        }
        
        private void CreateProductButton(string productId, string title, string price, ProductType type, bool isAvailable)
        {
            if (productButtonPrefab == null || productListParent == null) return;
            
            var buttonObj = Instantiate(productButtonPrefab, productListParent);
            var button = buttonObj.GetComponent<Button>();
            var text = buttonObj.GetComponentInChildren<Text>();
            
            if (text != null)
            {
                text.text = $"{title}\n{price}\n{type}\n{(isAvailable ? "Available" : "Not Available")}";
            }
            
            if (button != null)
            {
                button.interactable = isAvailable;
                button.onClick.AddListener(() => PurchaseProduct(productId).Forget());
            }
            
            _productButtons.Add(buttonObj);
        }
        
        private void ClearProductButtons()
        {
            foreach (var button in _productButtons)
            {
                if (button)
                    DestroyImmediate(button);
            }
            _productButtons.Clear();
        }
        
        private async UniTaskVoid PurchaseProduct(string productId)
        {
            UpdateStatus($"Purchasing {productId}...");
            var result = await IAPService.PurchaseAsync(productId);
            
            if (result.Success)
            {
                UpdateStatus($"Purchase successful: {productId}");
            }
            else
            {
                UpdateStatus($"Purchase failed: {result.ErrorMessage}");
            }
        }
        
        private async UniTaskVoid RestorePurchases()
        {
            UpdateStatus("Restoring purchases...");
            var result = await IAPService.RestorePurchasesAsync();
            
            if (result.Success)
            {
                UpdateStatus($"Restored {result.RestoredPurchases.Length} purchases");
            }
            else
            {
                UpdateStatus($"Restore failed: {result.ErrorMessage}");
            }
        }
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            }
            Debug.Log($"[IAPTestUI] {message}");
        }
        
        #region IAP Event Handlers
        
        private void OnIAPInitialized()
        {
            UpdateStatus("IAP initialized successfully!");
            RefreshProducts();
        }
        
        private void OnIAPInitializationFailed(string error)
        {
            UpdateStatus($"IAP initialization failed: {error}");
        }
        
        private void OnPurchaseCompleted(PurchaseResult result)
        {
            UpdateStatus($"Purchase completed: {result.ProductId} (State: {result.State})");
        }
        
        private void OnPurchaseFailed(string productId, string error)
        {
            UpdateStatus($"Purchase failed: {productId} - {error}");
        }
        
        private void OnPurchasesRestored(RestoreResult result)
        {
            UpdateStatus($"Purchases restored: {result.RestoredPurchases.Length} items");
        }
        
        private void OnPurchaseRestoreFailed(string error)
        {
            UpdateStatus($"Restore failed: {error}");
        }        
        #endregion
    }
}
