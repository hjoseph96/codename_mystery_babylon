using UnityEngine;

namespace SBS
{
    public class ParticleExtractor0 : ExtractorBase
    {
        public Color backgroundColor = Color.black;

        [Range(0, 1)]
        public float threshold = 0;

        public override void Extract(Camera camera, StudioModel model,
            VariationProperty variation, bool isShadow, ref Texture2D outTex)
        {
            Color[] colorsOnBlack = CaptureAndReadPixels(camera, backgroundColor);
            Color[] resultColors = new Color[colorsOnBlack.Length];

            for (int y = 0; y < outTex.height; y++)
            {
                for (int x = 0; x < outTex.width; x++)
                {
                    int index = y * outTex.width + x;
                    Color outColor = colorsOnBlack[index];

                    if (threshold == 0)
                    {
                        if (outColor == backgroundColor)
                            continue;
                    }
                    else
                    {
                        Vector3 colorVector1 = new Vector3(outColor.r, outColor.g, outColor.b);
                        Vector3 colorVector2 = new Vector3(backgroundColor.r, backgroundColor.g, backgroundColor.b);
                        if ((colorVector1 - colorVector2).magnitude < threshold)
                            continue;
                    }

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
