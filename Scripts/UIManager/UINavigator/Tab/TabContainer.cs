using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.Scripts.UIManager.UINavigator.Tab
{
    [RequireComponent(typeof(ToggleGroup))]
    public class TabContainer : MonoBehaviour
    {
        [SerializeField] private List<Tab> tabs = new();
        
        private int _initTab = 0;
        
        private void Start()
        {
            SetUp();
            SetInitTab();
            foreach (var tab in tabs)
            {
                tab.ToggleThisTab.onValueChanged.AddListener(delegate { OnChangeTab(tab); });
            }
            InitTab(_initTab);
        }

        protected virtual void SetInitTab()
        {
            _initTab = 0;
        }

        protected virtual void SetUp()
        {
            var toggleGroup = GetComponent<ToggleGroup>();
            foreach (var tab in tabs)
            {
                tab.ToggleThisTab.group = toggleGroup;
                tab.gameObject.SetActive(false);
            }

            tabs[0].gameObject.SetActive(true);
        }

        public void InitTab(int tabIndex)
        {
            tabs[tabIndex].ToggleThisTab.isOn = true;
        }

        protected async void OnChangeTab(Tab tab)
        {
            if (!tab.ToggleThisTab.isOn)
            {
                await tab.OnHide();
                tab.gameObject.SetActive(false);
            }
            else
            {
                tab.gameObject.SetActive(true);
                await tab.OnShow();
            }
        }
    }
}