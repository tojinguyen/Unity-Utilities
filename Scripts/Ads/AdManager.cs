using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_ANDROID || UNITY_IOS
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
#endif

namespace TirexGame.Utils.Ads
{
    public class AdManager : MonoSingleton<AdManager>, IAdManager
    {
        [Header("Configuration")]
        [SerializeField] private AdMobConfig config;
        
        [Header("Auto Initialize")]
        [SerializeField] private bool autoInitialize = true;
        
        private bool _isInitialized;
        
#if UNITY_ANDROID || UNITY_IOS
        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;
        
        private bool _isLoadingInterstitial;
        private bool _isLoadingRewarded;
        private int _interstitialRetryAttempts;
        private int _rewardedRetryAttempts;
        
        private UniTaskCompletionSource<bool> _interstitialLoadTcs;
        private UniTaskCompletionSource<bool> _interstitialShowTcs;
        private UniTaskCompletionSource<bool> _rewardedLoadTcs;
        private UniTaskCompletionSource<AdResult> _rewardedShowTcs;
#endif
        
        #region Properties
        
        public bool IsInitialized => _isInitialized;
        
        #endregion
        
        #region Events
        
        public event Action OnInitialized;
        public event Action<AdType> OnAdLoaded;
        public event Action<AdType, string> OnAdFailedToLoad;
        public event Action<AdType> OnAdShown;
        public event Action<AdType> OnAdClosed;
        public event Action<AdType, string> OnAdFailedToShow;
        public event Action<AdReward> OnRewardEarned;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            
            if (config == null)
            {
                Debug.LogError("[AdManager] AdMobConfig is not assigned!");
                return;
            }
            
            if (autoInitialize)
            {
                InitializeAsync().Forget();
            }
        }
        
        private void OnDestroy()
        {
#if UNITY_ANDROID || UNITY_IOS
            DestroyBanner();
            _interstitialAd?.Destroy();
            _rewardedAd?.Destroy();
#endif
        }
        
        #endregion
        
        #region Initialization
        
        public async UniTask InitializeAsync()
        {
            if (_isInitialized)
                return;
                
            if (config == null)
            {
                Debug.LogError("[AdManager] AdMobConfig is not assigned!");
                return;
            }
            
            Log("Initializing AdMob...");
            
#if UNITY_ANDROID || UNITY_IOS
            try
            {
                // Initialize the Google Mobile Ads SDK
                var tcs = new UniTaskCompletionSource();
                
                MobileAds.Initialize(initStatus =>
                {
                    Log("AdMob initialized successfully");
                    _isInitialized = true;
                    OnInitialized?.Invoke();
                    tcs.TrySetResult();
                });
                
                await tcs.Task;
                
                // Pre-load ads
                LoadInterstitialAsync().Forget();
                LoadRewardedAsync().Forget();
            }
            catch (Exception e)
            {
                Debug.LogError($"[AdManager] Failed to initialize AdMob: {e.Message}");
            }
#else
            Debug.LogWarning("[AdManager] AdMob is only supported on Android and iOS platforms");
            _isInitialized = true;
            OnInitialized?.Invoke();
#endif
        }
        
        #endregion
        
        #region Banner Ads
        
        public void ShowBanner(AdPosition position = AdPosition.Bottom)
        {
#if UNITY_ANDROID || UNITY_IOS
            if (!_isInitialized)
            {
                Debug.LogWarning("[AdManager] AdMob is not initialized yet");
                return;
            }
            
            if (_bannerView != null)
            {
                _bannerView.Show();
                return;
            }
            
            var adUnitIds = config.GetAdUnitIds();
            Log($"Loading banner ad: {adUnitIds.bannerId}");
            
            var adPosition = ConvertAdPosition(position);
            _bannerView = new BannerView(adUnitIds.bannerId, AdSize.Banner, adPosition);
            
            // Register for banner events
            _bannerView.OnBannerAdLoaded += OnBannerLoaded;
            _bannerView.OnBannerAdLoadFailed += OnBannerFailedToLoad;
            
            // Load and show banner
            var request = new AdRequest();
            _bannerView.LoadAd(request);
#else
            Debug.LogWarning("[AdManager] Banner ads are only supported on Android and iOS platforms");
#endif
        }
        
        public void HideBanner()
        {
#if UNITY_ANDROID || UNITY_IOS
            _bannerView?.Hide();
#endif
        }
        
