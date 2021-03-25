using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SBS
{
    public class AnimatedModelBaker : ModelBaker
    {
        private List<Texture2D> viewTextures = null;
        private List<ScreenPoint> viewPivots = null;
        private List<List<Texture2D>> viewTexturesList = new List<List<Texture2D>>();
        private List<List<ScreenPoint>> viewPivotsList = new List<List<ScreenPoint>>();

        protected override BakingState OnInitialize_()
        {
            currModel = setting.model.obj;

            return BakingState.BeginModel;
        }

        protected override BakingState OnBeginView_()
        {
            if (!trimClone.allUnified)
                exTexBound = new TextureBound();

            viewTextures = new List<Texture2D>();
            viewPivots = new List<ScreenPoint>();

            return BakingState.BeginFrame;
        }

        protected override BakingState OnBeginFrame_()
        {
#if UNITY_EDITOR
            int shownCurrFrameIndex = currFrameIndex + 1;
            float progress = (float)(currViewIndex * frameCount + shownCurrFrameIndex) / (checkedViewSize * frameCount);
            if (checkedViewSize == 0)
                EditorUtility.DisplayProgressBar("Progress...", "Frame: " + shownCurrFrameIndex + " (" + ((int)(progress * 100f)) + "%)", progress);
            else
                EditorUtility.DisplayProgressBar("Progress...", "View: " + currViewName + " | Frame: " + shownCurrFrameIndex + " (" + ((int)(progress * 100f)) + "%)", progress);
#endif

            return BakingState.CaptureFrame;
        }

        protected override BakingState OnCaptureFrame_(Texture2D tex, ScreenPoint pivot)
        {
            if (outputClone.type == OutputType.Separately && !trimClone.useUnifiedSize)
            {
                BakeSeparately(tex, pivot, currViewName, currFrameIndex);
            }
            else if (outputClone.type == OutputType.SpriteSheet || trimClone.useUnifiedSize)
            {
                viewTextures.Add(tex);
                viewPivots.Add(pivot);
            }

            return BakingState.EndFrame;
        }

        protected override BakingState OnEndView_()
        {
            if (outputClone.type == OutputType.Separately && trimClone.useUnifiedSize)
            {
                if (!trimClone.allUnified)
                {
                    TrimToUnifiedSize(viewTextures, viewPivots);
                    BakeSeparately(viewTextures, viewPivots, currViewName);
                }
                else
                {
                    viewTexturesList.Add(viewTextures);
                    viewPivotsList.Add(viewPivots);
                }
            }
            else if (outputClone.type == OutputType.SpriteSheet)
            {
                if (!outputClone.allInOneAtlas)
                {
                    Debug.Assert(!setting.IsSingleStaticModel());

                    if (!trimClone.useUnifiedSize)
                    {
                        // trimmed or not
                        BakeSpriteSheet(viewTextures, viewPivots, currViewName);
                    }
                    else
                    {
                        if (!trimClone.allUnified)
                        {
                            TrimToUnifiedSize(viewTextures, viewPivots);
                            BakeSpriteSheet(viewTextures, viewPivots, currViewName);
                        }
                        else
                        {
                            viewTexturesList.Add(viewTextures);
                            viewPivotsList.Add(viewPivots);
                        }
                    }
                }
                else // output.allInOneAtlas
                {
                    Debug.Assert(setting.IsStaticModel());
                    viewTexturesList.Add(viewTextures);
                    viewPivotsList.Add(viewPivots);
                }
            }

            return BakingState.Finalize;
        }

        protected override void OnFinalize_()
        {
            if (trimClone.allUnified || outputClone.allInOneAtlas)
            {
                Debug.Assert(viewTexturesList.Count == viewPivotsList.Count);

                List<string> viewNames = new List<string>();
                foreach (CheckedView checkedView in setting.view.checkedViews)
                    viewNames.Add(checkedView.name);

                if (trimClone.allUnified)
                {
                    Debug.Assert(!setting.IsSingleStaticModel());

                    for (int i = 0; i < viewTexturesList.Count; i++)
                    {
                        TrimToUnifiedSize(viewTexturesList[i], viewPivotsList[i]);

                        if (outputClone.type == OutputType.Separately)
                            BakeSeparately(viewTexturesList[i], viewPivotsList[i], viewNames[i]);
                        else if (outputClone.type == OutputType.SpriteSheet)
                            BakeSpriteSheet(viewTexturesList[i], viewPivotsList[i], viewNames[i]);
                    }
                }
                else if (outputClone.allInOneAtlas)
                {
                    Debug.Assert(setting.IsSingleStaticModel());

                    List<Texture2D> allViewTextures = new List<Texture2D>();
                    List<ScreenPoint> allViewPivots = new List<ScreenPoint>();
                    for (int i = 0; i < viewTexturesList.Count; i++)
                    {
                        allViewTextures.AddRange(viewTexturesList[i]);
                        allViewPivots.AddRange(viewPivotsList[i]);
                    }

                    if (trimClone.useUnifiedSize)
                        TrimToUnifiedSize(allViewTextures, allViewPivots);

                    Debug.Assert(allViewTextures.Count == allViewPivots.Count);
                    Debug.Assert(allViewTextures.Count == viewNames.Count);

                    BakeSpriteSheet(allViewTextures, allViewPivots, "", viewNames);
                }
            }
        }

        protected override void FinalizeOutputFilesForView(string viewName, Sprite[] sprites)
        {
#if UNITY_EDITOR
            Debug.Assert(viewName.Length > 0);

            AnimationClip animClip = new AnimationClip
            {
                frameRate = setting.frameSamples
            };

            if (setting.output.loopAnimationClip)
            {
                AnimationClipSettings animClipSettings = AnimationUtility.GetAnimationClipSettings(animClip);
                animClipSettings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(animClip, animClipSettings);
            }

            EditorCurveBinding spriteCurveBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

            ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                spriteKeyFrames[i] = new ObjectReferenceKeyframe();
                float unitTime = 1f / setting.frameSamples;
                spriteKeyFrames[i].time = setting.spriteInterval * i * unitTime;
                spriteKeyFrames[i].value = sprites[i];
            }

            AnimationUtility.SetObjectReferenceCurve(animClip, spriteCurveBinding, spriteKeyFrames);

            string clipFilePath = Path.Combine(outputPath, setting.path.fileName + "_" + viewName + ".anim");
            AssetDatabase.CreateAsset(animClip, clipFilePath);
#endif
        }
    }
}
