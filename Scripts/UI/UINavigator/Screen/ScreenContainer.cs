using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utils.Scripts.UIManager.UINavigator
{
    public class ScreenContainer : UIContainer
    {
        private static Stack<Screen> _stackScreen = new();
        internal static UIContainer Container;
        
        public static int Count => _stackScreen.Count;

        protected override void Awake()
        {
            base.Awake();
            _stackScreen = new();
            Container = this;
        }

        private void Start()
        {
            SetupStateScreen().Forget();
        }
        
        private async UniTaskVoid SetupStateScreen()
        {
            foreach (Transform screenTf in Container.transform)
            {
                var scr = screenTf.GetComponent<Screen>();
                if (scr)
                    _stackScreen.Push(scr);
            }
            if (_stackScreen.TryPeek(out var screen))
            {
                await screen.OnPushEnter();
            }
        }

        public static async UniTask<Screen> Push(string keyAddressable)
        {
            var screenSpawn = await SpawnUIGo(keyAddressable, Container.TransformContainer);

            var screen = screenSpawn.GetComponent<Screen>();
            if (screen is null)
                return null;
            if (_stackScreen.TryPeek(out var screenToHide))
            {
                if (screenToHide != null)
                    await screenToHide.OnPushExit(screen);
                else
                    _stackScreen.Pop();
            }

            await screen.OnPushEnter();
            _stackScreen.Push(screen);

            await UniTask.Yield();
            return screen;
        }

        public static async UniTask Pop()
        {
            if (_stackScreen.TryPeek(out var screen))
            {
                _stackScreen.Pop();

                if (_stackScreen.TryPeek(out var screenToShow))
                {
                    if (screenToShow != null)
                    {
                        if (!screenToShow.gameObject.activeSelf)
                        {
                            screenToShow.gameObject.SetActive(true);
                            await screenToShow.OnPopEnter();
                        }
                    } 
                    else
                    {
                        _stackScreen.Pop();
                    }
                }

                await screen.OnPopExit();
                Destroy(screen.gameObject);
            }
            await UniTask.Yield();
        }
        
        private void OnDestroy()
        {
            foreach (var item in _stackScreen)
                Destroy(item.gameObject);
            _stackScreen.Clear();
        }
    }
}