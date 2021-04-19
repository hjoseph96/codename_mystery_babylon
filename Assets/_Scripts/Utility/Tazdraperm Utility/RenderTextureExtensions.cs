using UnityEngine;

namespace Tazdraperm.Utility
{
    public static class RenderTextureExtensions
    {
        public static void SaveToFile(this RenderTexture rt, string path = "RenderTexture.png")
        {
            path = "Assets/" + path;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RenderTexture.active = null;

            var bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            Resources.Load(path);
        }
    }
}
