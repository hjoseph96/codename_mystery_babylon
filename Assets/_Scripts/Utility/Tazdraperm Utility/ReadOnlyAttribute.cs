using System;
using UnityEngine;

namespace Tazdraperm.Utility
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyInspectorAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
    public class ReadOnlyAttributeDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect rect, UnityEditor.SerializedProperty prop, GUIContent label)
        {
            var wasEnabled = GUI.enabled;
            GUI.enabled = false;
            UnityEditor.EditorGUI.PropertyField(rect, prop);
            GUI.enabled = wasEnabled;
        }
    }
#endif
}
