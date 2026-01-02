using UnityEngine;
using System.Collections.Generic;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Factory for creating floating texts with predefined styles and easy access
    /// </summary>
    public static class FloatingTextFactory
    {
        // Cached data presets
        private static readonly Dictionary<FloatingTextType, FloatingTextData> dataCache = new Dictionary<FloatingTextType, FloatingTextData>();

        /// <summary>
        /// Predefined floating text types
        /// </summary>
        public enum FloatingTextType
        {
            Damage,
            Healing,
            Critical,
            Miss,
            Experience,
            Gold,
            Custom
        }

        /// <summary>
        /// Create floating text using a preset type
        /// </summary>
        public static FloatingTextBase Create(string text, Vector3 position, FloatingTextType type, bool is3D = false)
        {
            FloatingTextData data = GetDataForType(type);

            if (is3D)
            {
                return FloatingTextManager.Instance.ShowText3D(text, position, data);
            }
            else
            {
                return FloatingTextManager.Instance.ShowTextAtWorldPosition(text, position, data);
            }
        }

        /// <summary>
        /// Create floating text at screen position
        /// </summary>
        public static FloatingTextBase CreateAtScreenPosition(string text, Vector3 screenPosition, FloatingTextType type)
        {
            FloatingTextData data = GetDataForType(type);
            return FloatingTextManager.Instance.ShowText2D(text, screenPosition, data);
        }

        /// <summary>
        /// Quick methods for common floating text types
        /// </summary>
        public static FloatingTextBase Damage(float amount, Vector3 position, bool is3D = false)
        {
            return Create(amount.ToString("0"), position, FloatingTextType.Damage, is3D);
        }

        public static FloatingTextBase Healing(float amount, Vector3 position, bool is3D = false)
        {
            return Create("+" + amount.ToString("0"), position, FloatingTextType.Healing, is3D);
        }

        public static FloatingTextBase Critical(float amount, Vector3 position, bool is3D = false)
        {
            return Create(amount.ToString("0") + "!", position, FloatingTextType.Critical, is3D);
        }

        public static FloatingTextBase Miss(Vector3 position, bool is3D = false)
        {
            return Create("MISS", position, FloatingTextType.Miss, is3D);
        }

        public static FloatingTextBase Experience(int amount, Vector3 position, bool is3D = false)
        {
            return Create("+" + amount + " XP", position, FloatingTextType.Experience, is3D);
        }

        public static FloatingTextBase Gold(int amount, Vector3 position, bool is3D = false)
        {
            return Create("+" + amount + " Gold", position, FloatingTextType.Gold, is3D);
        }

        /// <summary>
        /// Get or create data configuration for a specific type
        /// </summary>
        public static FloatingTextData GetDataForType(FloatingTextType type)
        {
            if (dataCache.TryGetValue(type, out FloatingTextData cachedData))
            {
                return cachedData;
            }

            FloatingTextData data = CreateDataForType(type);
            dataCache[type] = data;
            return data;
        }

        /// <summary>
        /// Create data configuration for a specific type
        /// </summary>
        private static FloatingTextData CreateDataForType(FloatingTextType type)
        {
            switch (type)
            {
                case FloatingTextType.Damage:
                    return FloatingTextData.CreateDamageDefault();

                case FloatingTextType.Healing:
                    return FloatingTextData.CreateHealingDefault();

                case FloatingTextType.Critical:
                    return FloatingTextData.CreateCriticalDefault();

                case FloatingTextType.Miss:
                    var missData = ScriptableObject.CreateInstance<FloatingTextData>();
                    missData.MoveDirection = new Vector3(0.5f, 1, 0);
                    missData.MoveSpeed = 1.5f;
                    missData.Lifetime = 1f;
                    missData.RandomizeDirection = false;
                    missData.FontSize = 32f;
                    missData.TextColor = new Color(0.7f, 0.7f, 0.7f);
                    missData.FontStyle = TMPro.FontStyles.Italic;
                    missData.ScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.8f);
                    missData.AlphaCurve = AnimationCurve.Linear(0, 1, 1, 0);
                    return missData;

                case FloatingTextType.Experience:
                    var xpData = ScriptableObject.CreateInstance<FloatingTextData>();
                    xpData.MoveDirection = new Vector3(0, 1, 0);
                    xpData.MoveSpeed = 2.5f;
                    xpData.Lifetime = 2f;
                    xpData.RandomizeDirection = false;
                    xpData.FontSize = 36f;
                    xpData.TextColor = new Color(0.5f, 0.5f, 1f); // Light blue
                    xpData.FontStyle = TMPro.FontStyles.Bold;
                    xpData.ScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
                    xpData.AlphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
                    return xpData;

                case FloatingTextType.Gold:
                    var goldData = ScriptableObject.CreateInstance<FloatingTextData>();
                    goldData.MoveDirection = new Vector3(0, 1, 0);
                    goldData.MoveSpeed = 2f;
                    goldData.Lifetime = 2f;
                    goldData.RandomizeDirection = false;
                    goldData.FontSize = 36f;
                    goldData.TextColor = new Color(1f, 0.84f, 0f); // Gold color
                    goldData.FontStyle = TMPro.FontStyles.Bold;
                    goldData.ScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
                    goldData.AlphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
                    return goldData;

                default:
                    return FloatingTextData.CreateDamageDefault();
            }
        }

        /// <summary>
        /// Register custom data configuration for a type
        /// </summary>
        public static void RegisterCustomData(FloatingTextType type, FloatingTextData data)
        {
            dataCache[type] = data;
        }

        /// <summary>
        /// Clear cached data
        /// </summary>
        public static void ClearCache()
        {
            dataCache.Clear();
        }

        /// <summary>
        /// Create builder for custom floating text
        /// </summary>
        public static FloatingTextBuilder Builder()
        {
            return new FloatingTextBuilder();
        }
    }

    /// <summary>
    /// Builder pattern for creating custom floating text with fluent API
    /// </summary>
    public class FloatingTextBuilder
    {
        private string text = "";
        private Vector3 position = Vector3.zero;
        private bool is3D = false;
        private bool useScreenPosition = false;
        private FloatingTextData data;

        public FloatingTextBuilder()
        {
            data = ScriptableObject.CreateInstance<FloatingTextData>();
        }

        public FloatingTextBuilder SetText(string value)
        {
            text = value;
            return this;
        }

        public FloatingTextBuilder SetPosition(Vector3 pos)
        {
            position = pos;
            return this;
        }

        public FloatingTextBuilder As3D()
        {
            is3D = true;
            useScreenPosition = false;
            return this;
        }

        public FloatingTextBuilder As2D()
        {
            is3D = false;
            return this;
        }

        public FloatingTextBuilder AtScreenPosition()
        {
            useScreenPosition = true;
            is3D = false;
            return this;
        }

        public FloatingTextBuilder WithDirection(Vector3 direction)
        {
            data.MoveDirection = direction;
            return this;
        }

        public FloatingTextBuilder WithSpeed(float speed)
        {
            data.MoveSpeed = speed;
            return this;
        }

        public FloatingTextBuilder WithLifetime(float lifetime)
        {
            data.Lifetime = lifetime;
            return this;
        }

        public FloatingTextBuilder WithColor(Color color)
        {
            data.TextColor = color;
            return this;
        }

        public FloatingTextBuilder WithFontSize(float size)
        {
            data.FontSize = size;
            return this;
        }

        public FloatingTextBuilder WithRandomDirection(float angle)
        {
            data.RandomizeDirection = true;
            data.RandomAngle = angle;
            return this;
        }

        public FloatingTextBuilder WithScaleCurve(AnimationCurve curve)
        {
            data.ScaleCurve = curve;
            return this;
        }

        public FloatingTextBuilder WithAlphaCurve(AnimationCurve curve)
        {
            data.AlphaCurve = curve;
            return this;
        }

        public FloatingTextBuilder WithData(FloatingTextData customData)
        {
            data = customData;
            return this;
        }

        public FloatingTextBuilder Bold()
        {
            data.FontStyle = TMPro.FontStyles.Bold;
            return this;
        }

        public FloatingTextBuilder Italic()
        {
            data.FontStyle = TMPro.FontStyles.Italic;
            return this;
        }

        /// <summary>
        /// Build and show the floating text
        /// </summary>
        public FloatingTextBase Show()
        {
            if (useScreenPosition)
            {
                return FloatingTextManager.Instance.ShowText2D(text, position, data);
            }
            else if (is3D)
            {
                return FloatingTextManager.Instance.ShowText3D(text, position, data);
            }
            else
            {
                return FloatingTextManager.Instance.ShowTextAtWorldPosition(text, position, data);
            }
        }
    }
}
