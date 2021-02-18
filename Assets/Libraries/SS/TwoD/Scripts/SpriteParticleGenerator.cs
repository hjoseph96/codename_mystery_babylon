using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace SS.TwoD
{
    public class SpriteParticleGenerator : MonoBehaviour, IGenerator
    {
        [SerializeField] string m_AnimationName;
        [SerializeField] float m_OriginalAnimationDuration = 1f;
        [SerializeField] float m_AnimationDuration = 1f;
        [SerializeField] float m_AnimationFrameRate = 8f;
        [SerializeField] bool m_AnimationLoop = true;
        [SerializeField] int m_AnimationDirections = -1;
        [SerializeField] GameObject m_ParticleSystemPrefab;
        [SerializeField] Material m_OutputMaterial;

		string m_CharacterName;
		GameObject[] m_SpritePrefabs;

        string m_SpritePackingTag;
        string m_SpritePath;
        string m_AnimationPath;
        string m_BodyRendererPath;
        string m_ShadowRendererPath;
        string m_ColorRendererPath;
        string m_DirectionMask;
        int m_MaxDirection;
        int m_Count;
        int m_Direction;
        int m_SpriteQuantity;
        int m_SpritePerFrame;
        float m_TextureScale;
        float m_TimeSpeed;
        GameObject m_Floor;
        Texture2D[] m_TexturesShadowOnly;
        Texture2D[] m_TexturesBodyOnly;
        Texture2D[] m_TexturesBodyColor;
        Transform m_RootObject;
        Color m_ShadowColor;
        SSRect[] m_Rects;
        Quaternion m_DefaultRotation;
        SpriteGeneratorManager.GamePlane m_GamePlane;
        SpriteGeneratorManager.ShadowType m_ShadowType;
        Sprite m_FakeShadowSprite;
        SpriteGeneratorManager.MatTexPack[] m_Colors;
        GameObject m_ParticleSystem;

        public Callback onDone
        {
            get;
            set;
        }

        public bool enabledComponent
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }

        public string progress
        {
            get;
            set;
        }

        public Color maskColor
        {
            get;
            set;
        }

        public void Generate()
        {
			ResetParameters();
			SetupCharacter();
            SetupTimeSpeed();
            GetTextureScale();
            CreateFolders();
            ChooseNextAnimation();

            if (m_Direction < m_MaxDirection)
            {
                TakeAllFramesOfADirection();
            }
            else
            {
                Done();
            }
        }

		public string animationName
        {
            get { return m_AnimationName; }
            set { m_AnimationName = value; }
        }
        
        public float originalAnimationDuration
        {
            get { return m_OriginalAnimationDuration; }
            set { m_OriginalAnimationDuration = value; }
        }

        public float animationDuration
        {
            get { return m_AnimationDuration; }
            set { m_AnimationDuration = value; }
        }

        public float animationFrameRate
        {
            get { return m_AnimationFrameRate; }
            set { m_AnimationFrameRate = value; }
        }

        public bool animationLoop
        {
            get { return m_AnimationLoop; }
            set { m_AnimationLoop = value; }
        }

        public int animationDirections
        {
            get { return m_AnimationDirections; }
            set { m_AnimationDirections = value; }
        }
        
        public GameObject particleSystemPrefab
        {
            get { return m_ParticleSystemPrefab; }
            set { m_ParticleSystemPrefab = value; }
        }
        
        public Material outputMaterial
        {
            get { return m_OutputMaterial; }
            set { m_OutputMaterial = value; }
        }

        void Start()
        {
			SetupParameters();
			SetupDirectionMask();
			progress = string.Empty;
        }
        
        void ResetParameters()
        {
            m_Count = 0;
            m_Direction = 0;
            m_Rects = null;
        }

		void SetupParameters()
		{
			SpriteGeneratorManager manager = GetComponent<SpriteGeneratorManager>();

			if (manager != null)
			{
				m_RootObject = manager.rootObject;
				m_MaxDirection = (int)manager.maxDirection;
				m_SpritePackingTag = manager.spritePackingTag;
				m_SpritePath = manager.spritePath;
				m_AnimationPath = manager.animationPath;
				m_BodyRendererPath = manager.bodyRendererPath;
				m_ShadowRendererPath = manager.shadowRendererPath;
				m_ColorRendererPath = manager.colorRendererPath;
				m_Floor = manager.floor;
				m_ShadowColor = manager.shadowColor;
				m_DefaultRotation = m_RootObject.rotation;
				m_GamePlane = manager.gamePlane;
				m_ShadowType = manager.shadowType;
				m_FakeShadowSprite = manager.fakeShadowSprite;
				m_Colors = manager.colors;
			}
		}

		void SetupDirectionMask()
		{
			m_DirectionMask = System.Convert.ToString(m_AnimationDirections, 2);

			int count = m_DirectionMask.Length;
			while (count < m_MaxDirection)
			{
				m_DirectionMask = "0" + m_DirectionMask;
				count++;
			}

			m_DirectionMask = m_DirectionMask.Substring(m_DirectionMask.Length - m_MaxDirection, m_MaxDirection);
		}

		void SetupCharacter()
		{
			SpriteGeneratorManager manager = GetComponent<SpriteGeneratorManager>();

			if (manager != null)
			{
				m_CharacterName = manager.characterName;
				m_SpritePrefabs = manager.spritePrefabs;
			}
		}

        void SetupTimeSpeed()
        {
            SpriteGeneratorManager manager = GetComponent<SpriteGeneratorManager>();
            
            if (manager != null)
            {
                float shotSpeed = manager.shotSpeed;
                m_SpriteQuantity = Mathf.RoundToInt(m_AnimationFrameRate * m_AnimationDuration);
                
                m_SpritePerFrame = 1;
                if (m_ShadowType == SpriteGeneratorManager.ShadowType.RealShadow) m_SpritePerFrame++;
                if (HasColor()) m_SpritePerFrame++;
                
                int totalSprite = m_SpritePerFrame * m_SpriteQuantity;
                float shotDuration = (float)totalSprite / shotSpeed;
                
                m_TimeSpeed = m_OriginalAnimationDuration / shotDuration;
            }
        }

        void GetTextureScale()
        {
            SpriteGeneratorManager manager = GetComponent<SpriteGeneratorManager>();

            if (manager != null)
            {
                m_TextureScale = manager.textureScale;
            }
        }

        void CreateFolders()
        {
            string spritefolder = GetSpriteAnimFolderPathFull();
            if (System.IO.Directory.Exists(spritefolder))
            {
                System.IO.Directory.Delete(spritefolder, true);
            }
            System.IO.Directory.CreateDirectory(spritefolder);

            if (!string.IsNullOrEmpty(m_AnimationPath))
            {
                string animfolder = GetAnimationClipFolderPathFull();
                if (System.IO.Directory.Exists(animfolder))
                {
                    System.IO.Directory.Delete(animfolder, true);
                }
                System.IO.Directory.CreateDirectory(animfolder);
            }
        }

        void TakeAllFramesOfADirection()
        {
            m_Count = 0;
            m_TexturesShadowOnly = new Texture2D[m_SpriteQuantity];
            m_TexturesBodyOnly = new Texture2D[m_SpriteQuantity];
            m_TexturesBodyColor = new Texture2D[m_SpriteQuantity];

            if (m_ParticleSystem != null)
            {
                Destroy(m_ParticleSystem);
            }
            
            m_ParticleSystem = Instantiate<GameObject>(m_ParticleSystemPrefab);
            m_ParticleSystem.transform.SetParent(m_RootObject);
            m_ParticleSystem.transform.localPosition = Vector3.zero;
            m_ParticleSystem.transform.localRotation = Quaternion.identity;
            m_ParticleSystem.transform.localScale = Vector3.one;

            if (m_SpriteQuantity <= 1)
            {
                TakeAFrameOfADirection();
            }
            else
            {
                InvokeRepeating("TakeAFrameOfADirection", 0, m_OriginalAnimationDuration / (m_SpriteQuantity / m_SpritePerFrame));
            }
        }

        void TakeAFrameOfADirection()
        {
            Time.timeScale = m_TimeSpeed;
            StartCoroutine(CoTakeAFrameOfADirection());
        }

        void ActivateFloor(bool active)
        {
            if (m_Floor != null)
            {
                m_Floor.SetActive(active);
            }
        }

        IEnumerator CoTakeAFrameOfADirection()
        {
            Time.timeScale = 0;

            if (m_ShadowType == SpriteGeneratorManager.ShadowType.RealShadow)
            {
                ActivateFloor(true);
                CastShadowOnly(true);
                yield return new WaitForEndOfFrame();
                m_TexturesShadowOnly [m_Count] = SS.Tools.ScreenshotTools.ScreenShot();
            }
            
            ActivateFloor(false);
            CastShadowOnly(false);
            SetTextures(0);
            yield return new WaitForEndOfFrame();
            m_TexturesBodyOnly[m_Count] = SS.Tools.ScreenshotTools.ScreenShot();
            
            if (HasColor())
            {
                SetTextures(1);
                yield return new WaitForEndOfFrame();
                m_TexturesBodyColor[m_Count] = SS.Tools.ScreenshotTools.ScreenShot();
                SetTextures(0);
            }
            
            Time.timeScale = m_TimeSpeed;
            
            m_Count++;
            if (m_Count >= m_SpriteQuantity)
            {
                Time.timeScale = 1;

                CancelInvoke("TakeAFrameOfADirection");
                yield return new WaitForEndOfFrame();
                
                yield return StartCoroutine(ResizeAll());
                Trim();
                SaveSpriteFile();
                Free();
                
                m_Direction++;
                m_RootObject.Rotate(new Vector3(0, 360f / m_MaxDirection, 0));
                
                ChooseNextAnimation();
                
                if (m_Direction < m_MaxDirection)
                {
                    Invoke("TakeAllFramesOfADirection", 0.5f);
                }
                else
                {
                    #if UNITY_EDITOR
                    AssetDatabase.Refresh();
                    SettingSprites();
                    ApplyPrefab();
                    #endif
                    
                    m_RootObject.rotation = m_DefaultRotation;
                    
                    Invoke("Done", 1);
                }
            }
        }

        void CastShadowOnly(bool shadowOnly)
        {
            if (m_ParticleSystem != null)
            {
                Renderer[] renderers = m_ParticleSystem.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers [i].shadowCastingMode = (!shadowOnly) ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
            }
        }

        void SetTextures(int index)
        {
            if (m_Colors != null)
            {
                for (int i = 0; i < m_Colors.Length; i++)
                {
                    m_Colors[i].material.mainTexture = (index == 0) ? m_Colors[i].texture1 : m_Colors[i].texture2;
                }
            }
        }

        bool HasColor()
        {
            return (m_Colors != null && m_Colors.Length > 0);
        }

        void ChooseNextAnimation()
        {
            while (m_Direction < m_MaxDirection && IsUnnecessaryDirection(m_Direction))
            {
                m_Direction++;
                m_RootObject.Rotate(new Vector3(0, 360f / m_MaxDirection, 0));
            }
        }

        bool IsUnnecessaryDirection(int dir)
        {
            return (m_DirectionMask[m_MaxDirection - 1 - dir] == '0');
        }

        void Done()
        {
            if (onDone != null)
            {
                onDone();
            }
        }

        void Resize(ref Texture2D tex, float scale = 0.125f)
        {
            if (tex != null)
            {
                Texture2D scaledTex = SS.Tools.TextureTools.ResizeTexture(tex, SS.Tools.TextureTools.ImageFilterMode.Average, scale);
                Destroy(tex);
                tex = scaledTex;
            }
        }

        IEnumerator ResizeAll()
        {
            int total = m_TexturesBodyOnly.Length * m_SpritePerFrame;
            int count = 0;

            for (int i = 0; i < m_TexturesBodyOnly.Length; i++)
            {
                Resize(ref m_TexturesBodyOnly[i], m_TextureScale);
                yield return 0;
                count++;
                progress = ((float)count / total).ToString("P");
                
                if (m_TexturesShadowOnly[i] != null)
                {
                    Resize(ref m_TexturesShadowOnly[i], m_TextureScale);
                    yield return 0;
                    count++;
                    progress = ((float)count / total).ToString("P");
                }

                if (HasColor())
                {
                    Resize(ref m_TexturesBodyColor[i], m_TextureScale);
                    yield return 0;
                    count++;
                    progress = ((float)count / total).ToString("P");
                }
            }

            progress = string.Empty;
            yield return 0;
        }

        void SaveSpriteFile()
        {
            for (int i = 0; i < m_TexturesBodyOnly.Length; i++)
            {
                string folder = GetSpriteFolderPathFull(m_Direction);
                string bodyfile = GetSpriteFilePath(i);
                string shadowFile = GetShadowSpriteFilePath(i);
                string colorFile = GetColorSpriteFilePath(i);

                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }

                if (m_TexturesBodyOnly[i] != null)
                {
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder, bodyfile), m_TexturesBodyOnly[i].EncodeToPNG());
                }

                if (m_TexturesShadowOnly[i] != null)
                {
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder, shadowFile), m_TexturesShadowOnly[i].EncodeToPNG());
                }

                if (m_TexturesBodyColor[i] != null)
                {
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder, colorFile), m_TexturesBodyColor[i].EncodeToPNG());
                }
            }
        }

        #region Sprite path
        string GetSpriteAnimFolderPathFull()
        {
            string folder = System.IO.Path.Combine(Application.dataPath, m_SpritePath + "/" + m_CharacterName + "/" + m_AnimationName);
            return folder;
        }

        string GetSpriteFolderPathFull(int direction)
        {
            return GetSpriteFolderPath(direction, Application.dataPath);
        }

        string GetSpriteFolderPathShort(int direction)
        {
            return GetSpriteFolderPath(direction, "Assets");
        }

        string GetSpriteFolderPath(int direction, string source)
        {
            string folder = System.IO.Path.Combine(source, m_SpritePath + "/" + m_CharacterName + "/" + m_AnimationName + "/" + direction);
            return folder;
        }

        string GetSpriteFilePath(int index)
        {
            string file = index + ".png";
            return file;
        }

        string GetShadowSpriteFilePath(int index)
        {
            string file = "s" + index + ".png";
            return file;
        }

        string GetColorSpriteFilePath(int index)
        {
            string file = "c" + index + ".png";
            return file;
        }

        string GetSpriteFilePathShort(int direction, int index)
        {
            string folder = GetSpriteFolderPathShort(direction);
            string file = GetSpriteFilePath(index);

            return System.IO.Path.Combine(folder, file);
        }

        string GetShadowSpriteFilePathShort(int direction, int index)
        {
            string folder = GetSpriteFolderPathShort(direction);
            string file = GetShadowSpriteFilePath(index);

            return System.IO.Path.Combine(folder, file);
        }

        string GetColorSpriteFilePathShort(int direction, int index)
        {
            string folder = GetSpriteFolderPathShort(direction);
            string file = GetColorSpriteFilePath(index);

            return System.IO.Path.Combine(folder, file);
        }
        #endregion

        #region Animation clip path
        string GetAnimationClipFolderPathFull()
        {
            return GetAnimationClipFolderPath(Application.dataPath);
        }

        string GetAnimationClipFolderPathShort()
        {
            return GetAnimationClipFolderPath("Assets");
        }

        string GetAnimationClipFolderPath(string source)
        {
            string folder = System.IO.Path.Combine(source, m_AnimationPath + "/" + m_CharacterName + "/" + m_AnimationName);
            return folder;
        }

        string GetAnimationClipFilePath(int direction)
        {
            string file = m_AnimationName + direction + ".anim";
            return file;
        }
        #endregion

        bool CompareColor(Color c1, Color c2)
        {
            if (Mathf.RoundToInt(c1.r * 1000) == Mathf.RoundToInt(c2.r * 1000)
                && Mathf.RoundToInt(c1.g * 1000) == Mathf.RoundToInt(c2.g * 1000)
                && Mathf.RoundToInt(c1.b * 1000) == Mathf.RoundToInt(c2.b * 1000))
            {
                return true;
            }

            return false;
        }

        bool CompareColorAlpha(Color c1, Color c2)
        {
            if (Mathf.RoundToInt(c1.r * 1000) == Mathf.RoundToInt(c2.r * 1000)
                && Mathf.RoundToInt(c1.g * 1000) == Mathf.RoundToInt(c2.g * 1000)
                && Mathf.RoundToInt(c1.b * 1000) == Mathf.RoundToInt(c2.b * 1000)
                && Mathf.RoundToInt(c1.a * 1000) == Mathf.RoundToInt(c2.a * 1000))
            {
                return true;
            }

            return false;
        }

        bool IsLowerColor(Color c1, Color c2)
        {
            return (c1.r + c1.g + c1.b < c2.r + c2.g + c2.b);
        }

        float GetAlphaOfColor(Color highest, Color lowest, float highestAlpha, float lowestAlpha, Color color)
        {
            float h = highest.r + highest.g + highest.b;
            float l = lowest.r + lowest.g + lowest.b;
            float c = color.r + color.g + color.b;

            float p = (c - l) / (h - l);

            return Mathf.Clamp01(highestAlpha - p * (highestAlpha - lowestAlpha));
        }

        SSRect GetMaxBound()
        {
            Color black = Color.black;
            int minX = 9999, maxX = 0, minY = 9999, maxY = 0;
            bool done;
            
            for (int i = 0; i < m_TexturesBodyOnly.Length; i++)
            {
                Color[] colorsBodyOnly = m_TexturesBodyOnly[i].GetPixels();
                
                Color[] colorsShadowOnly = null;
                if (m_TexturesShadowOnly[i] != null)
                {
                    colorsShadowOnly = m_TexturesShadowOnly[i].GetPixels();
                }
                
                done = false;
                for (int row = 0; row < m_TexturesBodyOnly[i].height; row++)
                {
                    for (int col = 0; col < m_TexturesBodyOnly[i].width; col++)
                    {
                        int idx = row * m_TexturesBodyOnly[i].width + col;
                        if ((colorsShadowOnly != null && !CompareColor(colorsShadowOnly[idx], maskColor)) || colorsBodyOnly[idx] != black)
                        {
                            if (row < minY)
                                minY = row;
                            done = true;
                            break;
                        }
                    }
                    if (done)
                        break;
                }
                
                done = false;
                for (int row = m_TexturesBodyOnly[i].height - 1; row >= 0 ; row--)
                {
                    for (int col = 0; col < m_TexturesBodyOnly[i].width; col++)
                    {
                        int idx = row * m_TexturesBodyOnly[i].width + col;
                        if ((colorsShadowOnly != null && !CompareColor(colorsShadowOnly[idx], maskColor)) || colorsBodyOnly[idx] != black)
                        {
                            if (row > maxY)
                                maxY = row;
                            done = true;
                            break;
                        }
                    }
                    if (done)
                        break;
                }
                
                done = false;
                for (int col = 0; col < m_TexturesBodyOnly[i].width; col++)
                {
                    for (int row = 0; row < m_TexturesBodyOnly[i].height; row++)
                    {
                        int idx = row * m_TexturesBodyOnly[i].width + col;
                        if ((colorsShadowOnly != null && !CompareColor(colorsShadowOnly[idx], maskColor)) || colorsBodyOnly[idx] != black)
                        {
                            if (col < minX)
                                minX = col;
                            done = true;
                            break;
                        }
                    }
                    if (done)
                        break;
                }
                
                done = false;
                for (int col = m_TexturesBodyOnly[i].width - 1; col >= 0; col--)
                {
                    for (int row = 0; row < m_TexturesBodyOnly[i].height; row++)
                    {
                        int idx = row * m_TexturesBodyOnly[i].width + col;
                        if ((colorsShadowOnly != null && !CompareColor(colorsShadowOnly[idx], maskColor)) || colorsBodyOnly[idx] != black)
                        {
                            if (col > maxX)
                                maxX = col;
                            done = true;
                            break;
                        }
                    }
                    if (done)
                        break;
                }
            }
            
            SSRect rect = new SSRect(minX, minY, (maxX - minX + 1), (maxY - minY + 1));
            
            return rect;
        }
        
        void Trim()
        {
            if (m_Rects == null)
            {
                m_Rects = new SSRect[m_MaxDirection];
            }
            
            m_Rects[m_Direction] = GetMaxBound();
            SSRect rect = m_Rects[m_Direction];
            
            for (int i = 0; i < m_TexturesBodyOnly.Length; i++)
            {
                // Body part
                Color[] colorsBodyOnly = m_TexturesBodyOnly[i].GetPixels(rect.x, rect.y, rect.width, rect.height);
                
                Destroy(m_TexturesBodyOnly[i]);
                m_TexturesBodyOnly[i] = new Texture2D(rect.width, rect.height);
                m_TexturesBodyOnly[i].SetPixels(colorsBodyOnly);
                m_TexturesBodyOnly[i].Apply();
                
                // Color part
                Color[] colorsBodyColor = (HasColor()) ? m_TexturesBodyColor[i].GetPixels(rect.x, rect.y, rect.width, rect.height) : null;
                
                // Shadow part
                Color[] colorsShadowOnly = null;
                if (m_TexturesShadowOnly[i] != null)
                {
                    colorsShadowOnly = m_TexturesShadowOnly[i].GetPixels(rect.x, rect.y, rect.width, rect.height);
                }
                
                Dictionary<string, int> colorDict = new Dictionary<string, int>();
                
                for (int row = 0; row < rect.height; row++)
                {
                    for (int col = 0; col < rect.width; col++)
                    {
                        int index = row * rect.width + col;
                        
                        // Color process
                        if (HasColor() && CompareColorAlpha(colorsBodyColor[index], colorsBodyOnly[index]))
                        {
                            colorsBodyColor[index] = Color.clear;
                        }
                        
                        // Shadow process
                        if (colorsShadowOnly != null && !CompareColor(maskColor, colorsShadowOnly[index]))
                        {
                            string key = colorsShadowOnly[index].ToString();
                            
                            if (!colorDict.ContainsKey(key))
                            {
                                colorDict.Add(key, 0);
                            }
                            
                            colorDict[key] += 1;
                        }
                    }
                }
                
                if (HasColor())
                {
                    Destroy(m_TexturesBodyColor[i]);
                    m_TexturesBodyColor[i] = new Texture2D(rect.width, rect.height);
                    m_TexturesBodyColor[i].SetPixels(colorsBodyColor);
                    m_TexturesBodyColor[i].Apply();
                }
                
                if (colorDict.Count > 0)
                {
                    string keyMax = string.Empty;
                    int valueMax = -1;
                    foreach (var item in colorDict)
                    {
                        if (item.Value > valueMax)
                        {
                            keyMax = item.Key;
                            valueMax = item.Value;
                        }
                    }
                    
                    Color highestColor = maskColor;
                    Color lowestColor = StringToColor(keyMax);
                    
                    for (int row = 0; row < rect.height; row++)
                    {
                        for (int col = 0; col < rect.width; col++)
                        {
                            int index = row * rect.width + col;
                            
                            float alpha = GetAlphaOfColor(highestColor, lowestColor, m_ShadowColor.a, 0, colorsShadowOnly[index]);
                            
                            colorsShadowOnly[index] = m_ShadowColor;
                            colorsShadowOnly[index].a = alpha;
                        }
                    }
                    
                    if (m_TexturesShadowOnly[i] != null)
                    {
                        Destroy(m_TexturesShadowOnly[i]);
                        m_TexturesShadowOnly[i] = new Texture2D(rect.width, rect.height);
                        m_TexturesShadowOnly[i].SetPixels(colorsShadowOnly);
                        m_TexturesShadowOnly[i].Apply();
                    }
                }
                else
                {
                    if (m_TexturesShadowOnly[i] != null)
                    {
                        Destroy(m_TexturesShadowOnly[i]);
                        m_TexturesShadowOnly[i] = null;
                    }
                }
            }
        }

        Color StringToColor(string s)
        {
            string c = s.Substring(5, s.Length - 6);
            string[] p = c.Split(new string[] { ", " }, System.StringSplitOptions.None);

            return new Color(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]));
        }

        void Free()
        {
            Free(ref m_TexturesShadowOnly);
            Free(ref m_TexturesBodyOnly);
            Free(ref m_TexturesBodyColor);
        }

        void Free(ref Texture2D[] texs)
        {
            for (int i = 0; i < texs.Length; i++)
            {
                if (texs[i] != null)
                {
                    Destroy(texs[i]);
                    texs[i] = null;
                }
            }

            texs = null;
        }

        #if UNITY_EDITOR
        void SettingSprites()
        {
            for (int i = 0; i < m_MaxDirection; i++)
            {
                if (!IsUnnecessaryDirection(i))
                {
                    for (int j = 0; j < m_SpriteQuantity; j++)
                    {
                        SettingSprite(i, GetSpriteFilePathShort(i, j));
                        SettingSprite(i, GetShadowSpriteFilePathShort(i, j));
                        SettingSprite(i, GetColorSpriteFilePathShort(i, j));
                    }
                }
            }
        }

        void SettingSprite(int dir, string path)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spritePackingTag = m_SpritePackingTag;
                textureImporter.mipmapEnabled = false;

                TextureImporterSettings texSettings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(texSettings);
                texSettings.spriteAlignment = (int)SpriteAlignment.Custom;
                texSettings.spritePivot = new Vector2((float)(SS.Tools.ScreenshotTools.maxWidth / 2 * m_TextureScale - m_Rects[dir].x) / m_Rects[dir].width, (float)(SS.Tools.ScreenshotTools.maxHeight / 2 * m_TextureScale - m_Rects[dir].y) / m_Rects[dir].height);
                textureImporter.SetTextureSettings(texSettings);

                textureImporter.SaveAndReimport();
            }
        }

        void ApplyPrefab()
        {
            for (int index = 0; index < m_SpritePrefabs.Length; index++)
            {
                SpriteAnimator[] animators = m_SpritePrefabs[index].GetComponents<SpriteAnimator>();
                SpriteAnimator animator = null;

                for (int i = 0; i < animators.Length; i++)
                {
                    if (animators[i].animationName == m_AnimationName)
                    {
                        animator = animators[i];
                        break;
                    }
                }

                if (animator == null)
                {
                    animator = m_SpritePrefabs[index].AddComponent<SpriteAnimator>();
                    animator.animationName = m_AnimationName;
                }

                animator.animationDuration = m_AnimationDuration;
                animator.directionSprites = new SpriteDirection[m_MaxDirection];

                for (int i = 0; i < m_MaxDirection; i++)
                {
                    animator.directionSprites[i] = new SpriteDirection();
                    animator.directionSprites[i].body = new Sprite[m_SpriteQuantity];
                    animator.directionSprites[i].shadows = new Sprite[m_SpriteQuantity];
                    animator.directionSprites[i].colors = new Sprite[m_SpriteQuantity];

                    for (int j = 0; j < m_SpriteQuantity; j++)
                    {
                        // Body
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GetSpriteFilePathShort(i, j));
                        animator.directionSprites[i].body[j] = sprite;

                        // Shadow
                        switch (m_ShadowType)
                        {
                            case SpriteGeneratorManager.ShadowType.RealShadow:
                                Sprite shadow = AssetDatabase.LoadAssetAtPath<Sprite>(GetShadowSpriteFilePathShort(i, j));
                                animator.directionSprites[i].shadows[j] = shadow;
                                break;

                            case SpriteGeneratorManager.ShadowType.FakeShadowEllipse:
                            case SpriteGeneratorManager.ShadowType.NoShadow:
                                animator.directionSprites[i].shadows[j] = null;
                                break;
                        }

                        // Color
                        Sprite spriteColor = AssetDatabase.LoadAssetAtPath<Sprite>(GetColorSpriteFilePathShort(i, j));
                        animator.directionSprites[i].colors[j] = spriteColor;
                    }

                    if (!string.IsNullOrEmpty(m_AnimationPath) && !IsUnnecessaryDirection(i))
                    {
                        string folder = GetAnimationClipFolderPathFull();
                        string shortFolder = GetAnimationClipFolderPathShort();
                        string file = GetAnimationClipFilePath(i);

                        if (!System.IO.Directory.Exists(folder))
                        {
                            System.IO.Directory.CreateDirectory(folder);
                        }
                        AnimationGenerator.GenerateSpriteAnimation(m_AnimationDuration, m_AnimationFrameRate, m_AnimationLoop, animator.directionSprites[i].body, m_BodyRendererPath, animator.directionSprites[i].shadows, m_ShadowRendererPath, shortFolder, file, animator.directionSprites[i].colors, m_ColorRendererPath);
                    }
                }

                AssignBodyAndShadow(animator);

                if (animator.bodyRenderer == null || animator.shadowRenderer == null)
                {
                    m_SpritePrefabs[index].SetActive(false);
                    GameObject go = Instantiate(m_SpritePrefabs[index]);
                    go.name = m_SpritePrefabs[index].name;
                    
                    CreateChildRenderer(go.transform, m_BodyRendererPath);
                    
                    if (!string.IsNullOrEmpty(m_ShadowRendererPath))
                    {
                        CreateChildRenderer(go.transform, m_ShadowRendererPath);
                    }
                    
                    if (!string.IsNullOrEmpty(m_ColorRendererPath))
                    {
                        CreateChildRenderer(go.transform, m_ColorRendererPath);
                    }
                    
                    PrefabUtility.ReplacePrefab(go, m_SpritePrefabs[index], ReplacePrefabOptions.ReplaceNameBased);
                    
                    Destroy(go);
                    
                    m_SpritePrefabs[index].SetActive(true);
                    animator = m_SpritePrefabs[index].GetComponent<SpriteAnimator>();
                    
                    animator.bodyRenderer = animator.transform.Find(m_BodyRendererPath).GetComponent<SpriteRenderer>();
                    
                    if (!string.IsNullOrEmpty(m_ShadowRendererPath))
                    {
                        animator.shadowRenderer = animator.transform.Find(m_ShadowRendererPath).GetComponent<SpriteRenderer>();
                    }
                    
                    if (!string.IsNullOrEmpty(m_ColorRendererPath))
                    {
                        animator.colorRenderer = animator.transform.Find(m_ColorRendererPath).GetComponent<SpriteRenderer>();
                    }
                }

                animator.SetDefaultSprite();

                switch (m_ShadowType)
                {
                    case SpriteGeneratorManager.ShadowType.RealShadow:
                        break;

                    case SpriteGeneratorManager.ShadowType.FakeShadowEllipse:
                        if (animator.shadowRenderer != null)
                        {
                            animator.shadowRenderer.color = new Color(0, 0, 0, m_ShadowColor.a);
                            animator.shadowRenderer.sprite = m_FakeShadowSprite;
                        }
                        break;

                    case SpriteGeneratorManager.ShadowType.NoShadow:
                        if (animator.shadowRenderer != null)
                        {
                            animator.shadowRenderer.sprite = null;
                        }
                        break;
                }

                EditorUtility.SetDirty(m_SpritePrefabs[index]);
            }
        }

        void AssignBodyAndShadow(SpriteAnimator animator)
        {
            if (animator.bodyRenderer == null)
            {
                Transform body = animator.transform.Find(m_BodyRendererPath);
                if (body != null)
                {
                    animator.bodyRenderer = body.GetComponent<SpriteRenderer>();
                }
            }

            if (animator.shadowRenderer == null)
            {
                Transform shadow = animator.transform.Find(m_ShadowRendererPath);
                if (shadow != null)
                {
                    animator.shadowRenderer = shadow.GetComponent<SpriteRenderer>();
                }
            }

            if (animator.colorRenderer == null)
            {
                Transform color = animator.transform.Find(m_ColorRendererPath);
                if (color != null)
                {
                    animator.colorRenderer = color.GetComponent<SpriteRenderer>();
                }
            }
        }

        Color MixingColor(Color fg, Color bg)
        {
            Color r = new Color();
            r.a = 1 - (1 - fg.a) * (1 - bg.a);
            if (r.a < 1.0e-6) return r; // Fully transparent -- R,G,B not important
            r.r = fg.r * fg.a / r.a + bg.r * bg.a * (1 - fg.a) / r.a;
            r.g = fg.g * fg.a / r.a + bg.g * bg.a * (1 - fg.a) / r.a;
            r.b = fg.b * fg.a / r.a + bg.b * bg.a * (1 - fg.a) / r.a;

            return r;
        }

        SpriteRenderer CreateChildRenderer(Transform t, string path)
        {
            string[] children = path.Split('/');

            int index = 0;
            Transform p = t;

            while (index < children.Length)
            {
                Transform c = p.Find(children[index]);
                if (c == null)
                {
                    GameObject go = new GameObject(children[index]);

                    c = go.transform;
                    c.SetParent(p);
                    c.localPosition = Vector3.zero;
                    switch (m_GamePlane)
                    {
                        case SpriteGeneratorManager.GamePlane.XZ:
                            c.localEulerAngles = new Vector3(90, 0, 0);
                            break;
                        case SpriteGeneratorManager.GamePlane.XY:
                            c.localEulerAngles = new Vector3(0, 0, 0);
                            break;
                    }
                    c.localScale = Vector3.one;
                }

                if (index == children.Length - 1)
                {
                    SpriteRenderer ren = c.GetComponent<SpriteRenderer>();
                    if (ren == null)
                    {
                        ren = c.gameObject.AddComponent<SpriteRenderer>();
                    }
                    ren.material = m_OutputMaterial;

                    return ren;
                }

                index++;
                p = c;
            }

            return null;
        }
        #endif

        void DummyMethod()
        {
            m_SpriteQuantity = 1;
            Debug.Log(m_SpritePackingTag);
            Debug.Log(m_SpritePrefabs);
            Debug.Log(m_AnimationDuration);
            Debug.Log(m_AnimationFrameRate);
            Debug.Log(m_AnimationLoop);
            Debug.Log(m_BodyRendererPath);
            Debug.Log(m_ShadowType);
            Debug.Log(m_FakeShadowSprite);
            Debug.Log(m_ShadowRendererPath);
            Debug.Log(m_ColorRendererPath);
            Debug.Log(m_GamePlane);
        }
    }
}