using UnityEngine;

namespace SBS
{
    public class ParticleExtractor1 : ExtractorBase
    {
        public Shader uniformShader;
        public AlpahExtractionChannel alphaExtractionChannel;

        public override void Extract(Camera camera, StudioModel model,
            VariationProperty variation, bool isShadow, ref Texture2D outTex)
        {
            if (uniformShader != null)
            {
                model.BackupAllShaders();
                model.ChangeAllShaders(uniformShader);
            }

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

                    float alpha = 1.0f - ExtractAlpha(pixelOnBlack, pixelOnWhite, alphaExtractionChannel);
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

            if (uniformShader != null)
                model.RestoreAllShaders();
        }
    }
}
