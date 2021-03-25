using UnityEditor;

namespace ToucanSystems
{
    [CustomEditor(typeof(Aesthetico))]
    [CanEditMultipleObjects]
    public class AestheticoEditor : Editor
    {
        private bool enableColoringEditor;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Aesthetico aesthetico = (Aesthetico)target;

            EditorGUI.BeginChangeCheck();

            DrawPixelizationBox();

            DrawColorMapBox();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawPixelizationBox()
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Pixelation", EditorStyles.boldLabel);

            SerializedProperty enablePixelation = serializedObject.FindProperty("enablePixelation");
            EditorGUILayout.PropertyField(enablePixelation);

            if (enablePixelation.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                SerializedProperty pixelCountX = serializedObject.FindProperty("pixelCountX");
                EditorGUILayout.PropertyField(pixelCountX, true);

                SerializedProperty forceSquarePixels = serializedObject.FindProperty("forceSquarePixels");

                if (!forceSquarePixels.boolValue)
                {
                    SerializedProperty pixelCountY = serializedObject.FindProperty("pixelCountY");
                    EditorGUILayout.PropertyField(pixelCountY, true);
                }
                EditorGUILayout.PropertyField(forceSquarePixels, true);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void DrawColorMapBox()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Coloring", EditorStyles.boldLabel);

            SerializedProperty enableColoring = serializedObject.FindProperty("enableColoring");
            EditorGUILayout.PropertyField(enableColoring, true);

            if (enableColoring.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                SerializedProperty coloringIntensity = serializedObject.FindProperty("coloringIntensity");
                EditorGUILayout.PropertyField(coloringIntensity, true);

                SerializedProperty colorsRange = serializedObject.FindProperty("colorsRange");
                EditorGUILayout.PropertyField(colorsRange, true);

                SerializedProperty colorCorrection = serializedObject.FindProperty("colorCorrection");
                EditorGUILayout.PropertyField(colorCorrection, true);

                SerializedProperty colorsMap = serializedObject.FindProperty("colorsMap");
                EditorGUILayout.PropertyField(colorsMap, true);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }
    }
}
