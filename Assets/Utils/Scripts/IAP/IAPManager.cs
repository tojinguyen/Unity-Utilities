using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
#endif

namespace TirexGame.Utils.IAP
{
    public class IAPManager : MonoSingleton<IAPManager>, IIAPManager
#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
        , IStoreListener
#endif
    {
        [Header("Configuration")] [SerializeField]
        private IAPConfig config;

        [Header("Auto Initialize")] [SerializeField]
        private bool autoInitialize = true;

        private bool _isInitialized;
        private Dictionary<string, ProductInfo> _productCatalog = new Dictionary<string, ProductInfo>();

#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
        private IStoreController _storeController;
        private IExtensionProvider _storeExtensionProvider;
        private UniTaskCompletionSource<bool> _initializationTcs;

        private Dictionary<string, UniTaskCompletionSource<PurchaseResult>> _purchaseTasks =
            new Dictionary<string, UniTaskCompletionSource<PurchaseResult>>();

        private UniTaskCompletionSource<RestoreResult> _restoreTcs;
#endif

        #region Properties

        public bool IsInitialized => _isInitialized;

        #endregion

        #region Events

        public event Action OnIAPInitialized;
        public event Action<string> OnInitializationFailed;
        public event Action<PurchaseResult> OnPurchaseCompleted;
        public event Action<string, string> OnIAPPurchaseFailed;
        public event Action<RestoreResult> OnPurchasesRestored;
        public event Action<string> OnPurchaseRestoreFailed;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            if (!config)
            {
                ConsoleLogger.LogError("[IAPManager] IAPConfig is not assigned!");
                return;
            }

            if (autoInitialize)
            {
                InitializeAsync().Forget();
            }
        }

        #endregion

        #region Initialization

