using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightingSettings;

#if UNITY_EDITOR
	using UnityEditor;
#endif

[ExecuteInEditMode] 
public class LightingManager2D : LightingMonoBehaviour {
	private static LightingManager2D instance;

	public CameraSettings[] cameraSettings = new CameraSettings[1];

	public bool debug = false;
	public int version = 0;
	public string version_string = "";

	public LightingSettings.Profile setProfile;
    public LightingSettings.Profile profile;

	// Sets Lighting Main Profile Settings for Lighting2D at the start of the scene
	private static bool initialized = false; 

	public Camera GetCamera(int id) {
		if (cameraSettings.Length <= id) {
			return(null);
		}

		return(cameraSettings[id].GetCamera());
	}

	public int GetCameraBufferID(int id) {
		if (cameraSettings.Length <= id) {
			return(0);
		}

		return(cameraSettings[id].bufferID);
	}

	public static void ForceUpdate() {
	}
	
	static public LightingManager2D Get() {
		if (instance != null) {
			return(instance);
		}

		foreach(LightingManager2D manager in Object.FindObjectsOfType(typeof(LightingManager2D))) {
			instance = manager;
			return(instance);
		}

		// Create New Light Manager
		GameObject gameObject = new GameObject();
		gameObject.name = "Lighting Manager 2D";

		instance = gameObject.AddComponent<LightingManager2D>();
		instance.Initialize();

		return(instance);
	}

	public void Initialize () {
		instance = this;

		transform.position = Vector3.zero;

		version = Lighting2D.VERSION;

		if (cameraSettings == null) {
			cameraSettings = new CameraSettings[1];
			cameraSettings[0] = new CameraSettings();
		}
	}

	public void Awake() {
		if (instance != null && instance != this) {

			switch(Lighting2D.ProjectSettings.managerInstance) {
				case LightingSettings.ManagerInstance.Static:
				case LightingSettings.ManagerInstance.DontDestroyOnLoad:
					
					Debug.LogWarning("Smart Lighting2D: Lighting Manager duplicate was found, new instance destroyed.", gameObject);

					foreach(LightingManager2D manager in Object.FindObjectsOfType(typeof(LightingManager2D))) {
						if (manager != instance) {
							manager.DestroySelf();
						}
					}

					return; // Cancel Initialization

				case LightingSettings.ManagerInstance.Dynamic:
					instance = this;
					
					Debug.LogWarning("Smart Lighting2D: Lighting Manager duplicate was found, old instance destroyed.", gameObject);

					foreach(LightingManager2D manager in Object.FindObjectsOfType(typeof(LightingManager2D))) {
						if (manager != instance) {
							manager.DestroySelf();
						}
					}
				break;
			}
			

		}

		LightingManager2D.initialized = false;
		SetupProfile();

		if (Application.isPlaying) {
			if (Lighting2D.ProjectSettings.managerInstance == LightingSettings.ManagerInstance.DontDestroyOnLoad) {
				DontDestroyOnLoad(instance.gameObject);
			}
		}
		
		Buffers.Get();
	}

	private void Update() {
		if (Lighting2D.disable) {
			return;
		}

		ForceUpdate(); // For Late Update Method?

		if (profile != null) {
			if (Lighting2D.Profile != profile) {
				Lighting2D.UpdateByProfile(profile);
			}
        }
	}

	void LateUpdate() {
		if (Lighting2D.disable) {
			return;
		}

		Camera camera = Buffers.Get().GetCamera();

		UpdateInternal();
		
		if (Lighting2D.Profile.qualitySettings.updateMethod == LightingSettings.UpdateMethod.LateUpdate) {
			RenderLoop();
			
			camera.enabled = false;
		} else {
			camera.enabled = true;
		}
	}

	public void SetupProfile() {
		if (LightingManager2D.initialized) {
			return;
		}

		LightingManager2D.initialized = true;

		LightingSettings.Profile profile = Lighting2D.Profile;
		Lighting2D.UpdateByProfile(profile);

		AtlasSystem.Manager.Initialize();
		Lighting2D.materials.Reset();
	}

	public void UpdateInternal() {
		if (Lighting2D.disable) {
			return;
		}

		foreach(CameraTransform cameraTransform in CameraTransform.list) {
			cameraTransform.Update();
		}

		SetupProfile();

		UpdateMaterials();

		UpdateMainBuffers();

		AtlasSystem.Manager.Update();
	}

