using UnityEngine;
using TMPro;

namespace TirexGame.Utils.UI
{
    [CreateAssetMenu(fileName = "FloatingTextData", menuName = "TirexGame/UI/Floating Text Data", order = 1)]
    public class FloatingTextData : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Direction the text moves in")]
        public Vector3 MoveDirection = new Vector3(0, 1, 0);

        [Tooltip("Speed of movement in units per second")]
        [Range(0f, 10f)]
        public float MoveSpeed = 2f;

        [Tooltip("How long the text stays visible")]
        [Range(0.1f, 10f)]
        public float Lifetime = 1.5f;

        [Header("Randomization")]
        [Tooltip("Add random variation to movement direction")]
        public bool RandomizeDirection = false;

        [Tooltip("Maximum angle deviation when randomizing (degrees)")]
        [Range(0f, 180f)]
        public float RandomAngle = 30f;

        [Header("Animation Curves")]
        [Tooltip("Scale animation over lifetime (0 to 1)")]
        public AnimationCurve ScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);

        [Tooltip("Alpha/opacity animation over lifetime (0 to 1)")]
        public AnimationCurve AlphaCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [Header("Text Appearance")]
        [Tooltip("Font size")]
        [Range(10f, 200f)]
        public float FontSize = 36f;

        [Tooltip("Text color")]
        public Color TextColor = Color.white;

        [Tooltip("Font style")]
        public FontStyles FontStyle = FontStyles.Normal;

        [Header("Advanced")]
        [Tooltip("Use world space for 3D or screen space for UI")]
        public bool UseWorldSpace = false;

        /// <summary>
        /// Create a copy of this data with modifications
        /// </summary>
        public FloatingTextData Clone()
        {
            FloatingTextData clone = CreateInstance<FloatingTextData>();
            clone.MoveDirection = MoveDirection;
            clone.MoveSpeed = MoveSpeed;
            clone.Lifetime = Lifetime;
            clone.RandomizeDirection = RandomizeDirection;
            clone.RandomAngle = RandomAngle;
            clone.ScaleCurve = new AnimationCurve(ScaleCurve.keys);
            clone.AlphaCurve = new AnimationCurve(AlphaCurve.keys);
            clone.FontSize = FontSize;
            clone.TextColor = TextColor;
            clone.FontStyle = FontStyle;
            clone.UseWorldSpace = UseWorldSpace;
            return clone;
        }

        /// <summary>
        /// Create default data for damage text (red, moves upward)
        /// </summary>
        public static FloatingTextData CreateDamageDefault()
        {
            FloatingTextData data = CreateInstance<FloatingTextData>();
            data.MoveDirection = new Vector3(0, 1, 0);
            data.MoveSpeed = 2f;
            data.Lifetime = 1.5f;
            data.FontSize = 36f;
            data.TextColor = Color.red;
            data.FontStyle = FontStyles.Bold;
            data.RandomizeDirection = true;
            data.RandomAngle = 30f;
            data.ScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
            data.AlphaCurve = AnimationCurve.Linear(0, 1, 1, 0);
            return data;
        }

        /// <summary>
        /// Create default data for healing text (green, moves upward)
        /// </summary>
        public static FloatingTextData CreateHealingDefault()
        {
            FloatingTextData data = CreateInstance<FloatingTextData>();
            data.MoveDirection = new Vector3(0, 1, 0);
            data.MoveSpeed = 2f;
            data.Lifetime = 1.5f;
            data.FontSize = 36f;
            data.TextColor = Color.green;
            data.FontStyle = FontStyles.Bold;
            data.RandomizeDirection = true;
            data.RandomAngle = 30f;
            data.ScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
            data.AlphaCurve = AnimationCurve.Linear(0, 1, 1, 0);
            return data;
        }

        /// <summary>
        /// Create default data for critical hit text (yellow, larger, moves upward faster)
        /// </summary>
        public static FloatingTextData CreateCriticalDefault()
        {
            FloatingTextData data = CreateInstance<FloatingTextData>();
            data.MoveDirection = new Vector3(0, 1, 0);
            data.MoveSpeed = 3f;
            data.Lifetime = 2f;
            data.FontSize = 48f;
            data.TextColor = Color.yellow;
            data.FontStyle = FontStyles.Bold;
            data.RandomizeDirection = true;
            data.RandomAngle = 40f;
            data.ScaleCurve = AnimationCurve.EaseInOut(0, 1.2f, 1, 1);
            data.AlphaCurve = AnimationCurve.Linear(0, 1, 1, 0);
            return data;
        }
    }
}
