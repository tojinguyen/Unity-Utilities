using UnityEngine;
using TirexGame.Utils.UI;

namespace TirexGame.Utils.UI.Examples
{
    /// <summary>
    /// Example script demonstrating basic floating text usage
    /// Attach this to any GameObject and click in Play mode
    /// </summary>
    public class FloatingTextExample : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool useWorldPosition = true;
        [SerializeField] private Vector3 spawnOffset = Vector3.up;

        [Header("Test Keys")]
        [SerializeField] private KeyCode damageKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode healingKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode criticalKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode missKey = KeyCode.Alpha4;
        [SerializeField] private KeyCode experienceKey = KeyCode.Alpha5;
        [SerializeField] private KeyCode goldKey = KeyCode.Alpha6;
        [SerializeField] private KeyCode customKey = KeyCode.Alpha7;
        [SerializeField] private KeyCode spamKey = KeyCode.Alpha8;

        private void Update()
        {
            Vector3 spawnPosition = transform.position + spawnOffset;

            // Basic preset examples
            if (Input.GetKeyDown(damageKey))
            {
                ShowDamageExample(spawnPosition);
            }

            if (Input.GetKeyDown(healingKey))
            {
                ShowHealingExample(spawnPosition);
            }

            if (Input.GetKeyDown(criticalKey))
            {
                ShowCriticalExample(spawnPosition);
            }

            if (Input.GetKeyDown(missKey))
            {
                ShowMissExample(spawnPosition);
            }

            if (Input.GetKeyDown(experienceKey))
            {
                ShowExperienceExample(spawnPosition);
            }

            if (Input.GetKeyDown(goldKey))
            {
                ShowGoldExample(spawnPosition);
            }

            if (Input.GetKeyDown(customKey))
            {
                ShowCustomExample(spawnPosition);
            }

            if (Input.GetKeyDown(spamKey))
            {
                ShowSpamExample(spawnPosition);
            }

            // Mouse click example
            if (Input.GetMouseButtonDown(0))
            {
                ShowAtMousePosition();
            }
        }

        /// <summary>
        /// Example: Show damage text
        /// </summary>
        private void ShowDamageExample(Vector3 position)
        {
            float damage = Random.Range(10, 100);
            FloatingTextFactory.Damage(damage, position, !useWorldPosition);
            Debug.Log($"Showing damage: {damage}");
        }

        /// <summary>
        /// Example: Show healing text
        /// </summary>
        private void ShowHealingExample(Vector3 position)
        {
            float healing = Random.Range(5, 50);
            FloatingTextFactory.Healing(healing, position, !useWorldPosition);
            Debug.Log($"Showing healing: {healing}");
        }

        /// <summary>
        /// Example: Show critical hit text
        /// </summary>
        private void ShowCriticalExample(Vector3 position)
        {
            float damage = Random.Range(50, 200);
            FloatingTextFactory.Critical(damage, position, !useWorldPosition);
            Debug.Log($"Showing critical: {damage}");
        }

        /// <summary>
        /// Example: Show miss text
        /// </summary>
        private void ShowMissExample(Vector3 position)
        {
            FloatingTextFactory.Miss(position, !useWorldPosition);
            Debug.Log("Showing miss");
        }

        /// <summary>
        /// Example: Show experience gained
        /// </summary>
        private void ShowExperienceExample(Vector3 position)
        {
            int xp = Random.Range(10, 100);
            FloatingTextFactory.Experience(xp, position, !useWorldPosition);
            Debug.Log($"Showing XP: {xp}");
        }

        /// <summary>
        /// Example: Show gold collected
        /// </summary>
        private void ShowGoldExample(Vector3 position)
        {
            int gold = Random.Range(10, 100);
            FloatingTextFactory.Gold(gold, position, !useWorldPosition);
            Debug.Log($"Showing gold: {gold}");
        }

        /// <summary>
        /// Example: Show custom text using builder pattern
        /// </summary>
        private void ShowCustomExample(Vector3 position)
        {
            FloatingTextFactory.Builder()
                .SetText("★ EPIC! ★")
                .SetPosition(position)
                .WithColor(new Color(1f, 0.5f, 1f)) // Purple
                .WithFontSize(72)
                .WithSpeed(4f)
                .WithLifetime(2f)
                .WithRandomDirection(45f)
                .Bold()
                .Show();

            Debug.Log("Showing custom text");
        }

        /// <summary>
        /// Example: Show multiple texts at once (stress test)
        /// </summary>
        private void ShowSpamExample(Vector3 position)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(0f, 2f),
                    Random.Range(-2f, 2f)
                );

                float damage = Random.Range(1, 50);
                FloatingTextFactory.Damage(damage, position + randomOffset, !useWorldPosition);
            }

            Debug.Log("Showing 20 floating texts (spam test)");
        }

        /// <summary>
        /// Example: Show text at mouse position
        /// </summary>
        private void ShowAtMousePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Hit something in 3D space
                FloatingTextFactory.Damage(Random.Range(10, 100), hit.point, !useWorldPosition);
            }
            else
            {
                // Show at screen position
                FloatingTextFactory.Builder()
                    .SetText("Click!")
                    .SetPosition(Input.mousePosition)
                    .AtScreenPosition()
                    .WithColor(Color.cyan)
                    .WithFontSize(40)
                    .Show();
            }
        }

        private void OnDrawGizmos()
        {
            // Visualize spawn position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + spawnOffset, 0.2f);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("Floating Text Examples", GUI.skin.box);
            GUILayout.Label($"1 - Show Damage ({damageKey})");
            GUILayout.Label($"2 - Show Healing ({healingKey})");
            GUILayout.Label($"3 - Show Critical ({criticalKey})");
            GUILayout.Label($"4 - Show Miss ({missKey})");
            GUILayout.Label($"5 - Show Experience ({experienceKey})");
            GUILayout.Label($"6 - Show Gold ({goldKey})");
            GUILayout.Label($"7 - Show Custom ({customKey})");
            GUILayout.Label($"8 - Spam Test ({spamKey})");
            GUILayout.Label($"Mouse Click - Show at cursor");
            GUILayout.Space(10);
            GUILayout.Label($"Mode: {(useWorldPosition ? "3D" : "2D")}");
            GUILayout.EndArea();
        }
    }
}
