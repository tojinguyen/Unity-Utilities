using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Loading step đơn giản với delay time.
    /// Hữu ích cho việc test hoặc tạo thời gian loading tối thiểu.
    /// </summary>
    public class DelayLoadingStep : BaseLoadingStep
    {
        private readonly float _delayTime;
        private readonly bool _useRealtime;
        
        public DelayLoadingStep(float delayTime, string stepName = "Delay", string description = "", bool useRealtime = false, float weight = 1f) 
            : base(stepName, !string.IsNullOrEmpty(description) ? description : $"Waiting for {delayTime:F1} seconds...", weight)
        {
            _delayTime = Mathf.Max(0f, delayTime);
            _useRealtime = useRealtime;
        }
        
        protected override async Task ExecuteStepAsync()
        {
            DebugLog($"Starting delay of {_delayTime:F1} seconds");
            
            float elapsedTime = 0f;
            
            while (elapsedTime < _delayTime && !_isCancelled)
            {
                float deltaTime = _useRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsedTime += deltaTime;
                
                UpdateProgress(elapsedTime / _delayTime);
                
                await Task.Yield(); // Yield control to allow cancellation
                ThrowIfCancelled();
            }
            
            DebugLog($"Delay completed after {elapsedTime:F1} seconds");
        }
    }
    
    /// <summary>
    /// Loading step với custom action.
    /// Cho phép thực thi bất kỳ hành động nào trong quá trình loading.
    /// </summary>
    public class CustomLoadingStep : BaseLoadingStep
    {
        private readonly Func<ILoadingStep, Task> _action;
        private readonly Action<ILoadingStep> _syncAction;
        
        public CustomLoadingStep(Func<ILoadingStep, Task> action, string stepName, string description = "", float weight = 1f, bool canSkip = false) 
            : base(stepName, description, weight, canSkip)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }
        
        public CustomLoadingStep(Action<ILoadingStep> syncAction, string stepName, string description = "", float weight = 1f, bool canSkip = false) 
            : base(stepName, description, weight, canSkip)
        {
            _syncAction = syncAction ?? throw new ArgumentNullException(nameof(syncAction));
        }
        
        protected override async Task ExecuteStepAsync()
        {
            DebugLog("Executing custom action");
            
            ThrowIfCancelled();
            
            if (_action != null)
            {
                await _action(this);
            }
            else if (_syncAction != null)
            {
                _syncAction(this);
                await Task.Yield();
            }
            
            DebugLog("Custom action completed");
        }
    }
    
    /// <summary>
    /// Loading step cho việc load asset từ Resources.
    /// </summary>
    public class ResourceLoadingStep : BaseLoadingStep
    {
        private readonly string _resourcePath;
        private readonly Type _assetType;
        private UnityEngine.Object _loadedAsset;
        
        public UnityEngine.Object LoadedAsset => _loadedAsset;
        
        public ResourceLoadingStep(string resourcePath, Type assetType = null, string stepName = "", float weight = 1f) 
            : base(!string.IsNullOrEmpty(stepName) ? stepName : $"Loading {System.IO.Path.GetFileName(resourcePath)}", 
                  $"Loading resource: {resourcePath}", weight)
        {
            _resourcePath = resourcePath ?? throw new ArgumentNullException(nameof(resourcePath));
            _assetType = assetType;
        }
        
        protected override async Task ExecuteStepAsync()
        {
            DebugLog($"Loading resource: {_resourcePath}");
            
            ThrowIfCancelled();
            UpdateProgress(0.1f);
            
            // Simulate async loading (Resources.Load is synchronous)
            await Task.Yield();
            ThrowIfCancelled();
            
            UpdateProgress(0.5f);
            
            try
            {
                if (_assetType != null)
                {
                    _loadedAsset = Resources.Load(_resourcePath, _assetType);
                }
                else
                {
                    _loadedAsset = Resources.Load(_resourcePath);
                }
                
                UpdateProgress(0.9f);
                await Task.Yield();
                
                if (_loadedAsset == null)
                {
                    throw new InvalidOperationException($"Failed to load resource: {_resourcePath}");
                }
                
                DebugLog($"Successfully loaded resource: {_resourcePath}");
            }
            catch (Exception ex)
            {
                DebugLog($"Failed to load resource: {_resourcePath} - {ex.Message}");
                throw;
            }
        }
        
        public T GetLoadedAsset<T>() where T : UnityEngine.Object
        {
            return _loadedAsset as T;
        }
    }
    
    /// <summary>
    /// Loading step mô phỏng việc khởi tạo hệ thống game.
    /// </summary>
    public class GameSystemInitializationStep : BaseLoadingStep
    {
        private readonly string[] _systemNames;
        private readonly float _timePerSystem;
        
        public GameSystemInitializationStep(string[] systemNames, float timePerSystem = 0.5f, float weight = 1f) 
            : base("Initialize Game Systems", "Initializing game systems...", weight)
        {
            _systemNames = systemNames ?? new[] { "Audio", "Input", "UI", "Save System" };
            _timePerSystem = Mathf.Max(0.1f, timePerSystem);
        }
        
        protected override async Task ExecuteStepAsync()
        {
            DebugLog($"Initializing {_systemNames.Length} game systems");
            
            for (int i = 0; i < _systemNames.Length; i++)
            {
                ThrowIfCancelled();
                
                string systemName = _systemNames[i];
                DebugLog($"Initializing {systemName}...");
                
                // Simulate system initialization time
                float systemProgress = 0f;
                while (systemProgress < 1f && !_isCancelled)
                {
                    systemProgress += Time.unscaledDeltaTime / _timePerSystem;
                    systemProgress = Mathf.Clamp01(systemProgress);
                    
                    float totalProgress = (i + systemProgress) / _systemNames.Length;
                    UpdateProgress(totalProgress);
                    
                    await Task.Yield();
                }
                
                DebugLog($"{systemName} initialized");
            }
            
            DebugLog("All game systems initialized");
        }
    }
}