        public void DestroyBanner()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (_bannerView != null)
            {
                _bannerView.OnBannerAdLoaded -= OnBannerLoaded;
                _bannerView.OnBannerAdLoadFailed -= OnBannerFailedToLoad;
                _bannerView.Destroy();
                _bannerView = null;
            }
#endif
        }
        
        #endregion
        
        #region Interstitial Ads
        
        public async UniTask<bool> LoadInterstitialAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (!_isInitialized)
            {
                Debug.LogWarning("[AdManager] AdMob is not initialized yet");
                return false;
            }
            
            if (_isLoadingInterstitial)
            {
                return await _interstitialLoadTcs.Task;
            }
            
            if (_interstitialAd != null)
            {
                return true; // Already loaded
            }
            
            _isLoadingInterstitial = true;
            _interstitialLoadTcs = new UniTaskCompletionSource<bool>();
            
            var adUnitIds = config.GetAdUnitIds();
            Log($"Loading interstitial ad: {adUnitIds.interstitialId}");
            
            var request = new AdRequest();
            
            InterstitialAd.Load(adUnitIds.interstitialId, request, (InterstitialAd ad, LoadAdError error) =>
            {
                _isLoadingInterstitial = false;
                
                if (error != null)
                {
                    LogError($"Interstitial ad failed to load: {error.GetMessage()}");
                    OnAdFailedToLoad?.Invoke(AdType.Interstitial, error.GetMessage());
                    
                    // Retry loading
                    if (_interstitialRetryAttempts < config.InterstitialLoadRetryAttempts)
                    {
                        _interstitialRetryAttempts++;
                        StartCoroutine(RetryLoadInterstitial());
                        return;
                    }
                    
                    _interstitialRetryAttempts = 0;
                    _interstitialLoadTcs.TrySetResult(false);
                    return;
                }
                
                Log("Interstitial ad loaded successfully");
                _interstitialAd = ad;
                _interstitialRetryAttempts = 0;
                
                // Register for interstitial events
                RegisterInterstitialEvents();
                
                OnAdLoaded?.Invoke(AdType.Interstitial);
                _interstitialLoadTcs.TrySetResult(true);
            });
            
            return await _interstitialLoadTcs.Task;
#else
            Debug.LogWarning("[AdManager] Interstitial ads are only supported on Android and iOS platforms");
            return false;
#endif
        }
        
        public bool IsInterstitialReady()
        {
#if UNITY_ANDROID || UNITY_IOS
            return _interstitialAd != null && _interstitialAd.CanShowAd();
#else
            return false;
#endif
        }
        
        public async UniTask<bool> ShowInterstitialAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (!IsInterstitialReady())
            {
                Debug.LogWarning("[AdManager] Interstitial ad is not ready");
                return false;
            }
            
            _interstitialShowTcs = new UniTaskCompletionSource<bool>();
            _interstitialAd.Show();
            
            return await _interstitialShowTcs.Task;
#else
            Debug.LogWarning("[AdManager] Interstitial ads are only supported on Android and iOS platforms");
            return false;
#endif
        }
        
        #endregion
        
        #region Rewarded Ads
        
        public async UniTask<bool> LoadRewardedAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (!_isInitialized)
            {
                Debug.LogWarning("[AdManager] AdMob is not initialized yet");
                return false;
            }
            
            if (_isLoadingRewarded)
            {
                return await _rewardedLoadTcs.Task;
            }
            
            if (_rewardedAd != null)
            {
                return true; // Already loaded
            }
            
            _isLoadingRewarded = true;
            _rewardedLoadTcs = new UniTaskCompletionSource<bool>();
            
            var adUnitIds = config.GetAdUnitIds();
            Log($"Loading rewarded ad: {adUnitIds.rewardedId}");
            
            var request = new AdRequest();
            
            RewardedAd.Load(adUnitIds.rewardedId, request, (RewardedAd ad, LoadAdError error) =>
            {
                _isLoadingRewarded = false;
                
                if (error != null)
                {
                    LogError($"Rewarded ad failed to load: {error.GetMessage()}");
                    OnAdFailedToLoad?.Invoke(AdType.Rewarded, error.GetMessage());
                    
                    // Retry loading
                    if (_rewardedRetryAttempts < config.RewardedLoadRetryAttempts)
                    {
                        _rewardedRetryAttempts++;
                        StartCoroutine(RetryLoadRewarded());
                        return;
                    }
                    
                    _rewardedRetryAttempts = 0;
                    _rewardedLoadTcs.TrySetResult(false);
                    return;
                }
                
                Log("Rewarded ad loaded successfully");
                _rewardedAd = ad;
                _rewardedRetryAttempts = 0;
                
                // Register for rewarded events
                RegisterRewardedEvents();
                
                OnAdLoaded?.Invoke(AdType.Rewarded);
                _rewardedLoadTcs.TrySetResult(true);
            });
            
            return await _rewardedLoadTcs.Task;
#else
            Debug.LogWarning("[AdManager] Rewarded ads are only supported on Android and iOS platforms");
            return false;
