
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
        
        /// <summary>
        /// Optional: Custom height for this specific item. 
        /// If not implemented or returns <= 0, the RecycleView will use the default height for the item type.
        /// </summary>
        float CustomHeight => -1f;
    }
}
