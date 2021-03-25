/*
The MIT License (MIT)

Copyright (c) 2014 Andrew Fray

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 */
using UnityEngine;
using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace TenPN.DecisionFlex.Demos
{
    [AddComponentMenu("TenPN/DecisionFlex/Demos/MainMenu")]
    public class MainMenu : MonoBehaviour
    {
        //////////////////////////////////////////////////

        //////////////////////////////////////////////////

        private void Start() 
        {
            // double-check we're unpaused.
            Time.timeScale = 1f;
        }

        private void OnGUI()
        {
            const float menuWidthProp = 0.5f;
            float menuWidth = Screen.width * menuWidthProp;
            float menuLeft = 0.5f * (Screen.width - menuWidth);

            const float menuHeightProp = 0.8f;
            float menuHeight = Screen.height * menuHeightProp;
            float menuTop = 0.5f * (Screen.height - menuHeight);

            var menuRect = new Rect(menuLeft, menuTop, 
                                    menuWidth, menuHeight);

            GUILayout.BeginArea(menuRect, GUI.skin.box);
            GUILayout.BeginVertical();

            Center(() => GUILayout.Label("DecisionFlex Demo"));

            GUILayout.FlexibleSpace();
            Center(() => GUILayout.Label("Choose a scene:"));

            var loadLevelLayout = GUILayout.MinWidth(menuWidth * 0.9f);
            var levelNames = new string[] { 
                "DecisionFlex_iPerson", 
                "DecisionFlex_WeaponSelection", 
                "DecisionFlex_UFOs" 
            };
            foreach(var levelName in levelNames)
            {
                Center(() => { 
                        var publicName = levelName.Replace("DecisionFlex_", "");
                        if (GUILayout.Button(publicName, loadLevelLayout))
                        {
#if UNITY_5_3_OR_NEWER
                            SceneManager.LoadScene(levelName);
#else
                            Application.LoadLevel(levelName);
#endif
                        }
                    });
            }
            GUILayout.FlexibleSpace();

            GUILayout.Label("Space shooter art by Kenny.nl, iPerson avatar by Noble Master Games");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void Center(System.Action controls)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            controls();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