        public async UniTask InitializeAsync()
        {
            if (_isInitialized)
                return;

            if (config == null)
            {
                ConsoleLogger.LogError("[IAPManager] IAPConfig is not assigned!");
                return;
            }

            Log("Initializing IAP...");

#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
            try
            {
                if (IsCurrentPlatformSupported())
                {
                    var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

                    // Add products from config
                    foreach (var productDef in config.Products)
                    {
                        var productType = ConvertProductType(productDef.type);
                        builder.AddProduct(productDef.productId, productType,
                            new IDs
                            {
                                { productDef.GetStoreSpecificId(), GooglePlay.Name },
                                { productDef.GetStoreSpecificId(), AppleAppStore.Name }
                            });

                        Log($"Added product: {productDef.productId} ({productDef.type})");
                    }

                    _initializationTcs = new UniTaskCompletionSource<bool>();
                    UnityPurchasing.Initialize(this, builder);

                    await _initializationTcs.Task;
                }
                else
                {
                    LogError("Current platform does not support Unity Purchasing");
                    OnInitializationFailed?.Invoke("Platform not supported");
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to initialize IAP: {e.Message}");
                OnInitializationFailed?.Invoke(e.Message);
            }
#else
            Log("Unity Purchasing is not available or platform not supported");
            _isInitialized = true;
            OnInitialized?.Invoke();
#endif
        }

        #endregion

        #region Purchase Methods

        public async UniTask<PurchaseResult> PurchaseAsync(string productId)
        {
#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
            if (!_isInitialized)
            {
                LogError("IAP is not initialized yet");
                return new PurchaseResult(false, productId, errorMessage: "IAP not initialized");
            }

            if (_storeController == null)
            {
                LogError("Store controller is null");
                return new PurchaseResult(false, productId, errorMessage: "Store controller unavailable");
            }

            var product = _storeController.products.WithID(productId);
            if (product == null)
            {
                LogError($"Product not found: {productId}");
                return new PurchaseResult(false, productId, errorMessage: "Product not found");
            }

            if (!product.availableToPurchase)
            {
                LogError($"Product not available for purchase: {productId}");
                return new PurchaseResult(false, productId, errorMessage: "Product not available");
            }

            Log($"Initiating purchase for: {productId}");

            var tcs = new UniTaskCompletionSource<PurchaseResult>();
            _purchaseTasks[productId] = tcs;

            _storeController.InitiatePurchase(product);

            return await tcs.Task;
#else
            LogError("Unity Purchasing is not available on this platform");
            return new PurchaseResult(false, productId, errorMessage: "Platform not supported");
#endif
        }

        public async UniTask<RestoreResult> RestorePurchasesAsync()
        {
#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
            if (!_isInitialized)
            {
                LogError("IAP is not initialized yet");
                return new RestoreResult(false, errorMessage: "IAP not initialized");
            }

            Log("Initiating purchase restoration...");

            _restoreTcs = new UniTaskCompletionSource<RestoreResult>();

            // Platform-specific restore
#if UNITY_IOS || UNITY_TVOS
            var appleExtensions = _storeExtensionProvider.GetExtension<IAppleExtensions>();
            appleExtensions.RestoreTransactions(OnRestoreResult);
#elif UNITY_ANDROID
            var googleExtensions = _storeExtensionProvider.GetExtension<IGooglePlayStoreExtensions>();
            googleExtensions.RestoreTransactions(OnRestoreResult);
#else
            OnRestoreResult(true);
#endif

            return await _restoreTcs.Task;
#else
            LogError("Unity Purchasing is not available on this platform");
            return new RestoreResult(false, errorMessage: "Platform not supported");
#endif
        }

        #endregion

        #region Product Info

        public bool IsProductAvailable(string productId)
        {
#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
            if (!_isInitialized || _storeController == null)
                return false;

            var product = _storeController.products.WithID(productId);
            return product != null && product.availableToPurchase;
#else
            return false;
#endif
        }

        public ProductInfo GetProductInfo(string productId)
        {
            if (_productCatalog.TryGetValue(productId, out var productInfo))
            {
                return productInfo;
            }

#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
            if (_isInitialized && _storeController != null)
            {
                var product = _storeController.products.WithID(productId);
                if (product != null)
                {
                    var info = CreateProductInfo(product);
                    _productCatalog[productId] = info;
                    return info;
                }
            }
#endif

            return new ProductInfo(productId, "", "", "", "", 0, ProductType.Consumable, false);
        }

        public ProductInfo[] GetAllProducts()
        {
            var products = new List<ProductInfo>();

#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
            if (_isInitialized && _storeController != null)
            {
                foreach (var product in _storeController.products.all)
                {
                    products.Add(GetProductInfo(product.definition.id));
                }
            }
#endif

            return products.ToArray();
        }

        #endregion

        #region Validation

        public async UniTask<ValidationResult> ValidatePurchaseAsync(string receipt, string productId)
        {
            if (!config.EnableReceiptValidation)
            {
                Log("Receipt validation is disabled");
                return new ValidationResult(true, productId);
            }

#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
            try
            {
                // Basic client-side validation using Unity's built-in validator
                var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                    AppleTangle.Data(), Application.identifier);

                var validationResult = validator.Validate(receipt, productId);

                Log($"Receipt validation result for {productId}: Valid");
                return new ValidationResult(true, productId);
            }
            catch (IAPSecurityException ex)
            {
                LogError($"Receipt validation failed for {productId}: {ex.Message}");
                return new ValidationResult(false, productId, ex.Message);
            }
            catch (Exception ex)
            {
                LogError($"Receipt validation error for {productId}: {ex.Message}");
                return new ValidationResult(false, productId, ex.Message);
            }
#else
            // If server validation URL is provided, implement server-side validation here
            if (!string.IsNullOrEmpty(config.ServerValidationUrl))
            {
                return await ValidateReceiptOnServer(receipt, productId);
            }
            
            Log("Receipt validation not available on this platform");
            return new ValidationResult(true, productId);
#endif
        }

        #endregion

        #region Unity IAP Callbacks

#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Log("IAP initialized successfully");

            _storeController = controller;
            _storeExtensionProvider = extensions;
            _isInitialized = true;

            // Cache product information
            foreach (var product in controller.products.all)
            {
                _productCatalog[product.definition.id] = CreateProductInfo(product);
                Log(
                    $"Product loaded: {product.definition.id} - {product.metadata.localizedTitle} ({product.metadata.localizedPriceString})");
            }

