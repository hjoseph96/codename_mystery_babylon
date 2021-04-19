using UnityEngine;
using System.Collections;

namespace SS.Tools
{
	public class ScreenshotTools
    {
        public enum RenderQuality
        {
            #if UNITY_5_3_OR_NEWER
            High = 8192,
            #else
            High = 4096,
            #endif
            Medium = 1024,
            Low = 512
        }

        public static RenderQuality renderQuality
        {
            get;
            set;
        }

        public static int maxHeight
        {
            get
            {
                return (int)renderQuality;
            }
        }

        public static int maxWidth
        {
            get
            {
                return Mathf.RoundToInt((float)maxHeight * Screen.width / Screen.height);
            }
        }

        public static Texture2D ScreenShot()
        {
            return ScreenShot(maxWidth, maxHeight);
        }

        public static Texture2D ScreenShot(int w, int h)
        {
            Camera cam = Camera.main;
            
            RenderTexture rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(w, h, TextureFormat.ARGB32, false);
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.Destroy(rt);
            
            System.GC.Collect();
            
            return screenShot;
        }

        public static Color ReadFirstPixel()
        {
            Camera cam = Camera.main;

            RenderTexture rt = new RenderTexture(maxWidth, maxHeight, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.Destroy(rt);

            Color color = screenShot.GetPixel(0, 0);
            Object.Destroy(screenShot);

            System.GC.Collect();

            return color;
        }

        static ScreenshotTools()
        {
            renderQuality = RenderQuality.Medium;
        }
    }
}
