/**
* Title : Aesthetico ™
* Author : Toucan Systems (http://toucan-systems.pl)
*
* Overview: Aesthetico allows you to pixelate and color grade camera's image output.
* Use instructions: Add Aesthetico script to camera as a new component.
*
* Version: 1.0.0.0
* Released: 2017.12
**/

using UnityEngine;

namespace ToucanSystems
{
    [ExecuteInEditMode]
    public class Aesthetico : MonoBehaviour
    {
        #region Variables
        public bool enablePixelation = true;

        [Range(0, 4096)]
        public int pixelCountX = 100;

        [Range(0, 4096)]
        public int pixelCountY = 100;

        public bool forceSquarePixels = true;

        public bool enableColoring = true;

        [Range(0, 1)]
        public float coloringIntensity = 1;

        [Range(2, 255)]
        public int colorsRange = 16;

        [Range(0.1f, 5)]
        public float colorCorrection = 1;

        public Gradient colorsMap;

        private Shader aestheticoShader;
        private Material material;
        private float aspectRatio;
        private Texture2D colorMapTexture;

        [SerializeField]
        [HideInInspector]
        private bool isGradientSet = false;
        #endregion

        private void Awake()
        {
            if (!isGradientSet)
            {
                InitializeGradient();
                isGradientSet = true;
            }

            aestheticoShader = Shader.Find("ToucanSystems/Aesthetico");

            material = new Material(aestheticoShader);

            OnValidate();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (aestheticoShader == null)
            {
                return;
            }

            aspectRatio = (float)Screen.height / Screen.width;

            material.SetFloat("_PixelCountU", pixelCountX);
            material.SetFloat("_PixelCountV", forceSquarePixels ? pixelCountX * aspectRatio : pixelCountY);
            material.SetFloat("_ColorsRange", colorsRange);
            material.SetTexture("_ColorsMap", colorMapTexture);
            material.SetFloat("_ColoringIntensity", coloringIntensity);
            material.SetInt("_EnablePixelation", enablePixelation ? 1 : 0);
            material.SetInt("_EnableColoring", enableColoring ? 1 : 0);
            material.SetFloat("_ColorCorrection", colorCorrection);

            Graphics.Blit(source, destination, material);
        }

        private void OnValidate()
        {
            if (aestheticoShader == null)
            {
                return;
            }

            if (material == null)
            {
                material = new Material(aestheticoShader);
            }

            int colorsCount = colorsMap.colorKeys.Length;
            colorMapTexture = new Texture2D(128, 1);
            Color32[] pixels = new Color32[128];

            for (int i = 0; i <= 127; i++)
            {
                pixels[i] = colorsMap.Evaluate((float)i / 128);
            }

            colorMapTexture.SetPixels32(pixels);
            colorMapTexture.Apply();
        }

        private void InitializeGradient()
        {
            GradientColorKey[] gck;
            GradientAlphaKey[] gak;
            colorsMap = new Gradient();

            gck = new GradientColorKey[2];
            gck[0].color = Color.white;
            gck[0].time = 0.0F;
            gck[1].color = Color.black;
            gck[1].time = 1.0F;

            gak = new GradientAlphaKey[2];
            gak[0].alpha = 1.0F;
            gak[0].time = 0.0F;
            gak[1].alpha = 1.0F;
            gak[1].time = 1.0F;

            colorsMap.SetKeys(gck, gak);
        }
    }
}
