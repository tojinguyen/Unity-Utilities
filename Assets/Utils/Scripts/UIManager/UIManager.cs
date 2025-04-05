using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

internal class UIManager : MonoBehaviour, IUIManager
{
    private static readonly string FeatureName = "UIManagerFeature";
    
    [SerializeField] private Transform _screenRoot;
    [SerializeField] private Transform _popupRoot;
    [SerializeField] private Transform _toastRoot;
    [SerializeField] private GameObject _backdropPrefab;

    private GameObject _backdrop;
    private readonly Stack<PopupBase> _popupStack = new();

    public async UniTask<T> ShowUI<T>(string address) where T : UIBase
    {
        var prefab = await AddressablesHelper.GetAssetAsync<GameObject>(new AssetReference(address), FeatureName);
        var parent = GetParentFor(typeof(T));
        var go = Instantiate(prefab, parent);
        var ui = go.GetComponent<T>();

        await ui.OnBeforeShow();
        go.SetActive(true);
        
        if (ui is PopupBase popup)
        {
            _popupStack.Push(popup);
            if (_backdrop == null)
                _backdrop = Instantiate(_backdropPrefab, _popupRoot);
        }
        
        await ui.OnDoneShow();
        
        return ui;
    }

    public async UniTask Pop()
    {
        if (_popupStack.Count == 0) return;
        var popup = _popupStack.Pop();

        await popup.OnBeforeHide();
        Destroy(popup.gameObject);
        await popup.OnAfterHide();

        if (_popupStack.Count == 0 && _backdrop != null)
            Destroy(_backdrop);
    }

    public UIBase Peek()
    {
        return _popupStack.Count > 0 ? _popupStack.Peek() : null;
    }

    private Transform GetParentFor(Type type)
    {
        if (typeof(ScreenBase).IsAssignableFrom(type)) return _screenRoot;
        if (typeof(PopupBase).IsAssignableFrom(type)) return _popupRoot;
        if (typeof(ToastBase).IsAssignableFrom(type)) return _toastRoot;
        return transform;
    }
}