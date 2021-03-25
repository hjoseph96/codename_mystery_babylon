using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(LightingManager2D))]
public class LightingManager2DEditor : Editor {
	static bool[] cameraFoldout = new bool[10];

	static bool[] cameraMaterialsFoldout = new bool[10];

	override public void OnInspectorGUI() {
		LightingManager2D script = target as LightingManager2D;

		LightingSettings.Profile newProfile = (LightingSettings.Profile)EditorGUILayout.ObjectField("Profile", script.setProfile, typeof(LightingSettings.Profile), true);
		if (newProfile != script.setProfile) {
			script.setProfile = newProfile;

			script.UpdateProfile();

			// LightMainBuffer2D.Clear();
			// Light2D.ForceUpdateAll();
		}
		
		EditorGUILayout.Space();

		int count = script.cameraSettings.Length;
		count = EditorGUILayout.IntSlider("Camera Count", count, 0, 10);
		if (count != script.cameraSettings.Length) {
			System.Array.Resize(ref script.cameraSettings, count);
		}

		EditorGUILayout.Space();

		for(int id = 0; id < script.cameraSettings.Length; id++) {
			CameraSettings cameraSetting = script.cameraSettings[id];

			cameraFoldout[id] = EditorGUILayout.Foldout(cameraFoldout[id], "Camera " + (id + 1) + " (" + cameraSetting.GetTypeName() + ")");

			if (cameraFoldout[id] == false) {
				EditorGUILayout.Space();
				continue;
			}

			EditorGUI.indentLevel++;

			cameraSetting.cameraType = (CameraSettings.CameraType)EditorGUILayout.EnumPopup("Camera Type", cameraSetting.cameraType);

			if (cameraSetting.cameraType == CameraSettings.CameraType.Custom) {
				cameraSetting.customCamera = (Camera)EditorGUILayout.ObjectField(cameraSetting.customCamera, typeof(Camera), true);
			}

			cameraSetting.bufferID = EditorGUILayout.Popup("Buffer Preset", (int)cameraSetting.bufferID, Lighting2D.Profile.bufferPresets.GetBufferLayers());

			cameraSetting.renderMode = (CameraSettings.RenderMode)EditorGUILayout.EnumPopup("Render Mode", cameraSetting.renderMode);

			if (cameraSetting.renderMode == CameraSettings.RenderMode.Draw) {
				cameraSetting.renderShader = (CameraSettings.RenderShader)EditorGUILayout.EnumPopup("Render Shader", cameraSetting.renderShader);
			
				if (cameraSetting.renderShader == CameraSettings.RenderShader.Custom) {
					cameraSetting.customMaterial = (Material)EditorGUILayout.ObjectField(cameraSetting.customMaterial, typeof(Material), true);
				}

				cameraSetting.renderLayerType = (CameraSettings.RenderLayerType)EditorGUILayout.EnumPopup("Render Layer Type", cameraSetting.renderLayerType);

				if (cameraSetting.renderLayerType == CameraSettings.RenderLayerType.Custom) {
					cameraSetting.renderLayerId = EditorGUILayout.LayerField("Render Layer", cameraSetting.renderLayerId);
				}

			}

			cameraMaterialsFoldout[id] = EditorGUILayout.Foldout(cameraMaterialsFoldout[id], "Materials");

			if (cameraMaterialsFoldout[id]) {
				EditorGUI.indentLevel++;

				CameraMaterials materials = cameraSetting.GetMaterials();


				int matCount = materials.materials.Length;
				matCount = EditorGUILayout.IntField("Count", matCount);
				if (matCount !=  materials.materials.Length) {
					System.Array.Resize(ref materials.materials, matCount);
				}

				for(int i = 0; i < materials.materials.Length; i++) {
					materials.materials[i] = (Material)EditorGUILayout.ObjectField(materials.materials[i], typeof(Material), true);
				
				}

				EditorGUI.indentLevel--;
			}


	

			cameraSetting.id = id;

			script.cameraSettings[id] = cameraSetting;

			EditorGUI.indentLevel--;

			EditorGUILayout.Space();
		}

	


	/*
		

	cameraSetting.renderMode = (CameraSettings.RenderMode)EditorGUILayout.EnumPopup("Render Mode", cameraSetting.renderMode);

	if (cameraSetting.renderMode == CameraSettings.RenderMode.Draw) {
		cameraSetting.renderShader = (CameraSettings.RenderShader)EditorGUILayout.EnumPopup("Render Shader", cameraSetting.renderShader);
	
		if (cameraSetting.renderShader == CameraSettings.RenderShader.Custom) {
		cameraSetting.customMaterial = (Material)EditorGUILayout.ObjectField(cameraSetting.customMaterial, typeof(Material), true);
		}
	}*/

		

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("version " + Lighting2D.VERSION_STRING);

		string buttonName = "";
		if (script.version < Lighting2D.VERSION) {
			buttonName += "Re-Initialize (Outdated)";
			GUI.backgroundColor = Color.red;

			Reinitialize(script);

			return;
		} else {
			buttonName += "Re-Initialize";
		}
		
		if (GUILayout.Button(buttonName)) {
			Reinitialize(script);
		}

		if (GUI.changed) {
			Light2D.ForceUpdateAll();

			LightingManager2D.ForceUpdate();

			if (EditorApplication.isPlaying == false) {
				
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				
			}
		}
	}

	public void Reinitialize(LightingManager2D manager) {
		Debug.Log("Lighting Manager 2D: reinitialized");

		if (manager.version > 0 && manager.version < Lighting2D.VERSION) {
			Debug.Log("Lighting Manager 2D: version update from " + manager.version_string + " to " + Lighting2D.VERSION_STRING);
		}

		foreach(Transform transform in manager.transform) {
			DestroyImmediate(transform.gameObject);
		}
			
		manager.Initialize();

		Light2D.ForceUpdateAll();

		LightingManager2D.ForceUpdate();

		if (EditorApplication.isPlaying == false) {
			
			EditorUtility.SetDirty(target);
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			
		}
	}
}