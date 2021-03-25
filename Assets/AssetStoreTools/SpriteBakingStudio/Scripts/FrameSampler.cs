using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SBS
{
    public class FrameSampler
    {
        private static FrameSampler instance;
        public static FrameSampler GetInstance()
        {
            if (instance == null)
                instance = new FrameSampler();
            return instance;
        }

        private SpriteBakingStudio studio = null;
        private StudioSetting setting = null;

        public delegate void EndDelegate();
        public EndDelegate OnEnd;

        public enum SamplingState
        {
            Initialize = 0,
            BeginFrame,
            CaptureFrame,
            EndFrame,
            Finalize
        }
        private SimpleStateMachine<SamplingState> stateMachine;

        private int currFrameNumber = 0;
        private float currFrameTime = 0.0f;

        private int resolutionX, resolutionY;
        private const int MIN_LENGTH = 200;

        private CameraClearFlags tmpCamClearFlags;
        private Color tmpCamBgColor;

        private int frameSize = 1;

        private ScreenPoint screenPivot = new ScreenPoint(0, 0);
        private TextureBound exTexBound;

        private Vector3 camViewInitPos = Vector3.zero;

		private double prevTime = 0.0;

        public bool IsSamplingNow()
        {
            return (stateMachine != null);
        }

        public void SampleFrames(SpriteBakingStudio studio)
        {
            this.studio = studio;
            this.setting = studio.setting;

#if UNITY_EDITOR
            EditorApplication.update -= UpdateState;
            EditorApplication.update += UpdateState;
#endif

            stateMachine = new SimpleStateMachine<SamplingState>();
            stateMachine.AddState(SamplingState.Initialize, OnInitialize);
            stateMachine.AddState(SamplingState.BeginFrame, OnBeginFrame);
            stateMachine.AddState(SamplingState.CaptureFrame, OnCaptureFrame);
            stateMachine.AddState(SamplingState.EndFrame, OnEndFrame);
            stateMachine.AddState(SamplingState.Finalize, OnFinalize);

            stateMachine.ChangeState(SamplingState.Initialize);
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
                setting.model.obj.UpdateModel(Frame.begin);

                currFrameNumber = 0;
                currFrameTime = 0.0f;
                resolutionX = (int)setting.frame.resolution.x;
                resolutionY = (int)setting.frame.resolution.y;
                Camera.main.targetTexture = new RenderTexture(resolutionX, resolutionY, 24, RenderTextureFormat.ARGB32);
                tmpCamClearFlags = Camera.main.clearFlags;
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                tmpCamBgColor = Camera.main.backgroundColor;

                frameSize = !setting.IsStaticModel() ? setting.frame.frameSize : 1;

                TileUtils.HideAllTiles();

                Vector3 screenPivot3D = Camera.main.WorldToScreenPoint(setting.model.obj.GetPivotPosition());
                screenPivot.x = (int)screenPivot3D.x;
                screenPivot.y = (int)screenPivot3D.y;

                exTexBound = new TextureBound();

                studio.samplings.Clear();
                studio.selectedFrames.Clear();

                camViewInitPos = Camera.main.transform.position;

                stateMachine.ChangeState(SamplingState.BeginFrame);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(SamplingState.Finalize);
            }
        }

        public void OnBeginFrame()
		{
            try
            {
#if UNITY_EDITOR
                int shownCurrFrameNumber = currFrameNumber + 1;
                float progress = (float)shownCurrFrameNumber / frameSize;
                EditorUtility.DisplayProgressBar("Sampling...", "Frame: " + shownCurrFrameNumber + " (" + ((int)(progress * 100f)) + "%)", progress);
#endif

                float frameRatio = 0.0f;
                if (currFrameNumber > 0 && currFrameNumber < frameSize)
                    frameRatio = (float)currFrameNumber / (float)(frameSize - 1);

                currFrameTime = setting.model.obj.GetTimeForRatio(frameRatio);

                setting.model.obj.UpdateModel(new Frame(currFrameNumber, currFrameTime));

                stateMachine.ChangeState(SamplingState.CaptureFrame);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(SamplingState.Finalize);
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

                studio.samplings.Add(new SamplingData(tex, currFrameTime));

                TextureBound texBound = new TextureBound();
                if (!TextureUtils.CalcTextureBound(tex, screenPivot, texBound))
                {
                    texBound.minX = screenPivot.x - 1;
                    texBound.maxX = screenPivot.x + 1;
                    texBound.minY = screenPivot.y - 1;
                    texBound.maxY = screenPivot.y + 1;
                }

                TextureUtils.CalcTextureSymmetricBound(
                    false, setting.IsTopView(),
                    screenPivot, resolutionX, resolutionY,
                    texBound, exTexBound);

                stateMachine.ChangeState(SamplingState.EndFrame);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                stateMachine.ChangeState(SamplingState.Finalize);
            }
        }

        public void OnEndFrame()
        {
            currFrameNumber++;

            if (currFrameNumber < frameSize)
                stateMachine.ChangeState(SamplingState.BeginFrame);
            else
                stateMachine.ChangeState(SamplingState.Finalize);
        }

        public void OnFinalize()
        {
#if UNITY_EDITOR
            EditorApplication.update -= UpdateState;
#endif

            try
            {
                stateMachine = null;

                TileUtils.UpdateTile(setting);

                setting.model.obj.UpdateModel(Frame.begin);

                Camera.main.targetTexture = null;
                Camera.main.clearFlags = tmpCamClearFlags;
                Camera.main.backgroundColor = tmpCamBgColor;

                if (setting.IsStaticRealShadow())
                    StudioUtility.BakeStaticShadow(setting);

                TrimAll();

                for (int i = 0; i < studio.samplings.Count; i++)
                    studio.selectedFrames.Add(new Frame(i, studio.samplings[i].time));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
#if UNITY_EDITOR
                EditorUtility.ClearProgressBar();
#endif

                if (OnEnd != null)
                    OnEnd();
            }
        }

        private void TrimAll()
        {
            try
            {
                int margin = 5;
                if (exTexBound.minX - margin >= 0)
                    exTexBound.minX -= margin;
                if (exTexBound.maxX + margin < resolutionX)
                    exTexBound.maxX += margin;
                if (exTexBound.minY - margin >= 0)
                    exTexBound.minY -= margin;
                if (exTexBound.maxY + margin < resolutionY)
                    exTexBound.maxY += margin;

                int trimWidth = exTexBound.maxX - exTexBound.minX + 1;
                int trimHeight = exTexBound.maxY - exTexBound.minY + 1;

                foreach (SamplingData sample in studio.samplings)
                {
                    Color[] originalColors = sample.tex.GetPixels();
                    Color[] resultColors = new Color[trimWidth * trimHeight];

                    for (int y = 0; y < trimHeight; y++)
                    {
                        for (int x = 0; x < trimWidth; x++)
                        {
                            int index = (exTexBound.minY + y) * sample.tex.width + (exTexBound.minX + x);
                            Color color = originalColors[index];
                            if (color != Color.clear)
                                resultColors[y * trimWidth + x] = color;
                        }
                    }

                    Texture2D trimTex = new Texture2D(trimWidth, trimHeight, TextureFormat.ARGB32, false);
                    trimTex.SetPixels(resultColors);

                    if (trimWidth >= trimHeight && trimWidth > MIN_LENGTH)
                    {
                        float ratio = (float)trimHeight / (float)trimWidth;
                        int newTrimWidth = MIN_LENGTH;
                        int newTrimHeight = Mathf.RoundToInt(newTrimWidth * ratio);
                        sample.tex = TextureUtils.ScaleTexture(trimTex, newTrimWidth, newTrimHeight);
                    }
                    else if (trimWidth < trimHeight && trimHeight > MIN_LENGTH)
                    {
                        float ratio = (float)trimWidth / (float)trimHeight;
                        int newTrimHeight = MIN_LENGTH;
                        int newTrimWidth = Mathf.RoundToInt(newTrimHeight * ratio);
                        sample.tex = TextureUtils.ScaleTexture(trimTex, newTrimWidth, newTrimHeight);
                    }
                    else
                    {
                        sample.tex = trimTex;
                    }

                    sample.tex.Apply();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