	public void UpdateLoop() {
		// Colliders

		if (DayLightCollider2D.List.Count > 0) {
			foreach(DayLightCollider2D dayLightCollider2D in DayLightCollider2D.List) {
				dayLightCollider2D.UpdateLoop();
			}
		}
		
		if (LightCollider2D.List.Count > 0) {
			foreach(LightCollider2D lightCollider2D in LightCollider2D.List) {
				lightCollider2D.UpdateLoop();
			}
		}

		// Lights

		if (LightSprite2D.List.Count > 0) {
			foreach(LightSprite2D lightSprite2D in LightSprite2D.List) {
				lightSprite2D.UpdateLoop();
			}
		}
		
		if (Light2D.List.Count > 0) {
			foreach(Light2D light2D in Light2D.List) {
				light2D.UpdateLoop();
			}
		}

		// Mesh Renderers

		if (OnRenderMode.List.Count > 0) {
			foreach(OnRenderMode render in OnRenderMode.List) {
				render.UpdateLoop();
			}
		}
	}

	public void RenderLoop() {
		if (Lighting2D.disable) {
			return;
		}

		if (cameraSettings.Length < 1) {
			return;
		}

		UpdateLoop();
		
		if (LightBuffer2D.List.Count > 0) {
			foreach(LightBuffer2D buffer in LightBuffer2D.List) {
				buffer.Render();
			}
		}
		
		if (LightMainBuffer2D.List.Count > 0) {
			foreach(LightMainBuffer2D buffer in LightMainBuffer2D.List) {
				buffer.Render();
			}
		}
	}

	public void UpdateMainBuffers() {
		for(int i = 0; i < cameraSettings.Length; i++) {
			CameraSettings cameraSetting = cameraSettings[i];

			if (cameraSetting.renderMode == CameraSettings.RenderMode.Disabled) {
				continue;
			}
			
			LightMainBuffer2D buffer = LightMainBuffer2D.Get(cameraSetting);

			if (buffer != null) {
				buffer.cameraSettings.renderMode = cameraSetting.renderMode;

				buffer.cameraSettings.renderLayerId = cameraSetting.renderLayerId;

				if (buffer.cameraSettings.customMaterial != cameraSetting.customMaterial) {
					buffer.cameraSettings.customMaterial = cameraSetting.customMaterial;

					buffer.ClearMaterial();
				}

				if (buffer.cameraSettings.renderShader != cameraSetting.renderShader) {
					buffer.cameraSettings.renderShader = cameraSetting.renderShader;

					buffer.ClearMaterial();
				}
				

				foreach(Material material in cameraSetting.GetMaterials().materials) {
					if (material == null) {
						continue;
					}

					Camera camera = cameraSetting.GetCamera();
					if (cameraSetting.cameraType == CameraSettings.CameraType.SceneView) {
						CameraMaterials.SetMaterial(2, material, camera, buffer.renderTexture);
					} else {
						CameraMaterials.SetMaterial(1, material, camera, buffer.renderTexture);
					}
					
				}
			}
		}

		for(int i = 0; i < LightMainBuffer2D.List.Count; i++) {
			LightMainBuffer2D buffer = LightMainBuffer2D.List[i];

			if (buffer != null) {
				buffer.Update();
			}
			
		}

		if (LightMainBuffer2D.List.Count > 0) {
			foreach(LightMainBuffer2D buffer in LightMainBuffer2D.List) {
				if (Lighting2D.disable) {
					buffer.updateNeeded = false;	
					return;
				}

				CameraSettings cameraSettings = buffer.cameraSettings;
				bool render = cameraSettings.renderMode != CameraSettings.RenderMode.Disabled;

				if (render && cameraSettings.GetCamera() != null) {
					buffer.updateNeeded = true;
				
				} else {
					buffer.updateNeeded = false;
				}
			}
		}
	}
	
	public void UpdateMaterials() {
		if (Lighting2D.materials.Initialize(Lighting2D.QualitySettings.HDR)) {
			LightMainBuffer2D.Clear();
			LightBuffer2D.Clear();

			Light2D.ForceUpdateAll();
		}
	}

	public bool IsSceneView() {
		for(int i = 0; i < cameraSettings.Length; i++) {
			CameraSettings cameraSetting = cameraSettings[i];
			
			if (cameraSetting.cameraType == CameraSettings.CameraType.SceneView) {
				if (cameraSetting.renderMode == CameraSettings.RenderMode.Draw) {
					return(true);
				}
			}
		}
		
		return(false);
	}