#endif
        }
        
        public bool IsRewardedReady()
        {
#if UNITY_ANDROID || UNITY_IOS
            return _rewardedAd != null && _rewardedAd.CanShowAd();
#else
            return false;
#endif
        }
        
        public async UniTask<AdResult> ShowRewardedAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (!IsRewardedReady())
            {
                Debug.LogWarning("[AdManager] Rewarded ad is not ready");
                return new AdResult(false, errorMessage: "Rewarded ad is not ready");
            }
            
            _rewardedShowTcs = new UniTaskCompletionSource<AdResult>();
            _rewardedAd.Show((Reward reward) =>
            {
                Log($"Rewarded ad completed. Reward: {reward.Type} x{reward.Amount}");
                var adReward = new AdReward(reward.Type, reward.Amount);
                OnRewardEarned?.Invoke(adReward);
            });
            
            return await _rewardedShowTcs.Task;
#else
            Debug.LogWarning("[AdManager] Rewarded ads are only supported on Android and iOS platforms");
            return new AdResult(false, errorMessage: "Platform not supported");
#endif
        }
        
        #endregion
        
        #region Event Handlers
        
#if UNITY_ANDROID || UNITY_IOS
        private void OnBannerLoaded()
        {
            Log("Banner ad loaded successfully");
            OnAdLoaded?.Invoke(AdType.Banner);
        }
        
        private void OnBannerFailedToLoad(LoadAdError error)
        {
            LogError($"Banner ad failed to load: {error.GetMessage()}");
            OnAdFailedToLoad?.Invoke(AdType.Banner, error.GetMessage());
        }
        
        private void RegisterInterstitialEvents()
        {
            if (_interstitialAd == null) return;
            
            _interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                Log("Interstitial ad opened");
                OnAdShown?.Invoke(AdType.Interstitial);
            };
            
            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Log("Interstitial ad closed");
                OnAdClosed?.Invoke(AdType.Interstitial);
                
                // Clean up and preload next ad
                _interstitialAd.Destroy();
                _interstitialAd = null;
                _interstitialShowTcs?.TrySetResult(true);
                
                LoadInterstitialAsync().Forget();
            };
            
            _interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                LogError($"Interstitial ad failed to show: {error.GetMessage()}");
                OnAdFailedToShow?.Invoke(AdType.Interstitial, error.GetMessage());
                
                // Clean up
                _interstitialAd.Destroy();
                _interstitialAd = null;
                _interstitialShowTcs?.TrySetResult(false);
                
                LoadInterstitialAsync().Forget();
            };
        }
        
        private void RegisterRewardedEvents()
        {
            if (_rewardedAd == null) return;
            
            _rewardedAd.OnAdFullScreenContentOpened += () =>
            {
                Log("Rewarded ad opened");
                OnAdShown?.Invoke(AdType.Rewarded);
            };
            
            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                Log("Rewarded ad closed");
                OnAdClosed?.Invoke(AdType.Rewarded);
                
                // Clean up and preload next ad
                _rewardedAd.Destroy();
                _rewardedAd = null;
                _rewardedShowTcs?.TrySetResult(new AdResult(true));
                
                LoadRewardedAsync().Forget();
            };
            
            _rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                LogError($"Rewarded ad failed to show: {error.GetMessage()}");
                OnAdFailedToShow?.Invoke(AdType.Rewarded, error.GetMessage());
                
                // Clean up
                _rewardedAd.Destroy();
                _rewardedAd = null;
                _rewardedShowTcs?.TrySetResult(new AdResult(false, errorMessage: error.GetMessage()));
                
                LoadRewardedAsync().Forget();
            };
        }
#endif
        
        #endregion
        
        #region Helper Methods
        
#if UNITY_ANDROID || UNITY_IOS
        private AdPosition ConvertAdPosition(AdPosition position)
        {
            return position switch
            {
                AdPosition.Top => GoogleMobileAds.Api.AdPosition.Top,
                AdPosition.Bottom => GoogleMobileAds.Api.AdPosition.Bottom,
                AdPosition.TopLeft => GoogleMobileAds.Api.AdPosition.TopLeft,
                AdPosition.TopRight => GoogleMobileAds.Api.AdPosition.TopRight,
                AdPosition.BottomLeft => GoogleMobileAds.Api.AdPosition.BottomLeft,
                AdPosition.BottomRight => GoogleMobileAds.Api.AdPosition.BottomRight,
                AdPosition.Center => GoogleMobileAds.Api.AdPosition.Center,
                _ => GoogleMobileAds.Api.AdPosition.Bottom
            };
        }
        
        private IEnumerator RetryLoadInterstitial()
        {
            yield return new WaitForSeconds(config.RetryDelay);
            LoadInterstitialAsync().Forget();
        }
        
        private IEnumerator RetryLoadRewarded()
        {
            yield return new WaitForSeconds(config.RetryDelay);
            LoadRewardedAsync().Forget();
        }
#endif
        
        private void Log(string message)
        {
            if (config != null && config.EnableLogging)
            {
                Debug.Log($"[AdManager] {message}");
            }
        }
        
        private void LogError(string message)
        {
            if (config != null && config.EnableLogging)
            {
                Debug.LogError($"[AdManager] {message}");
            }
        }
        
        #endregion
    }
}