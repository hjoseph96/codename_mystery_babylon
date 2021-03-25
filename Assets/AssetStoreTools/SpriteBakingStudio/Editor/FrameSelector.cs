using System;
using UnityEditor;
using UnityEngine;

namespace SBS
{
    public class FrameSelector : ScriptableWizard
    {
        public static FrameSelector instance;

        private SpriteBakingStudio studio = null;

        Vector2 whatPos = Vector2.zero;
        private const float LABEL_HEIGHT = 20.0f;

        void OnEnable()
        {
			minSize = new Vector2(400f, 300f);
            instance = this;
        }

        void OnDisable()
        {
            instance = null;

            if (FramePreviewer.instance != null)
                FramePreviewer.instance.Close();
        }

        public void SetStudio(SpriteBakingStudio studio)
        {
            this.studio = studio;
        }

        void OnGUI()
        {
            try
            {
                if (studio == null)
                    return;

                if (studio.samplings == null || studio.samplings.Count == 0)
                    return;

                int texWidth = studio.samplings[0].tex.width;
                int texHeight = studio.samplings[0].tex.height;

                float padding = 10.0f;
                int colSize = Mathf.FloorToInt(Screen.width / (texWidth + padding));
                if (colSize < 1)
                    colSize = 1;

                if (studio.samplings.Count > 1)
                {
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Select all"))
                    {
                        studio.selectedFrames.Clear();
                        for (int i = 0; i < studio.samplings.Count; ++i)
                            studio.selectedFrames.Add(new Frame(i, studio.samplings[i].time));
                    }

                    if (GUILayout.Button("Select each half"))
                    {
                        int modular = 0;
                        if (studio.selectedFrames.Count >= 2)
                        {
                            if (studio.selectedFrames[0].number == 0)
                                modular = 1;
                            else if (studio.selectedFrames[0].number == 1)
                                modular = 0;
                        }

                        studio.selectedFrames.Clear();
                        for (int i = 0; i < studio.samplings.Count; ++i)
                        {
                            if (i % 2 == modular)
                                studio.selectedFrames.Add(new Frame(i, studio.samplings[i].time));
                        }
                    }

                    if (GUILayout.Button("Clear all"))
                        studio.selectedFrames.Clear();

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(padding);

                whatPos = GUILayout.BeginScrollView(whatPos);
                {
                    Rect rect = new Rect(padding, 0, texWidth, texHeight);

                    int col = 0;
                    int rowCount = 0;
                    for (int smpi = 0; smpi < studio.samplings.Count; ++smpi)
                    {
                        SamplingData sampling = studio.samplings[smpi];

                        if (col >= colSize)
                        {
                            col = 0;
                            rowCount++;

                            rect.x = padding;
                            rect.y += texHeight + padding + LABEL_HEIGHT;

                            GUILayout.EndHorizontal();
                            GUILayout.Space(texHeight + padding);
                        }

                        if (col == 0)
                            GUILayout.BeginHorizontal();

                        if (GUI.Button(rect, ""))
                        {
                            if (studio.selectedFrames.Count == 0)
                            {
                                studio.selectedFrames.Add(new Frame(smpi, sampling.time));
                            }
                            else
                            {
                                bool exist = false;
                                foreach (Frame selectedFrame in studio.selectedFrames)
                                {
                                    if (smpi == selectedFrame.number)
                                    {
                                        exist = true;
                                        break;
                                    }
                                }

                                int inserti = 0;
                                for (; inserti < studio.selectedFrames.Count; ++inserti)
                                {
                                    if (smpi < studio.selectedFrames[inserti].number)
                                        break;
                                }

                                if (exist)
                                    studio.selectedFrames.Remove(new Frame(smpi, 0));
                                else
                                    studio.selectedFrames.Insert(inserti, new Frame(smpi, sampling.time));
                            }
                        }

                        EditorGUI.DrawTextureTransparent(rect, sampling.tex);

                        foreach (Frame selectedFrame in studio.selectedFrames)
                        {
                            if (selectedFrame.number == smpi)
                            {
                                DrawingUtils.DrawOutline(rect, Color.red, 2.0f);
                                break;
                            }
                        }

                        GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
                        GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
                        GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, LABEL_HEIGHT),
                            smpi.ToString(), "ProgressBarBack");
                        GUI.contentColor = Color.white;
                        GUI.backgroundColor = Color.white;

                        col++;
                        rect.x += texWidth + padding;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(texHeight + padding);

                    GUILayout.Space(rowCount * 26);
                }
                GUILayout.EndScrollView();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Close();
            }
        }
    }
}
