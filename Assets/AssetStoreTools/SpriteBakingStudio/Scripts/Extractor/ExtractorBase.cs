using UnityEngine;

namespace SBS
{
    public abstract class ExtractorBase : MonoBehaviour
    {
        public enum ExtractionMethod
        {
            OneShader,
            TwoShader
        }

        public enum AlpahExtractionChannel
        {
            Red,
            Green,
            Blue,
            Mixed
        }

        public enum ColorExtractionBackground
        {
            Black,
            White
        }

        public abstract void Extract(Camera camera, StudioModel model,
            VariationProperty variation, bool isShadow, ref Texture2D outTex);

        protected Color[] CaptureAndReadPixels(Camera camera, Color color)
        {
            camera.backgroundColor = color;
            camera.Render();
            Texture2D tex = new Texture2D(camera.targetTexture.width, camera.targetTexture.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            return tex.GetPixels();
        }

        protected float ExtractAlpha(Color pixelOnBlack, Color pixelOnWhite, AlpahExtractionChannel channel)
        {
            float colorDiff = 0f;

            switch (channel)
            {
                case AlpahExtractionChannel.Red:
                    colorDiff = pixelOnWhite.r - pixelOnBlack.r;
                    break;

                case AlpahExtractionChannel.Green:
                    colorDiff = pixelOnWhite.g - pixelOnBlack.g;
                    break;

                case AlpahExtractionChannel.Blue:
                    colorDiff = pixelOnWhite.b - pixelOnBlack.b;
                    break;

                case AlpahExtractionChannel.Mixed:
                    float redDiff = pixelOnWhite.r - pixelOnBlack.r;
                    float greenDiff = pixelOnWhite.g - pixelOnBlack.g;
                    float blueDiff = pixelOnWhite.b - pixelOnBlack.b;
                    colorDiff = Mathf.Min(Mathf.Min(redDiff, greenDiff), blueDiff);
                    break;
            }

            return Mathf.Clamp01(colorDiff);
        }

        protected Color ExtractColor(float alpha, Color color, bool isShadowCamera = false)
        {
            Color resultColor = Color.clear;
            if (alpha != 0f)
            {
                if (isShadowCamera)
                    resultColor = Color.black;
                else
                    resultColor = color / alpha;
                resultColor.a = alpha;
            }

            return resultColor;
        }
    }
}
