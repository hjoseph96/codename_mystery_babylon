using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

[System.Serializable]
public class CameraMaterials {
	public Material[] materials = new Material[1];

	//public static readonly int _Radius = Shader.PropertyToID("_Radius");
	// public static readonly int _BackgroundTexture = Shader.PropertyToID("_SuperBlurTexture");
    // blurMaterial.SetFloat(Uniforms._Radius, radius);
	
	public static void ClearMaterial(Material material) {
		material.SetFloat("_Cam1_Size_X", 0);
		material.SetFloat("_Cam1_Size_Y", 0);

		material.SetFloat("_Cam2_Size_X", 0);
		material.SetFloat("_Cam2_Size_Y", 0);
	}

	public static void SetMaterial(int id, Material material, Camera camera, LightTexture lightTexture) {
		float ratio = (float)camera.pixelRect.width / camera.pixelRect.height;

		switch(id) {
			case 1:
				material.SetTexture("_Cam1_Texture", lightTexture.renderTexture);

				material.SetFloat("_Cam1_Position_X", camera.transform.position.x);
				material.SetFloat("_Cam1_Position_Y", camera.transform.position.y);

				material.SetFloat("_Cam1_Size_X", camera.orthographicSize * ratio * 2);
				material.SetFloat("_Cam1_Size_Y", camera.orthographicSize * 2);

				material.SetFloat("_Cam1_Size_Rotation", camera.transform.eulerAngles.z * Mathf.Deg2Rad);
			break;

			case 2:
				material.SetTexture("_Cam2_Texture", lightTexture.renderTexture);

				material.SetFloat("_Cam2_Position_X", camera.transform.position.x);
				material.SetFloat("_Cam2_Position_Y", camera.transform.position.y);

				material.SetFloat("_Cam2_Size_X", camera.orthographicSize * ratio * 2);
				material.SetFloat("_Cam2_Size_Y", camera.orthographicSize * 2);

				material.SetFloat("_Cam2_Size_Rotation", camera.transform.eulerAngles.z * Mathf.Deg2Rad);
			break;
		}
		
	}

	public void Add(Material material) {
		foreach(Material m in  materials) {
			if (m == material) {
				Debug.Log("Lighting Manager 2D: Failed to add material (material already added!");
				return;
			}
		}

		for(int i = 0 ; i < materials.Length; i++) {
			if (materials[i] != null) {
				continue;
			}
			materials[i] = material;

			return;
		}

		System.Array.Resize(ref materials, materials.Length + 1);

		materials[materials.Length - 1] = material;
	}

	public void Remove(Material material) {
		for(int i = 0 ; i < materials.Length; i++) {
			if (materials[i] != material) {
				continue;
			}
			materials[i] = null;

			return;
		}

		Debug.LogWarning("Lighting Manager 2D: Removing material that does not exist");
	}

	public void SetCamera(int id) {

	}

}

[System.Serializable]
public struct CameraSettings {
	public int id;

	public enum RenderMode { Draw, Hidden, Disabled }
	public enum CameraType { MainCamera, Custom, SceneView };
	public enum RenderShader { Multiply, Additive, Custom };
	public enum RenderLayerType { Default, Custom };
	
	public RenderLayerType renderLayerType;
	public CameraType cameraType;
	public Camera customCamera;
	public RenderMode renderMode;
	public RenderShader renderShader;
	public Material customMaterial;

	public Material customMaterialInstance;

	public CameraMaterials materials;
	
	public CameraMaterials GetMaterials() {
		if (materials == null) {
			materials = new CameraMaterials();
		}

		return(materials);
	}

	public Material GetMaterial() {
		if (customMaterialInstance == null) {
			if (customMaterial != null) {
				customMaterialInstance = new Material(customMaterial);
			}
		}

		return(customMaterialInstance);
	}

	public int bufferID;

	public int renderLayerId;

	public string GetTypeName() {
		switch(cameraType) {
			case CameraType.SceneView:
				return("Scene View");

			case CameraType.MainCamera:
				return("Main Camera Tag");

			case CameraType.Custom:
				return("Custom");

			default:
				return("Unknown");
		}
	}

	public int GetLayerId() {
		if (renderLayerType == RenderLayerType.Custom) {
			return(renderLayerId);
		} else {
			Camera camera = GetCamera();

			if (camera != null && cameraType == CameraType.SceneView) {
				return(Lighting2D.ProjectSettings.editorView.sceneViewLayer);
			} else {
				return(Lighting2D.ProjectSettings.editorView.gameViewLayer);
			}
		}
	}

	public CameraSettings(int id = 0) {
		this.id = id;

		renderMode = RenderMode.Draw;

		cameraType = CameraType.MainCamera;

		renderShader = RenderShader.Multiply;

		renderLayerType = RenderLayerType.Default;

		customMaterial = null;

		customMaterialInstance = null;

		bufferID = 0;

		renderLayerId = 0;

		customCamera = null;

		materials = new CameraMaterials();
	}

	public Camera GetCamera() {
		Camera camera = null;
		switch(cameraType) {
			case CameraType.MainCamera:
				camera = Camera.main;

				if (camera != null) {
					if (camera.orthographic == false) {
						return(null);
					}
				}

				return(Camera.main);

			case CameraType.Custom:
				camera = customCamera;

				if (camera != null) {
					if (camera.orthographic == false) {
						return(null);
					}
				}

				return(customCamera);


            case CameraType.SceneView:
			
				#if UNITY_EDITOR
					SceneView sceneView = SceneView.lastActiveSceneView;

					if (sceneView != null) {
						camera = sceneView.camera;

						#if UNITY_2019_1_OR_NEWER
						
							if (SceneView.lastActiveSceneView.sceneLighting == false) {
								camera = null;
							}

						#else
						
							if (SceneView.lastActiveSceneView.m_SceneLighting == false) {
								camera = null;
							}

						#endif
					}
	
					if (camera != null && camera.orthographic == false) {
						camera = null;
					}

					if (camera != null) {
						if (camera.orthographic == false) {
							return(null);
						}
					}

					return(camera);

				#else
					return(null);

				#endif
				
		}

		return(null);
	}

	public bool Equals(CameraSettings obj) {
        return this.bufferID == obj.bufferID && this.customCamera == obj.customCamera && this.cameraType == obj.cameraType;
    }

	public override int GetHashCode() {
        return this.GetHashCode();
    }
}