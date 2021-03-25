using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpriteFlashTools.PropertyAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
       AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class SpriteRendererNoneSelectedColorIndicator : PropertyAttribute
    {
        public bool reverseCondition = false;
        public float transparencyMultiplier = 1f;

        public SpriteRendererNoneSelectedColorIndicator(bool _reverseCondition = false, float _transparencyMultiplier = 1f)
        {
            this.transparencyMultiplier = _transparencyMultiplier;
            this.reverseCondition = _reverseCondition;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
       AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class FloatMinAndMaxValue : PropertyAttribute
    {
        public float minValue = 1f;
        public float maxValue = 1f;

        public FloatMinAndMaxValue(float _minValue = Mathf.NegativeInfinity, float _maxValue = Mathf.Infinity)
        {
            this.minValue = _minValue;
            this.maxValue = _maxValue;
        }
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
       AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class DoubleMinAndMaxValue : PropertyAttribute
    {
        public double minValue = 1f;
        public double maxValue = 1f;

        public DoubleMinAndMaxValue(double _minValue = Mathf.NegativeInfinity, double _maxValue = Mathf.Infinity)
        {
            this.minValue = _minValue;
            this.maxValue = _maxValue;
        }
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class StringListPopup : PropertyAttribute
    {

        public string[] list;
        public string conditionalSourceField;
        public string conditionalString;
        public bool hideInInspector;
        public bool reverseCondition;


        public StringListPopup(string[] _list)
        {
            this.list = _list;
        }

        public StringListPopup(string _conditionalSourceField, string _conditionalString, bool _hideInInspector = true, bool _reverseCondition = false, params string[] _list)
        {
            this.list = _list;
            this.conditionalSourceField = _conditionalSourceField;
            this.conditionalString = _conditionalString;
            this.hideInInspector = _hideInInspector;
            this.reverseCondition = _reverseCondition;
        }
    }


}
