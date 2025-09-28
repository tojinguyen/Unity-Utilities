using UnityEngine;
using TirexGame.Utils.EventCenter;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Ví dụ đơn giản về cách sử dụng Event System
    /// Simple example of how to use the Event System
    /// </summary>
    public class SimpleEventExample : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableLogs = true;
        
        #region Event Definitions - Định nghĩa các Event
        
        // Event đơn giản - Simple event
        public struct PlayerJumped
        {
            public float JumpHeight;
            public Vector3 Position;
            
            public PlayerJumped(float height, Vector3 pos)
            {
                JumpHeight = height;
                Position = pos;
            }
        }
        
        // Event với nhiều thông tin - Event with more data
        public struct ItemCollected
        {
            public string ItemName;
            public int Points;
            public bool IsRare;
            
            public ItemCollected(string name, int points, bool rare)
            {
                ItemName = name;
                Points = points;
                IsRare = rare;
            }
        }
        
        // Event thông báo đơn giản - Simple notification event
        public struct GamePaused
        {
            public bool IsPaused;
            
            public GamePaused(bool paused)
            {
                IsPaused = paused;
            }
        }
        
        #endregion
        
        #region Unity Events
        
        private void Start()
        {
            // Debug EventSystem initialization
            Debug.Log("[SimpleExample] Starting initialization...");
            Debug.Log($"[SimpleExample] EventSystem.IsInitialized: {EventSystem.IsInitialized}");
            Debug.Log($"[SimpleExample] EventCenterService.IsAvailable: {EventCenterService.IsAvailable}");
            
            // Try to initialize explicitly if needed
            if (!EventSystem.IsInitialized)
            {
                Debug.Log("[SimpleExample] EventSystem not initialized, attempting to initialize...");
                EventSystem.Initialize();
            }
            
            // Try DIRECT subscription first to test
            TestDirectSubscription();
            
            SetupEventListeners();
            Log("✅ Event listeners đã được thiết lập!");
            
            // Test immediate event to verify system is working
            Debug.Log("[SimpleExample] Testing immediate event...");
            TestPlayerJump();
        }
        
        private void TestDirectSubscription()
        {
            Debug.Log("[SimpleExample] 🧪 Testing DIRECT subscription (no extension methods)...");
            
            try
            {
                // Direct subscription without extension methods
                var subscription = EventSystem.Subscribe<PlayerJumped>(OnDirectPlayerJumped);
                Debug.Log("[SimpleExample] ✅ Direct subscription created successfully!");
                
                // Store subscription for cleanup later
                if (directSubscription != null)
                {
                    directSubscription.Dispose();
                }
                directSubscription = subscription;
                
                Debug.Log("[SimpleExample] Direct subscription stored for testing");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimpleExample] ❌ Direct subscription failed: {ex.Message}");
                Debug.LogError($"[SimpleExample] Stack trace: {ex.StackTrace}");
            }
        }
        
        private IEventSubscription directSubscription;
        
        private void OnDirectPlayerJumped(PlayerJumped jumpEvent)
        {
            Debug.Log($"[SimpleExample] 🎯🎯🎯 OnDirectPlayerJumped CALLED! Height: {jumpEvent.JumpHeight:F1}m at {jumpEvent.Position}");
            Debug.Log($"[SimpleExample] 🎯🎯🎯 THIS PROVES DIRECT SUBSCRIPTION WORKS!");
        }
        
        private void Update()
        {
            // Nhấn phím để test events - Press keys to test events
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TestPlayerJump();
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                TestItemCollection();
            }
            
            if (Input.GetKeyDown(KeyCode.P))
            {
                TestGamePause();
            }
        }
        
        #endregion
        
        #region Event Setup - Thiết lập Events
        
        private void SetupEventListeners()
        {
            Debug.Log("[SimpleExample] Setting up event listeners...");
            
            // Check EventSystem status before subscribing
            Debug.Log($"[SimpleExample] EventSystem.IsInitialized: {EventSystem.IsInitialized}");
            Debug.Log($"[SimpleExample] EventCenterService.IsAvailable: {EventCenterService.IsAvailable}");
            Debug.Log($"[SimpleExample] EventCenterService.Current: {EventCenterService.Current}");
            
            try
            {
                // 🎯 SỬ DỤNG EXTENSION METHODS - TỰ ĐỘNG CLEANUP KHI OBJECT BỊ HỦY!
                // 🎯 USING EXTENSION METHODS - AUTO CLEANUP WHEN OBJECT IS DESTROYED!
                
                // 1. Đăng ký lắng nghe event PlayerJumped (tự động unsubscribe)
                // Subscribe to PlayerJumped event (auto unsubscribe)
                Debug.Log("[SimpleExample] Subscribing to PlayerJumped event...");
                this.SubscribeWithCleanup<PlayerJumped>(OnPlayerJumped);
                Debug.Log("[SimpleExample] ✅ PlayerJumped subscription completed");
                
                // 2. Đăng ký lắng nghe event ItemCollected (tự động unsubscribe)
                // Subscribe to ItemCollected event (auto unsubscribe)
                Debug.Log("[SimpleExample] Subscribing to ItemCollected event...");
                this.SubscribeWithCleanup<ItemCollected>(OnItemCollected);
                Debug.Log("[SimpleExample] ✅ ItemCollected subscription completed");
                
                // 3. Đăng ký lắng nghe event GamePaused (tự động unsubscribe)
                // Subscribe to GamePaused event (auto unsubscribe)
                Debug.Log("[SimpleExample] Subscribing to GamePaused event...");
                this.SubscribeWithCleanup<GamePaused>(OnGamePaused);
                Debug.Log("[SimpleExample] ✅ GamePaused subscription completed");
                
                // 4. Ví dụ về conditional subscription - chỉ lắng nghe item hiếm (tự động unsubscribe)
                // Example of conditional subscription - only listen to rare items (auto unsubscribe)
                Debug.Log("[SimpleExample] Subscribing to rare ItemCollected events...");
                this.SubscribeWhenWithCleanup<ItemCollected>(OnRareItemCollected, 
                    item => item.IsRare);
                Debug.Log("[SimpleExample] ✅ Rare ItemCollected subscription completed");
                
                // 5. Ví dụ về one-time subscription - chỉ lắng nghe lần đầu (tự động unsubscribe)
                // Example of one-time subscription - only listen once (auto unsubscribe)
                Debug.Log("[SimpleExample] Subscribing to first PlayerJumped event...");
                this.SubscribeOnceWithCleanup<PlayerJumped>(OnFirstJump);
                Debug.Log("[SimpleExample] ✅ First PlayerJumped subscription completed");
                
                Debug.Log("[SimpleExample] ✅ All event subscriptions completed successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimpleExample] ❌ Error setting up event listeners: {ex.Message}");
                Debug.LogError($"[SimpleExample] ❌ Stack trace: {ex.StackTrace}");
            }
            
            // 💡 KHÔNG CẦN GỌI UNSUBSCRIBE TRONG OnDestroy NỮA!
            // 💡 NO NEED TO CALL UNSUBSCRIBE IN OnDestroy ANYMORE!
        }
        
        #endregion
        
        #region Event Handlers - Xử lý Events
        
        private void OnPlayerJumped(PlayerJumped jumpEvent)
        {
            Debug.Log($"[SimpleExample] OnPlayerJumped called - Height: {jumpEvent.JumpHeight:F1}m");
            Log($"🦘 Player nhảy cao {jumpEvent.JumpHeight:F1}m tại vị trí {jumpEvent.Position}");
        }
        
        private void OnItemCollected(ItemCollected itemEvent)
        {
            Debug.Log($"[SimpleExample] OnItemCollected called - Item: {itemEvent.ItemName}");
            Log($"📦 Thu thập {itemEvent.ItemName} (+{itemEvent.Points} điểm)");
            
            if (itemEvent.IsRare)
            {
                Log($"⭐ Wow! {itemEvent.ItemName} là item hiếm!");
            }
        }
        
        private void OnGamePaused(GamePaused pauseEvent)
        {
            if (pauseEvent.IsPaused)
            {
                Log("⏸️ Game đã tạm dừng");
            }
            else
            {
                Log("▶️ Game tiếp tục");
            }
        }
        
        private void OnRareItemCollected(ItemCollected rareItem)
        {
            Log($"💎 [Special] Thu thập item hiếm: {rareItem.ItemName}!");
        }
        
        private void OnFirstJump(PlayerJumped firstJump)
        {
            Log($"🎯 [Tutorial] Lần nhảy đầu tiên! Cao {firstJump.JumpHeight:F1}m");
        }
        
        #endregion
        
        #region Test Methods - Phương thức Test
        
        private void TestPlayerJump()
        {
            Debug.Log("[SimpleExample] TestPlayerJump called");
            
            // Check EventSystem status before publishing
            Debug.Log($"[SimpleExample] Before publish - EventSystem.IsInitialized: {EventSystem.IsInitialized}");
            Debug.Log($"[SimpleExample] Before publish - EventCenterService.IsAvailable: {EventCenterService.IsAvailable}");
            
            var jumpHeight = Random.Range(1f, 5f);
            var position = transform.position + Random.insideUnitSphere * 2f;
            
            var jumpEvent = new PlayerJumped(jumpHeight, position);
            Debug.Log($"[SimpleExample] Publishing PlayerJumped event with height: {jumpHeight:F1}m");
            
            try
            {
                EventSystem.Publish(jumpEvent);
                Debug.Log("[SimpleExample] PlayerJumped event published successfully");
                
                // Wait a frame and check if event was processed
                StartCoroutine(CheckEventProcessing());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimpleExample] ❌ Error publishing event: {ex.Message}");
                Debug.LogError($"[SimpleExample] ❌ Stack trace: {ex.StackTrace}");
            }
        }
        
        private System.Collections.IEnumerator CheckEventProcessing()
        {
            yield return null; // Wait one frame
            Debug.Log("[SimpleExample] Checking if event was processed after one frame...");
        }
        
        private void TestItemCollection()
        {
            Debug.Log("[SimpleExample] TestItemCollection called");
            var items = new[] { "Coin", "Gem", "Potion", "Key", "Crystal" };
            var randomItem = items[Random.Range(0, items.Length)];
            var points = Random.Range(10, 100);
            var isRare = Random.Range(0, 100) < 20; // 20% chance for rare item
            
            var itemEvent = new ItemCollected(randomItem, points, isRare);
            Debug.Log($"[SimpleExample] Publishing ItemCollected event - Item: {randomItem}, Rare: {isRare}");
            EventSystem.Publish(itemEvent);
            Debug.Log("[SimpleExample] ItemCollected event published");
        }
        
        private void TestGamePause()
        {
            var currentPauseState = Time.timeScale == 0f;
            var newPauseState = !currentPauseState;
            
            Time.timeScale = newPauseState ? 0f : 1f;
            
            var pauseEvent = new GamePaused(newPauseState);
            EventSystem.Publish(pauseEvent);
        }
        
        #endregion
        
        #region Context Menu - Menu ngữ cảnh
        
        [ContextMenu("Test Player Jump")]
        private void MenuTestJump()
        {
            TestPlayerJump();
        }
        
        [ContextMenu("Test Item Collection")]
        private void MenuTestItem()
        {
            TestItemCollection();
        }
        
        [ContextMenu("Test Rare Item")]
        private void MenuTestRareItem()
        {
            var rareEvent = new ItemCollected("Dragon Sword", 1000, true);
            EventSystem.Publish(rareEvent);
        }
        
        [ContextMenu("Test Batch Events")]
        private void MenuTestBatch()
        {
            var events = new ItemCollected[]
            {
                new ItemCollected("Coin", 10, false),
                new ItemCollected("Gem", 50, false),
                new ItemCollected("Rare Crystal", 500, true)
            };
            
            EventSystem.PublishBatch(events);
            Log($"📦 Đã publish {events.Length} events cùng lúc!");
        }
        
        #endregion
        
        #region GUI - Giao diện test
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            // Increase GUI area size and make buttons bigger
            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            
            // Style for larger buttons
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fixedHeight = 40
            };
            
            var headerStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14
            };
            
            GUILayout.Label("🎮 Simple Event System Example", headerStyle);
            
            GUILayout.Space(10);
            
            GUILayout.Label("Nhấn phím để test:", labelStyle);
            GUILayout.Label("SPACE - Player Jump", labelStyle);
            GUILayout.Label("C - Collect Item", labelStyle);
            GUILayout.Label("P - Pause/Unpause", labelStyle);
            
            GUILayout.Space(15);
            
            if (GUILayout.Button("🦘 Test Jump", buttonStyle))
            {
                TestPlayerJump();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("📦 Test Item", buttonStyle))
            {
                TestItemCollection();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("⏸️ Toggle Pause", buttonStyle))
            {
                TestGamePause();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("💎 Test Rare Item", buttonStyle))
            {
                var rareEvent = new ItemCollected("Legendary Sword", 1000, true);
                EventSystem.Publish(rareEvent);
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
        
        #region Utility
        
        private void Log(string message)
        {
            if (enableLogs)
            {
                Debug.Log($"[SimpleExample] {message}");
            }
        }
        
        [ContextMenu("Debug Direct Subscribe Test")]
        private void DebugDirectSubscribeTest()
        {
            Debug.Log("[SimpleExample] 🧪 Testing direct EventSystem subscription...");
            
            try
            {
                // Test direct subscription without extension methods
                var subscription = EventSystem.Subscribe<PlayerJumped>(DirectPlayerJumpedHandler);
                Debug.Log("[SimpleExample] ✅ Direct subscription created successfully");
                
                // Test publish
                var testEvent = new PlayerJumped(999f, Vector3.zero);
                Debug.Log("[SimpleExample] Publishing test event directly...");
                EventSystem.Publish(testEvent);
                Debug.Log("[SimpleExample] Test event published");
                
                // Clean up
                subscription?.Dispose();
                Debug.Log("[SimpleExample] Direct subscription disposed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimpleExample] ❌ Direct subscribe test failed: {ex.Message}");
                Debug.LogError($"[SimpleExample] ❌ Stack trace: {ex.StackTrace}");
            }
        }
        
        private void DirectPlayerJumpedHandler(PlayerJumped jumpEvent)
        {
            Debug.Log($"[SimpleExample] 🎯 DirectPlayerJumpedHandler called! Height: {jumpEvent.JumpHeight:F1}m");
        }
        
        [ContextMenu("Force Initialize EventSystem")]
        private void ForceInitializeEventSystem()
        {
            Debug.Log("[SimpleExample] 🔄 Force initializing EventSystem...");
            
            try
            {
                EventSystem.Initialize();
                Debug.Log($"[SimpleExample] EventSystem.IsInitialized: {EventSystem.IsInitialized}");
                Debug.Log($"[SimpleExample] EventCenterService.IsAvailable: {EventCenterService.IsAvailable}");
                Debug.Log($"[SimpleExample] EventCenterService.Current: {EventCenterService.Current}");
                
                if (EventCenterService.Current != null)
                {
                    Debug.Log("[SimpleExample] ✅ EventSystem force initialized successfully!");
                }
                else
                {
                    Debug.LogError("[SimpleExample] ❌ EventSystem initialization failed - Current is null");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimpleExample] ❌ Force initialization failed: {ex.Message}");
                Debug.LogError($"[SimpleExample] ❌ Stack trace: {ex.StackTrace}");
            }
        }
        
        // 🎉 KHÔNG CẦN OnDestroy NỮA! EXTENSION METHODS SẼ TỰ ĐỘNG CLEANUP!
        // 🎉 NO MORE OnDestroy NEEDED! EXTENSION METHODS WILL AUTO CLEANUP!
        // private void OnDestroy() { /* No longer needed! */ }
        
        private void OnDestroy()
        {
            // Clean up direct subscription for testing
            if (directSubscription != null)
            {
                directSubscription.Dispose();
                Debug.Log("[SimpleExample] Direct subscription cleaned up in OnDestroy");
            }
        }
        
        #endregion
    }
}