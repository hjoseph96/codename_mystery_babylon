using System;
using UnityEngine;

namespace SBS
{
    public enum ShadowType
    {
        None,
        Simple,
        Real
    }

    public enum RealShadowMethod
    {
        Dynamic,
        Static
    }

    [Serializable]
    public class SimpleShadowProperty
    {
        public GameObject gameObject = null;
        public Vector2 scale = Vector3.one;
        public bool autoScale = false;
    }

    [Serializable]
    public class ShadowProperty : PropertyBase
    {
        public ShadowType type = ShadowType.None;
        public RealShadowMethod method = RealShadowMethod.Dynamic;
        public Camera camera = null;
        public GameObject fieldObj = null;
        public bool staticShadowVisible = true;
        public Vector3 cameraPosition = new Vector3(0, 100, 0);
        public Vector3 fieldPosition = Vector3.zero;
        public float fieldRotation = 180f;
        public Vector3 fieldScale = Vector3.one;
        public bool autoAdjustField = true;
        public float opacity = 0.8f;
        public bool shadowOnly = false;
    }
}
