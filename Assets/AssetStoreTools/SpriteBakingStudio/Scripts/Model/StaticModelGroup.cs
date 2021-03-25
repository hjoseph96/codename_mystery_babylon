using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SBS
{
    [Serializable]
    public class StaticModelPair
    {
        public StudioStaticModel Model { get; set; }
        public bool Checked { get; set; }

        public StaticModelPair(StudioStaticModel model_, bool checked_)
        {
            Model = model_;
            Checked = checked_;
        }
    }
    
    public class StaticModelGroup : StudioModel
    {
        [SerializeField]
        public List<StaticModelPair> modelPairs = new List<StaticModelPair>();

        public string rootDirectory;

        private StudioStaticModel biggestModel = null;
        private StudioStaticModel displayModel = null;

        public override Vector3 GetSize()
        {
            if (biggestModel == null)
                RefreshBiggestModel();
            return biggestModel != null ? biggestModel.GetSize() : Vector3.one;
        }

        public override Vector3 GetMinPos()
        {
            if (biggestModel == null)
                RefreshBiggestModel();
            return biggestModel != null ? biggestModel.GetMinPos() : Vector3.zero;
        }

        public override Vector3 GetMaxPos()
        {
            if (biggestModel == null)
                RefreshBiggestModel();
            return biggestModel != null ? biggestModel.GetMaxPos() : Vector3.zero;
        }

        public override float GetTimeForRatio(float ratio) { return 0f; }

        public override void UpdateModel(Frame frame) { }

        public override bool IsReady()
        {
            bool ready = false;

            foreach (StaticModelPair pair in modelPairs)
            {
                if (pair.Model != null && pair.Checked)
                {
                    if (!pair.Model.IsReady())
                        return false;
                    ready = true;
                }
            }

            return ready;
        }

        public override bool IsTileAvailable()
        {
            return true;
        }

        public void InitModelsPosition()
        {
            foreach (StaticModelPair pair in modelPairs)
            {
                if (pair.Model != null)
                    pair.Model.transform.position = Vector3.zero;
            }
        }

        public void RefreshModels()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform transf = transform.GetChild(i);
                if (transf != null)
                    DestroyImmediate(transf.gameObject);
            }

            modelPairs.Clear();
            displayModel = null;

#if UNITY_EDITOR
            int assetRootIndex = rootDirectory.IndexOf("Assets");
            if (assetRootIndex < 0)
            {
                Debug.LogError(string.Format("{0} is out of the Assets folder.", rootDirectory));
                return;
            }
            string dirPath = rootDirectory.Substring(assetRootIndex);

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string assetPath in allAssetPaths)
            {
                if (assetPath.IndexOf(dirPath) < 0)
                    continue;

                GameObject prf = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                if (prf == null)
                    continue;

                if (modelPairs.Find(x => x.Model.name == prf.name) != null)
                    continue;

                GameObject obj = null;

                Transform transf = transform.Find(prf.name);
                if (transf == null)
                {
                    obj = Instantiate(prf, Vector3.zero, Quaternion.identity);
                    obj.name = prf.name;
                }
                else
                {
                    obj = transf.gameObject;
                }

                obj.transform.parent = transform;
                obj.transform.localRotation = Quaternion.identity;
                obj.SetActive(false);

                StudioStaticModel model = obj.GetComponent<StudioStaticModel>();
                if (model == null)
                    model = obj.AddComponent<StudioStaticModel>();

                model.AutoFindMeshRenderer();

                modelPairs.Add(new StaticModelPair(model, true));
            }
#endif

            if (modelPairs.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, modelPairs.Count);
                SetDisplayModel(modelPairs[index].Model);
            }

            RefreshBiggestModel();
        }

        private void RefreshBiggestModel()
        {
            Vector3 maxSize = Vector3.zero;
            foreach (StaticModelPair pair in modelPairs)
            {
                if (pair.Model != null || pair.Checked)
                {
                    Vector3 size = pair.Model.GetSize();
                    if (size.sqrMagnitude > maxSize.sqrMagnitude)
                    {
                        maxSize = size;
                        biggestModel = pair.Model;
                    }
                }
            }
        }

        public void SetDisplayModel(StudioStaticModel model) // not null
        {
            if (displayModel == model)
                return;

            if (displayModel != null)
                displayModel.gameObject.SetActive(false);

            displayModel = model;
            model.gameObject.SetActive(true);
        }

        public List<StudioModel> GetCheckedModels()
        {
            List<StudioModel> checkedModels = new List<StudioModel>();

            foreach (StaticModelPair pair in modelPairs)
            {
                if (pair.Model != null && pair.Checked)
                    checkedModels.Add(pair.Model);
            }

            return checkedModels;
        }

        public void OnInitialize()
        {
            if (displayModel != null)
                displayModel.gameObject.SetActive(false);
        }

        public void OnFinalize()
        {
            if (displayModel != null)
            {
                displayModel.gameObject.SetActive(true);
            }
            else
            {
                RefreshBiggestModel();
                SetDisplayModel(biggestModel);
            }
        }
    }
}
