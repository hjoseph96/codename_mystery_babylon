using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using SpriteFlashTools.Extensions;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;


[CustomEditor(typeof(SpriteFlashTool))]
public class SpriteFlashToolEditor : Editor
{
    // Settings:
    private readonly string previewButtonColorString = "#31c5f4";  //  F6BE02FF   F9BC04FF
    private readonly string colorPresetsPath = "Assets/Sprite Flash Tool/Data/Color_Presets.txt";
    private static bool settings = false;

    // Ref:
    SpriteFlashTool scriptRef;
    private SpriteRenderer _myRenderer;
    private int _matAlpha;
    private int _matColor;

    // Local
    private static double timeUntilDeactivation = 0;
    private static double timeOfDeactivation = 0;
    private static bool isActivted = false;
    private float flashedAmountCaptured = 0f;
    //private int additionalHeightForPreviewMode = 0;
    public static string currentlySelectedPart = "";

    // GUI
    private static bool previewMode = false;
    private Color previewModeCapturedFlashColor;
    private float previewModeCapturedFlashAmount;
    private GUIStyle flashButtonStyle;
    private GUIStyle flashPartButtonStyle;
    private GUIStyle previewButtonStyle;
    private GUIStyle resetButtonStyle;
    private GUIStyle saveColorPresetButtonStyle;
    private GUIStyle colorPresetButtonStyle;
    private GUIStyle settingsButtonStyle;
    //private GUIStyle copyPasteButtonStyle;

    private Color previewButtonColor;

    // Shader:
    private readonly string[] _shaderChoices = new[] { "Default", "Diffuse" };

    void SetUpReferences()
    {
        scriptRef = (SpriteFlashTool)target;
        _myRenderer = scriptRef.gameObject.GetComponent<SpriteRenderer>();
        _matAlpha = Shader.PropertyToID("_FlashAmount");
        _matColor = Shader.PropertyToID("_FlashColor");
    }

    void Awake()
    {
        SetUpReferences();
        try
        {
            if (Shader.Find("Sprites/Flash Tool/" + scriptRef.currentShader) != _myRenderer.sharedMaterial.shader)
            {
                SetUpMaterial(true, false);
            }

            if (scriptRef.useMultipleSprites)
            {
                bool setUpAdditionalParts = false;
                for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
                {
                    if (Shader.Find("Sprites/Flash Tool/" + scriptRef.myAdditionalPartsList[i].selectedMaterial) != scriptRef.myAdditionalPartsList[i].spriteRenderer.sharedMaterial.shader)
                    {
                        setUpAdditionalParts = true;
                        break;
                    }
                }
                SetUpMaterial(false, setUpAdditionalParts);
            }
        }
        catch
        {
            SetUpMaterial(true, true);
        }
    }
    void OnEnable()
    {
        if (!_myRenderer)
            SetUpReferences();
        EditorApplication.update += CustomEditorUpdateCallback;
        InitInspector();
    }
    void OnDisable()
    {
        EditorApplication.update -= CustomEditorUpdateCallback;
    }

    public override void OnInspectorGUI()
    {
        if (InspectorGUIStart(alwaysDrawInspector) == false)
            return;

        EditorGUI.BeginChangeCheck();

        if (!SpriteFlashToolExtensions.FileExists(colorPresetsPath))
        {
            EditorGUILayout.HelpBox("Color Preset File doesn't exist. \n" + colorPresetsPath, MessageType.Error, true);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create File", GUILayout.Width(85), GUILayout.Height(25)))
                SpriteFlashToolExtensions.CreateFile(colorPresetsPath);
            if (GUILayout.Button("Open Folder", GUILayout.Width(85), GUILayout.Height(25)))
                SpriteFlashToolExtensions.OpenFile("Assets/Sprite Flash Tool/Data");
            EditorGUILayout.EndHorizontal();
            return;
        }
        //additionalHeightForPreviewMode = 0;

        flashButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 20,
            fixedWidth = 70
        };

        flashPartButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 20,
            fixedWidth = 80
        };

        previewButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 20,
            fixedWidth = 90
        };

        resetButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 18,
            fixedWidth = 54,
            fontSize = 9
        };

        saveColorPresetButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 15,
            fixedWidth = 45,
            fontSize = 11
        };

        colorPresetButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 15,
            fixedWidth = 15
        };

        settingsButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 18,
            fixedWidth = 45
        };

        //copyPasteButtonStyle = new GUIStyle(GUI.skin.button);
        //copyPasteButtonStyle.fixedHeight = 18;
        //copyPasteButtonStyle.fixedWidth = 45;

        if (!ColorUtility.TryParseHtmlString(previewButtonColorString, out previewButtonColor))
            previewButtonColor = GUI.contentColor;
        //if (SpriteFlashToolExtensions.GetFileLinesCount(colorPresetsPath) > 0)
        //    additionalHeightForPreviewMode += 15;
        //if (settings)
        //    additionalHeightForPreviewMode += 22;

        if (previewMode)
        {
            //var r = EditorGUILayout.BeginVertical();
            //EditorGUILayout.EndVertical();
            //r = new Rect(r.x - 13, r.y - 17, EditorGUIUtility.currentViewWidth, r.height + 195 + additionalHeightForPreviewMode + 20 * GetColorPresetsRowCount());
            //EditorGUI.DrawRect(r, previewButtonColor * 0.3f);

            //EditorGUILayout.LabelField("Preview Mode");
            GUI.backgroundColor = previewButtonColor;
            EditorGUILayout.HelpBox("You are now in Preview Mode. Change the color and amount to see how it would look when Flash happens.", MessageType.Info, true);
            GUI.backgroundColor = GUI.contentColor;
        }

        //EditorGUILayout.BeginHorizontal();
        //if (GUILayout.Button("Copy", copyPasteButtonStyle))
        //    UnityEditorInternal.ComponentUtility.CopyComponent(scriptRef);
        //if (GUILayout.Button("Paste", copyPasteButtonStyle))
        //    UnityEditorInternal.ComponentUtility.PasteComponentValues(scriptRef);
        //EditorGUILayout.EndHorizontal();

        //EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();
        settings = EditorGUILayout.ToggleLeft("Color Presets", settings);

        if (settings)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find", settingsButtonStyle))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(colorPresetsPath).GetInstanceID());
                SpriteFlashToolExtensions.OpenFileExplorer(colorPresetsPath);
            }
            if (GUILayout.Button("Open", settingsButtonStyle))
                SpriteFlashToolExtensions.OpenFile(colorPresetsPath);

            if (GUILayout.Button("Copy", settingsButtonStyle))
                SpriteFlashToolExtensions.GetFullTextFileInString(colorPresetsPath, true).CopyToClipboard();

            if (GUILayout.Button("Clear", settingsButtonStyle))
                SpriteFlashToolExtensions.ClearTextFile(colorPresetsPath);

            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.HelpBox("You are now in Preview Mode. change the color and amount to see how it would look like when Flash happens.", MessageType.Info, true);

        }
        EditorGUILayout.EndVertical();
        if (SpriteFlashToolExtensions.GetFileLinesCount(colorPresetsPath) > 0)
        {
            //GUILayout.Label("Color Presets");
            EditorGUILayout.BeginHorizontal();
            using (var sr = new StreamReader(colorPresetsPath))
            {
                string line;
                int _currentColorCount = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if (_currentColorCount > (int)(EditorGUIUtility.currentViewWidth / 21.5f))
                    {
                        _currentColorCount = 0;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }

                    Color _currentColor;
                    if (ColorUtility.TryParseHtmlString(line, out _currentColor))
                    {
                        GUI.backgroundColor = _currentColor;
                        if (GUILayout.Button("", colorPresetButtonStyle))
                        {
                            if (Event.current.button == 1 || Event.current.button == 2)
                            {
                                sr.Close();
                                SpriteFlashToolExtensions.DeleteLineInFile(colorPresetsPath, line);
                                break;
                            }
                            else if (Event.current.button == 0)
                            {
                                scriptRef.flashColor = _currentColor;
                            }
                        }
                        _currentColorCount++;
                    }
                };
            }
            GUI.backgroundColor = GUI.contentColor;
            EditorGUILayout.EndHorizontal();
        }



        EditorGUILayout.BeginHorizontal();
        scriptRef.flashColor = EditorGUILayout.ColorField("Flash Color", scriptRef.flashColor);
        if (GUILayout.Button("Save", saveColorPresetButtonStyle))
            SaveColorPreset();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        scriptRef.flashAmount = EditorGUILayout.Slider("Flash Amount", scriptRef.flashAmount, 0f, 1f);
        scriptRef.duration = EditorGUILayout.DoubleField("Duration", scriptRef.duration);
        if (scriptRef.duration < 0)
            scriptRef.duration = 0;
        scriptRef.decreaseAmountOverTime = EditorGUILayout.Toggle("Decrease Amount Over Time", scriptRef.decreaseAmountOverTime);


        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = GUI.contentColor;

        if (GUILayout.Button("Flash", flashButtonStyle))
            Flash();

        string allText = "";
        if (scriptRef.useMultipleSprites)
        {
            allText = " All"; ;
            if (GUILayout.Button("Flash" + allText, flashButtonStyle))
                Flash(true);
        }


        if (previewMode)
        {
            GUI.backgroundColor = previewButtonColor;
            if (GUILayout.Button("Exit Preview", previewButtonStyle))
                DisablePreviewMode();
            GUI.backgroundColor = Color.grey * 0.5f;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", resetButtonStyle))
                ResetAll();
            EditorGUILayout.EndHorizontal();

        }
        else
        {


            GUI.backgroundColor = previewButtonColor;
            if (GUILayout.Button("Preview" + allText, previewButtonStyle))
                EnablePreviewMode();
            GUI.backgroundColor = Color.grey * 0.5f;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear" + allText, resetButtonStyle))
                ResetAll();
            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = GUI.contentColor;



        //EditorGUILayout.Space();


        int previouslySelected = 0;
        for (int i = 0; i < _shaderChoices.Length; i++)
        {
            if (_shaderChoices[i] == scriptRef.currentShader)
            {
                previouslySelected = i;
                break;
            }
        }
        int _shaderChoiceIndex = EditorGUILayout.Popup("Selected Material", previouslySelected, _shaderChoices);
        if (previouslySelected != _shaderChoiceIndex)
        {
            scriptRef.currentShader = _shaderChoices[_shaderChoiceIndex];
            SetUpMaterial(true, false);
        }

        if (scriptRef.useMultipleSprites)
        {
            bool setUpAdditionalParts = false;
            for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
            {
                try
                {
                    if (Shader.Find("Sprites/Flash Tool/" + scriptRef.myAdditionalPartsList[i].selectedMaterial) != scriptRef.myAdditionalPartsList[i].spriteRenderer.sharedMaterial.shader)
                    {
                        setUpAdditionalParts = true;
                        break;
                    }
                }
                catch
                {
                    scriptRef.myAdditionalPartsList[i].partName = "New Part " + i.ToString();
                    scriptRef.myAdditionalPartsList[i].selectedMaterial = _shaderChoices[0];
                    scriptRef.myAdditionalPartsList[i].flashAmount = 1f;
                    scriptRef.myAdditionalPartsList[i].duration = 1f;
                    scriptRef.myAdditionalPartsList[i].flashColor = Color.white;
                    scriptRef.myAdditionalPartsList[i].decreaseAmountOverTime = true;
                    SetUpMaterial(false, true);
                    //Debug.Log(scriptRef.myAdditionalPartsList[i].selectedMaterial);
                    //Debug.Log(scriptRef.myAdditionalPartsList[i].sprite.sharedMaterial.shader);
                    break;
                }
            }
            SetUpMaterial(false, setUpAdditionalParts);
        }


        scriptRef.useMultipleSprites = EditorGUILayout.ToggleLeft("Use Mltiple Sprites", scriptRef.useMultipleSprites);


        //           V1.2.1
        //if (scriptRef.useMultipleSprites)
        //{
        //    var serializedObject = new SerializedObject(target);
        //    var property = serializedObject.FindProperty("myRendererList");
        //    //serializedObject.Update();
        //    EditorGUILayout.PropertyField(property, true);
        //    serializedObject.ApplyModifiedProperties();
        //}


        if (scriptRef.useMultipleSprites)
        {

            EditorGUILayout.BeginHorizontal();

            List<String> _partChoisesList = new List<string>();
            for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
                _partChoisesList.Add(scriptRef.myAdditionalPartsList[i].partName);

            int previouslySelectedPart = 0;
            for (int i = 0; i < _partChoisesList.Count; i++)
            {
                if (_partChoisesList[i] == currentlySelectedPart)
                {
                    previouslySelectedPart = i;
                    break;
                }
            }


            if (GUILayout.Button("Flash Part", flashPartButtonStyle))
            {
                if (scriptRef.myAdditionalPartsList.Count > 0)
                {
                    if(EditorApplication.isPlaying)
                    {
                        scriptRef.FlashPart(_partChoisesList[previouslySelectedPart]);
                    }
                    else
                    {
                        int partID = 0;
                        for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
                        {
                            if (scriptRef.myAdditionalPartsList[i].partName == currentlySelectedPart)
                            {
                                partID = i;
                                break;
                            }
                        }
                        if (scriptRef.myAdditionalPartsList[partID].spriteRenderer)
                        {
                            Material tempMatForPart = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.myAdditionalPartsList[partID].selectedMaterial));
                            tempMatForPart.SetColor(_matColor, scriptRef.myAdditionalPartsList[partID].flashColor);
                            tempMatForPart.SetFloat(_matAlpha, scriptRef.myAdditionalPartsList[partID].flashAmount);
                            scriptRef.myAdditionalPartsList[partID].spriteRenderer.sharedMaterial = tempMatForPart;

                            scriptRef.myAdditionalPartsList[partID].flashedAmountCaptured = scriptRef.myAdditionalPartsList[partID].flashAmount;

                            scriptRef.myAdditionalPartsList[partID].timeOfDeactivation = EditorApplication.timeSinceStartup + scriptRef.myAdditionalPartsList[partID].duration;
                            scriptRef.myAdditionalPartsList[partID].isActivted = true;
                        }
                        else
                        {
                            SetUpMaterial(false, true);
                        }
                    }
                }
            }

            
            int _partChoiceIndex = EditorGUILayout.Popup("", previouslySelectedPart, _partChoisesList.ToArray());
            if (previouslySelectedPart != _partChoiceIndex)
            {
                currentlySelectedPart = _partChoisesList[_partChoiceIndex];
            }
            EditorGUILayout.EndHorizontal();


            var _property = serializedObject.FindProperty("myAdditionalPartsList");
            DrawPropertySortableArray(_property);
        }

       

        if (GUI.changed && !EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(scriptRef);
            EditorSceneManager.MarkSceneDirty(scriptRef.gameObject.scene);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            InitInspector(true);
        }

        if (ListReordedbool)
        {
            ListReordedbool = false;
        }

        //Event _event = Event.current;
    }
    

    private void SetUpMaterial(bool mainRenderer = true, bool additionalParts = false)
    {
        if (!EditorApplication.isPlaying)
        {
            //if(!_myRenderer.sharedMaterial)
            //{
            //    Debug.LogWarning("Sprite Flash Tool Update v1.2:\n" +
            //        "Move 'Materials' folder(located: Sprite Flash Tool/Materials) under 'Resources' folder.\n" +
            //        "If there is no resources folder");
            //}

            if (mainRenderer)
                _myRenderer.sharedMaterial = Resources.Load("Materials/SpriteFlashTool_" + scriptRef.currentShader) as Material;

            if (scriptRef.useMultipleSprites && additionalParts)
            {
                for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
                {
                    if (scriptRef.myAdditionalPartsList[i].spriteRenderer != null)
                    {
                        scriptRef.myAdditionalPartsList[i].spriteRenderer.sharedMaterial = Resources.Load("Materials/SpriteFlashTool_" + scriptRef.myAdditionalPartsList[i].selectedMaterial) as Material;
                    }
                }
            }
        }
    }

    private int GetColorPresetsRowCount()
    {
        int _rowCount = 0;
        int _currentCount = 0;
        if (SpriteFlashToolExtensions.GetFileLinesCount(colorPresetsPath) > 0)
            _rowCount = 1;
        using (var sr = new StreamReader(colorPresetsPath))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                Color _currentColor;
                if (ColorUtility.TryParseHtmlString(line, out _currentColor))
                    _currentCount++;
                if (_currentCount > (int)(EditorGUIUtility.currentViewWidth / 21.5f))
                {
                    _currentCount = 0;
                    _rowCount++;
                }
            };
        }
        return _rowCount;
    }

    private void SaveColorPreset()
    {
        string _currentFlashColor = "#" + ColorUtility.ToHtmlStringRGB(scriptRef.flashColor);
        if (!SpriteFlashToolExtensions.LineAlreadyExist(colorPresetsPath, _currentFlashColor, "Color Preset already exists!"))
            SpriteFlashToolExtensions.AddNewLineToFile(colorPresetsPath, _currentFlashColor);

    }


    private void ResetAll()
    {
        DisablePreviewMode();
        Deactivate(true);
    }


    private void EnablePreviewMode()
    {
        previewMode = true;

        if (EditorApplication.isPlaying)
        {
            scriptRef.Deactivate();

            _myRenderer.material.SetColor(_matColor, scriptRef.flashColor);
            _myRenderer.material.SetFloat(_matAlpha, scriptRef.flashAmount);

            if (scriptRef.useMultipleSprites)
            {
                for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
                {
                    if (scriptRef.myAdditionalPartsList[i].spriteRenderer != null)
                    {
                        scriptRef.myAdditionalPartsList[i].spriteRenderer.material.SetColor(_matColor, scriptRef.myAdditionalPartsList[i].flashColor);
                        scriptRef.myAdditionalPartsList[i].spriteRenderer.material.SetFloat(_matAlpha, scriptRef.myAdditionalPartsList[i].flashAmount);
                    }
                }
            }
        }
        else
        {
            Material temp = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.currentShader));
            temp.SetColor(_matColor, scriptRef.flashColor);
            temp.SetFloat(_matAlpha, scriptRef.flashAmount);
            _myRenderer.sharedMaterial = temp;
            if (scriptRef.useMultipleSprites)
            {
                for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
                {
                    if (scriptRef.myAdditionalPartsList[i].spriteRenderer != null)
                    {
                        Material tempMatForPart = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.myAdditionalPartsList[i].selectedMaterial));
                        tempMatForPart.SetColor(_matColor, scriptRef.myAdditionalPartsList[i].flashColor);
                        tempMatForPart.SetFloat(_matAlpha, scriptRef.myAdditionalPartsList[i].flashAmount);
                        scriptRef.myAdditionalPartsList[i].spriteRenderer.sharedMaterial = tempMatForPart;
                    }
                }
            }
        }
        previewModeCapturedFlashColor = scriptRef.flashColor;
        previewModeCapturedFlashAmount = scriptRef.flashAmount;

        if (scriptRef.useMultipleSprites)
        {
            for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
            {
                scriptRef.myAdditionalPartsList[i].previewModeCapturedFlashColor = scriptRef.myAdditionalPartsList[i].flashColor;
                scriptRef.myAdditionalPartsList[i].previewModeCapturedFlashAmount = scriptRef.myAdditionalPartsList[i].flashAmount;
            }
        }

    }

    private void DisablePreviewMode()
    {
        previewMode = false;

        if (EditorApplication.isPlaying)
        {
            _myRenderer.material.SetFloat(_matAlpha, 0f);
            if (scriptRef.useMultipleSprites)
            {
                for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
                {
                    if (scriptRef.myAdditionalPartsList[i].spriteRenderer != null)
                    {
                        scriptRef.myAdditionalPartsList[i].spriteRenderer.material.SetFloat(_matAlpha, 0f);
                    }
                }
            }
        }
        else
        {
            Material temp = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.currentShader));
            temp.SetFloat(_matAlpha, 0f);
            _myRenderer.sharedMaterial = temp;
            if (scriptRef.useMultipleSprites)
            {
                for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
                {
                    if (scriptRef.myAdditionalPartsList[i].spriteRenderer != null)
                    {
                        Material tempMatForPart = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.myAdditionalPartsList[i].selectedMaterial));
                        tempMatForPart.SetFloat(_matAlpha, 0f);
                        scriptRef.myAdditionalPartsList[i].spriteRenderer.sharedMaterial = tempMatForPart;
                    }
                }
            }
        }
    }


    private void Flash(bool _all = false)
    {
        if (previewMode)
        {
            if (EditorApplication.isPlaying)
            {
                DisablePreviewMode();
                if (_all)
                    scriptRef.FlashAll();
                else
                    scriptRef.Flash();
                return;
            }
            else
            {
                DisablePreviewMode();
                //Debug.Log("You should exit the Preview Mode in order to Flash.");
            }
            //return;
        }
        if (EditorApplication.isPlaying)
        {
            if (_all)
                scriptRef.FlashAll();
            else
                scriptRef.Flash();
            return;
        }
        Material temp = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.currentShader));
        temp.SetColor(_matColor, scriptRef.flashColor);
        temp.SetFloat(_matAlpha, scriptRef.flashAmount);
        _myRenderer.sharedMaterial = temp;
        timeOfDeactivation = EditorApplication.timeSinceStartup + scriptRef.duration;
        flashedAmountCaptured = scriptRef.flashAmount;
        isActivted = true;

        if (_all && scriptRef.useMultipleSprites)
        {
            for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
            {
                if (scriptRef.myAdditionalPartsList[i].spriteRenderer != null)
                {
                    Material tempMatForPart = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.myAdditionalPartsList[i].selectedMaterial));
                    tempMatForPart.SetColor(_matColor, scriptRef.myAdditionalPartsList[i].flashColor);
                    tempMatForPart.SetFloat(_matAlpha, scriptRef.myAdditionalPartsList[i].flashAmount);
                    scriptRef.myAdditionalPartsList[i].spriteRenderer.sharedMaterial = tempMatForPart;
                    scriptRef.myAdditionalPartsList[i].flashedAmountCaptured = scriptRef.myAdditionalPartsList[i].flashAmount;
                    scriptRef.myAdditionalPartsList[i].timeOfDeactivation = EditorApplication.timeSinceStartup + scriptRef.myAdditionalPartsList[i].duration;
                    scriptRef.myAdditionalPartsList[i].isActivted = true;
                }
            }
        }
    }

    private void Deactivate(bool deactivateAllParts = false)
    {
        Material temp = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.currentShader));
        temp.SetFloat(_matAlpha, 0f);
        _myRenderer.sharedMaterial = temp;
        isActivted = false;


        if (deactivateAllParts && scriptRef.useMultipleSprites)
        {
            for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
            {
                if (scriptRef.myAdditionalPartsList[i].spriteRenderer != null)
                {
                    Material tempMatForPart = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.myAdditionalPartsList[i].selectedMaterial));
                    tempMatForPart.SetFloat(_matAlpha, 0f);
                    scriptRef.myAdditionalPartsList[i].spriteRenderer.sharedMaterial = tempMatForPart;
                }
            }
        }
    }



    private void CustomEditorUpdateCallback()
    {
        if (isActivted)
        {
            if (scriptRef.decreaseAmountOverTime)
            {

                Material temp = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.currentShader));
                temp.SetColor(_matColor, scriptRef.flashColor);
                timeUntilDeactivation = timeOfDeactivation - EditorApplication.timeSinceStartup;
                flashedAmountCaptured = scriptRef.flashAmount * (float)(timeUntilDeactivation / scriptRef.duration);
                temp.SetFloat(_matAlpha, flashedAmountCaptured);
                _myRenderer.sharedMaterial = temp;

            }
            if (EditorApplication.timeSinceStartup >= timeOfDeactivation)
                Deactivate();
        }

        if (scriptRef.useMultipleSprites && !EditorApplication.isPlaying)
        {
            for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
            {
                if (scriptRef.myAdditionalPartsList[i].spriteRenderer != null && scriptRef.myAdditionalPartsList[i].isActivted)
                {
                    if (scriptRef.myAdditionalPartsList[i].decreaseAmountOverTime)
                    {
                        Material tempMatForPart = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.myAdditionalPartsList[i].selectedMaterial));
                        tempMatForPart.SetColor(_matColor, scriptRef.myAdditionalPartsList[i].flashColor);
                        scriptRef.myAdditionalPartsList[i].timeUntilDeactivation = scriptRef.myAdditionalPartsList[i].timeOfDeactivation - EditorApplication.timeSinceStartup;
                        scriptRef.myAdditionalPartsList[i].flashedAmountCaptured = scriptRef.myAdditionalPartsList[i].flashAmount * (float)(scriptRef.myAdditionalPartsList[i].timeUntilDeactivation / scriptRef.myAdditionalPartsList[i].duration);
                        tempMatForPart.SetFloat(_matAlpha, scriptRef.myAdditionalPartsList[i].flashedAmountCaptured);
                        scriptRef.myAdditionalPartsList[i].spriteRenderer.sharedMaterial = tempMatForPart;
                    }
                    if (EditorApplication.timeSinceStartup >= scriptRef.myAdditionalPartsList[i].timeOfDeactivation)
                    {
                        scriptRef.myAdditionalPartsList[i].isActivted = false;
                        Material tempMatForPart = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.myAdditionalPartsList[i].selectedMaterial));
                        tempMatForPart.SetFloat(_matAlpha, 0f);
                        scriptRef.myAdditionalPartsList[i].spriteRenderer.sharedMaterial = tempMatForPart;
                    }
                }
            }
        }


        if (previewMode)
        {
            if (previewModeCapturedFlashColor != scriptRef.flashColor || previewModeCapturedFlashAmount != scriptRef.flashAmount)
            {
                if (EditorApplication.isPlaying)
                {
                    _myRenderer.material.SetColor(_matColor, scriptRef.flashColor);
                    _myRenderer.material.SetFloat(_matAlpha, scriptRef.flashAmount);
                }
                else
                {
                    Material temp = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.currentShader));
                    temp.SetColor(_matColor, scriptRef.flashColor);
                    temp.SetFloat(_matAlpha, scriptRef.flashAmount);
                    _myRenderer.sharedMaterial = temp;
                }
                previewModeCapturedFlashColor = scriptRef.flashColor;
                previewModeCapturedFlashAmount = scriptRef.flashAmount;
            }


            if (scriptRef.useMultipleSprites)
            {
                for (int i = 0; i < scriptRef.myAdditionalPartsList.Count; i++)
                {
                    if (scriptRef.myAdditionalPartsList[i].previewModeCapturedFlashColor != scriptRef.myAdditionalPartsList[i].flashColor
                        || scriptRef.myAdditionalPartsList[i].previewModeCapturedFlashAmount != scriptRef.myAdditionalPartsList[i].flashAmount)
                    {
                        if (scriptRef.myAdditionalPartsList[i].spriteRenderer != null)
                        {
                            if (EditorApplication.isPlaying)
                            {
                                scriptRef.myAdditionalPartsList[i].spriteRenderer.material.SetColor(_matColor, scriptRef.myAdditionalPartsList[i].flashColor);
                                scriptRef.myAdditionalPartsList[i].spriteRenderer.material.SetFloat(_matAlpha, scriptRef.myAdditionalPartsList[i].flashAmount);
                            }
                            else
                            {
                                Material tempMatForPart = new Material(Shader.Find("Sprites/Flash Tool/" + scriptRef.myAdditionalPartsList[i].selectedMaterial));
                                tempMatForPart.SetColor(_matColor, scriptRef.myAdditionalPartsList[i].flashColor);
                                tempMatForPart.SetFloat(_matAlpha, scriptRef.myAdditionalPartsList[i].flashAmount);
                                scriptRef.myAdditionalPartsList[i].spriteRenderer.sharedMaterial = tempMatForPart;
                            }
                            scriptRef.myAdditionalPartsList[i].previewModeCapturedFlashAmount = scriptRef.myAdditionalPartsList[i].flashAmount;
                            scriptRef.myAdditionalPartsList[i].previewModeCapturedFlashColor = scriptRef.myAdditionalPartsList[i].flashColor;
                        }
                    }
                }
            }
        }
    }





    #region ReorderableClassList


    public bool isSubEditor;

    private readonly List<SortableListData> listIndex = new List<SortableListData>();
    private readonly Dictionary<string, Editor> editableIndex = new Dictionary<string, Editor>();

    protected bool alwaysDrawInspector = false;
    protected bool isInitialized = false;
    protected bool hasSortableArrays = false;
    protected bool hasEditable = false;

    protected static string GetGrandParentPath(SerializedProperty property)
    {
        string parent = property.propertyPath;
        int firstDot = property.propertyPath.IndexOf('.');
        if (firstDot > 0)
            parent = property.propertyPath.Substring(0, firstDot);
        return parent;
    }

    protected static bool FORCE_INIT = false;
    [DidReloadScripts]
    private static void HandleScriptReload()
    {
        FORCE_INIT = true;
        EditorApplication.delayCall = () => { EditorApplication.delayCall = () => { FORCE_INIT = false; }; };
    }

    private static GUIStyle styleHighlight;

    protected class SortableListData
    {
        public string Parent { get; private set; }
        public Func<int, string> ElementHeaderCallback = null;

        private readonly Dictionary<string, ReorderableList> propIndex = new Dictionary<string, ReorderableList>();
        private readonly Dictionary<string, Action<SerializedProperty, Object[]>> propDropHandlers = new Dictionary<string, Action<SerializedProperty, Object[]>>();

        public SortableListData(string parent)
        {
            Parent = parent;
        }

        public void AddProperty(SerializedProperty property)
        {
            if (GetGrandParentPath(property).Equals(Parent) == false)
                return;

            ReorderableList propList = new ReorderableList(
                                           property.serializedObject, property,
                                           draggable: true, displayHeader: false,
                                           displayAddButton: true, displayRemoveButton: true)
            {
                headerHeight = 5
            };

            propList.drawElementCallback = delegate (Rect rect, int index, bool active, bool focused)
            {
                SerializedProperty targetElement = property.GetArrayElementAtIndex(index);

                bool isExpanded = targetElement.isExpanded;
                rect.height = EditorGUI.GetPropertyHeight(targetElement, GUIContent.none, isExpanded);

                if (targetElement.hasVisibleChildren)
                    rect.xMin += 10;

                GUIContent propHeader = new GUIContent(targetElement.displayName);
                if (ElementHeaderCallback != null)
                    propHeader.text = ElementHeaderCallback(index);

                //EditorGUI.BeginProperty(rect, propHeader, targetElement);



                EditorGUI.PropertyField(rect, targetElement, propHeader, isExpanded);

                //SerializedProperty sprite = targetElement.FindPropertyRelative("sprite");
                //SerializedProperty partName = targetElement.FindPropertyRelative("partName");
                //SerializedProperty flashColor = targetElement.FindPropertyRelative("flashColor");
                //SerializedProperty flashAmount = targetElement.FindPropertyRelative("flashAmount");
                //
                //EditorGUI.PropertyField(rect, sprite, propHeader, isExpanded);
                //rect.y += 20f;
                //EditorGUI.PropertyField(rect, partName, propHeader, isExpanded);
                //rect.y += 20f;
                //EditorGUI.PropertyField(rect, flashColor, propHeader, isExpanded);
                //rect.y += 20f;
                //EditorGUI.PropertyField(rect, flashAmount, propHeader, isExpanded);

                //EditorGUI.EndProperty();

                //List<SFT_Part> decorators = scri




            };

#if UNITY_5_3_OR_NEWER
            propList.elementHeightCallback = index => ElementHeightCallback(property, index);

            propList.drawElementBackgroundCallback = (rect, index, active, focused) =>
            {
                if (styleHighlight == null)
                    styleHighlight = GUI.skin.FindStyle("MeTransitionSelectHead");
                if (focused == false)
                    return;
                rect.height = ElementHeightCallback(property, index);
                GUI.Box(rect, GUIContent.none, styleHighlight);
            };
#endif
            propIndex.Add(property.propertyPath, propList);
            propList.onReorderCallback += ListReorded;
        }

        private float ElementHeightCallback(SerializedProperty property, int index)
        {
            SerializedProperty arrayElement = property.GetArrayElementAtIndex(index);
            float calculatedHeight = EditorGUI.GetPropertyHeight(arrayElement,
                GUIContent.none,
                arrayElement.isExpanded);
            calculatedHeight += 3;
            return calculatedHeight;
        }

        public bool DoLayoutProperty(SerializedProperty property)
        {
            if (propIndex.ContainsKey(property.propertyPath) == false)
                return false;


            var addNewButtonStyle = new GUIStyle(GUI.skin.button);
            addNewButtonStyle.normal.textColor = Color.black;
            addNewButtonStyle.fixedWidth = 80;
            addNewButtonStyle.fixedHeight = 18;

            string headerText = string.Format("{0} [{1}]", property.displayName, property.arraySize);
            EditorGUILayout.PropertyField(property, new GUIContent(headerText), false);

            Rect dropRect = GUILayoutUtility.GetLastRect();

            if (property.isExpanded)
            {
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.192f, 0.772f, 0.956f);
                if (GUILayout.Button("Add New", addNewButtonStyle))
                {
                    property.InsertArrayElementAtIndex(0);
                }
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("Expand All", addNewButtonStyle))
                {
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        property.GetArrayElementAtIndex(i).isExpanded = true;
                    }
                }
                if (GUILayout.Button("Minimize All", addNewButtonStyle))
                {
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        property.GetArrayElementAtIndex(i).isExpanded = false;
                    }
                }

                GUILayout.EndHorizontal();
                propIndex[property.propertyPath].DoLayoutList();
            }

            Event evt = Event.current;
            if (evt == null)
                return true;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (dropRect.Contains(evt.mousePosition) == false)
                    return true;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    Action<SerializedProperty, Object[]> handler = null;
                    if (propDropHandlers.TryGetValue(property.propertyPath, out handler))
                    {
                        if (handler != null)
                            handler(property, DragAndDrop.objectReferences);
                    }
                    else
                    {
                        foreach (Object dragged_object in DragAndDrop.objectReferences)
                        {
                            if (dragged_object.GetType() != property.GetType())
                                continue;

                            int newIndex = property.arraySize;
                            property.arraySize++;

                            SerializedProperty target = property.GetArrayElementAtIndex(newIndex);
                            target.objectReferenceInstanceIDValue = dragged_object.GetInstanceID();
                        }
                    }
                    evt.Use();
                }
            }
            return true;
        }
    }

    public static bool ListReordedbool = false;
    public static void ListReorded(ReorderableList list) { ListReordedbool = true; }

    protected struct ContextMenuData
    {
        public string menuItem;
        public MethodInfo function;
        public MethodInfo validate;

        public ContextMenuData(string item)
        {
            menuItem = item;
            function = null;
            validate = null;
        }
    }

    protected Dictionary<string, ContextMenuData> contextData = new Dictionary<string, ContextMenuData>();

    ~SpriteFlashToolEditor()
    {
        listIndex.Clear();
        editableIndex.Clear();
        isInitialized = false;
    }

    #region Initialization

    protected virtual void InitInspector(bool force)
    {
        if (force)
            isInitialized = false;
        InitInspector();
    }

    protected virtual void InitInspector()
    {
        if (isInitialized && FORCE_INIT == false)
            return;
        FindTargetProperties();
        //FindContextMenu();
    }

    protected void FindTargetProperties()
    {
        listIndex.Clear();
        editableIndex.Clear();

        SerializedProperty iterProp = serializedObject.GetIterator();
        if (iterProp.NextVisible(true))
        {
            do
            {
                if (iterProp.isArray && iterProp.propertyType != SerializedPropertyType.String)
                {
                    bool canTurnToList = iterProp.HasAttribute<Reorderable>();
                    if (canTurnToList)
                    {
                        hasSortableArrays = true;
                        CreateListData(serializedObject.FindProperty(iterProp.propertyPath));
                    }
                }
                if (iterProp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Type propType = iterProp.GetTypeReflection();
                    if (propType == null)
                        continue;
                }
            } while (iterProp.NextVisible(true));
        }
        isInitialized = true;
        if (hasSortableArrays == false)
        {
            listIndex.Clear();
        }
    }

    private IEnumerable<MethodInfo> GetAllMethods(Type t)
    {
        if (t == null)
            return Enumerable.Empty<MethodInfo>();
        var binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        return t.GetMethods(binding).Concat(GetAllMethods(t.BaseType));
    }

    //private void FindContextMenu()
    //{
    //    contextData.Clear();
    //    Type targetType = target.GetType();
    //    Type contextMenuType = typeof(ContextMenu);
    //    MethodInfo[] methods = GetAllMethods(targetType).ToArray();
    //    for (int index = 0; index < methods.GetLength(0); ++index)
    //    {
    //        MethodInfo methodInfo = methods[index];
    //        foreach (ContextMenu contextMenu in methodInfo.GetCustomAttributes(contextMenuType, false))
    //        {
    //            if (contextData.ContainsKey(contextMenu.menuItem))
    //            {
    //                var data = contextData[contextMenu.menuItem];
    //                if (contextMenu.validate)
    //                    data.validate = methodInfo;
    //                else
    //                    data.function = methodInfo;
    //                contextData[data.menuItem] = data;
    //            }
    //            else
    //            {
    //                var data = new ContextMenuData(contextMenu.menuItem);
    //                if (contextMenu.validate)
    //                    data.validate = methodInfo;
    //                else
    //                    data.function = methodInfo;
    //                contextData.Add(data.menuItem, data);
    //            }
    //        }
    //    }
    //}

    private void CreateListData(SerializedProperty property)
    {
        string parent = GetGrandParentPath(property);

        SortableListData data = listIndex.Find(listData => listData.Parent.Equals(parent));
        if (data == null)
        {
            data = new SortableListData(parent);
            listIndex.Add(data);
        }

        data.AddProperty(property);
        object[] attr = property.GetAttributes<Reorderable>();
        if (attr != null && attr.Length == 1)
        {
            Reorderable arrayAttr = (Reorderable)attr[0];
            if (arrayAttr != null)
            {
                HandleReorderableOptions(arrayAttr, property, data);
            }
        }
    }

    private void HandleReorderableOptions(Reorderable arrayAttr, SerializedProperty property, SortableListData data)
    {
        if (string.IsNullOrEmpty(arrayAttr.elementHeader) == false)
            data.ElementHeaderCallback = i => string.Format("{0} {1}", arrayAttr.elementHeader, (arrayAttr.headerZeroIndex ? i : i + 1));
    }

    #endregion

    protected bool InspectorGUIStart(bool force = false)
    {

        if (hasSortableArrays && listIndex.Count == 0)
            InitInspector();
        if (hasEditable && editableIndex.Count == 0)
            InitInspector();

        bool cannotDrawOrderable = (hasSortableArrays == false || listIndex.Count == 0);
        bool cannotDrawEditable = (hasEditable == false || editableIndex.Count == 0);
        if (cannotDrawOrderable && cannotDrawEditable && force == false)
        {
            if (isSubEditor)
                DrawPropertiesExcluding(serializedObject, "m_Script");
            else
                base.OnInspectorGUI();

            return false;
        }

        serializedObject.Update();
        return true;
    }

    protected virtual void DrawInspector()
    {
        DrawPropertiesAll();
    }



    protected enum IterControl
    {
        Draw,
        Continue,
        Break
    }

    protected void IterateDrawProperty(SerializedProperty property, Func<IterControl> filter = null)
    {
        if (property.NextVisible(true))
        {
            int depth = property.Copy().depth;
            do
            {
                if (property.depth != depth)
                    break;
                if (isSubEditor && property.name.Equals("m_Script"))
                    continue;

                if (filter != null)
                {
                    var filterResult = filter();
                    if (filterResult == IterControl.Break)
                        break;
                    if (filterResult == IterControl.Continue)
                        continue;
                }

                DrawPropertySortableArray(property);
            } while (property.NextVisible(false));
        }
    }

    protected void DrawPropertySortableArray(SerializedProperty property)
    {
        SortableListData listData = null;
        if (listIndex.Count > 0)
            listData = listIndex.Find(data => property.propertyPath.StartsWith(data.Parent));

        if (listData != null)
        {
            if (listData.DoLayoutProperty(property) == false)
            {
                EditorGUILayout.PropertyField(property, false);

                if (property.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    SerializedProperty targetProp = serializedObject.FindProperty(property.propertyPath);
                    IterateDrawProperty(targetProp);
                    EditorGUI.indentLevel--;
                }
            }
        }
        else
        {
            SerializedProperty targetProp = serializedObject.FindProperty(property.propertyPath);

            bool isStartProp = targetProp.propertyPath.StartsWith("m_");
            using (new EditorGUI.DisabledScope(isStartProp))
            {
                EditorGUILayout.PropertyField(targetProp, targetProp.isExpanded);
            }
        }
    }

    public void DrawPropertiesAll()
    {
        SerializedProperty iterProp = serializedObject.GetIterator();
        IterateDrawProperty(iterProp);
    }

    #endregion










}
