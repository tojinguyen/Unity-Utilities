using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public virtual async UniTask OnBeforeShow() => await UniTask.Yield();
    public virtual async UniTask OnDoneShow() => await UniTask.Yield();
    public virtual async UniTask OnBeforeHide() => await UniTask.Yield();
    public virtual async UniTask OnAfterHide() => await UniTask.Yield();
}