            OnInitialized?.Invoke();
            _initializationTcs?.TrySetResult(true);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            LogError($"IAP initialization failed: {error}");
            OnInitializationFailed?.Invoke(error.ToString());
            _initializationTcs?.TrySetResult(false);
        }

        // Add newer method signature for initialization failure
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            LogError($"IAP initialization failed: {error} - {message}");
            OnInitializationFailed?.Invoke($"{error}: {message}");
            _initializationTcs?.TrySetResult(false);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var productId = args.purchasedProduct.definition.id;
            var receipt = args.purchasedProduct.receipt;
            var transactionId = args.purchasedProduct.transactionID;

            Log($"Purchase completed: {productId}");

            var result = new PurchaseResult(true, productId, transactionId, receipt, PurchaseState.Purchased);

            // Validate receipt if enabled
            if (config.EnableReceiptValidation)
            {
                ValidatePurchaseAsync(receipt, productId).ContinueWith(validationResult =>
                {
                    if (validationResult.IsValid)
                    {
                        CompletePurchase(result);
                    }
                    else
                    {
                        var errorResult = new PurchaseResult(false, productId, transactionId, receipt,
                            PurchaseState.Failed, "Receipt validation failed");
                        CompletePurchase(errorResult);
                    }
                }).Forget();

                return PurchaseProcessingResult.Pending;
            }
            else
            {
                CompletePurchase(result);
                return PurchaseProcessingResult.Complete;
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            var productId = product.definition.id;
            LogError($"Purchase failed: {productId} - {failureReason}");

            var result = new PurchaseResult(false, productId, errorMessage: failureReason.ToString());

            if (_purchaseTasks.TryGetValue(productId, out var tcs))
            {
                tcs.TrySetResult(result);
                _purchaseTasks.Remove(productId);
            }

            OnIAPPurchaseFailed?.Invoke(productId, failureReason.ToString());
        }
#endif

        #endregion

        #region Helper Methods

#if UNITY_PURCHASING && ((UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) || UNITY_EDITOR)
        private void CompletePurchase(PurchaseResult result)
        {
            if (_purchaseTasks.TryGetValue(result.ProductId, out var tcs))
            {
                tcs.TrySetResult(result);
                _purchaseTasks.Remove(result.ProductId);
            }

            OnPurchaseCompleted?.Invoke(result);
        }

        private ProductInfo CreateProductInfo(Product product)
        {
            var configProduct = config.GetProduct(product.definition.id);
            var productType = configProduct?.type ?? ProductType.Consumable;

            return new ProductInfo(
                product.definition.id,
                product.metadata.localizedTitle,
                product.metadata.localizedDescription,
                product.metadata.localizedPriceString,
                product.metadata.isoCurrencyCode,
                product.metadata.localizedPrice,
                productType,
                product.availableToPurchase
            );
        }

        private UnityEngine.Purchasing.ProductType ConvertProductType(ProductType type)
        {
            return type switch
            {
                ProductType.Consumable => UnityEngine.Purchasing.ProductType.Consumable,
                ProductType.NonConsumable => UnityEngine.Purchasing.ProductType.NonConsumable,
                ProductType.Subscription => UnityEngine.Purchasing.ProductType.Subscription,
                _ => UnityEngine.Purchasing.ProductType.Consumable
            };
        }

        private bool IsCurrentPlatformSupported()
        {
            return Application.platform == RuntimePlatform.Android ||
                   Application.platform == RuntimePlatform.IPhonePlayer ||
                   Application.platform == RuntimePlatform.OSXPlayer ||
                   Application.platform == RuntimePlatform.tvOS;
        }

        private void OnRestoreResult(bool success)
        {
            if (success)
            {
                Log("Purchases restored successfully");
                var restoredPurchases = GetRestoredPurchases();
                var result = new RestoreResult(true, restoredPurchases);
                OnPurchasesRestored?.Invoke(result);
                _restoreTcs?.TrySetResult(result);
            }
            else
            {
                LogError("Purchase restoration failed");
                var result = new RestoreResult(false, errorMessage: "Restoration failed");
                OnPurchaseRestoreFailed?.Invoke("Restoration failed");
                _restoreTcs?.TrySetResult(result);
            }
        }

        private PurchaseResult[] GetRestoredPurchases()
        {
            var restored = new List<PurchaseResult>();

            if (_storeController != null)
            {
                foreach (var product in _storeController.products.all)
                {
                    if (product.hasReceipt)
                    {
                        var result = new PurchaseResult(true, product.definition.id,
                            product.transactionID, product.receipt, PurchaseState.Restored);
                        restored.Add(result);
                    }
                }
            }

            return restored.ToArray();
        }
#endif

        private async UniTask<ValidationResult> ValidateReceiptOnServer(string receipt, string productId)
        {
            try
            {
                // Implement server-side validation here
                // This is a placeholder for server validation logic
                Log($"Server validation not implemented for {productId}");
                return new ValidationResult(true, productId);
            }
            catch (Exception ex)
            {
                LogError($"Server validation error: {ex.Message}");
                return new ValidationResult(false, productId, ex.Message);
            }
        }

        private void Log(string message)
        {
            if (config && config.EnableLogging)
            {
                ConsoleLogger.Log($"[IAPManager] {message}");
            }
        }

        private void LogError(string message)
        {
            ConsoleLogger.LogError($"[IAPManager] {message}");
        }

        #endregion
    }
}