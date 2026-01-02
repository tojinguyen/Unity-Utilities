using UnityEngine;
using TirexGame.Utils.UI;

namespace TirexGame.Utils.UI.Examples
{
    /// <summary>
    /// Example of integrating floating text with a combat system
    /// Shows damage, healing, and combat-related text
    /// </summary>
    public class CombatSystemExample : MonoBehaviour
    {
        [Header("Combat Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float criticalChance = 0.2f;
        [SerializeField] private float criticalMultiplier = 2f;
        [SerializeField] private float dodgeChance = 0.1f;

        [Header("Visual Settings")]
        [SerializeField] private Vector3 textOffset = new Vector3(0, 2, 0);
        [SerializeField] private bool use3DText = false;

        [Header("Auto-Attack Settings")]
        [SerializeField] private bool autoAttack = false;
        [SerializeField] private float attackInterval = 1.5f;
        private float nextAttackTime;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (autoAttack && Time.time >= nextAttackTime)
            {
                TakeDamage(Random.Range(10f, 30f));
                nextAttackTime = Time.time + attackInterval;
            }

            // Manual controls
            if (Input.GetKeyDown(KeyCode.D))
            {
                TakeDamage(Random.Range(10f, 50f));
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                Heal(Random.Range(10f, 30f));
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                TakeDamage(Random.Range(50f, 100f), forceCritical: true);
            }
        }

        /// <summary>
        /// Take damage and show appropriate floating text
        /// </summary>
        public void TakeDamage(float damage, bool forceCritical = false)
        {
            // Check for dodge
            if (!forceCritical && Random.value < dodgeChance)
            {
                ShowDodge();
                return;
            }

            // Check for critical hit
            bool isCritical = forceCritical || Random.value < criticalChance;
            if (isCritical)
            {
                damage *= criticalMultiplier;
            }

            // Apply damage
            currentHealth = Mathf.Max(0, currentHealth - damage);

            // Show floating text
            Vector3 position = transform.position + textOffset;
            if (isCritical)
            {
                FloatingTextFactory.Critical(damage, position, use3DText);

                // Add extra effect for critical
                ShowExtraEffect("CRITICAL!", position + Vector3.up * 0.5f, Color.yellow);
            }
            else
            {
                FloatingTextFactory.Damage(damage, position, use3DText);
            }

            // Check for death
            if (currentHealth <= 0)
            {
                ShowDeath();
            }

            Debug.Log($"Took {damage} damage. Health: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Heal and show floating text
        /// </summary>
        public void Heal(float amount)
        {
            if (currentHealth >= maxHealth)
            {
                ShowExtraEffect("Full Health!", transform.position + textOffset, Color.cyan);
                return;
            }

            float actualHealing = Mathf.Min(amount, maxHealth - currentHealth);
            currentHealth += actualHealing;

            Vector3 position = transform.position + textOffset;
            FloatingTextFactory.Healing(actualHealing, position, use3DText);

            Debug.Log($"Healed {actualHealing}. Health: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Show dodge/miss effect
        /// </summary>
        private void ShowDodge()
        {
            Vector3 position = transform.position + textOffset;
            FloatingTextFactory.Miss(position, use3DText);
            Debug.Log("Dodged!");
        }

        /// <summary>
        /// Show death effect
        /// </summary>
        private void ShowDeath()
        {
            Vector3 position = transform.position + textOffset;

            FloatingTextFactory.Builder()
                .SetText("DEFEATED")
                .SetPosition(position)
                .WithColor(new Color(0.5f, 0, 0))
                .WithFontSize(60)
                .WithSpeed(1f)
                .WithLifetime(3f)
                .Bold()
                .Show();

            Debug.Log("Defeated!");
        }

        /// <summary>
        /// Show custom effect text
        /// </summary>
        private void ShowExtraEffect(string text, Vector3 position, Color color)
        {
            FloatingTextFactory.Builder()
                .SetText(text)
                .SetPosition(position)
                .WithColor(color)
                .WithFontSize(32)
                .WithSpeed(1.5f)
                .WithLifetime(1.5f)
                .Show();
        }

        /// <summary>
        /// Public method to apply buff/debuff text
        /// </summary>
        public void ShowStatusEffect(string effectName, bool isBuff)
        {
            Color color = isBuff ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f);
            string prefix = isBuff ? "+" : "-";

            FloatingTextFactory.Builder()
                .SetText(prefix + effectName)
                .SetPosition(transform.position + textOffset + Vector3.up * 0.5f)
                .WithColor(color)
                .WithFontSize(28)
                .WithSpeed(1f)
                .WithLifetime(2f)
                .Italic()
                .Show();

            Debug.Log($"Status effect: {effectName} (Buff: {isBuff})");
        }

        /// <summary>
        /// Show level up effect
        /// </summary>
        public void ShowLevelUp(int newLevel)
        {
            Vector3 position = transform.position + textOffset;

            FloatingTextFactory.Builder()
                .SetText($"LEVEL {newLevel}!")
                .SetPosition(position)
                .WithColor(Color.yellow)
                .WithFontSize(64)
                .WithSpeed(2f)
                .WithLifetime(2.5f)
                .WithRandomDirection(30f)
                .Bold()
                .Show();

            Debug.Log($"Level up to {newLevel}!");
        }

        /// <summary>
        /// Show combo text
        /// </summary>
        public void ShowCombo(int comboCount)
        {
            float sizeMultiplier = 1f + (comboCount * 0.1f);
            Color color = Color.Lerp(Color.white, Color.red, comboCount / 10f);

            FloatingTextFactory.Builder()
                .SetText($"x{comboCount} COMBO")
                .SetPosition(transform.position + textOffset + Vector3.right)
                .WithColor(color)
                .WithFontSize(40 * sizeMultiplier)
                .WithSpeed(2f)
                .WithLifetime(1.5f)
                .Bold()
                .Show();

            Debug.Log($"Combo: x{comboCount}");
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize text spawn position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + textOffset, 0.3f);
        }

        private void OnGUI()
        {
            if (!autoAttack)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 200));
                GUILayout.Label("Combat Example Controls", GUI.skin.box);
                GUILayout.Label("D - Take Damage");
                GUILayout.Label("H - Heal");
                GUILayout.Label("K - Critical Hit");
                GUILayout.Space(10);
                GUILayout.Label($"Health: {currentHealth:F0}/{maxHealth:F0}");

                if (GUILayout.Button("Enable Auto-Attack"))
                {
                    autoAttack = true;
                }

                GUILayout.EndArea();
            }
            else
            {
                GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 150));
                GUILayout.Label("Combat Example - Auto Mode", GUI.skin.box);
                GUILayout.Label($"Health: {currentHealth:F0}/{maxHealth:F0}");

                if (GUILayout.Button("Disable Auto-Attack"))
                {
                    autoAttack = false;
                }

                if (GUILayout.Button("Reset Health"))
                {
                    currentHealth = maxHealth;
                }

                GUILayout.EndArea();
            }
        }
    }
}
