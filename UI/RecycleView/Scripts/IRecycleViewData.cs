
namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Interface for data objects to be displayed in the RecycleView.
    /// Each data object must provide an ItemType identifier.
    /// </summary>
    public interface IRecycleViewData
    {
        /// <summary>
        /// The type of UI item this data represents. This should correspond to a
        /// registered item type in the RecycleView's settings.
        /// </summary>
        int ItemType { get; }
    }
}
