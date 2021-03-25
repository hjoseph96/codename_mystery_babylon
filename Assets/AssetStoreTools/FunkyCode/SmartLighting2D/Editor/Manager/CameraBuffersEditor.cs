using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Buffers))]
public class CameraBuffersEditor : Editor {
	static bool cameraFoldout = false;
	static bool lightFoldout = false;

	override public void OnInspectorGUI() {
		Buffers script = target as Buffers;

		cameraFoldout = EditorGUILayout.Foldout(cameraFoldout, "Cameras");

		if (cameraFoldout) {

			EditorGUI.indentLevel++;

			foreach(LightMainBuffer2D buffer in LightMainBuffer2D.List) {
				EditorGUILayout.ObjectField("Camera Target", buffer.cameraSettings.GetCamera(), typeof(Camera), true);
				
				EditorGUILayout.EnumPopup("Camera Type", buffer.cameraSettings.cameraType);
				EditorGUILayout.EnumPopup("Render Mode", buffer.cameraSettings.renderMode);
				EditorGUILayout.EnumPopup("Render Shader", buffer.cameraSettings.renderShader);
				EditorGUILayout.ObjectField("Render Texture", buffer.renderTexture.renderTexture, typeof(Texture), true);

				EditorGUILayout.Space();
			}

			EditorGUI.indentLevel--;

			EditorGUILayout.Space();

		}

		lightFoldout = EditorGUILayout.Foldout(lightFoldout, "Lights");

		if (lightFoldout) {

			EditorGUI.indentLevel++;

			foreach(LightBuffer2D buffer in LightBuffer2D.List) {
				EditorGUILayout.LabelField(buffer.name);
				EditorGUILayout.ObjectField("Lighting Source", buffer.Light, typeof(Light2D), true);

				EditorGUILayout.Toggle("Is Free", buffer.Free);

				EditorGUILayout.ObjectField("Render Texture", buffer.renderTexture.renderTexture, typeof(Texture), true);

				if (buffer.collisionTexture == null) {
					EditorGUILayout.ObjectField("Collision Texture (null)", null, typeof(Texture), true);
				} else {
					EditorGUILayout.ObjectField("Collision Texture", buffer.collisionTexture.renderTexture, typeof(Texture), true);
				}

				EditorGUILayout.Space();
			}

			EditorGUI.indentLevel--;

			EditorGUILayout.Space();
		}


		EditorGUILayout.Foldout(true, "Internal");
		EditorGUI.indentLevel++;

		EditorGUILayout.ObjectField("Mask Material", Lighting2D.materials.GetMask(), typeof(Material), true);

		EditorGUI.indentLevel--;
		
	}
}