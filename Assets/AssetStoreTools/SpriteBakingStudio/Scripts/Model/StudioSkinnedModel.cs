using UnityEngine;

namespace SBS
{
    public class StudioSkinnedModel : StudioAnimatedModel
    {
        public SkinnedMeshRenderer mainMeshRenderer;
        public Vector3 computedMinPos = Vector3.zero;
        public Vector3 computedMaxPos = Vector3.zero;
        public Vector3 computedSize = Vector3.zero;

        public bool rootMotion = false;
        public Transform xzCenterBone = null;
        public bool fixYtoBottom = true;

#if UNITY_2018_1_OR_NEWER
        public float currDegree = 0.0f;
#endif

        public override Vector3 ComputedCenter
        {
            get
            {
                if (originType == OriginType.Center)
                {
                    return transform.position;
                }
                else if (originType == OriginType.Bottom)
                {
                    float x = transform.position.x;
                    float y = transform.position.y + GetSize().y / 2.0f;
                    float z = transform.position.z;
                    return new Vector3(x, y, z);
                }
                return Vector3.zero;
            }
        }
        public override Vector3 ComputedBottom
        {
            get
            {
                if (originType == OriginType.Center)
                {
                    float x = transform.position.x;
                    float y = transform.position.y - GetSize().y / 2.0f;
                    float z = transform.position.z;
                    return new Vector3(x, y, z);
                }
                else if (originType == OriginType.Bottom)
                {
                    return transform.position;
                }
                return Vector3.zero;
            }
        }

        public override Vector3 GetSize()
        {
            if (mainMeshRenderer != null)
                return mainMeshRenderer.bounds.size;
            else
                return computedSize;
        }
        public override Vector3 GetDynamicSize()
        {
            if (mainMeshRenderer != null)
                return mainMeshRenderer.localBounds.size;
            else
                return computedSize;
        }

        public override Vector3 GetMinPos()
        {
            if (mainMeshRenderer != null)
                return mainMeshRenderer.bounds.min;
            else
                return computedMinPos;
        }
        public override Vector3 GetExactMinPos()
        {
            if (mainMeshRenderer != null)
                return mainMeshRenderer.localBounds.min;
            else
                return GetMinPos();
        }

        public override Vector3 GetMaxPos()
        {
            if (mainMeshRenderer != null)
                return mainMeshRenderer.bounds.max;
            else
                return computedMaxPos;
        }
        public override Vector3 GetExactMaxPos()
        {
            if (mainMeshRenderer != null)
                return mainMeshRenderer.localBounds.max;
            else
                return GetMaxPos();
        }

        public override bool IsReady()
        {
            if (animClip == null)
                return false;

            return (mainMeshRenderer != null || computedSize.magnitude > 0.0f);
        }

        public override bool IsTileAvailable()
        {
            return true;
        }

        public override void UpdateModel(Frame frame)
        {
            if (animClip != null)
                animClip.SampleAnimation(gameObject, frame.time);

            if (customizer != null)
                customizer.UpdateFrame(frame.number, frame.time);

#if UNITY_2018_1_OR_NEWER
            TransformUtils.RotateModel(this, currDegree);
#endif

            doRootMotion();
        }

        private void doRootMotion()
        {
            if (!rootMotion || xzCenterBone == null)
                return;

            float bottomY = 0.0f;
            if (!fixYtoBottom)
            {
                bottomY = float.MaxValue;
                Transform[] transforms = xzCenterBone.GetComponentsInChildren<Transform>();
                foreach (Transform trsf in transforms)
                    bottomY = Mathf.Min(trsf.position.y, bottomY);
            }

            Vector3 translation = new Vector3(ComputedBottom.x - xzCenterBone.position.x,
                                              ComputedBottom.y - bottomY,
                                              ComputedBottom.z - xzCenterBone.position.z);
            xzCenterBone.Translate(translation, Space.World);

#if UNITY_2018_1_OR_NEWER
            transform.position = fixYtoBottom ? Vector3.zero : new Vector3(0, transform.position.y, 0);
#endif
        }

        public override void DrawGizmoMore()
        {
            Gizmos.color = Color.yellow;
            float sphereRadius = GetSize().magnitude / 20.0f;
            Gizmos.DrawSphere(transform.position, sphereRadius);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(ComputedCenter, sphereRadius);

            Gizmos.color = Color.cyan;
            sphereRadius = GetSize().magnitude / 40.0f;
            Gizmos.DrawSphere(GetPivotPosition(), sphereRadius);
        }
    }
}
