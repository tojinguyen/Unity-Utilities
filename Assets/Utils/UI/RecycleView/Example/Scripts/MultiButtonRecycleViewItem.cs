using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    /// <summary>
    /// Example showing how to use multiple buttons in a single item for better performance
    /// </summary>
    public class MultiButtonRecycleViewItem : RecycleViewItem<TextMessageData>
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image backgroundImage;
        
        [Header("Action Buttons")]
        [SerializeField] private Button editButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button shareButton;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;

        private TextMessageData _currentData;

        public override void Initialize(RecycleView parent)
        {
            base.Initialize(parent);
            
            // Setup additional action buttons
            if (editButton != null)
            {
                editButton.onClick.RemoveAllListeners();
                editButton.onClick.AddListener(OnEditClicked);
            }
            
            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(OnDeleteClicked);
            }
            
            if (shareButton != null)
            {
                shareButton.onClick.RemoveAllListeners();
                shareButton.onClick.AddListener(OnShareClicked);
            }
        }

        public override void BindData(TextMessageData data, int index)
        {
            _currentData = data;
            messageText.text = $"{index}: {data.Message}";
            
            // Reset visual state
            if (backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }
        }

        // Main item click (entire item clickable area)
        protected override void OnItemClicked()
        {
            base.OnItemClicked();
            
            // Highlight item on main click
            if (backgroundImage != null)
            {
                backgroundImage.color = highlightColor;
            }
            
            Debug.Log($"Main item clicked: {_currentData?.Message}");
        }

        // Edit button specific action
        private void OnEditClicked()
        {
            Debug.Log($"Edit clicked for: {_currentData?.Message} at index {CurrentDataIndex}");
            
            // Example: Could open edit dialog, change to edit mode, etc.
            if (_currentData != null)
            {
                _currentData.Message += " [EDITED]";
                ParentRecycleView.RefreshItem(CurrentDataIndex);
            }
            
            // Notify parent about edit action
            // You could create custom events for this
        }

        // Delete button specific action  
        private void OnDeleteClicked()
        {
            Debug.Log($"Delete clicked for: {_currentData?.Message} at index {CurrentDataIndex}");
            
            // Example: Could show confirmation dialog, mark for deletion, etc.
            // Note: Don't actually delete from list here, let the controller handle it
            
            // Custom event could be triggered here
            // ParentRecycleView.OnItemDeleteRequested?.Invoke(this);
        }

        // Share button specific action
        private void OnShareClicked()
        {
            Debug.Log($"Share clicked for: {_currentData?.Message} at index {CurrentDataIndex}");
            
            // Example: Could open share menu, copy to clipboard, etc.
            
            // Custom event for share action
            // ParentRecycleView.OnItemShareRequested?.Invoke(this);
        }

        // Example method to enable/disable action buttons based on data
        public void SetActionsEnabled(bool canEdit, bool canDelete, bool canShare)
        {
            if (editButton != null) editButton.interactable = canEdit;
            if (deleteButton != null) deleteButton.interactable = canDelete;
            if (shareButton != null) shareButton.interactable = canShare;
        }

        // Example method to show/hide action buttons
        public void SetActionsVisible(bool visible)
        {
            if (editButton != null) editButton.gameObject.SetActive(visible);
            if (deleteButton != null) deleteButton.gameObject.SetActive(visible);
            if (shareButton != null) shareButton.gameObject.SetActive(visible);
        }
    }
}