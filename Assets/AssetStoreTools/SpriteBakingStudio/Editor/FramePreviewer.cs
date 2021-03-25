using System;
using UnityEditor;
using UnityEngine;

namespace SBS
{
    public class FramePreviewer : ScriptableWizard
    {
        public static FramePreviewer instance;

        private SpriteBakingStudio studio = null;
        private StudioSetting setting = null;

        private bool playing = false;
        private bool looping = true;
        private int selectedFrameIndex = 0;
        private int frameNumber = 0;
        private float nextFrameTime = 0f;
        private float nextSpriteTime = 0f;

        public void OnEnable()
        {
            instance = this;

            EditorApplication.update -= UpdateState;
            EditorApplication.update += UpdateState;

            Reset();
            playing = true;
        }

        public void OnDisable()
        {
            instance = null;

            EditorApplication.update -= UpdateState;

            if (FrameSelector.instance != null)
                FrameSelector.instance.Close();
        }

        public void SetStudio(SpriteBakingStudio studio)
        {
            this.studio = studio;
            this.setting = studio.setting;
        }

        void OnGUI()
        {
            try
            {
                if (studio == null)
                    return;

                if (studio.samplings.Count == 0 || studio.selectedFrames.Count == 0)
                    return;

                const float MIN_EDITOR_WIDTH = 220f;
                const float BUTTOM_HEIGHT = 18f;
                const float MENU_HEIGHT = 60;
                const float MARGIN = 2f;

                int spriteWidth = studio.samplings[0].tex.width;
                int spriteHeight = studio.samplings[0].tex.height;

                float frameLabelWidth = 20f;
                if (studio.selectedFrames.Count > 100)
                    frameLabelWidth = 28f;

                float editorWidth = Mathf.Max(spriteWidth + (MARGIN+frameLabelWidth) * 2f, MIN_EDITOR_WIDTH);

                position = new Rect(position.x, position.y, editorWidth, MENU_HEIGHT + (float)spriteHeight + MARGIN);

                int oldFrameRate = setting.frameSamples;
                setting.frameSamples = EditorGUILayout.IntField("Samples", setting.frameSamples, GUILayout.Width(MIN_EDITOR_WIDTH - 10f));
                if (setting.frameSamples <= 0)
                    setting.frameSamples = 20;

                int oldSpriteInterval = setting.spriteInterval;
                setting.spriteInterval = EditorGUILayout.IntField("Sprite Interval", setting.spriteInterval);
                if (setting.spriteInterval <= 0)
                    setting.spriteInterval = 1;

                if (oldFrameRate != setting.frameSamples || oldSpriteInterval != setting.spriteInterval)
                    Reset();

                bool oldPlaying = playing;
                Rect playRect = new Rect(MARGIN, 40f, editorWidth / 2f - MARGIN, BUTTOM_HEIGHT);
                playing = GUI.Toggle(playRect, playing, "Play", GUI.skin.button);
                if (playing != oldPlaying)
                {
                    if (playing)
                    {
                        Reset();
                        playing = true;
                    }
                    else
                    {
                        playing = false;
                    }
                }

                Rect loopRect = new Rect(MARGIN + editorWidth / 2f, 40f, editorWidth / 2f - MARGIN, BUTTOM_HEIGHT);
                looping = GUI.Toggle(loopRect, looping, "Loop", GUI.skin.button);

                float spritePosX = MARGIN + frameLabelWidth;
                if (spriteWidth < editorWidth)
                    spritePosX = (editorWidth - spriteWidth) / 2f;
                float spritePosY = MENU_HEIGHT;

                Rect spriteRect = new Rect(spritePosX, spritePosY, spriteWidth, spriteHeight);
                EditorGUI.DrawTextureTransparent(spriteRect, studio.samplings[frameNumber].tex);
                DrawingUtils.DrawOutline(spriteRect, Color.black, 1f);

                GUI.enabled = false;
                Rect frameLabelRect = new Rect(1.0f, spritePosY, frameLabelWidth, 15f);
                GUI.DrawTexture(frameLabelRect, Texture2D.whiteTexture);
                GUI.TextField(frameLabelRect, frameNumber.ToString());
                GUI.enabled = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.update -= UpdateState;
                Close();
            }
        }

        public void UpdateState()
        {
            try
            {
                if (studio.selectedFrames.Count == 0)
                    return;

                if (!playing)
                    return;

                if (Time.realtimeSinceStartup > nextFrameTime)
                {
                    float unitTime = 1f / setting.frameSamples;
                    nextFrameTime += unitTime;

                    if (Time.realtimeSinceStartup > nextSpriteTime)
                    {
                        selectedFrameIndex = (selectedFrameIndex + 1) % studio.selectedFrames.Count;
                        frameNumber = studio.selectedFrames[selectedFrameIndex].number;
                        Repaint();

                        if (selectedFrameIndex >= studio.selectedFrames.Count - 1)
                        {
                            if (!looping)
                            {
                                playing = false;
                                return;
                            }
                        }

                        nextSpriteTime += unitTime * setting.spriteInterval;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.update -= UpdateState;
                Close();
            }
        }

        private void Reset()
        {
            selectedFrameIndex = 0;
            frameNumber = 0;
            nextFrameTime = Time.realtimeSinceStartup;
            nextSpriteTime = Time.realtimeSinceStartup;
        }
    }
}
