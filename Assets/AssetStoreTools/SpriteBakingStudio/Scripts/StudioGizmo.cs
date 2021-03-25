using UnityEngine;

namespace SBS
{
    public class StudioGizmo : MonoBehaviour
    {
        public Color color = Color.green;

        [Range(0, 5)]
        public float size = 0.5f;

        void OnDrawGizmos()
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position, size);
        }
    }
}
