using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Interface định nghĩa Strategy Pattern cho animation của Loading UI.
    /// Implement interface này trong project của bạn để tạo animation hoàn toàn custom.
    /// 
    /// Example (trong project game của bạn):
    /// <code>
    /// public class DOTweenLoadingAnimation : MonoBehaviour, ILoadingAnimationStrategy
    /// {
    ///     public async UniTask PlayShowAnimation(GameObject target, CancellationToken ct)
    ///     {
    ///         await target.transform.DOScale(Vector3.one, 0.3f).ToUniTask(cancellationToken: ct);
    ///     }
    ///     public async UniTask PlayHideAnimation(GameObject target, CancellationToken ct)
    ///     {
    ///         await target.transform.DOScale(Vector3.zero, 0.3f).ToUniTask(cancellationToken: ct);
    ///     }
    ///     public void PlayIdleAnimation(GameObject target) { }
    ///     public void StopIdleAnimation(GameObject target) { }
    /// }
    /// </code>
    /// </summary>
    public interface ILoadingAnimationStrategy
    {
        /// <summary>
        /// Animation khi Loading UI xuất hiện (Show).
        /// Được gọi khi bắt đầu loading.
        /// </summary>
        /// <param name="target">GameObject chứa Loading UI</param>
        /// <param name="ct">CancellationToken để hủy animation nếu cần</param>
        UniTask PlayShowAnimation(GameObject target, CancellationToken ct);

        /// <summary>
        /// Animation khi Loading UI biến mất (Hide).
        /// Được gọi khi loading hoàn thành.
        /// </summary>
        /// <param name="target">GameObject chứa Loading UI</param>
        /// <param name="ct">CancellationToken để hủy animation nếu cần</param>
        UniTask PlayHideAnimation(GameObject target, CancellationToken ct);

        /// <summary>
        /// Bắt đầu animation idle (lặp liên tục) trong khi loading đang chạy.
        /// Ví dụ: spinner xoay, pulse effect...
        /// Có thể bỏ trống nếu không cần idle animation.
        /// </summary>
        /// <param name="target">GameObject chứa Loading UI</param>
        void PlayIdleAnimation(GameObject target);

        /// <summary>
        /// Dừng idle animation.
        /// Được gọi trước khi Play Hide Animation.
        /// </summary>
        /// <param name="target">GameObject chứa Loading UI</param>
        void StopIdleAnimation(GameObject target);
    }
}
