using UnityEngine;
using System.Collections;
using TirexGame.Utils.UI;

namespace TirexGame.Utils.UI.Examples
{
    /// <summary>
    /// Advanced example showing various floating text effects and techniques
    /// Demonstrates animations, chaining, and creative uses
    /// </summary>
    public class AdvancedFloatingTextExample : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode chainKey = KeyCode.C;
        [SerializeField] private KeyCode waveKey = KeyCode.W;
        [SerializeField] private KeyCode explosionKey = KeyCode.E;
        [SerializeField] private KeyCode countdownKey = KeyCode.T;
        [SerializeField] private KeyCode rainKey = KeyCode.R;

        private bool isAnimating = false;

        private void Update()
        {
            if (Input.GetKeyDown(chainKey) && !isAnimating)
            {
                StartCoroutine(ShowChainEffect());
            }

            if (Input.GetKeyDown(waveKey) && !isAnimating)
            {
                StartCoroutine(ShowWaveEffect());
            }

            if (Input.GetKeyDown(explosionKey))
            {
                ShowExplosionEffect();
            }

            if (Input.GetKeyDown(countdownKey) && !isAnimating)
            {
                StartCoroutine(ShowCountdownEffect());
            }

            if (Input.GetKeyDown(rainKey) && !isAnimating)
            {
                StartCoroutine(ShowRainEffect());
            }
        }

        /// <summary>
        /// Chain effect - texts appear one after another
        /// </summary>
        private IEnumerator ShowChainEffect()
        {
            isAnimating = true;

            string[] messages = { "First!", "Second!", "Third!", "Done!" };

            for (int i = 0; i < messages.Length; i++)
            {
                Vector3 position = transform.position + Vector3.up * (i * 0.5f);

                FloatingTextFactory.Builder()
                    .SetText(messages[i])
                    .SetPosition(position)
                    .WithColor(Color.Lerp(Color.red, Color.green, i / (float)messages.Length))
                    .WithFontSize(40)
                    .WithSpeed(2f)
                    .Show();

                yield return new WaitForSeconds(0.3f);
            }

            isAnimating = false;
        }

        /// <summary>
        /// Wave effect - texts appear in a wave pattern
        /// </summary>
        private IEnumerator ShowWaveEffect()
        {
            isAnimating = true;

            int count = 10;
            float radius = 3f;

            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                Vector3 position = transform.position + offset;

                FloatingTextFactory.Builder()
                    .SetText("~")
                    .SetPosition(position)
                    .WithColor(new Color(0.5f, 0.5f, 1f))
                    .WithFontSize(48)
                    .WithSpeed(3f)
                    .WithLifetime(2f)
                    .Show();

                yield return new WaitForSeconds(0.1f);
            }

            isAnimating = false;
        }

        /// <summary>
        /// Explosion effect - texts burst outward
        /// </summary>
        private void ShowExplosionEffect()
        {
            int count = 20;
            Vector3 center = transform.position;

            for (int i = 0; i < count; i++)
            {
                // Random direction
                Vector3 direction = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized;

                // Create custom data with random direction
                FloatingTextData data = ScriptableObject.CreateInstance<FloatingTextData>();
                data.MoveDirection = direction;
                data.MoveSpeed = Random.Range(3f, 6f);
                data.Lifetime = Random.Range(1f, 2f);
                data.TextColor = new Color(
                    Random.Range(0.8f, 1f),
                    Random.Range(0.3f, 0.6f),
                    Random.Range(0f, 0.2f)
                );
                data.FontSize = Random.Range(30f, 50f);
                data.ScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
                data.AlphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

                string[] symbols = { "*", "★", "✦", "○", "●" };
                string text = symbols[Random.Range(0, symbols.Length)];

                FloatingTextManager.Instance.ShowText3D(text, center, data);
            }
        }

        /// <summary>
        /// Countdown effect - counts down from 3
        /// </summary>
        private IEnumerator ShowCountdownEffect()
        {
            isAnimating = true;

            for (int i = 3; i > 0; i--)
            {
                FloatingTextFactory.Builder()
                    .SetText(i.ToString())
                    .SetPosition(transform.position)
                    .WithColor(i == 1 ? Color.red : Color.yellow)
                    .WithFontSize(80 + (4 - i) * 20)
                    .WithSpeed(0.5f)
                    .WithLifetime(1f)
                    .Bold()
                    .Show();

                yield return new WaitForSeconds(1f);
            }

            // GO!
            FloatingTextFactory.Builder()
                .SetText("GO!")
                .SetPosition(transform.position)
                .WithColor(Color.green)
                .WithFontSize(100)
                .WithSpeed(3f)
                .WithLifetime(1.5f)
                .Bold()
                .Show();

            isAnimating = false;
        }

        /// <summary>
        /// Rain effect - texts fall from above
        /// </summary>
        private IEnumerator ShowRainEffect()
        {
            isAnimating = true;

            for (int i = 0; i < 30; i++)
            {
                Vector3 position = transform.position + new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(3f, 5f),
                    Random.Range(-5f, 5f)
                );

                FloatingTextData data = ScriptableObject.CreateInstance<FloatingTextData>();
                data.MoveDirection = Vector3.down;
                data.MoveSpeed = Random.Range(2f, 4f);
                data.Lifetime = Random.Range(2f, 3f);
                data.TextColor = new Color(0.5f, 0.5f, 1f, 0.8f);
                data.FontSize = 30f;
                data.ScaleCurve = AnimationCurve.Linear(0, 1, 1, 1);
                data.AlphaCurve = AnimationCurve.Linear(0, 1, 1, 0);

                FloatingTextManager.Instance.ShowText3D("💧", position, data);

                yield return new WaitForSeconds(0.1f);
            }

            isAnimating = false;
        }

        /// <summary>
        /// Helper method to show spiral pattern
        /// </summary>
        public void ShowSpiralEffect()
        {
            StartCoroutine(SpiralCoroutine());
        }

        private IEnumerator SpiralCoroutine()
        {
            isAnimating = true;

            int count = 50;
            float radius = 0.5f;
            float height = 5f;

            for (int i = 0; i < count; i++)
            {
                float progress = i / (float)count;
                float angle = progress * Mathf.PI * 6f; // 3 rotations
                float currentHeight = progress * height;

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * radius,
                    currentHeight,
                    Mathf.Sin(angle) * radius
                );

                Vector3 position = transform.position + offset;

                FloatingTextFactory.Builder()
                    .SetText("●")
                    .SetPosition(position)
                    .WithColor(Color.HSVToRGB(progress, 1f, 1f))
                    .WithFontSize(30)
                    .WithSpeed(1f)
                    .WithLifetime(3f)
                    .Show();

                yield return new WaitForSeconds(0.05f);
            }

            isAnimating = false;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 210, 400, 200));
            GUILayout.Label("Advanced Examples", GUI.skin.box);

            if (!isAnimating)
            {
                GUILayout.Label($"C - Chain Effect ({chainKey})");
                GUILayout.Label($"W - Wave Effect ({waveKey})");
                GUILayout.Label($"E - Explosion Effect ({explosionKey})");
                GUILayout.Label($"T - Countdown Effect ({countdownKey})");
                GUILayout.Label($"R - Rain Effect ({rainKey})");

                if (GUILayout.Button("Spiral Effect"))
                {
                    ShowSpiralEffect();
                }
            }
            else
            {
                GUILayout.Label("Animation in progress...");
            }

            GUILayout.EndArea();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
