using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SBS
{
    public class StaticModelGroupBaker : ModelBaker
    {
        private int currModelIndex = 0;
        private List<StudioModel> checkedModels = new List<StudioModel>();

        private List<Texture2D> modelTextures = null;
        private List<ScreenPoint> modelPivots = null;
        private List<List<Texture2D>> modelTexturesList = new List<List<Texture2D>>();
        private List<List<ScreenPoint>> modelPivotsList = new List<List<ScreenPoint>>();
        private List<TextureBound> modelTexBounds = new List<TextureBound>();

        protected override BakingState OnInitialize_()
        {
            setting.GetStaticModelGroup().OnInitialize();

            currModelIndex = 0;
            checkedModels = setting.GetStaticModelGroup().GetCheckedModels();

            modelTexturesList.Clear();
            modelPivotsList.Clear();
            modelTexBounds.Clear();

            return BakingState.BeginModel;
        }

        protected override BakingState OnBeginModel_()
        {
            if (!trimClone.allUnified)
                exTexBound = new TextureBound();

            currModel = checkedModels[currModelIndex];
            currModel.gameObject.SetActive(true);

            modelTextures = new List<Texture2D>();
            modelPivots = new List<ScreenPoint>();

            return BakingState.BeginView;
        }

        protected override BakingState OnBeginFrame_()
        {
#if UNITY_EDITOR
            int shownCurrViewIndex = currViewIndex + 1;
            float progress = (float)(currModelIndex * checkedViewSize + shownCurrViewIndex) / (checkedModels.Count * checkedViewSize);
            if (checkedModels.Count == 0)
                EditorUtility.DisplayProgressBar("Progress...", "View: " + shownCurrViewIndex + " (" + ((int)(progress * 100f)) + "%)", progress);
            else
                EditorUtility.DisplayProgressBar("Progress...", "Model: " + currModel.name + " | View: " + shownCurrViewIndex + " (" + ((int)(progress * 100f)) + "%)", progress);
#endif

            return BakingState.CaptureFrame;
        }

        protected override BakingState OnCaptureFrame_(Texture2D tex, ScreenPoint pivot)
        {
            if (outputClone.type == OutputType.Separately && !trimClone.useUnifiedSize)
            {
                BakeSeparately(tex, pivot, currModel.name, currViewName);
            }
            else if (outputClone.type == OutputType.SpriteSheet || trimClone.useUnifiedSize)
            {
                modelTextures.Add(tex);
                modelPivots.Add(pivot);
            }

            return BakingState.EndFrame;
        }

        protected override BakingState OnEndModel_()
        {
            List<string> viewNames = new List<string>();
            foreach (CheckedView checkedView in setting.view.checkedViews)
                viewNames.Add(checkedView.name);

            if (outputClone.type == OutputType.Separately && trimClone.useUnifiedSize)
            {
                if (!trimClone.allUnified)
                {
                    TrimToUnifiedSize(modelTextures, modelPivots);
                    BakeSeparately(modelTextures, modelPivots, currModel.name, viewNames);
                }
                else
                {
                    modelTexturesList.Add(modelTextures);
                    modelPivotsList.Add(modelPivots);
                }
            }
            else if (outputClone.type == OutputType.SpriteSheet)
            {
                if (!trimClone.useUnifiedSize && !outputClone.allInOneAtlas)
                {
                    // trimmed or not
                    BakeSpriteSheet(modelTextures, modelPivots, currModel.name, viewNames);
                }
                else // trim.useUnifiedSize || output.allInOneAtlas
                {
                    if (!trimClone.allUnified && !outputClone.allInOneAtlas)
                    {
                        Debug.Assert(trimClone.useUnifiedSize);
                        TrimToUnifiedSize(modelTextures, modelPivots);
                        BakeSpriteSheet(modelTextures, modelPivots, currModel.name, viewNames);
                    }
                    else // trim.allUnified || output.allInOneAtlas
                    {
                        modelTexturesList.Add(modelTextures);
                        modelPivotsList.Add(modelPivots);

                        if (trimClone.useUnifiedSize && !trimClone.allUnified && outputClone.allInOneAtlas)
                            modelTexBounds.Add(exTexBound);
                    }
                }
            }

            currModel.gameObject.SetActive(false);

            currModelIndex++;

            if (currModelIndex < checkedModels.Count)
                return BakingState.BeginModel;
            else
                return BakingState.Finalize;
        }

        protected override void OnFinalize_()
        {
            setting.GetStaticModelGroup().OnFinalize();

            if (trimClone.allUnified || outputClone.allInOneAtlas)
            {
                Debug.Assert(modelTexturesList.Count == modelPivotsList.Count);

                if (!outputClone.allInOneAtlas)
                {
                    Debug.Assert(trimClone.allUnified);

                    List<string> viewNames = new List<string>();
                    foreach (CheckedView checkedView in setting.view.checkedViews)
                        viewNames.Add(checkedView.name);

                    for (int i = 0; i < modelTexturesList.Count; i++)
                    {
                        TrimToUnifiedSize(modelTexturesList[i], modelPivotsList[i]);

                        if (outputClone.type == OutputType.Separately)
                            BakeSeparately(modelTexturesList[i], modelPivotsList[i], checkedModels[i].name, viewNames);
                        else if (outputClone.type == OutputType.SpriteSheet)
                            BakeSpriteSheet(modelTexturesList[i], modelPivotsList[i], checkedModels[i].name, viewNames);
                    }
                }
                else // output.allInOneAtlas
                {
                    Debug.Assert(outputClone.type == OutputType.SpriteSheet);

                    if (!trimClone.useUnifiedSize)
                    {
                        // trimmed or not
                        List<Texture2D> allModelTextures = new List<Texture2D>();
                        List<ScreenPoint> allModelPivots = new List<ScreenPoint>();
                        List<string> allModelViewNames = new List<string>();
                        for (int i = 0; i < modelTexturesList.Count; i++)
                        {
                            allModelTextures.AddRange(modelTexturesList[i]);
                            allModelPivots.AddRange(modelPivotsList[i]);

                            foreach (CheckedView checkedView in setting.view.checkedViews)
                                allModelViewNames.Add(checkedModels[i].name + "_" + checkedView.name);
                        }

                        Debug.Assert(allModelTextures.Count == allModelPivots.Count);
                        Debug.Assert(allModelTextures.Count == allModelViewNames.Count);

                        BakeSpriteSheet(allModelTextures, allModelPivots, "", allModelViewNames);
                    }
                    else // trim.useUnifiedSize
                    {
                        List<Texture2D> allModelTextures = new List<Texture2D>();
                        List<ScreenPoint> allModelPivots = new List<ScreenPoint>();
                        List<string> allModelViewNames = new List<string>();
                        for (int modeli = 0; modeli < modelTexturesList.Count; modeli++)
                        {
                            if (!trimClone.allUnified)
                            {
                                Debug.Assert(modelTexturesList.Count == modelTexBounds.Count);
                                for (int texi = 0; texi < modelTexturesList[modeli].Count; ++texi)
                                {
                                    modelTexturesList[modeli][texi] =
                                        TextureUtils.TrimTexture(modelTexturesList[modeli][texi], modelTexBounds[modeli], trimClone.spriteMargin);
                                    TextureUtils.UpdatePivot(modelPivotsList[modeli][texi], modelTexBounds[modeli], trimClone.spriteMargin);
                                }
                            }

                            allModelTextures.AddRange(modelTexturesList[modeli]);
                            allModelPivots.AddRange(modelPivotsList[modeli]);

                            foreach (CheckedView checkedView in setting.view.checkedViews)
                                allModelViewNames.Add(checkedModels[modeli].name + "_" + checkedView.name);
                        }

                        Debug.Assert(allModelTextures.Count == allModelPivots.Count);
                        Debug.Assert(allModelTextures.Count == allModelViewNames.Count);

                        if (trimClone.allUnified)
                            TrimToUnifiedSize(allModelTextures, allModelPivots);

                        BakeSpriteSheet(allModelTextures, allModelPivots, "", allModelViewNames);
                    }
                }
            }
        }
    }
}
