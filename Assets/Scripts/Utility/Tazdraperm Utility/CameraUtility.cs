using UnityEngine;

namespace Tazdraperm.Utility
{
    public static class CameraUtility
    {
        public static Camera Main;

        static CameraUtility()
        {
            Main = Camera.main;
        }

        public static Vector2 MouseToWorldPosition()
        {
            var pos = Input.mousePosition;
            pos.z = Main.nearClipPlane;
            return Main.ScreenToWorldPoint(pos);
        }
    }
}