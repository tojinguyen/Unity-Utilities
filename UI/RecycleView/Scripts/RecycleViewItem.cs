using UnityEngine;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Abstract base class for all UI items that can be displayed in a RecycleView.
    /// </summary>
    public abstract class RecycleViewItem : MonoBehaviour
    {
        /// <summary>
        /// A reference to the parent RecycleView that manages this item.
        /// </summary>
        public RecycleView ParentRecycleView { get; private set; }

        /// <summary>
        /// The data index this item currently represents in the master list.
        /// </summary>
        public int CurrentDataIndex { get; private set; }

        /// <summary>
        /// Initializes the item with a reference to its parent view.
        /// This is called by the RecycleView when the item is first instantiated.
        /// </summary>
        /// <param name="parent">The parent RecycleView</param>
        public virtual void Initialize(RecycleView parent)
        {
            ParentRecycleView = parent;
        }

        /// <summary>
        /// This method is called by the RecycleView when this item needs to display new data.
        /// Implement this in your concrete item class to update the UI elements.
        /// </summary>
        /// <param name="data">The data to display</param>
        /// <param name="index">The index of this data in the list</param>
        public abstract void BindData(IRecycleViewData data, int index);

        /// <summary>
        /// A helper method to notify the parent RecycleView that this item was clicked.
        /// You can call this from a Button's OnClick event in the prefab.
        /// </summary>
        public void NotifyItemClicked()
        {
            ParentRecycleView.OnItemClicked?.Invoke(this);
        }
    }
}
