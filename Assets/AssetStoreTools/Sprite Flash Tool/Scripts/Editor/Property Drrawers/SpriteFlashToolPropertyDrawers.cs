using UnityEngine;
using UnityEditor;
using System;

namespace SpriteFlashTools.PropertyAttributes
{
    [CustomPropertyDrawer(typeof(SpriteRendererNoneSelectedColorIndicator))]
    public class SpriteRendererConditionalPlusMessageBoxPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SpriteRendererNoneSelectedColorIndicator condHAtt = (SpriteRendererNoneSelectedColorIndicator)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);
            if (enabled)
            {
                Color apliedColor = Color.red;
                apliedColor.a = condHAtt.transparencyMultiplier;
                GUI.backgroundColor = apliedColor;
            }
            EditorGUI.PropertyField(position, property, label, true);
            if (enabled)
                GUI.backgroundColor = GUI.contentColor;
        }
        private bool GetConditionalHideAttributeResult(SpriteRendererNoneSelectedColorIndicator condHAtt, SerializedProperty property)
        {
            bool enabled;
            if (condHAtt.reverseCondition)
            {
                if (property.objectReferenceValue == false)
                    enabled = true;
                else
                    enabled = false;
            }
            else
            {
                if (property.objectReferenceValue == false)
                    enabled = false;
                else
                    enabled = true;
            }
            return enabled;
        }
    }


    [CustomPropertyDrawer(typeof(FloatMinAndMaxValue))]
    public class FloatMinAndMaxValuePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
            FloatMinAndMaxValue _attribure = (FloatMinAndMaxValue)attribute;
            if (property.floatValue < _attribure.minValue)
                property.floatValue = _attribure.minValue;
            if (property.floatValue > _attribure.maxValue)
                property.floatValue = _attribure.maxValue;
        }
    }

    [CustomPropertyDrawer(typeof(DoubleMinAndMaxValue))]
    public class DoubleMinAndMaxValuePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
            DoubleMinAndMaxValue _attribure = (DoubleMinAndMaxValue)attribute;
            if (property.doubleValue < _attribure.minValue)
                property.doubleValue = _attribure.minValue;
            if (property.doubleValue > _attribure.maxValue)
                property.doubleValue = _attribure.maxValue;
        }
    }

    [CustomPropertyDrawer(typeof(StringListPopup))]
    public class StringListPopupDrawer : PropertyDrawer
    {

        //public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        //	var stringInList = attribute as StringListPopup;
        //	var list = stringInList.list;
        //	if (property.propertyType == SerializedPropertyType.String) {
        //		int index = Mathf.Max (0, Array.IndexOf (list, property.stringValue));
        //		index = EditorGUI.Popup (position, property.displayName, index, list);
        //
        //		property.stringValue = list [index];
        //
        //
        //			
        //	} else {
        //		base.OnGUI (position, property, label);
        //	}
        //}

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            StringListPopup condHAtt = (StringListPopup)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

            bool wasEnabled = GUI.enabled;
            GUI.enabled = enabled;
            if (!condHAtt.hideInInspector || enabled)
            {
                var list = condHAtt.list;
                int index = Mathf.Max(0, Array.IndexOf(list, property.stringValue));
                index = EditorGUI.Popup(position, property.displayName, index, list);

                property.stringValue = list[index];



                //EditorGUI.PropertyField(position, property, label, true);
            }

            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            StringListPopup condHAtt = (StringListPopup)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

            if (!condHAtt.hideInInspector || enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private bool GetConditionalHideAttributeResult(StringListPopup condHAtt, SerializedProperty property)
        {
            bool enabled = true;
            string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
            string conditionPath = propertyPath.Replace(property.name, condHAtt.conditionalSourceField); //changes the path to the conditionalsource property path
            SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            if (sourcePropertyValue != null)
            {
                if (condHAtt.reverseCondition)
                {
                    if (sourcePropertyValue.stringValue != condHAtt.conditionalString)
                        enabled = true;
                    else
                        enabled = false;
                }
                else
                {
                    if (sourcePropertyValue.stringValue != condHAtt.conditionalString)
                        enabled = false;
                    else
                        enabled = true;
                }
            }
            else
            {
                //Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.conditionalSourceField);
            }
            return enabled;
        }


    }
}
