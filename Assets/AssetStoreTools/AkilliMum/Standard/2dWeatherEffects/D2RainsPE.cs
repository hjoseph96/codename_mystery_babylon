//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace AkilliMum.Standard.D2WeatherEffects
{
    [ExecuteInEditMode]
    public class D2RainsPE : EffectBase
    {
        public Transform CamTransform;
        private Vector3 _firstPosition;
        private Vector3 _difference;
        public float CameraSpeedMultiplier = 1f;

        public Color Color = new Color(1f, 1f, 1f, 1f);
        [Range(1, 50)]
        public float ParticleMultiplier = 10.0f;
        public float Tail = 0.03f;
        public float Speed = 4.0f;
        public float Direction = 0.2f;
        public float Luminance = 1f;
        [Range(0.01f, 10)]
        public float Zoom = 1.2f;
        public bool DarkMode = false;
        [Range(0f, 0.1f)]
        public float LuminanceAdder = 0.002f;

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
                if (_material.HasProperty("_Speed"))
                {
                    _material.SetFloat("_Speed", Speed);
                }
                if (_material.HasProperty("_Tail"))
                {
                    _material.SetFloat("_Tail", Tail);
                }
                if (_material.HasProperty("_Direction"))
                {
                    _material.SetFloat("_Direction", Direction);
                }
                if (_material.HasProperty("_Zoom"))
                {
                    _material.SetFloat("_Zoom", Zoom);
                }
                if (_material.HasProperty("_DarkMode"))
                {
                    _material.SetFloat("_DarkMode", DarkMode == true ? 1 : 0);
                }
                if (_material.HasProperty("_DarkMultiplier"))
                {
                    _material.SetFloat("_DarkMultiplier", Luminance);
                }
                if (_material.HasProperty("_Multiplier"))
                {
                    _material.SetFloat("_Multiplier", ParticleMultiplier);
                }
                if (_material.HasProperty("_LuminanceAdd"))
                {
                    _material.SetFloat("_LuminanceAdd", LuminanceAdder);
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