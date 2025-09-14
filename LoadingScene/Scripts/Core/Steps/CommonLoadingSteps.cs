using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    public class DelayLoadingStep : BaseLoadingStep
    {
        private readonly float _delayTime;
        private readonly bool _useRealtime;

        public DelayLoadingStep(float delayTime, string stepName = "Delay", string description = "",
            bool useRealtime = false, float weight = 1f)
            : base(stepName,
                !string.IsNullOrEmpty(description) ? description : $"Waiting for {delayTime:F1} seconds...", weight)
        {
            _delayTime = Mathf.Max(0f, delayTime);
            _useRealtime = useRealtime;
        }

        protected override async Task ExecuteStepAsync()
        {
            DebugLog($"Starting delay of {_delayTime:F1} seconds");

            var elapsedTime = 0f;

            while (elapsedTime < _delayTime && !_isCancelled)
            {
                float deltaTime = _useRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsedTime += deltaTime;

                UpdateProgressInternal(elapsedTime / _delayTime);

                await Task.Yield(); // Yield control to allow cancellation
                ThrowIfCancelled();
            }

            DebugLog($"Delay completed after {elapsedTime:F1} seconds");
        }
    }

    public class CustomLoadingStep : BaseLoadingStep
    {
        private readonly Func<ILoadingStep, Task> _action;
        private readonly Action<ILoadingStep> _syncAction;

        public CustomLoadingStep(Func<ILoadingStep, Task> action, string stepName, string description = "",
            float weight = 1f, bool canSkip = false)
            : base(stepName, description, weight, canSkip)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public CustomLoadingStep(Action<ILoadingStep> syncAction, string stepName, string description = "",
            float weight = 1f, bool canSkip = false)
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
            UpdateProgressInternal(0.1f);

            await Task.Yield();
            ThrowIfCancelled();

            UpdateProgressInternal(0.5f);

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

                UpdateProgressInternal(0.9f);
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

            float systemProgress = 0f;
            for (var i = 0; i < _systemNames.Length; i++)
            {
                ThrowIfCancelled();

                var systemName = _systemNames[i];
                DebugLog($"Initializing {systemName}...");

                while (systemProgress < 1f && !_isCancelled)
                {
                    systemProgress += Time.unscaledDeltaTime / _timePerSystem;
                    systemProgress = Mathf.Clamp01(systemProgress);

                    var totalProgress = (i + systemProgress) / _systemNames.Length;

                    UpdateProgressInternal(totalProgress);
                    await Task.Yield();
                }

                DebugLog($"{systemName} initialized");
            }

            DebugLog("All game systems initialized");
        }
    }
}