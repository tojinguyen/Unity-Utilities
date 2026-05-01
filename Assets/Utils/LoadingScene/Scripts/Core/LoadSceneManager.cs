using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace TirexGame.Utils.LoadingScene
{
    public class LoadSceneManager : MonoBehaviour
    {
        private static LoadSceneManager _instance;

        [SerializeField] private DefaultLoadingUIController _ui;
        [SerializeField] private float _completionDelay = 0.8f;

        private float _currentProgress;
        private AsyncOperationHandle<SceneInstance> _loadedSceneHandle;
        private bool _hasLoadedScene;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static async UniTask LoadSceneAsync(string sceneName)
        {
            if (_instance == null)
            {
                ConsoleLogger.LogError("[LoadSceneManager] No instance found in scene!");
                return;
            }
            await _instance.LoadInternal(sceneName);
        }

        private async UniTask LoadInternal(string sceneName)
        {
            Scene sceneToUnload = SceneManager.GetActiveScene();
            AsyncOperationHandle<SceneInstance> previousHandle = _loadedSceneHandle;
            bool unloadViaAddressables = _hasLoadedScene;

            _currentProgress = 0f;
            if (_ui != null)
            {
                _ui.SetProgress(0f);
                await _ui.ShowUI();
            }

            var handle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            await FakeProgressUntilDone(handle);

            _loadedSceneHandle = handle;
            _hasLoadedScene = true;

            SceneManager.SetActiveScene(handle.Result.Scene);

            if (unloadViaAddressables)
                await Addressables.UnloadSceneAsync(previousHandle).ToUniTask();
            else if (sceneToUnload.isLoaded)
                await SceneManager.UnloadSceneAsync(sceneToUnload).ToUniTask();

            await SmoothTo(1f, speed: 2f);
            await UniTask.Delay(TimeSpan.FromSeconds(_completionDelay));

            if (_ui != null) _ui.HideUI();
        }

        private async UniTask FakeProgressUntilDone(AsyncOperationHandle<SceneInstance> handle)
        {
            float[] stops = GenerateStops();

            foreach (float stop in stops)
            {
                if (handle.IsDone) return;

                await SmoothTo(stop, speed: UnityEngine.Random.Range(0.08f, 0.2f));

                if (handle.IsDone) return;

                float pause = UnityEngine.Random.Range(0.1f, 0.45f);
                float elapsed = 0f;
                while (elapsed < pause && !handle.IsDone)
                {
                    elapsed += Time.unscaledDeltaTime;
                    await UniTask.Yield();
                }
            }

            if (!handle.IsDone)
                await handle.ToUniTask();
        }

        private static float[] GenerateStops()
        {
            int count = UnityEngine.Random.Range(3, 6);
            float[] stops = new float[count];
            float band = 0.75f / count;

            for (int i = 0; i < count; i++)
            {
                float lo = 0.1f + i * band;
                stops[i] = UnityEngine.Random.Range(lo, lo + band * 0.6f);
            }

            return stops;
        }

        private async UniTask SmoothTo(float target, float speed)
        {
            while (_currentProgress < target - 0.001f)
            {
                _currentProgress = Mathf.MoveTowards(_currentProgress, target, speed * Time.unscaledDeltaTime);
                if (_ui != null) _ui.SetProgress(_currentProgress);
                await UniTask.Yield();
            }

            _currentProgress = target;
            if (_ui != null) _ui.SetProgress(_currentProgress);
        }
    }
}
