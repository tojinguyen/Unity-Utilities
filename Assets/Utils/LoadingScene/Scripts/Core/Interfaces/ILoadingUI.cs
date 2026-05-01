using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.LoadingScene
{
    public interface ILoadingUI
    {
        UniTask ShowUI();
        void HideUI();
        void SetProgress(float progress);
    }
}
