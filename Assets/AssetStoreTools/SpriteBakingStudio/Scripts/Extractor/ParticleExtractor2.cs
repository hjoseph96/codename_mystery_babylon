using UnityEngine;

namespace SBS
{
    public class ParticleExtractor2 : ExtractorBase
    {
        public Shader alphaUniformShader;
        public AlpahExtractionChannel alphaExtractionChannel;

        public Shader colorUniformShader;
        public ColorExtractionBackground colorExtractionBackground;

        public override void Extract(Camera camera, StudioModel model,
            VariationProperty variation, bool isShadow, ref Texture2D outTex)
        {
            if (alphaUniformShader != null || colorUniformShader != null)
                model.BackupAllShaders();

            if (alphaUniformShader != null)
                model.ChangeAllShaders(alphaUniformShader);

            Color[] aeColorsOnBlack = CaptureAndReadPixels(camera, Color.black);
            Color[] aeColorsOnWhite = CaptureAndReadPixels(camera, Color.white);

            if (colorUniformShader)
            {
                model.ChangeAllShaders(colorUniformShader);
            }
            else
            {
                if (alphaUniformShader)
                    model.RestoreAllShaders();
            }

            Color[] ceColorsOnBlack = CaptureAndReadPixels(camera, Color.black);
            Color[] ceColorsOnWhite = CaptureAndReadPixels(camera, Color.white);

            if (colorUniformShader)
                model.RestoreAllShaders();

            Color[] resultColors = new Color[aeColorsOnBlack.Length];

            for (int y = 0; y < outTex.height; y++)
            {
                for (int x = 0; x < outTex.width; x++)
                {
                    int index = y * outTex.width + x;

                    float alpha = 1.0f - ExtractAlpha(aeColorsOnBlack[index], aeColorsOnWhite[index], alphaExtractionChannel);
                    Debug.Assert(alpha >= 0f);

                    if (alpha == 0f)
                        continue;

                    Color cePixel = Color.clear;
                    if (colorExtractionBackground == ColorExtractionBackground.Black)
                        cePixel = ceColorsOnBlack[index];
                    else if (colorExtractionBackground == ColorExtractionBackground.White)
                        cePixel = ceColorsOnWhite[index];
                    Color outColor = ExtractColor(alpha, cePixel, isShadow);

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
