#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Crosstales.TrueRandom.EditorUtil;

namespace Crosstales.TrueRandom.EditorExtension
{
   /// <summary>Custom editor for the 'script'-class.</summary>
   [InitializeOnLoad]
   [CustomEditor(typeof(TRManager))]
   public class scriptEditor : Editor
   {
      #region Variables

      private bool showIntegerGenerator;
      private bool showIntResults;
      private int intNumber = 1;
      private int intMin = 1;
      private int intMax = 10;

      private bool showFloatGenerator;
      private bool showFloatResults;
      private int floatNumber = 1;
      private float floatMin;
      private float floatMax = 1;

      private bool showSequenceGenerator;
      private bool showSeqResults;
      private int seqtMin = 1;
      private int seqMax = 6;
      private int seqNumber;

      private bool showStringGenerator;
      private bool showStringResults;
      private int stringNumber = 1;
      private int stringLength = 6;
      private bool digits = true;
      private bool upper = true;
      private bool lower = true;
      private bool unique;

      private bool showVector2Generator;
      private bool showVector2Results;
      private int vector2Number = 1;
      private Vector2 vector2Min = Vector2.zero;
      private Vector2 vector2Max = Vector2.one;

      private bool showVector3Generator;
      private bool showVector3Results;
      private int vector3Number = 1;
      private Vector3 vector3Min = Vector3.zero;
      private Vector3 vector3Max = Vector3.one;

      private bool showVector4Generator;
      private bool showVector4Results;
      private int vector4Number = 1;
      private Vector4 vector4Min = Vector4.zero;
      private Vector4 vector4Max = Vector4.one;

      private TRManager script;

      #endregion


      #region Static constructor

      static scriptEditor()
      {
         EditorApplication.hierarchyWindowItemOnGUI += hierarchyItemCB;
      }

      #endregion


      #region Editor methods

      public void OnEnable()
      {
         script = (TRManager)target;

         EditorApplication.update += onUpdate;
         script.OnQuotaUpdate += onUpdateQuota;

         //onUpdate();
      }

      public void OnDisable()
      {
         EditorApplication.update -= onUpdate;
         script.OnQuotaUpdate -= onUpdateQuota;
      }

