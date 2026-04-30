using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
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
