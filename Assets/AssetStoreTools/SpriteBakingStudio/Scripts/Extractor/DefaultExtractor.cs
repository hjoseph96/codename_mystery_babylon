using UnityEngine;

namespace SBS
{
    public class DefaultExtractor : ExtractorBase
    {
        public override void Extract(Camera camera, StudioModel model,
            VariationProperty variation, bool isShadow, ref Texture2D outTex)
        {
            Color[] colorsOnBlack = CaptureAndReadPixels(camera, Color.black);
            Color[] colorsOnWhite = CaptureAndReadPixels(camera, Color.white);
            Color[] resultColors = new Color[colorsOnBlack.Length];

            for (int y = 0; y < outTex.height; y++)
            {
                for (int x = 0; x < outTex.width; x++)
                {
                    int index = y * outTex.width + x;
                    Color pixelOnBlack = colorsOnBlack[index];
                    Color pixelOnWhite = colorsOnWhite[index];

                    float redDiff = pixelOnWhite.r - pixelOnBlack.r;
                    float greenDiff = pixelOnWhite.g - pixelOnBlack.g;
                    float blueDiff = pixelOnWhite.b - pixelOnBlack.b;

                    float alpha = 1.0f - Mathf.Min(Mathf.Min(redDiff, greenDiff), blueDiff);
                    Debug.Assert(alpha >= 0f);

                    if (alpha == 0f)
                        continue;

                    Color outColor = ExtractColor(alpha, pixelOnBlack, isShadow);

                    if (variation.on && !isShadow)
                    {
                        outColor = StudioUtility.BlendColors(variation.tintColor, outColor,
                            variation.tintBlendFactor, variation.imageBlendFactor);
                    }

                    resultColors[y * outTex.width + x] = outColor;
                }
            }

            outTex.SetPixels(resultColors);
        }
    }
}
