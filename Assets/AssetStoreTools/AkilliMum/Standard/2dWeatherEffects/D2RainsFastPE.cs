//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace AkilliMum.Standard.D2WeatherEffects
{
    [ExecuteInEditMode]
    public class D2RainsFastPE : EffectBase
    {
        public Transform CamTransform;
        private Vector3 _firstPosition;
        private Vector3 _difference;
        public float CameraSpeedMultiplier = 1f;

        public Color Color = new Color(1f, 1f, 1f, 1f);
        public Texture2D Noise;
        public float Density = 1.4f;
        public float Speed = 0.1f;
        public float Exposure = 5f;
        public float Direction = -0.32f;

        public Shader Shader;
        private Material _material;

        private void Awake()
        {
            _firstPosition = CamTransform.position;
        }

        private void Update()
        {
            //_difference = CamTransform.position - _firstPosition;
            //_previousPosition = CamTransform.position;
            _difference = Vector3.Lerp(_firstPosition, CamTransform.position, Time.time);
            _firstPosition = CamTransform.position;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

            if (_material)
            {
                DestroyImmediate(_material);
                _material = null;
            }
            if (Shader)
            {
                _material = new Material(Shader);
                _material.hideFlags = HideFlags.HideAndDontSave;

                if (_material.HasProperty("_Color"))
                {
                    _material.SetColor("_Color", Color);
                }
                if (_material.HasProperty("_NoiseTex"))
                {
                    _material.SetTexture("_NoiseTex", Noise);
                }
                if (_material.HasProperty("_Density"))
                {
                    _material.SetFloat("_Density", Density);
                }
                if (_material.HasProperty("_Speed"))
                {
                    _material.SetFloat("_Speed", Speed);
                }
                if (_material.HasProperty("_Exposure"))
                {
                    _material.SetFloat("_Exposure", Exposure);
                }
                if (_material.HasProperty("_Direction"))
                {
                    _material.SetFloat("_Direction", Direction);
                }
                if (_material.HasProperty("_CameraSpeedMultiplier"))
                {
                    _material.SetFloat("_CameraSpeedMultiplier", CameraSpeedMultiplier);
                }
                if (_material.HasProperty("_UVChangeX"))
                {
                    _material.SetFloat("_UVChangeX", _difference.x);
                }
                if (_material.HasProperty("_UVChangeY"))
                {
                    _material.SetFloat("_UVChangeY", _difference.y);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}