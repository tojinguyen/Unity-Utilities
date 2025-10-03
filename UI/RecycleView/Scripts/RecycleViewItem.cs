using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Non-generic abstract base class for all UI items. 
    /// This is used by the core RecycleView system to manage items of different types.
    /// </summary>
    public abstract class RecycleViewItem : MonoBehaviour
    {
        [Header("Click Settings")]
        [Tooltip("Optional Button component for click handling. If not assigned, one will be created automatically.")]
        [SerializeField] protected Button clickButton;
        
        public RecycleView ParentRecycleView { get; internal set; }
        public int CurrentDataIndex { get; internal set; }

        public virtual void Initialize(RecycleView parent)
        {
            ParentRecycleView = parent;
            SetupClickHandler();
        }
        
        private void SetupClickHandler()
        {
            // Use assigned button if available, otherwise try to get or create one
            if (clickButton == null)
            {
                clickButton = GetComponent<Button>();
                if (clickButton == null)
                {
                    clickButton = gameObject.AddComponent<Button>();
                    // Set button to have no graphic so it's invisible but still clickable
                    clickButton.targetGraphic = null;
                }
            }
            
            // Remove existing listeners to avoid duplicates
            clickButton.onClick.RemoveAllListeners();
            // Add click listener
            clickButton.onClick.AddListener(NotifyItemClicked);
        }

        /// <summary>
        /// Non-generic method for the RecycleView to call. 
        /// This will be implemented by the generic child class.
        /// </summary>
        public abstract void BindData(IRecycleViewData data, int index);

        public void NotifyItemClicked()
        {
            OnItemClicked();
            ParentRecycleView.OnItemClicked?.Invoke(this);
        }

        /// <summary>
        /// Override this method to handle item click events.
        /// </summary>
        protected virtual void OnItemClicked()
        {
            // Default implementation does nothing
        }
    }

    /// <summary>
    /// Generic base class for UI items, providing a strongly-typed BindData method.
    /// Developers should inherit from this class.
    /// </summary>
    /// <typeparam name="TData">The type of data this item will display, must implement IRecycleViewData</typeparam>
    public abstract class RecycleViewItem<TData> : RecycleViewItem where TData : class, IRecycleViewData
    {
        /// <summary>
        /// Sealed override of the non-generic BindData. This handles the type casting.
        /// </summary>
        public sealed override void BindData(IRecycleViewData data, int index)
        {
            CurrentDataIndex = index;
            // Safe cast handled internally
            TData typedData = data as TData;
            if (typedData == null)
            {
                Debug.LogError($"Invalid data type passed to item. Expected {typeof(TData)} but got {data.GetType()} at index {index}", gameObject);
                return;
            }
            BindData(typedData, index);
        }

        /// <summary>
        /// Implement this in your concrete item class to update the UI elements with strongly-typed data.
        /// </summary>
        /// <param name="data">The strongly-typed data to display</param>
        /// <param name="index">The index of this data in the list</param>
        public abstract void BindData(TData data, int index);
    }
}