      public override void OnInspectorGUI()
      {
         EditorHelper.BannerOC();

         DrawDefaultInspector();

         EditorHelper.SeparatorUI();

         if (Util.Config.SHOW_QUOTA && !script.PRNG)
         {
            GUILayout.Label("Quota", EditorStyles.boldLabel);
            GUILayout.Label(script.CurrentQuota.ToString());

            EditorHelper.SeparatorUI();
         }

         if (script.isActiveAndEnabled)
         {
            GUILayout.Label("Test-Drive", EditorStyles.boldLabel);

            if (Util.Helper.isEditorMode)
            {
               showIntegerGenerator = EditorGUILayout.Foldout(showIntegerGenerator, "Generate random integers");

               if (showIntegerGenerator)
               {
                  EditorGUI.indentLevel++;

                  intMin = EditorGUILayout.IntField(new GUIContent("Min", "Smallest possible number (range: -1'000'000'000 - 1'000'000'000)"), intMin);
                  intMax = EditorGUILayout.IntField(new GUIContent("Max", "Biggest possible number (range: -1'000'000'000 - 1'000'000'000)"), intMax);
                  intNumber = EditorGUILayout.IntField(new GUIContent("Number", "How many numbers you want to generate (range: 1 - 10'000)"), intNumber);

                  GUI.enabled = !script.isGenerating;
                  if (GUILayout.Button(new GUIContent(script.isGenerating ? " Generating... Please wait." : " Generate", EditorHelper.Icon_Generate, "Generate random integers.")))
                  {
                     script.GenerateInteger(intMin, intMax, Mathf.Abs(intNumber));
                  }
#if !UNITY_WSA && !UNITY_WEBGL && !UNITY_XBOXONE
                  GUI.enabled = script.CurrentIntegers.Count > 0;
                  if (GUILayout.Button(new GUIContent(" Save TXT", EditorHelper.Icon_Save, "Save random integers as text-file.")))
                  {
                     string path = EditorUtility.SaveFilePanel("Save random integers as text-file", string.Empty, "Integers.txt", "txt");
                     if (!string.IsNullOrEmpty(path))
                     {
                        Util.Helper.SaveAsText(path, script.CurrentIntegers);
                     }
                  }
#endif
                  GUI.enabled = true;

                  EditorGUI.indentLevel++;

                  showIntResults = EditorGUILayout.Foldout(showIntResults, $"Results ({script.CurrentIntegers.Count})");
                  if (showIntResults)
                  {
                     EditorGUI.indentLevel++;

                     foreach (int res in script.CurrentIntegers)
                     {
                        EditorGUILayout.SelectableLabel(res.ToString(), GUILayout.Height(16), GUILayout.ExpandHeight(false));
                     }

                     EditorGUI.indentLevel--;
                  }

                  EditorGUI.indentLevel -= 2;
               }

               EditorHelper.SeparatorUI();

               showFloatGenerator = EditorGUILayout.Foldout(showFloatGenerator, "Generate random floats");
               if (showFloatGenerator)
               {
                  EditorGUI.indentLevel++;
                  floatMin = EditorGUILayout.FloatField(new GUIContent("Min", "Smallest possible number (range: -1'000'000'000 - 1'000'000'000)"), floatMin);
                  floatMax = EditorGUILayout.FloatField(new GUIContent("Max", "Biggest possible number (range: -1'000'000'000 - 1'000'000'000)"), floatMax);
                  floatNumber = EditorGUILayout.IntField(new GUIContent("Number", "How many numbers you want to generate (range: 1 - 10'000)"), floatNumber);

                  GUI.enabled = !script.isGenerating;
                  if (GUILayout.Button(new GUIContent(script.isGenerating ? " Generating... Please wait." : " Generate", EditorHelper.Icon_Generate, "Generate random floats.")))
                  {
                     script.GenerateFloat(floatMin, floatMax, Mathf.Abs(floatNumber));
                  }
#if !UNITY_WSA && !UNITY_WEBGL && !UNITY_XBOXONE
                  GUI.enabled = script.CurrentFloats.Count > 0;
                  if (GUILayout.Button(new GUIContent(" Save TXT", EditorHelper.Icon_Save, "Save random floats as text-file.")))
                  {
                     string path = EditorUtility.SaveFilePanel("Save random floats as text-file", string.Empty, "Floats.txt", "txt");
                     if (!string.IsNullOrEmpty(path))
                     {
                        Util.Helper.SaveAsText(path, script.CurrentFloats);
                     }
                  }
#endif
                  EditorGUI.indentLevel++;

                  showFloatResults = EditorGUILayout.Foldout(showFloatResults, $"Results ({script.CurrentFloats.Count})");
                  if (showFloatResults)
                  {
                     EditorGUI.indentLevel++;

                     foreach (float res in script.CurrentFloats)
                     {
                        EditorGUILayout.SelectableLabel(res.ToString(), GUILayout.Height(16), GUILayout.ExpandHeight(false));
                     }

                     EditorGUI.indentLevel--;
                  }

                  EditorGUI.indentLevel -= 2;
               }

               EditorHelper.SeparatorUI();

               showSequenceGenerator = EditorGUILayout.Foldout(showSequenceGenerator, "Generate random sequence");
               if (showSequenceGenerator)
               {
                  EditorGUI.indentLevel++;
                  seqtMin = EditorGUILayout.IntField(new GUIContent("Min", "Start of the interval (range: -1'000'000'000 - 1'000'000'000)"), seqtMin);
                  seqMax = EditorGUILayout.IntField(new GUIContent("Max", "End of the interval (range: -1'000'000'000 - 1'000'000'000)"), seqMax);
                  seqNumber = EditorGUILayout.IntField(new GUIContent("Number (0 = all)", "How many numbers you have in the result (max range: max - min, optional)"), seqNumber);

                  GUI.enabled = !script.isGenerating;
                  if (GUILayout.Button(new GUIContent(script.isGenerating ? " Generating... Please wait." : " Generate", EditorHelper.Icon_Generate, "Generate random sequence.")))
                  {
                     script.GenerateSequence(seqtMin, seqMax, seqNumber);
                  }
#if !UNITY_WSA && !UNITY_WEBGL && !UNITY_XBOXONE
                  GUI.enabled = script.CurrentSequence.Count > 0;
                  if (GUILayout.Button(new GUIContent(" Save TXT", EditorHelper.Icon_Save, "Save random sequence as text-file.")))
                  {
                     string path = EditorUtility.SaveFilePanel("Save random sequence as text-file", string.Empty, "Sequence.txt", "txt");
                     if (!string.IsNullOrEmpty(path))
                     {
                        Util.Helper.SaveAsText(path, script.CurrentSequence);
                     }
                  }
#endif
                  EditorGUI.indentLevel++;

                  showSeqResults = EditorGUILayout.Foldout(showSeqResults, "Results (" + script.CurrentSequence.Count + ")");
                  if (showSeqResults)
                  {
                     EditorGUI.indentLevel++;

                     foreach (int res in script.CurrentSequence)
                     {
                        EditorGUILayout.SelectableLabel(res.ToString(), GUILayout.Height(16), GUILayout.ExpandHeight(false));
                     }

                     EditorGUI.indentLevel--;
                  }

                  EditorGUI.indentLevel -= 2;
               }

               EditorHelper.SeparatorUI();

               showStringGenerator = EditorGUILayout.Foldout(showStringGenerator, "Generate random strings");
               if (showStringGenerator)
               {
                  EditorGUI.indentLevel++;
                  stringLength = EditorGUILayout.IntField(new GUIContent("Length", "How long the strings should be (range: 1 - 20)"), stringLength);
                  stringNumber = EditorGUILayout.IntField(new GUIContent("Number", "How many strings you want to generate (range: 1 - 10'000)"), stringNumber);
                  digits = EditorGUILayout.Toggle(new GUIContent("Digits", "Allow digits (0-9)"), digits);
                  upper = EditorGUILayout.Toggle(new GUIContent("Uppercase letters", "Allow uppercase (A-Z) letters"), upper);
                  lower = EditorGUILayout.Toggle(new GUIContent("Lowercase letters", "Allow lowercase (a-z) letters"), lower);
                  unique = EditorGUILayout.Toggle(new GUIContent("Unique", "String shoud be unique in the result"), unique);

                  GUI.enabled = !script.isGenerating;
                  if (GUILayout.Button(new GUIContent(script.isGenerating ? " Generating... Please wait." : " Generate", EditorHelper.Icon_Generate, "Generate random strings.")))
                  {
                     script.GenerateString(stringLength, stringNumber, digits, upper, lower, unique);
                  }
#if !UNITY_WSA && !UNITY_WEBGL && !UNITY_XBOXONE
                  GUI.enabled = script.CurrentStrings.Count > 0;
                  if (GUILayout.Button(new GUIContent(" Save TXT", EditorHelper.Icon_Save, "Save random strings as text-file.")))
                  {
                     string path = EditorUtility.SaveFilePanel("Save random strings as text-file", string.Empty, "Strings.txt", "txt");
                     if (!string.IsNullOrEmpty(path))
                     {
                        Util.Helper.SaveAsText(path, script.CurrentStrings);
                     }
                  }
#endif
                  EditorGUI.indentLevel++;

                  showStringResults = EditorGUILayout.Foldout(showStringResults, "Results (" + script.CurrentStrings.Count + ")");
                  if (showStringResults)
                  {
                     EditorGUI.indentLevel++;

                     foreach (string res in script.CurrentStrings)
                     {
                        EditorGUILayout.SelectableLabel(res, GUILayout.Height(16), GUILayout.ExpandHeight(false));
                     }

                     EditorGUI.indentLevel--;
                  }

                  EditorGUI.indentLevel -= 2;
               }

               EditorHelper.SeparatorUI();

               showVector2Generator = EditorGUILayout.Foldout(showVector2Generator, "Generate random Vector2");
               if (showVector2Generator)
               {
                  EditorGUI.indentLevel++;
                  vector2Min = EditorGUILayout.Vector2Field(new GUIContent("Min", "Smallest possible Vector2 (range: -1'000'000'000 - 1'000'000'000)"), vector2Min);
                  vector2Max = EditorGUILayout.Vector2Field(new GUIContent("Max", "Biggest possible Vector2 (range: -1'000'000'000 - 1'000'000'000)"), vector2Max);
                  vector2Number = EditorGUILayout.IntField(new GUIContent("Number", "How many Vector2 you want to generate (range: 1 - 10'000)"), vector2Number);

                  GUI.enabled = !script.isGenerating;
                  if (GUILayout.Button(new GUIContent(script.isGenerating ? " Generating... Please wait." : " Generate", EditorHelper.Icon_Generate, "Generate random Vector2.")))
                  {
                     script.GenerateVector2(vector2Min, vector2Max, Mathf.Abs(vector2Number));
                  }
#if !UNITY_WSA && !UNITY_WEBGL && !UNITY_XBOXONE
                  GUI.enabled = script.CurrentVector2.Count > 0;
                  if (GUILayout.Button(new GUIContent(" Save TXT", EditorHelper.Icon_Save, "Save random Vector2 as text-file.")))
                  {
                     string path = EditorUtility.SaveFilePanel("Save random Vector2 as text-file", string.Empty, "Vector2.txt", "txt");
                     if (!string.IsNullOrEmpty(path))
                     {
                        Util.Helper.SaveAsText(path, script.CurrentVector2);
                     }
                  }
#endif
                  EditorGUI.indentLevel++;

                  showVector2Results = EditorGUILayout.Foldout(showVector2Results, "Results (" + script.CurrentVector2.Count + ")");
                  if (showVector2Results)
                  {
                     EditorGUI.indentLevel++;

                     foreach (Vector2 res in script.CurrentVector2)
                     {
                        EditorGUILayout.SelectableLabel(res.x + ", " + res.y, GUILayout.Height(16), GUILayout.ExpandHeight(false));
                     }

                     EditorGUI.indentLevel--;
                  }

                  EditorGUI.indentLevel -= 2;
               }

               EditorHelper.SeparatorUI();

               showVector3Generator = EditorGUILayout.Foldout(showVector3Generator, "Generate random Vector3");
               if (showVector3Generator)
               {
                  EditorGUI.indentLevel++;
                  vector3Min = EditorGUILayout.Vector3Field(new GUIContent("Min", "Smallest possible Vector3 (range: -1'000'000'000 - 1'000'000'000)"), vector3Min);
                  vector3Max = EditorGUILayout.Vector3Field(new GUIContent("Max", "Biggest possible Vector3 (range: -1'000'000'000 - 1'000'000'000)"), vector3Max);
                  vector3Number = EditorGUILayout.IntField(new GUIContent("Number", "How many Vector3 you want to generate (range: 1 - 10'000)"), vector3Number);

                  GUI.enabled = !script.isGenerating;
                  if (GUILayout.Button(new GUIContent(script.isGenerating ? " Generating... Please wait." : " Generate", EditorHelper.Icon_Generate, "Generate random Vector3.")))
                  {
                     script.GenerateVector3(vector3Min, vector3Max, Mathf.Abs(vector3Number));
                  }
#if !UNITY_WSA && !UNITY_WEBGL && !UNITY_XBOXONE
                  GUI.enabled = script.CurrentVector3.Count > 0;
                  if (GUILayout.Button(new GUIContent(" Save TXT", EditorHelper.Icon_Save, "Save random Vector3 as text-file.")))
                  {
                     string path = EditorUtility.SaveFilePanel("Save random Vector3 as text-file", string.Empty, "Vector3.txt", "txt");
                     if (!string.IsNullOrEmpty(path))
                     {
                        Util.Helper.SaveAsText(path, script.CurrentVector3);
                     }
                  }
#endif
                  EditorGUI.indentLevel++;

                  showVector3Results = EditorGUILayout.Foldout(showVector3Results, "Results (" + script.CurrentVector3.Count + ")");
                  if (showVector3Results)
                  {
                     EditorGUI.indentLevel++;

                     foreach (Vector3 res in script.CurrentVector3)
                     {
                        EditorGUILayout.SelectableLabel(res.x + ", " + res.y + ", " + res.z, GUILayout.Height(16), GUILayout.ExpandHeight(false));
                     }

                     EditorGUI.indentLevel--;
                  }

                  EditorGUI.indentLevel -= 2;
               }


               EditorHelper.SeparatorUI();

               showVector4Generator = EditorGUILayout.Foldout(showVector4Generator, "Generate random Vector4");
               if (showVector4Generator)
               {
                  EditorGUI.indentLevel++;
                  vector4Min = EditorGUILayout.Vector4Field("Min", vector4Min);
                  vector4Max = EditorGUILayout.Vector4Field("Max", vector4Max);
                  vector4Number = EditorGUILayout.IntField(new GUIContent("Number", "How many Vector4 you want to generate (range: 1 - 10'000)"), vector4Number);

                  GUI.enabled = !script.isGenerating;
                  if (GUILayout.Button(new GUIContent(script.isGenerating ? " Generating... Please wait." : " Generate", EditorHelper.Icon_Generate, "Generate random Vector4.")))
                  {
                     script.GenerateVector4(vector4Min, vector4Max, Mathf.Abs(vector4Number));
                  }
#if !UNITY_WSA && !UNITY_WEBGL && !UNITY_XBOXONE
                  GUI.enabled = script.CurrentVector4.Count > 0;
                  if (GUILayout.Button(new GUIContent(" Save TXT", EditorHelper.Icon_Save, "Save random Vector4 as text-file.")))
                  {
                     string path = EditorUtility.SaveFilePanel("Save random Vector4 as text-file", string.Empty, "Vector4.txt", "txt");
                     if (!string.IsNullOrEmpty(path))
                     {
                        Util.Helper.SaveAsText(path, script.CurrentVector4);
                     }
                  }
#endif
                  EditorGUI.indentLevel++;

                  showVector4Results = EditorGUILayout.Foldout(showVector4Results, "Results (" + script.CurrentVector4.Count + ")");
                  if (showVector4Results)
                  {
                     EditorGUI.indentLevel++;

                     foreach (Vector4 res in script.CurrentVector4)
                     {
                        EditorGUILayout.SelectableLabel(res.x + ", " + res.y + ", " + res.z + ", " + res.w, GUILayout.Height(16), GUILayout.ExpandHeight(false));
                     }

                     EditorGUI.indentLevel--;
                  }

                  EditorGUI.indentLevel -= 2;
               }
            }
            else
            {
               EditorGUILayout.HelpBox("Disabled in Play-mode!", MessageType.Info);
            }
         }
         else
         {
            EditorGUILayout.HelpBox("Script is disabled!", MessageType.Info);
         }
      }

      #endregion


      #region Private methods

      private void onUpdate()
      {
         Repaint();
      }

      private void onUpdateQuota(int e)
      {
         //Debug.Log("Quota: " + e, this);
         Repaint();
      }

      private static void hierarchyItemCB(int instanceID, Rect selectionRect)
      {
         if (EditorConfig.HIERARCHY_ICON)
         {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (go != null && go.GetComponent<TRManager>())
            {
               Rect r = new Rect(selectionRect);
               r.x = r.width - 4;

               GUI.Label(r, EditorHelper.Logo_Asset_Small);
            }
         }
      }

      #endregion
   }
}
#endif
// © 2016-2021 crosstales LLC (https://www.crosstales.com)