	private void OnDisable() {
		if (profile != null) {
			if (Application.isPlaying) {
				if (setProfile != profile) {
					if (Lighting2D.Profile == profile) {
						Lighting2D.RemoveProfile();
					}
				}
			}
		}

		#if UNITY_EDITOR
			#if UNITY_2019_1_OR_NEWER
				SceneView.beforeSceneGui -= OnSceneView;
				//SceneView.duringSceneGui -= OnSceneView;
			#else
				SceneView.onSceneGUIDelegate -= OnSceneView;
			#endif
		#endif
	}

	public void UpdateProfile() {
		if (setProfile == null) {
            setProfile = Lighting2D.ProjectSettings.Profile;
        } 

		if (Application.isPlaying == true) {
			profile = Object.Instantiate(setProfile);
		} else {
			profile = setProfile;
		}
	}

	private void OnEnable() {
		foreach(OnRenderMode onRenderMode in Object.FindObjectsOfType(typeof(OnRenderMode))) {
			onRenderMode.DestroySelf();
		}

		Scriptable.LightSprite2D.List.Clear();

		UpdateProfile();
		UpdateMaterials();

		for(int i = 0; i < cameraSettings.Length; i++) {
			CameraSettings cameraSetting = cameraSettings[i];

			foreach(Material material in cameraSetting.GetMaterials().materials) {
				if (material == null) {
					continue;
				}

				Camera camera = cameraSetting.GetCamera();
				if (cameraSetting.cameraType == CameraSettings.CameraType.SceneView) {
					CameraMaterials.ClearMaterial(material);
				}
				
			}
		}
	
		Update();
		LateUpdate();
	
		#if UNITY_EDITOR
			#if UNITY_2019_1_OR_NEWER
				SceneView.beforeSceneGui += OnSceneView;
				//SceneView.duringSceneGui += OnSceneView;
			#else
				SceneView.onSceneGUIDelegate += OnSceneView;
			#endif	
		#endif	
	}

	public void OnRenderObject() {
		if (Lighting2D.RenderingMode != RenderingMode.OnPostRender) {
			return;
		}
		
		foreach(LightMainBuffer2D buffer in LightMainBuffer2D.List) {
			Rendering.LightMainBuffer.DrawPost(buffer);
		}
	}

	private void OnDrawGizmos() {

		if (Lighting2D.ProjectSettings.editorView.drawGizmos != EditorDrawGizmos.Always) {
			return;
		}

		DrawGizmos();
	}
	
	private void DrawGizmos() {
	
		if (isActiveAndEnabled == false) {
			return;
		}

		Gizmos.color = new Color(0, 1f, 1f);

		if (Lighting2D.ProjectSettings.editorView.drawGizmosBounds == EditorGizmosBounds.Rectangle) {
			for(int i = 0; i < cameraSettings.Length; i++) {
				CameraSettings cameraSetting = cameraSettings[i];

				Camera camera = cameraSetting.GetCamera();

				if (camera != null) {
					Rect cameraRect = CameraTransform.GetWorldRect(camera);

					GizmosHelper.DrawRect(transform.position, cameraRect);
				}
			}
		}

		for(int i = 0; i < Scriptable.LightSprite2D.List.Count; i++) {
			Scriptable.LightSprite2D light = Scriptable.LightSprite2D.List[i];

			Rect rect = light.lightSpriteShape.GetWorldRect();

			Gizmos.color = new Color(1f, 0.5f, 0.25f);

			GizmosHelper.DrawPolygon(light.lightSpriteShape.GetSpriteWorldPolygon(), transform.position);

			Gizmos.color = new Color(0, 1f, 1f);
			GizmosHelper.DrawRect(transform.position, rect);
		}
	}

	#if UNITY_EDITOR
		static public void OnSceneView(SceneView sceneView) {
			LightingManager2D manager = LightingManager2D.Get();
	
			if (manager.IsSceneView() == false) {
				return;
			}

			ForceUpdate();

			manager.UpdateLoop();
			manager.RenderLoop();

			Buffers lightingCamera = Buffers.Get();;

			if (lightingCamera != null) {
				lightingCamera.enabled = false;
				lightingCamera.enabled = true;
			}
		}
	#endif
}