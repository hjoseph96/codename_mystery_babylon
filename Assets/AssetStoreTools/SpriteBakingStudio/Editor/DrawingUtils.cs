using UnityEngine;
using UnityEditor;

namespace SBS
{
    public class DrawingUtils
    {
        public static void DrawSpriteBackground(Rect rect)
        {
            DrawSpriteBackground(rect.x, rect.y, rect.width, rect.height);
        }

        public static void DrawSpriteBackground(float startPosX, float startPosY, float spriteWidth, float spriteHeight)
        {
            Texture2D grayTexture = new Texture2D(1, 1);
            grayTexture.SetPixel(0, 0, Color.gray);
            grayTexture.Apply();

            const float BG_RECT_LEN = 10f;
            int bgRectCol = Mathf.CeilToInt(spriteWidth / BG_RECT_LEN);
            int index = 0;

            for (float y = startPosY; y < startPosY + spriteHeight; y += BG_RECT_LEN)
            {
                float height = BG_RECT_LEN;
                if (y + BG_RECT_LEN > startPosY + spriteHeight)
                    height = startPosY + spriteHeight - y;

                for (float x = startPosX; x < startPosX + spriteWidth; x += BG_RECT_LEN)
                {
                    Texture2D tex = index % 2 == 0 ? Texture2D.whiteTexture : grayTexture;

                    float width = BG_RECT_LEN;
                    if (x + BG_RECT_LEN > startPosX + spriteWidth)
                        width = startPosX + spriteWidth - x;

                    GUI.DrawTexture(new Rect(x, y, width, height), tex);

                    ++index;
                }

                if (bgRectCol % 2 == 0)
                    index++;
            }
        }

        public static void DrawOutline(Rect rect, Color color, float lineWidth)
        {
            Texture2D tex = Texture2D.whiteTexture;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, lineWidth, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMax - lineWidth, rect.yMin, lineWidth, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, lineWidth), tex);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - lineWidth, rect.width, lineWidth), tex);
            GUI.color = Color.white;
        }

        public static bool DrawWideButton(string name)
        {
            GUIStyle style = new GUIStyle("button");
            style.fontSize = Global.WIDE_BUTTON_FONT_SIZE;
            return GUILayout.Button(name, style, GUILayout.Height(Global.WIDE_BUTTON_HEIGHT));
        }

        public static bool DrawNarrowButton(string name, int width = 0)
        {
            GUIStyle style = new GUIStyle("button");
            style.fontSize = Global.NARROW_BUTTON_FONT_SIZE;

            if (width == 0)
                return GUILayout.Button(name, style, GUILayout.Height(Global.NARROW_BUTTON_HEIGHT));
            else
                return GUILayout.Button(name, style, GUILayout.Width(width), GUILayout.Height(Global.NARROW_BUTTON_HEIGHT));
        }

        public static bool DrawMiddleButton(string name)
        {
            GUIStyle style = new GUIStyle("button");
            style.fontSize = Global.MIDDLE_BUTTON_FONT_SIZE;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool previewUpdateClicked = GUILayout.Button(name, style, GUILayout.Width(Global.MIDDLE_BUTTON_WIDTH), GUILayout.Height(Global.MIDDLE_BUTTON_HEIGHT));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            return previewUpdateClicked;
        }
    }
}
