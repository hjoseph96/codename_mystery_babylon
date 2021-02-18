using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SS.TwoD
{
    public class SpriteGeneratorManager : MonoBehaviour
    {
        public enum Directions
        {
            Sixteen = 16,
            Twelve = 12,
            Eight = 8,
            Four = 4
        }

        public enum ShadowType
        {
            RealShadow = 0,
            FakeShadowEllipse = 1,
            NoShadow = 2
        }

        public enum GamePlane
        {
            XZ,
            XY
        }
        
        public enum RotateAxis
        {
            X,
            Y,
            Z
        }

        public enum State
        {
            WAIT,
            SETUP,
            READY,
            GENERATING,
            FINISH,
			VALIDATION_FAILED
        }

        [System.Serializable]
        public class MatTexPack : System.Object
        {
            public Material material;
            public Texture texture1;
            public Texture texture2;
        }

        public float textureScale
        {
            get;
            set;
        }

        public string characterName;
        public Transform rootObject;
        public Animator animator;
        public Directions maxDirection = Directions.Twelve;
        public RotateAxis rotateAxis = RotateAxis.Y;
        public ShadowType shadowType = ShadowType.RealShadow;
        public string spritePackingTag;
        public string spritePath;
        public string animationPath;
        public string bodyRendererPath;
        public string shadowRendererPath;
        public string colorRendererPath;
        public GameObject[] spritePrefabs;
        public GameObject floor;
        public GameObject[] pivots;
        public Color shadowColor;
        public int pixelHeight = 100;
        public GamePlane gamePlane;
        public Sprite fakeShadowSprite;
        public MatTexPack[] colors;
        public SS.Tools.ScreenshotTools.RenderQuality renderQuality = Tools.ScreenshotTools.RenderQuality.Medium;
        public Transform lineHorizontalBottom;
        public Transform lineHorizontalTop;

        int m_Count;
		int m_CharacterIndex = -1;
        Color m_MaskColor;
        IGenerator[] m_SpriteGenerators;
        State m_State;
		SpriteGeneratorCharacterManager m_CharMan;

        public float shotSpeed
        {
            get;
            protected set;
        }

        void Awake()
        {
            SS.Tools.ScreenshotTools.renderQuality = renderQuality;
        }

        IEnumerator Start()
        {
			// Character Manager
			m_CharMan = FindObjectOfType<SpriteGeneratorCharacterManager>();
			if (m_CharMan != null)
			{
				m_CharMan.ActivateCharacter(0);
			}

            m_State = State.WAIT;

            // Deactivate pivot
            for (int i = 0; i < pivots.Length; i++)
            {
                pivots[i].SetActive(false);
            }

            yield return new WaitForSeconds(1);

            m_State = State.SETUP;

            yield return new WaitForEndOfFrame();
            float time = Time.time;
            yield return new WaitForEndOfFrame();

            // Set mask color
            Texture2D tex = SS.Tools.ScreenshotTools.ScreenShot();
            m_MaskColor = tex.GetPixel(0, 0);
            Destroy(tex);

            yield return new WaitForEndOfFrame();
            shotSpeed = 1f / (Time.time + 0.125f - time);

            // Calculate scale
            float characterHeight = Mathf.Abs(Camera.main.WorldToScreenPoint(lineHorizontalTop.position).y - Camera.main.WorldToScreenPoint(lineHorizontalBottom.position).y);
            if (characterHeight < 0.00001f)
            {
                ValidationFailed("The top line and bot line MUST NOT be overlapping on the Game View!");
                yield break;
            }
            float screenHeight = Screen.height;
            float height = characterHeight / screenHeight * SS.Tools.ScreenshotTools.maxHeight;
            textureScale = (float)pixelHeight / height;

            // Setup sprite generators
            List<IGenerator> temp = new List<IGenerator>();
            m_SpriteGenerators = GetComponents<IGenerator>();
            for (int i = 0; i < m_SpriteGenerators.Length; i++)
            {
                if (m_SpriteGenerators[i].enabledComponent)
                {
                    m_SpriteGenerators[i].onDone += OnGenerated;
                    m_SpriteGenerators[i].maskColor = m_MaskColor;

                    temp.Add(m_SpriteGenerators[i]);
                }
            }
            m_SpriteGenerators = temp.ToArray();

			// Validate parameters
			if (m_CharMan != null)
			{
				if (m_CharMan.characters.Length == 0)
				{
					ValidationFailed("You must to assign your characters to SpriteGeneratorCharacterManager");
				}
				else if (m_CharMan.outputPrefabs.Length != m_CharMan.characters.Length)
				{
					ValidationFailed("You must click this before playing: From menu, SS / TwoD / Multi Characters / Generate output prefabs");
				}
			}

			if (m_State != State.VALIDATION_FAILED)
			{
				m_State = State.READY;
			}
        }

		void SetupCharacter()
		{
			characterName = m_CharMan.outputPrefabs[m_CharacterIndex].name;
			animator = m_CharMan.characters[m_CharacterIndex];
			spritePrefabs = new GameObject[1];
			spritePrefabs[0] = m_CharMan.outputPrefabs[m_CharacterIndex];
		}

		void ValidationFailed(string log)
		{
			Debug.LogWarning(log);
			m_State = State.VALIDATION_FAILED;
		}

        void Update()
        {
            if (m_State == State.READY)
            {
				m_State = State.GENERATING;

				if (m_CharMan == null)
				{
					if (m_SpriteGenerators.Length > 0)
					{
						m_SpriteGenerators[0].Generate();
					}
					else
					{
						m_State = State.FINISH;
					}
				}
				else
				{
					NextOrFinish();
				}
            }
        }

        void OnGUI()
        {
            switch (m_State)
            {
                case State.WAIT:
                    GUI.Label(new Rect(50, 50, Screen.width, 100), "Please wait...");
                    break;
                case State.READY:
                    GUI.Label(new Rect(50, 50, Screen.width, 100), "Start generating sprites");
                    break;
                case State.GENERATING:
                    GUI.Label(new Rect(50, 50, Screen.width, 100), m_SpriteGenerators[m_Count].progress);
                    break;
                case State.FINISH:
                    GUI.Label(new Rect(50, 50, Screen.width, 100), "Done!");
                    break;
            }
        }

        void OnGenerated()
        {
            m_Count++;

            if (m_Count < m_SpriteGenerators.Length)
            {
                m_SpriteGenerators[m_Count].Generate();
            }
            else
            {
				if (m_CharMan == null)
				{
					Finish();
				}
				else
				{
					NextOrFinish();
				}
            }
        }

		void NextOrFinish()
		{
			m_CharacterIndex++;

			while (m_CharacterIndex < m_CharMan.characters.Length && m_CharMan.willGenerate[m_CharacterIndex] == false)
			{
				m_CharacterIndex++;
			}

			if (m_CharacterIndex < m_CharMan.characters.Length)
			{
				m_Count = 0;
				m_CharMan.ActivateCharacter(m_CharacterIndex);
				SetupCharacter();
				m_SpriteGenerators[m_Count].Generate();
			}
			else
			{
				Finish();
			}
		}

		void Finish()
		{
			System.GC.Collect();
			m_State = State.FINISH;
#if UNITY_EDITOR
			StartCoroutine(CoStopPlaying());
#endif
		}

#if UNITY_EDITOR
        IEnumerator CoStopPlaying()
        {
            yield return new WaitForSeconds(1);
            UnityEditor.EditorApplication.playmodeStateChanged = OpenThenPlayNextScene;
            UnityEditor.EditorApplication.isPlaying = false;
        }
        
        public static void OpenThenPlayNextScene()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.playmodeStateChanged = null;
                
                string scenePath = UnityEditor.EditorPrefs.GetString("ss2d_scenePath_auto", "Scenes/Gen");
                string[] sceneArray = UnityEditor.EditorPrefs.GetString("ss2d_sceneArray_auto_runtime", string.Empty).Split(',');
                List<string> sceneList = null;
                
                if (sceneArray.Length > 0 && !string.IsNullOrEmpty(sceneArray[0]))
                {
                    sceneList = new List<string>(sceneArray);
                }
                
                if (sceneList != null && sceneList.Count > 0)
                {
                    string fullScenePath = Path.Combine(Application.dataPath, Path.Combine(scenePath, sceneList[0] + ".unity"));
                    sceneList.RemoveAt(0);
                    UnityEditor.EditorPrefs.SetString("ss2d_sceneArray_auto_runtime", StringTools.MergeStringArray(sceneList.ToArray(), ","));
                    SS.Tools.SceneTools.OpenScene(fullScenePath);
                    AutoPlayScene.Play();
                }
            }
        }
#endif
	}
}