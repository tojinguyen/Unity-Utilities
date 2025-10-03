using UnityEngine;
using TirexGame.Utils.Data;

namespace TirexGame.Utils.Data.Example
{
    /// <summary>
    /// Test script to verify DataManager initialization order
    /// This script should run AFTER DataManager is initialized
    /// </summary>
    public class DataManagerInitializationTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool testOnAwake = true;
        [SerializeField] private bool testOnStart = true;
        
        private void Awake()
        {
            if (testOnAwake)
            {
                Debug.Log($"[InitTest] Testing DataManager access in Awake() at {System.DateTime.Now:HH:mm:ss.fff}");
                TestDataManagerAccess("Awake");
            }
        }
        
        private void Start()
        {
            if (testOnStart)
            {
                Debug.Log($"[InitTest] Testing DataManager access in Start() at {System.DateTime.Now:HH:mm:ss.fff}");
                TestDataManagerAccess("Start");
            }
        }
        
        private void TestDataManagerAccess(string phase)
        {
            try
            {
                // This should trigger DataManager initialization if not already done
                var config = new DataManagerConfig { EnableLogging = true };
                DataManager.Initialize(config);
                
                Debug.Log($"[InitTest] ✅ DataManager access successful in {phase}()");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InitTest] ❌ DataManager access failed in {phase}(): {ex.Message}");
            }
        }
        
        [ContextMenu("Test DataManager Status")]
        public void TestDataManagerStatus()
        {
            Debug.Log($"[InitTest] Manual test at {System.DateTime.Now:HH:mm:ss.fff}");
            TestDataManagerAccess("Manual");
        }
    }
}