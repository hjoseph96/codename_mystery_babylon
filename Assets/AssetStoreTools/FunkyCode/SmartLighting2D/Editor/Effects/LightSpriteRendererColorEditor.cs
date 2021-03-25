using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CanEditMultipleObjects]
[CustomEditor(typeof(LightSpriteRendererColor))]
public class LightSpriteRendererColorEditor : Editor {

	override public void OnInspectorGUI() {
		LightSpriteRendererColor script = target as LightSpriteRendererColor;

		//script.nightLayer = (LightingLayer)EditorGUILayout.Popup("Layer (Night)", (int)script.nightLayer, Lighting2D.Profile.layers.nightLayers.GetNames());

        script.color = EditorGUILayout.ColorField("Color", script.color);

		if (GUI.changed){
            if (EditorApplication.isPlaying == false) {
                EditorUtility.SetDirty(target);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
		}
	}
}