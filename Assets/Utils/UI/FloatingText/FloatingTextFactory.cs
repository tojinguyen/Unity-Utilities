using UnityEngine;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Factory for creating floating texts using builder pattern or direct creation
    /// </summary>
    public static class FloatingTextFactory
    {
        /// <summary>
        /// Create floating text with custom data configuration
        /// </summary>
        public static FloatingTextBase Create(string text, Vector3 position, FloatingTextData data, bool is3D = false)
        {
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
        /// Create floating text at screen position with custom data configuration
        /// </summary>
        public static FloatingTextBase CreateAtScreenPosition(string text, Vector3 screenPosition, FloatingTextData data)
        {
            return FloatingTextManager.Instance.ShowText2D(text, screenPosition, data);
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
