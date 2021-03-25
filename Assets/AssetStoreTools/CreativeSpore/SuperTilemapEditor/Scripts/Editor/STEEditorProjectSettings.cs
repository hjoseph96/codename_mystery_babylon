using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HutongGames.PlayMakerEditor
{
	[InitializeOnLoad]
	public static class STEEditorProjectSettings
	{
		static STEEditorProjectSettings()
		{
            AddAlwaysIncludedShader("Sprites/Stencil-Default");
            AddAlwaysIncludedShader("Sprites/Stencil-Diffuse");
        }

        //ref: https://forum.unity.com/threads/modify-always-included-shaders-with-pre-processor.509479/#post-3509413
        public static bool AddAlwaysIncludedShader(string shaderName)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogErrorFormat("{0} shader not found!", shaderName);
                return false;
            }

            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            var serializedObject = new SerializedObject(graphicsSettingsObj);
            var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
            for (int i = 0; i < arrayProp.arraySize; ++i)
            {
                var arrayElem = arrayProp.GetArrayElementAtIndex(i);
                if (shader == arrayElem.objectReferenceValue)
                {
                    return false;
                }
            }

            int arrayIndex = arrayProp.arraySize;
            arrayProp.arraySize++;
            arrayProp.GetArrayElementAtIndex(arrayIndex).objectReferenceValue = shader;

            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();

            Debug.LogFormat("{0} shader successfully added to GraphicsSettings", shaderName);

            return true;
        }
    }
}
