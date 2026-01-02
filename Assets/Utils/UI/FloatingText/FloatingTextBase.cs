using UnityEngine;
using TMPro;
using System;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Base class for floating text with common animation and lifecycle logic
    /// </summary>
    public abstract class FloatingTextBase : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] protected Vector3 moveDirection = new Vector3(0, 1, 0);
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float lifetime = 1.5f;
        [SerializeField] protected AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
        [SerializeField] protected AnimationCurve alphaCurve = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] protected bool randomizeDirection = false;
        [SerializeField] protected float randomAngle = 30f;

        protected float elapsedTime;
        protected Vector3 startPosition;
        protected Vector3 startScale;
        protected Color startColor;
        protected bool isActive;
        protected Action<FloatingTextBase> onComplete;

        /// <summary>
        /// Initialize and show the floating text
        /// </summary>
        public virtual void Show(string text, Vector3 position, FloatingTextData data = null, Action<FloatingTextBase> completionCallback = null)
        {
            gameObject.SetActive(true);
            isActive = true;
            elapsedTime = 0f;
            onComplete = completionCallback;

            // Apply data if provided
            if (data != null)
            {
                ApplyData(data);
            }

            // Set position
            SetPosition(position);
            startPosition = position;
            startScale = transform.localScale;

            // Apply random direction if enabled
            Vector3 direction = moveDirection;
            if (randomizeDirection && randomAngle > 0)
            {
                float angle = UnityEngine.Random.Range(-randomAngle, randomAngle);
                direction = Quaternion.Euler(0, 0, angle) * moveDirection;
            }
            moveDirection = direction.normalized;

            // Set text and get initial color
            SetText(text);
            startColor = GetTextColor();
        }

        /// <summary>
        /// Apply floating text data configuration
        /// </summary>
        protected virtual void ApplyData(FloatingTextData data)
        {
            moveDirection = data.MoveDirection;
            moveSpeed = data.MoveSpeed;
            lifetime = data.Lifetime;
            scaleCurve = data.ScaleCurve;
            alphaCurve = data.AlphaCurve;
            randomizeDirection = data.RandomizeDirection;
            randomAngle = data.RandomAngle;

            SetFontSize(data.FontSize);
            SetColor(data.TextColor);
            SetFontStyle(data.FontStyle);
        }

        protected virtual void Update()
        {
            if (!isActive) return;

            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / lifetime);

            // Update position
            UpdatePosition(moveSpeed * Time.deltaTime);

            // Update scale
            float scaleMultiplier = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = startScale * scaleMultiplier;

            // Update alpha
            float alpha = alphaCurve.Evaluate(normalizedTime);
            Color currentColor = startColor;
            currentColor.a = alpha;
            SetColor(currentColor);

            // Check if completed
            if (elapsedTime >= lifetime)
            {
                Complete();
            }
        }

        /// <summary>
        /// Complete and hide the floating text
        /// </summary>
        protected virtual void Complete()
        {
            isActive = false;
            gameObject.SetActive(false);
            onComplete?.Invoke(this);
        }

        // Abstract methods to be implemented by derived classes
        protected abstract void SetPosition(Vector3 position);
        protected abstract void UpdatePosition(float deltaMovement);
        protected abstract void SetText(string text);
        protected abstract Color GetTextColor();
        public abstract void SetColor(Color color);
        public abstract void SetFontSize(float size);
        public abstract void SetFontStyle(FontStyles style);
        public abstract TMP_Text GetTextComponent();
    }
}
