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
            SetupEventListeners();
            Log("✅ Event listeners đã được thiết lập!");
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
            // 1. Đăng ký lắng nghe event PlayerJumped
            // Subscribe to PlayerJumped event
            EventSystem.Subscribe<PlayerJumped>(OnPlayerJumped);
            
            // 2. Đăng ký lắng nghe event ItemCollected
            // Subscribe to ItemCollected event  
            EventSystem.Subscribe<ItemCollected>(OnItemCollected);
            
            // 3. Đăng ký lắng nghe event GamePaused
            // Subscribe to GamePaused event
            EventSystem.Subscribe<GamePaused>(OnGamePaused);
            
            // 4. Ví dụ về conditional subscription - chỉ lắng nghe item hiếm
            // Example of conditional subscription - only listen to rare items
            EventSystem.SubscribeWhen<ItemCollected>(OnRareItemCollected, 
                item => item.IsRare);
            
            // 5. Ví dụ về one-time subscription - chỉ lắng nghe lần đầu
            // Example of one-time subscription - only listen once
            EventSystem.SubscribeOnce<PlayerJumped>(OnFirstJump);
        }
        
        #endregion
        
        #region Event Handlers - Xử lý Events
        
        private void OnPlayerJumped(PlayerJumped jumpEvent)
        {
            Log($"🦘 Player nhảy cao {jumpEvent.JumpHeight:F1}m tại vị trí {jumpEvent.Position}");
        }
        
        private void OnItemCollected(ItemCollected itemEvent)
        {
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
            var jumpHeight = Random.Range(1f, 5f);
            var position = transform.position + Random.insideUnitSphere * 2f;
            
            var jumpEvent = new PlayerJumped(jumpHeight, position);
            EventSystem.Publish(jumpEvent);
        }
        
        private void TestItemCollection()
        {
            var items = new[] { "Coin", "Gem", "Potion", "Key", "Crystal" };
            var randomItem = items[Random.Range(0, items.Length)];
            var points = Random.Range(10, 100);
            var isRare = Random.Range(0, 100) < 20; // 20% chance for rare item
            
            var itemEvent = new ItemCollected(randomItem, points, isRare);
            EventSystem.Publish(itemEvent);
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
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("🎮 Simple Event System Example", GUI.skin.box);
            
            GUILayout.Label("Nhấn phím để test:");
            GUILayout.Label("SPACE - Player Jump");
            GUILayout.Label("C - Collect Item");
            GUILayout.Label("P - Pause/Unpause");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("🦘 Test Jump"))
            {
                TestPlayerJump();
            }
            
            if (GUILayout.Button("📦 Test Item"))
            {
                TestItemCollection();
            }
            
            if (GUILayout.Button("⏸️ Toggle Pause"))
            {
                TestGamePause();
            }
            
            if (GUILayout.Button("💎 Test Rare Item"))
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
        
        // Cleanup khi object bị destroy
        // Cleanup when object is destroyed
        private void OnDestroy()
        {
            // Tự động unsubscribe khi object bị destroy
            // Automatically unsubscribe when object is destroyed
            // (EventSystem sẽ tự động cleanup, nhưng best practice là explicit unsubscribe)
            // (EventSystem will auto cleanup, but best practice is explicit unsubscribe)
        }
        
        #endregion
    }
}