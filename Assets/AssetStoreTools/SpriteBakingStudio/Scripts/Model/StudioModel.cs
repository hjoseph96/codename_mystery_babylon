using System.Collections.Generic;
using UnityEngine;

namespace SBS
{
    [DisallowMultipleComponent]
    public abstract class StudioModel : MonoBehaviour
    {
        public enum OriginType
        {
            Bottom = 0,
            Center
        }

        public enum ForwardType
        {
            PositiveZ = 0,
            NegativeZ,
            PositiveX,
            NegativeX
        }

        public FrameUpdater customizer;

        public OriginType originType = OriginType.Bottom;
        public ForwardType forwordType = ForwardType.PositiveZ;

        public Vector3 pivotOffset = Vector3.zero;
        public bool directionDependentPivot = true;

        [SerializeField]
        public SimpleShadowProperty simpleShadow = new SimpleShadowProperty();

        private Dictionary<int, string> shaderBackup = new Dictionary<int, string>();

        public virtual Vector3 ComputedCenter
        {
            get
            {
                return transform.position;
            }
        }
        public virtual Vector3 ComputedBottom
        {
            get
            {
                return transform.position;
            }
        }

        public Vector3 ComputedForward
        {
            get
            {
                switch (forwordType)
                {
                    case ForwardType.PositiveZ:
                        return transform.forward;
                    case ForwardType.NegativeZ:
                        return -transform.forward;
                    case ForwardType.PositiveX:
                        return transform.right;
                    case ForwardType.NegativeX:
                        return -transform.right;
                }
                return Vector3.zero;
            }
        }
        public Vector3 ComputedRight
        {
            get
            {
                switch (forwordType)
                {
                    case ForwardType.PositiveZ:
                        return transform.right;
                    case ForwardType.NegativeZ:
                        return -transform.right;
                    case ForwardType.PositiveX:
                        return -transform.forward;
                    case ForwardType.NegativeX:
                        return transform.forward;
                }
                return Vector3.zero;
            }
        }

        public Vector3 GetPivotPosition()
        {
            if (pivotOffset.sqrMagnitude > 0)
            {
                if (directionDependentPivot)
                {
                    Vector3 relativeOffset = new Vector3(ComputedForward.x * pivotOffset.x,
                                                         ComputedForward.y * pivotOffset.y,
                                                         ComputedForward.z * pivotOffset.z);
                    return ComputedBottom + relativeOffset;
                }
                else
                {
                    return ComputedBottom + pivotOffset;
                }
            }

            return ComputedBottom;
        }

        public abstract Vector3 GetSize();
        public virtual Vector3 GetDynamicSize()
        {
            return GetSize();
        }
        public abstract Vector3 GetMinPos();
        public virtual Vector3 GetExactMinPos()
        {
            return GetMinPos();
        }
        public abstract Vector3 GetMaxPos();
        public virtual Vector3 GetExactMaxPos()
        {
            return GetMaxPos();
        }

        public abstract float GetTimeForRatio(float ratio);
        public abstract void UpdateModel(Frame frame);

        public abstract bool IsReady();

        public virtual bool IsTileAvailable()
        {
            return false;
        }

        public virtual void DrawGizmoMore() { }

        void OnDrawGizmos()
        {
            if (GetSize().magnitude == 0.0f)
                return;

            Gizmos.color = Color.yellow;
            float lineLength = GetSize().magnitude;
            Vector3 headPos = transform.position + transform.forward * lineLength;
            Gizmos.DrawLine(transform.position, headPos);
            float arrowLength = lineLength / 10.0f;
            Vector3 arrowEnd = headPos + (-transform.forward) * arrowLength;
            Vector3 arrowEnd1 = arrowEnd + transform.right * arrowLength;
            Vector3 arrowEnd2 = arrowEnd + (-transform.right) * arrowLength;
            Gizmos.DrawLine(headPos, arrowEnd1);
            Gizmos.DrawLine(headPos, arrowEnd2);

            Gizmos.color = Color.magenta;
            headPos = ComputedCenter + ComputedForward * lineLength;
            Gizmos.DrawLine(ComputedCenter, headPos);
            arrowEnd = headPos + (-ComputedForward) * arrowLength;
            arrowEnd1 = arrowEnd + ComputedRight * arrowLength;
            arrowEnd2 = arrowEnd + (-ComputedRight) * arrowLength;
            Gizmos.DrawLine(headPos, arrowEnd1);
            Gizmos.DrawLine(headPos, arrowEnd2);

            DrawGizmoMore();
        }

        public Vector3 GetRatioBetweenSizes()
        {
            Vector3 dynamicSize = GetDynamicSize();
            Vector3 staticSize = GetSize();

            return new Vector3
            (
                dynamicSize.x / staticSize.x,
                dynamicSize.y / staticSize.y,
                dynamicSize.z / staticSize.z
            );
        }

        public void RescaleSimpleShadow()
        {
            if (simpleShadow.gameObject == null)
                return;
            if (simpleShadow.scale.magnitude == 0f)
                return;

            Renderer shadowRenderer = simpleShadow.gameObject.GetComponent<Renderer>();
            if (!IsReady() || shadowRenderer == null)
                return;

            Transform shadowTransform = simpleShadow.gameObject.transform;
            shadowTransform.position = Vector3.zero;
            shadowTransform.localScale = Vector3.one;

            Vector3 modelSize = simpleShadow.autoScale ? GetSize() : GetDynamicSize();
            float xScaleRatio = modelSize.x / shadowRenderer.bounds.size.x;
            float zScaleRatio = modelSize.z / shadowRenderer.bounds.size.z;

            if (xScaleRatio > 0f && zScaleRatio > 0f)
            {
                xScaleRatio *= simpleShadow.scale.x;
                zScaleRatio *= simpleShadow.scale.y;

                shadowTransform.localScale = new Vector3
                (
                    shadowTransform.localScale.x * xScaleRatio,
                    1.0f,
                    shadowTransform.localScale.z * zScaleRatio
                );
            }
            else
            {
                shadowTransform.localScale = new Vector3
                (
                    simpleShadow.scale.x,
                    1.0f,
                    simpleShadow.scale.y
                );
            }
        }

        public void BackupAllShaders()
        {
            if (shaderBackup.Keys.Count > 0)
                shaderBackup.Clear();

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer rndr in renderers)
            {
                if (rndr.gameObject.GetComponent<DontApplyUniformShader>())
                    continue;

                Material mtrl = rndr.sharedMaterial;
                if (mtrl != null && mtrl.shader != null)
                {
                    if (!shaderBackup.ContainsKey(mtrl.GetInstanceID()))
                        shaderBackup.Add(mtrl.GetInstanceID(), mtrl.shader.name);
                }
            }
        }

        public void ChangeAllShaders(Shader shader)
        {
            if (shader == null)
            {
                Debug.LogError("shader is null");
                return;
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer rndr in renderers)
            {
                if (rndr.gameObject.GetComponent<DontApplyUniformShader>())
                    continue;

                Material mtrl = rndr.sharedMaterial;
                if (mtrl != null && mtrl.shader != null)
                    mtrl.shader = shader;
            }
        }

        public void RestoreAllShaders()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer rndr in renderers)
            {
                if (rndr.gameObject.GetComponent<DontApplyUniformShader>())
                    continue;

                Material mtrl = rndr.sharedMaterial;
                if (mtrl != null && mtrl.shader != null)
                    mtrl.shader = Shader.Find(shaderBackup[mtrl.GetInstanceID()]);
            }

            shaderBackup.Clear();
        }
    }
}
