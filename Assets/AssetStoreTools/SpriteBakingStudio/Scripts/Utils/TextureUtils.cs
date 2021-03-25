using System;
using System.IO;
using UnityEngine;

namespace SBS
{
    public class TextureUtils
    {
        public static bool CalcTextureBound(Texture2D tex, ScreenPoint pivot, TextureBound bound)
        {
            bound.minX = int.MaxValue;
            bound.maxX = int.MinValue;
            bound.minY = int.MaxValue;
            bound.maxY = int.MinValue;

            Color[] colors = tex.GetPixels();

            bool validPixelExist = false;
            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    float alpha = colors[y * tex.width + x].a;
                    if (alpha != 0)
                    {
                        bound.minX = x;
                        validPixelExist = true;
                        goto ENDMINX;
                    }
                }
            }

        ENDMINX:
            if (!validPixelExist)
                return false;

            validPixelExist = false;  
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = bound.minX; x < tex.width; x++)
                {
                    float alpha = colors[y * tex.width + x].a;
                    if (alpha != 0)
                    {
                        bound.minY = y;
                        validPixelExist = true;
                        goto ENDMINY;
                    }
                }
            }

        ENDMINY:
            if (!validPixelExist)
                return false;

            validPixelExist = false;
            for (int x = tex.width - 1; x >= bound.minX; x--)
            {
                for (int y = bound.minY; y < tex.height; y++)
                {
                    float alpha = colors[y * tex.width + x].a;
                    if (alpha != 0)
                    {
                        bound.maxX = x;
                        validPixelExist = true;
                        goto ENDMAXX;
                    }
                }
            }

        ENDMAXX:
            if (!validPixelExist)
                return false;

            validPixelExist = false;
            for (int y = tex.height - 1; y >= bound.minY; y--)
            {
                for (int x = bound.minX; x <= bound.maxX; x++)
                {
                    float alpha = colors[y * tex.width + x].a;
                    if (alpha != 0)
                    {
                        bound.maxY = y;
                        validPixelExist = true;
                        goto ENDMAXY;
                    }
                }
            }

        ENDMAXY:
            if (!validPixelExist)
                return false;

            pivot.x = Mathf.Clamp(pivot.x, bound.minX, bound.maxX);
            pivot.y = Mathf.Clamp(pivot.y, bound.minY, bound.maxY);

            return true;
        }

        public static void CalcTextureSymmetricBound(
            bool symmetricAroundPivot, bool verticalSymmectric,
            ScreenPoint pivot, int maxWidth, int maxHeight,
            TextureBound bound, TextureBound exBound)
        {
            if (pivot == null)
                return;

            if (symmetricAroundPivot)
            {
                int stt2pivot = pivot.x - bound.minX;
                int pivot2end = bound.maxX - pivot.x;
                if (stt2pivot > pivot2end)
                {
                    bound.maxX = pivot.x + stt2pivot;
                    if (bound.maxX >= maxWidth)
                        bound.maxX = maxWidth - 1;
                }
                else if (pivot2end > stt2pivot)
                {
                    bound.minX = pivot.x - pivot2end;
                    if (bound.minX < 0)
                        bound.minX = 0;
                }

                if (verticalSymmectric)
                {
                    stt2pivot = pivot.y - bound.minY;
                    pivot2end = bound.maxY - pivot.y;
                    if (stt2pivot > pivot2end)
                    {
                        bound.maxY = pivot.y + stt2pivot;
                        if (bound.maxY >= maxHeight)
                            bound.maxY = maxHeight - 1;
                    }
                    else if (pivot2end > stt2pivot)
                    {
                        bound.minY = pivot.y - pivot2end;
                        if (bound.minY < 0)
                            bound.minY = 0;
                    }
                }
            }

            exBound.minX = Mathf.Min(exBound.minX, bound.minX);
            exBound.maxX = Mathf.Max(exBound.maxX, bound.maxX);
            exBound.minY = Mathf.Min(exBound.minY, bound.minY);
            exBound.maxY = Mathf.Max(exBound.maxY, bound.maxY);
        }

        public static Texture2D TrimTexture(Texture2D tex, TextureBound bound, int margin = 0)
        {
            if (tex == null)
                return Texture2D.whiteTexture;

            int trimmedWidth = bound.maxX - bound.minX + 1 + margin * 2;
            int trimmedHeight = bound.maxY - bound.minY + 1 + margin * 2;

            if (trimmedWidth < 0 || trimmedHeight < 0)
                return Texture2D.whiteTexture;

            Color[] originalColors = tex.GetPixels();
            Color[] resultColors = new Color[trimmedWidth * trimmedHeight];

            for (int y = 0; y < trimmedHeight; y++)
            {
                for (int x = 0; x < trimmedWidth; x++)
                {
                    Color color = Color.clear;
                    if (x >= margin && y >= margin && x <= trimmedWidth - margin && y <= trimmedHeight - margin)
                    {
                        int index = (bound.minY + y - margin) * tex.width + (bound.minX + x - margin);
                        if (index >= originalColors.Length)
                            continue;
                        color = originalColors[index];
                    }
                    resultColors[y * trimmedWidth + x] = color;
                }
            }

            Texture2D trimmedTex = new Texture2D(trimmedWidth, trimmedHeight, TextureFormat.ARGB32, false);
            trimmedTex.SetPixels(resultColors);

            return trimmedTex;
        }

        public static void UpdatePivot(ScreenPoint pivot, TextureBound bound, int margin = 0)
        {
            pivot.x -= (bound.minX - margin);
            pivot.y -= (bound.minY - margin);
        }

        public static Texture2D MoveTextureBy(Texture2D tex, int moveX, int moveY)
        {
            if (tex == null)
                return Texture2D.whiteTexture;

            // TODO: Use GetPixels and SetPixels.

            int newTexWidth = tex.width + Math.Abs(moveX);
            int newTexHeight = tex.height + Math.Abs(moveY);

            Texture2D movedTex = new Texture2D(newTexWidth, newTexHeight, TextureFormat.ARGB32, false);
            for (int y = 0; y < newTexHeight; y++)
            {
                for (int x = 0; x < newTexWidth; x++)
                {
                    Color color = Color.clear;

                    int refX = x - moveX, refY = y - moveY;
                    if (refX >= 0 && refY >= 0 && refX <= tex.width && refY <= tex.height)
                        color = tex.GetPixel(refX, refY);

                    movedTex.SetPixel(x, y, color);
                }
            }

            return movedTex;
        }

        public static Texture2D ScaleTexture(Texture2D source, int destWidth, int destHeight)
        {
            Color[] pixels = new Color[destWidth * destHeight];
            float incX = 1.0f / (float)destWidth;
            float incY = 1.0f / (float)destHeight;
            for (int i = 0; i < pixels.Length; i++)
            {
                float u = incX * ((float)i % destWidth);
                float v = incY * ((float)Mathf.Floor(i / destWidth));
                pixels[i] = source.GetPixelBilinear(u, v);
            }

            Texture2D dest = new Texture2D(destWidth, destHeight, source.format, false);
            dest.SetPixels(pixels, 0);

            return dest;
        }

        public static string SaveTexture(string dirPath, string fileName, Texture2D tex)
        {
            string filePath = "";

            try
            {
#if UNITY_WEBPLAYER
                Debug.Log("Don't set 'Build Setting > Platform' to WebPlayer!");
#else
                filePath = Path.Combine(dirPath, fileName + ".png");
                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(filePath, bytes);
#endif
            }
            catch (Exception e)
            {
                throw e;
            }

            return filePath;
        }
    }
}
