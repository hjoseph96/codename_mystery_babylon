using UnityEngine;
using System.IO;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SBS
{
    public class StudioUtility
    {
        public static Texture2D PrepareShadowAndExtractTexture(StudioSetting setting)
        {
            Texture2D resultTexture = null;

            if (setting.shadow.type != ShadowType.None)
            {
                if (setting.shadow.type == ShadowType.Simple)
                    setting.model.obj.RescaleSimpleShadow();
                else if (setting.IsStaticRealShadow())
                    BakeStaticShadow(setting);

                if (setting.shadow.shadowOnly)
                {
                    PickOutStaticShadow(setting);
                    {
                        Vector3 originalPosition = ThrowOutModelFarAway(setting.model.obj);
                        {
                            resultTexture = ExtractTexture(Camera.main, setting);
                        }
                        PutModelBackInPlace(setting.model.obj, originalPosition);
                    }
                    PushInStaticShadow(setting);
                }
                else if (!setting.shadow.shadowOnly && (setting.variation.on && setting.variation.excludeShadow))
                {
                    PickOutStaticShadow(setting);
                    {
                        GameObject shadowObject = null;
                        if (setting.shadow.type == ShadowType.Simple)
                            shadowObject = setting.model.obj.simpleShadow.gameObject;
                        else if (setting.shadow.type == ShadowType.Real)
                            shadowObject = setting.shadow.fieldObj;

                        // Shadow Pass
                        Vector3 originalPosition = ThrowOutModelFarAway(setting.model.obj);
                        Texture2D shadowTexture = ExtractTexture(Camera.main, setting, true);
                        PutModelBackInPlace(setting.model.obj, originalPosition);

                        // Model Pass
                        shadowObject.SetActive(false);
                        Texture2D modelTexture = ExtractTexture(Camera.main, setting);
                        shadowObject.SetActive(true);

                        // merge texture
                        resultTexture = MergeTextures(setting, shadowTexture, modelTexture);
                    }
                    PushInStaticShadow(setting);
                }
                else
                {
                    resultTexture = ExtractTexture(Camera.main, setting);
                }
            }
            else
            {
                resultTexture = ExtractTexture(Camera.main, setting);
            }

            return resultTexture;
        }

        public static void BakeStaticShadow(StudioSetting setting)
        {
            TileUtils.HideAllTiles();

            setting.shadow.camera.CopyFrom(Camera.main);
            setting.shadow.camera.transform.position = setting.shadow.cameraPosition;
            TransformUtils.LookAtModel(setting.shadow.camera.transform, setting.model.obj);
            setting.shadow.camera.targetDisplay = 1;

            setting.shadow.fieldObj.SetActive(false);

            setting.shadow.camera.targetTexture = new RenderTexture(setting.shadow.camera.pixelWidth, setting.shadow.camera.pixelHeight, 24, RenderTextureFormat.ARGB32);
            Color bgColor = setting.shadow.camera.backgroundColor;

            Texture2D rawTex = ExtractTexture(setting.shadow.camera, setting, true);

            string dirPath = Path.Combine(Application.dataPath, "SpriteBakingStudio/Shadow");
            int assetRootIndex = dirPath.IndexOf("Assets");
            if (assetRootIndex < 0)
            {
                Debug.LogError(string.Format("{0} is out of the Assets folder.", dirPath));
                return;
            }
            dirPath = dirPath.Substring(assetRootIndex);
            string filePath = TextureUtils.SaveTexture(dirPath, Global.STATIC_SHADOW_NAME, rawTex);

#if UNITY_EDITOR
            AssetDatabase.ImportAsset(filePath);

            Texture2D assetTex = AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D)) as Texture2D;
            if (assetTex != null)
            {
                MeshRenderer rndr = setting.shadow.fieldObj.GetComponent<MeshRenderer>();
                if (rndr == null)
                    return;

                rndr.sharedMaterial.mainTexture = assetTex;

                setting.shadow.camera.transform.position = new Vector3(0f, 500f, 0f);
                setting.shadow.camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                setting.shadow.fieldObj.transform.position = Vector3.zero;
                setting.shadow.fieldObj.transform.rotation = Quaternion.Euler(0f, 180, 0f);

                UpdateShadowFieldSize(setting.shadow.camera, setting.shadow.fieldObj);

                setting.shadow.camera.transform.position = setting.shadow.cameraPosition;
                TransformUtils.LookAtModel(setting.shadow.camera.transform, setting.model.obj);
                setting.shadow.fieldObj.transform.position = setting.shadow.fieldPosition;
                setting.shadow.fieldObj.transform.rotation = Quaternion.Euler(0f, setting.shadow.fieldRotation, 0f);

                setting.shadow.fieldObj.SetActive(true);
            }
#endif

            setting.shadow.camera.backgroundColor = bgColor;
            setting.shadow.camera.targetTexture = null;
        }

        public static void PickOutStaticShadow(StudioSetting setting)
        {
            if (setting.shadow.type == ShadowType.Simple)
                setting.model.obj.simpleShadow.gameObject.transform.parent = null;
        }

        public static void PushInStaticShadow(StudioSetting setting)
        {
            if (setting.shadow.type == ShadowType.Simple)
                setting.model.obj.simpleShadow.gameObject.transform.parent = setting.model.obj.gameObject.transform;
        }

        public static Vector3 ThrowOutModelFarAway(StudioModel model)
        {
            Vector3 originalPosition = model.transform.position;
            model.transform.position = CreateFarAwayPosition();
            return originalPosition;
        }

        public static void PutModelBackInPlace(StudioModel model, Vector3 originalPosition)
        {
            model.transform.position = originalPosition;
        }

        public static Vector3 CreateFarAwayPosition()
        {
            return new Vector3(10000f, 0f, 0f);
        }

        public static Texture2D ExtractTexture(Camera camera, StudioSetting setting, bool isShadow = false)
        {
            if (camera == null || setting.extract.obj == null)
                return Texture2D.whiteTexture;

            RenderTexture.active = camera.targetTexture;

            Texture2D resultTex = new Texture2D(camera.targetTexture.width, camera.targetTexture.height, TextureFormat.ARGB32, false);
            setting.extract.obj.Extract(camera, setting.model.obj, setting.variation, isShadow, ref resultTex);

            RenderTexture.active = null;

            return resultTex;
        }

        public static Texture2D MergeTextures(StudioSetting setting, Texture2D shadowTexture, Texture2D bodyTexture)
        {
            if (shadowTexture.width != bodyTexture.width || shadowTexture.height != bodyTexture.height)
            {
                Debug.LogError("shadowTexture.width != bodyTexture.width || shadowTexture.height != bodyTexture.height");
                return bodyTexture;
            }

            Color[] shadowColors = shadowTexture.GetPixels();
            Color[] bodyColors = bodyTexture.GetPixels();

            for (int y = 0; y < shadowTexture.height; y++)
            {
                for (int x = 0; x < shadowTexture.width; x++)
                {
                    int index = y * shadowTexture.width + x;

                    Color bodyPixel = bodyColors[index];
                    if (bodyPixel == Color.clear)
                        continue;

                    Color shadowPixel = shadowColors[index];
                    Color resultColor = (shadowPixel == Color.clear ? bodyPixel :
                        BlendColors(bodyPixel, shadowPixel, setting.variation.bodyBlendFactor, setting.variation.shadowBlendFactor));

                    shadowColors[index] = resultColor;
                }
            }

            shadowTexture.SetPixels(shadowColors);

            return shadowTexture;
        }

        public static Color BlendColors(Color srcColor, Color dstColor, BlendFactor srcFactor, BlendFactor dstFactor)
        {
            return srcColor * MakeBlendFactor(srcColor, dstColor, srcFactor) +
                   dstColor * MakeBlendFactor(srcColor, dstColor, dstFactor);
        }

        public static Color MakeBlendFactor(Color srcPixel, Color dstPixel, BlendFactor factor)
        {
            switch (factor)
            {
            case BlendFactor.Zero:
                return Color.clear;

            case BlendFactor.One:
                return Color.white;

            case BlendFactor.SrcColor:
                return new Color(
                    srcPixel.r,
                    srcPixel.g,
                    srcPixel.b,
                    srcPixel.a);

            case BlendFactor.OneMinusSrcColor:
                return new Color(
                    (1f - srcPixel.r),
                    (1f - srcPixel.g),
                    (1f - srcPixel.b),
                    (1f - srcPixel.a));

            case BlendFactor.DstColor:
                return new Color(
                    dstPixel.r,
                    dstPixel.g,
                    dstPixel.b,
                    dstPixel.a);

            case BlendFactor.OneMinusDstColor:
                return new Color(
                    (1f - dstPixel.r),
                    (1f - dstPixel.g),
                    (1f - dstPixel.b),
                    (1f - dstPixel.a));

            case BlendFactor.SrcAlpha:
                return new Color(
                    srcPixel.a,
                    srcPixel.a,
                    srcPixel.a,
                    srcPixel.a);

            case BlendFactor.OneMinusSrcAlpha:
                return new Color(
                    (1f - srcPixel.a),
                    (1f - srcPixel.a),
                    (1f - srcPixel.a),
                    (1f - srcPixel.a));

            case BlendFactor.DstAlpha:
                return new Color(
                    dstPixel.a,
                    dstPixel.a,
                    dstPixel.a,
                    dstPixel.a);

            case BlendFactor.OneMinusDstAlpha:
                return new Color(
                    (1f - dstPixel.a),
                    (1f - dstPixel.a),
                    (1f - dstPixel.a),
                    (1f - dstPixel.a));
            }

            return Color.white;
        }

        public static void UpdateShadowFieldSize(Camera camera, GameObject fieldObject)
        {
            MeshRenderer fieldRenderer = fieldObject.GetComponent<MeshRenderer>();
            if (fieldRenderer == null)
                return;

            Vector3 maxWorldPos = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight));
            Vector3 minWorldPos = camera.ScreenToWorldPoint(Vector3.zero);
            Vector3 texWorldSize = maxWorldPos - minWorldPos;

            fieldObject.transform.localScale = Vector3.one;
            fieldObject.transform.position = Vector3.zero;

            fieldObject.transform.localScale = new Vector3
            (
                texWorldSize.x / fieldRenderer.bounds.size.x,
                1f,
                texWorldSize.z / fieldRenderer.bounds.size.z
            );
        }

        public static string MakeDateTimeString()
        {
            DateTime now = DateTime.Now;
            string year = now.Year.ToString();
            string month = now.Month >= 10 ? now.Month.ToString() : "0" + now.Month;
            string day = now.Day >= 10 ? now.Day.ToString() : "0" + now.Day;
            string hour = now.Hour >= 10 ? now.Hour.ToString() : "0" + now.Hour;
            string minute = now.Minute >= 10 ? now.Minute.ToString() : "0" + now.Minute;
            string second = now.Second >= 10 ? now.Second.ToString() : "0" + now.Second;
            return year.Substring(2, 2) + month + day + "_" + hour + minute + second;
        }
    }
}
