using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamingGarrison
{
    public class PixelPerfectFollowCamera : MonoBehaviour
    {
        [Tooltip("The object the camera will follow")]
        public Transform m_target;
        [Tooltip("How much time it takes for the camera to reach it's target position (in seconds)")]
        public float m_smoothTime = 0.1f;
        [Tooltip("How many pixels in one unity unit - change to match your sprites")]
        public int m_pixelsPerUnit = 16;
        [Tooltip("How much the camera should zoom.  Use powers of 2 to maintain pixel-perfect results")]
        public float m_zoom = 1.0f;
        [Tooltip("How much time it takes for the camera to reach it's target zoom level (in seconds)")]
        public float m_zoomSmoothTime = 0.1f;

        float m_zDistance;
        float m_gridSize;
        private Vector3 m_velocity = Vector3.zero;
        private Vector3 m_position;
        private float m_orthoSizeVelocity = 0.0f;

        bool m_trackingEnabled = true;
        Camera m_camera;

        // Use this for initialization
        void Start()
        {
            m_camera = GetComponent<Camera>();
            m_zDistance = transform.position.z;
            if (m_target != null)
            {
                Vector3 targetPosition = m_target.transform.position;
                m_position = new Vector3(targetPosition.x, targetPosition.y, m_zDistance);
            }
            MakeCameraPixelPerfect();
        }

        public void DisableTracking()
        {
            m_trackingEnabled = false;
        }

        public void EnableTracking()
        {
            m_trackingEnabled = true;
        }

        public Vector3 RoundVector3(Vector3 value)
        {
            return new Vector3(Mathf.Round(value.x), Mathf.Round(value.y), Mathf.Round(value.z));
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_trackingEnabled)
            {
                return;
            }
            MakeCameraPixelPerfect();

            if (m_target != null)
            {
                Vector3 targetCameraPosition = new Vector3(m_target.position.x, m_target.position.y, m_zDistance);
                m_position = Vector3.SmoothDamp(m_position, targetCameraPosition, ref m_velocity, m_smoothTime);
            }

            float pixelsWide = Screen.width / m_zoom;
            float pixelsHigh = Screen.height / m_zoom;
            float unitsWide = pixelsWide * m_gridSize;
            float unitsHigh = pixelsHigh * m_gridSize;

            Vector3 cornerWorldSpace = m_position - new Vector3(unitsWide, unitsHigh, 0.0f) * 0.5f;
            Vector3 cornerPixels = cornerWorldSpace * m_pixelsPerUnit;
            cornerPixels = RoundVector3(cornerPixels);
            cornerWorldSpace = cornerPixels * m_gridSize; // Now rounded to nearest pixel
            Vector3 centerWorldSpace = cornerWorldSpace + new Vector3(unitsWide, unitsHigh, 0.0f) * 0.5f;
            transform.position = centerWorldSpace;
        }

        void MakeCameraPixelPerfect()
        {
            // Some validation to stop it freaking out
            if (m_zoom == 0.0f)
            {
                m_zoom = 0.01f;
            }
            if (m_pixelsPerUnit == 0)
            {
                m_pixelsPerUnit = 1;
            }

            m_gridSize = 1.0f / m_pixelsPerUnit;

            if (m_camera.orthographic)
            {
                float unitsY = ((Screen.height * 0.5f * m_gridSize) / m_zoom);
                float targetOrthoSize = unitsY;
                m_camera.orthographicSize = Mathf.SmoothDamp(m_camera.orthographicSize, targetOrthoSize, ref m_orthoSizeVelocity, m_zoomSmoothTime);
            }
            else
            {
                // Change the field of view to suit the camera position
                //float unitsY = ((Screen.height * m_gridSize) / m_zoom);
                //float unitsX = Mathf.Abs(m_zDistance);
                //float screenAngle = Mathf.Atan(unitsY / unitsX);
                //camera.fieldOfView = screenAngle * Mathf.Rad2Deg;

                // Move the camera to fit the field view
                float unitsY = (Screen.height) * m_gridSize;
                float frustumInnerAngles = (180.0f - m_camera.fieldOfView) / 2.0f * Mathf.PI / 180.0f;
                float newCamDist = Mathf.Tan(frustumInnerAngles) * (unitsY * 0.5f);
                m_zDistance = -newCamDist / m_zoom;
            }
        }
    }
}
