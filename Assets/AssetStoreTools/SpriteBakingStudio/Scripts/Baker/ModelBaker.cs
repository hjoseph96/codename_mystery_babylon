using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SBS
{
    public abstract class ModelBaker
    {
        protected enum BakingState
        {
            Initialize,
            BeginModel,
            BeginView,
            BeginFrame,
            CaptureFrame,
            EndView,
            EndFrame,
            EndModel,
            Finalize
        }
        private SimpleStateMachine<BakingState> stateMachine = null;

        protected abstract BakingState OnInitialize_();
        protected virtual BakingState OnBeginModel_() { return BakingState.BeginView; }
        protected virtual BakingState OnBeginView_() { return BakingState.BeginFrame; }
        protected abstract BakingState OnBeginFrame_();
        protected abstract BakingState OnCaptureFrame_(Texture2D tex, ScreenPoint pivot);
        protected virtual BakingState OnEndView_() { return BakingState.EndModel; }
        protected virtual BakingState OnEndModel_() { return BakingState.Finalize; }
        protected abstract void OnFinalize_();

        protected SpriteBakingStudio studio;
        protected StudioSetting setting;

        protected StudioModel currModel = null;

        protected int checkedViewSize = 0;
        protected int currViewIndex = 0;
        protected string currViewName;

        protected int frameCount = 0;
        protected int currFrameIndex = 0;
        private int resolutionX, resolutionY;

        private CameraClearFlags tempCamClearFlags;
        private Color tempCamBgColor;
        private float tempCameraSize;
        private Vector3 tempCameraPos;
        private Quaternion tempCameraRot;

        protected TrimProperty trimClone = null;
        protected OutputProperty outputClone = null;

        protected string outputPath;

        private ScreenPoint currPivot = new ScreenPoint(0, 0);
        protected TextureBound exTexBound;

        private Vector3 camViewInitPos = Vector3.zero;

        private double prevTime = 0.0;
        
        public bool IsBakingNow()
        {
            return (stateMachine != null);
        }

        public void Bake(SpriteBakingStudio studio_)
        {
            studio = studio_;
            setting = studio_.setting;

#if UNITY_EDITOR
            EditorApplication.update -= UpdateState;
            EditorApplication.update += UpdateState;
#endif

            stateMachine = new SimpleStateMachine<BakingState>();
            stateMachine.AddState(BakingState.Initialize, OnInitialize);
            stateMachine.AddState(BakingState.BeginModel, OnBeginModel);
            stateMachine.AddState(BakingState.BeginView, OnBeginView);
            stateMachine.AddState(BakingState.BeginFrame, OnBeginFrame);
            stateMachine.AddState(BakingState.CaptureFrame, OnCaptureFrame);
            stateMachine.AddState(BakingState.EndView, OnEndView);
            stateMachine.AddState(BakingState.EndFrame, OnEndFrame);
            stateMachine.AddState(BakingState.EndModel, OnEndModel);
            stateMachine.AddState(BakingState.Finalize, OnFinalize);

            stateMachine.ChangeState(BakingState.Initialize);
        }

        public void UpdateState()
        {
            if (stateMachine != null)
                stateMachine.Update();
        }

        public void OnInitialize()
        {
            try
            {
#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("Progress...", "Ready...", 0.0f);
#endif

                checkedViewSize = setting.view.CountCheckedViews();

                resolutionX = (int)setting.frame.resolution.x;
                resolutionY = (int)setting.frame.resolution.y;
                Camera.main.targetTexture = new RenderTexture(resolutionX, resolutionY, 24, RenderTextureFormat.ARGB32);
                tempCamClearFlags = Camera.main.clearFlags;
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                tempCamBgColor = Camera.main.backgroundColor;
                tempCameraSize = Camera.main.orthographicSize;
                tempCameraPos = Camera.main.transform.position;
                tempCameraRot = Camera.main.transform.rotation;

                BakingState nextState = OnInitialize_();

                exTexBound = new TextureBound();

                if (studio.samplings.Count == 0)
                {
                    if (setting.IsStaticModel())
                    {
                        studio.selectedFrames.Add(Frame.begin);
                    }
                    else
                    {
                        for (int i = 0; i < setting.frame.frameSize; ++i)
                        {
                            float frameRatio = 0.0f;
                            if (i > 0 && i < setting.frame.frameSize)
                                frameRatio = (float)i / (float)(setting.frame.frameSize - 1);

                            float time = currModel.GetTimeForRatio(frameRatio);
                            studio.selectedFrames.Add(new Frame(i, time));
                        }
                    }
                }

                frameCount = studio.selectedFrames.Count;

                TileUtils.HideAllTiles();

                int assetRootIndex = setting.path.directoryPath.IndexOf("Assets");
                if (assetRootIndex < 0)
                    throw new Exception(string.Format("{0} is out of the Assets folder.", outputPath));
                string dirPath = setting.path.directoryPath.Substring(assetRootIndex);
                string folderName = setting.path.fileName + "_" + StudioUtility.MakeDateTimeString();
                outputPath = Path.Combine(dirPath, folderName);
                Directory.CreateDirectory(outputPath);

                if (setting.output.type == OutputType.SpriteSheet)
                    InitializeOutputFiles();
                
                trimClone = setting.trim.CloneForBaking(setting.IsSingleStaticModel());
                outputClone = setting.output.CloneForBaking(setting.IsStaticModel());

                stateMachine.ChangeState(nextState);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(BakingState.Finalize);
            }
        }

        public void OnBeginModel()
        {
            try
            {
                BakingState nextState = OnBeginModel_();

                currModel.UpdateModel(Frame.begin);

                currViewIndex = 0;

                stateMachine.ChangeState(nextState);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(BakingState.Finalize);
            }
        }

        public void OnBeginView()
        {
            try
            {
                BakingState nextState = OnBeginView_();

                setting.view.checkedViews[currViewIndex].func();
                currViewName = setting.view.checkedViews[currViewIndex].name;

                Vector3 pivot3D = Camera.main.WorldToScreenPoint(currModel.GetPivotPosition());
                currPivot.x = (int)pivot3D.x;
                currPivot.y = (int)pivot3D.y;

                currFrameIndex = 0;

                camViewInitPos = Camera.main.transform.position;

                stateMachine.ChangeState(nextState);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(BakingState.Finalize);
            }
        }

        public void OnBeginFrame()
        {
            try
            {
                BakingState nextState = OnBeginFrame_();

                Frame frame = studio.selectedFrames[currFrameIndex];
                currModel.UpdateModel(frame);

                stateMachine.ChangeState(nextState);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(BakingState.Finalize);
            }
        }

        public void OnCaptureFrame()
        {
            try
            {
#if UNITY_EDITOR
                double deltaTime = EditorApplication.timeSinceStartup - prevTime;
                if (deltaTime < setting.frame.delay)
                    return;
                prevTime = EditorApplication.timeSinceStartup;
#endif

                Camera.main.transform.position = camViewInitPos;

                Texture2D tex = StudioUtility.PrepareShadowAndExtractTexture(setting);

                TextureBound texBound = new TextureBound();
                if (!TextureUtils.CalcTextureBound(tex, currPivot, texBound))
                {
                    texBound.minX = currPivot.x - 1;
                    texBound.maxX = currPivot.x + 1;
                    texBound.minY = currPivot.y - 1;
                    texBound.maxY = currPivot.y + 1;
                }

                ScreenPoint pivot = new ScreenPoint(currPivot.x, currPivot.y);

                if (trimClone.on)
                {
                    if (!trimClone.useUnifiedSize)
                    {
                        tex = TextureUtils.TrimTexture(tex, texBound, trimClone.spriteMargin);
                        TextureUtils.UpdatePivot(pivot, texBound, trimClone.spriteMargin);
                    }
                }

                BakingState nextState = OnCaptureFrame_(tex, pivot);

                if (outputClone.type == OutputType.SpriteSheet || trimClone.useUnifiedSize)
                {
                    if (trimClone.useUnifiedSize)
                    {
                        TextureUtils.CalcTextureSymmetricBound(
                            trimClone.pivotSymmetrically,
                            setting.IsTopView(),
                            pivot, resolutionX, resolutionY,
                            texBound, exTexBound);
                    }
                }

                stateMachine.ChangeState(nextState);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(BakingState.Finalize);
            }
        }

        public void OnEndFrame()
        {
            try
            {
                currFrameIndex++;

                if (currFrameIndex < frameCount)
                    stateMachine.ChangeState(BakingState.BeginFrame);
                else
                    stateMachine.ChangeState(BakingState.EndView);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(BakingState.Finalize);
            }
        }

        public void OnEndView()
        {
            try
            {
                BakingState endState = OnEndView_();

                currViewIndex++;

                if (currViewIndex < checkedViewSize)
                    stateMachine.ChangeState(BakingState.BeginView);
                else
                    stateMachine.ChangeState(endState);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(BakingState.Finalize);
            }
        }

        public void OnEndModel()
        {
            try
            {
                BakingState nextState = OnEndModel_();

                stateMachine.ChangeState(nextState);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(BakingState.Finalize);
            }
        }

        public void OnFinalize()
        {
            try
            {
#if UNITY_EDITOR
                EditorApplication.update -= UpdateState;
#endif
                stateMachine = null;

                if (studio.samplings.Count == 0)
                    studio.selectedFrames.Clear();

                currModel.UpdateModel(Frame.begin);

                if (setting.view.checkedViews.Count >= 2)
                    setting.view.checkedViews[0].func();

                if (setting.IsStaticRealShadow() && setting.shadow.staticShadowVisible)
                    StudioUtility.BakeStaticShadow(setting);

                OnFinalize_();

                TileUtils.UpdateTile(setting);

                Camera.main.targetTexture = null;
                Camera.main.clearFlags = tempCamClearFlags;
                Camera.main.backgroundColor = tempCamBgColor;
                Camera.main.orthographicSize = tempCameraSize;
                Camera.main.transform.position = tempCameraPos;
                Camera.main.transform.rotation = tempCameraRot;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
#if UNITY_EDITOR
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
#endif
            }
        }

        protected void TrimToUnifiedSize(List<Texture2D> textures, List<ScreenPoint> pivots)
        {
            try
            {
                for (int i = 0; i < textures.Count; ++i)
                {
                    textures[i] = TextureUtils.TrimTexture(textures[i], exTexBound, trimClone.spriteMargin);
                    TextureUtils.UpdatePivot(pivots[i], exTexBound, trimClone.spriteMargin);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        protected void BakeSeparately(List<Texture2D> textures, List<ScreenPoint> pivots, string subName, List<string> detailNames)
        {
            Debug.Assert(textures.Count == pivots.Count);
            Debug.Assert(textures.Count == detailNames.Count);

            try
            {
                for (int i = 0; i < textures.Count; i++)
                    BakeSeparately(textures[i], pivots[i], subName, detailNames[i]);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        protected void BakeSeparately(List<Texture2D> textures, List<ScreenPoint> pivots, string subName)
        {
            try
            {
                for (int i = 0; i < textures.Count; i++)
                    BakeSeparately(textures[i], pivots[i], subName, i);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        protected void BakeSeparately(Texture2D tex, ScreenPoint pivot, string subName, int frame)
        {
            try
            {
                string detailName = frame.ToString().PadLeft((frameCount - 1).ToString().Length, '0');
                BakeSeparately(tex, pivot, subName, detailName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        protected void BakeSeparately(Texture2D tex, ScreenPoint pivot, string subName, string detailName = "")
        {
            try
            {
                string fileFullName = setting.path.fileName;
                if (subName.Length > 0)
                    fileFullName += "_" + subName;
                if (detailName.Length > 0)
                    fileFullName += "_" + detailName;

                string filePath = TextureUtils.SaveTexture(outputPath, fileFullName, tex);

                if (tex != null)
                {
#if UNITY_EDITOR    
                    AssetDatabase.ImportAsset(filePath);

                    TextureImporter texImporter = (TextureImporter)AssetImporter.GetAtPath(filePath);
                    if (texImporter != null)
                    {
                        texImporter.textureType = TextureImporterType.Sprite;
                        texImporter.spriteImportMode = SpriteImportMode.Multiple;

                        SpriteMetaData[] metaData = new SpriteMetaData[1];
                        metaData[0].name = "0";
                        metaData[0].rect = new Rect(0.0f, 0.0f, (float)tex.width, (float)tex.height);
                        metaData[0].alignment = (int)SpriteAlignment.Custom;
                        metaData[0].pivot = new Vector2((float)pivot.x / (float)tex.width,
                                                        (float)pivot.y / (float)tex.height);

                        texImporter.spritesheet = metaData;
                        AssetDatabase.ImportAsset(filePath);
                    }
#endif
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        protected void BakeSpriteSheet(List<Texture2D> textures, List<ScreenPoint> pivots, string subName)
        {
            try
            {
                List<string> spriteNames = new List<string>();
                for (int i = 0; i < textures.Count; ++i)
                    spriteNames.Add(i.ToString());
                BakeSpriteSheet(textures, pivots, subName, spriteNames);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        protected void BakeSpriteSheet(List<Texture2D> textures, List<ScreenPoint> pivots, string subName, List<string> spriteNames)
        {
            Debug.Assert(textures.Count == pivots.Count);
            Debug.Assert(textures.Count == spriteNames.Count);

            try
            {
                int atlasLength = 64;
                if (!int.TryParse(studio.atlasSizes[outputClone.atlasSizeIndex], out atlasLength))
                    atlasLength = 2048;

                Rect[] atlasRects = null;
                Texture2D atlasTex = null;

                if (outputClone.algorithm == PackingAlgorithm.Optimized)
                {
                    atlasTex = new Texture2D(atlasLength, atlasLength, TextureFormat.ARGB32, false);

                    atlasRects = atlasTex.PackTextures(textures.ToArray(), outputClone.spritePadding, atlasLength);
                    for (int i = 0; i < atlasRects.Length; i++)
                    {
                        Texture2D tex = textures[i];
                        float newX = atlasRects[i].x * atlasTex.width;
                        float newY = atlasRects[i].y * atlasTex.height;
                        atlasRects[i] = new Rect(newX, newY, (float)tex.width, (float)tex.height);
                    }
                }
                else if (outputClone.algorithm == PackingAlgorithm.InOrder)
                {
                    int maxSpriteWidth = int.MinValue;
                    int maxSpriteHeight = int.MinValue;
                    foreach (Texture2D tex in textures)
                    {
                        maxSpriteWidth = Mathf.Max(tex.width, maxSpriteWidth);
                        maxSpriteHeight = Mathf.Max(tex.height, maxSpriteHeight);
                    }

                    while (atlasLength < maxSpriteWidth || atlasLength < maxSpriteHeight)
                        atlasLength *= 2;

                    int atlasWidth = atlasLength;
                    int atlasHeight = atlasLength;

                    while (true)
                    {
                        atlasTex = new Texture2D(atlasWidth, atlasHeight, TextureFormat.ARGB32, false);
                        // make clear colors
                        Color[] resultColors = new Color[atlasTex.height * atlasTex.width];
                        atlasTex.SetPixels(resultColors);

                        atlasRects = new Rect[textures.Count];
                        int originY = atlasHeight - maxSpriteHeight;

                        bool needMultiply = false;

                        int atlasRectIndex = 0;
                        int currX = 0, currY = originY;
                        foreach (Texture2D tex in textures)
                        {
                            if (currX + tex.width > atlasWidth)
                            {
                                if (currY - maxSpriteHeight < 0)
                                {
                                    needMultiply = true;
                                    break;
                                }
                                currX = 0;
                                currY -= (maxSpriteHeight + outputClone.spritePadding);
                            }
                            WriteSpriteToAtlas(atlasTex, tex, currX, currY);
                            atlasRects[atlasRectIndex++] = new Rect(currX, currY, tex.width, tex.height);
                            currX += (tex.width + outputClone.spritePadding);
                        }

                        if (needMultiply)
                        {
                            if (atlasWidth == atlasHeight)
                                atlasWidth *= 2;
                            else // atlasWidth > atlasHeight
                                atlasHeight *= 2;

                            if (atlasWidth > 8192)
                            {
                                Debug.Log("Output sprite sheet size is bigger than 8192 X 8192");
                                return;
                            }
                        }
                        else
                        {
                            atlasLength = atlasWidth;
                            break;
                        }
                    }
                }

                string fileFullName = setting.path.fileName;
                if (subName.Length > 0)
                    fileFullName += "_" + subName;

                string filePath = TextureUtils.SaveTexture(outputPath, fileFullName, atlasTex);

#if UNITY_EDITOR
                AssetDatabase.ImportAsset(filePath);

                TextureImporter texImporter = (TextureImporter)AssetImporter.GetAtPath(filePath);
                if (texImporter != null)
                {
                    texImporter.textureType = TextureImporterType.Sprite;
                    texImporter.spriteImportMode = SpriteImportMode.Multiple;
                    texImporter.maxTextureSize = atlasLength;

                    int texCount = textures.Count;
                    SpriteMetaData[] metaData = new SpriteMetaData[texCount];
                    for (int i = 0; i < texCount; i++)
                    {
                        string name = spriteNames[i];
                        if (!setting.IsStaticModel())
                            name = name.PadLeft((spriteNames.Count - 1).ToString().Length, '0');
                        metaData[i].name = name;
                        metaData[i].rect = atlasRects[i];
                        metaData[i].alignment = (int)SpriteAlignment.Custom;
                        metaData[i].pivot = new Vector2((float)pivots[i].x / (float)textures[i].width,
                                                        (float)pivots[i].y / (float)textures[i].height);
                    }
                    texImporter.spritesheet = metaData;

                    AssetDatabase.ImportAsset(filePath);
                }

                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(filePath).OfType<Sprite>().ToArray();
                FinalizeOutputFilesForView(subName, sprites);
#endif
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WriteSpriteToAtlas(Texture2D atlasTex, Texture2D spriteTex, int startX, int startY)
        {
            Color[] spriteColors = spriteTex.GetPixels();
            Color[] atlasColors = atlasTex.GetPixels();

            for (int y = 0; y < spriteTex.height; ++y)
            {
                for (int x = 0; x < spriteTex.width; ++x)
                {
                    Color color = spriteColors[y * spriteTex.width + x];
                    int index = (startY + y) * atlasTex.width + (startX + x);
                    atlasColors[index] = color;
                }
            }

            atlasTex.SetPixels(atlasColors);
        }

        protected virtual void InitializeOutputFiles() { }
        protected virtual void FinalizeOutputFilesForView(string viewName, Sprite[] sprites) { }
    }
}
