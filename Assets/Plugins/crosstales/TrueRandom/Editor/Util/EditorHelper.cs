#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Crosstales.TrueRandom.EditorUtil
{
   /// <summary>Editor helper class.</summary>
   public abstract class EditorHelper : Common.EditorUtil.BaseEditorHelper
   {
      #region Static variables

      /// <summary>Start index inside the "GameObject"-menu.</summary>
      public const int GO_ID = 38;

      /// <summary>Start index inside the "Tools"-menu.</summary>
      public const int MENU_ID = 12018; // 1, T = 20, R = 18

      private static Texture2D logo_asset;
      private static Texture2D logo_asset_small;

      private static Texture2D icon_generate;

      #endregion


      #region Static properties

      public static Texture2D Logo_Asset => loadImage(ref logo_asset, "logo_asset_pro.png");

      public static Texture2D Logo_Asset_Small => loadImage(ref logo_asset_small, "logo_asset_small_pro.png");

      public static Texture2D Icon_Generate => loadImage(ref icon_generate, "icon_generate.png");

      #endregion


      #region Static methods

      /// <summary>Shows a "True Random unavailable"-UI.</summary>
      public static void TRUnavailable()
      {
         EditorGUILayout.HelpBox("True Random not available!", MessageType.Warning);

         EditorGUILayout.HelpBox("Did you add the '" + Util.Constants.TRUERANDOM_SCENE_OBJECT_NAME + "'-prefab to the scene?", MessageType.Info);

         GUILayout.Space(8);

         if (GUILayout.Button(new GUIContent(" Add " + Util.Constants.TRUERANDOM_SCENE_OBJECT_NAME, Icon_Plus, "Add the '" + Util.Constants.TRUERANDOM_SCENE_OBJECT_NAME + "'-prefab to the current scene.")))
            InstantiatePrefab(Util.Constants.TRUERANDOM_SCENE_OBJECT_NAME);
      }

      /// <summary>Instantiates a prefab.</summary>
      /// <param name="prefabName">Name of the prefab.</param>
      public static void InstantiatePrefab(string prefabName)
      {
         PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath("Assets" + EditorConfig.PREFAB_PATH + prefabName + ".prefab", typeof(GameObject)));
         UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
      }

      /// <summary>Checks if the 'TrueRandom'-prefab is in the scene.</summary>
      /// <returns>True if the 'TrueRandom'-prefab is in the scene.</returns>
      public static bool isTrueRandomInScene => GameObject.FindObjectOfType(typeof(TRManager)) != null; //GameObject.Find(Util.Constants.TRUERANDOM_SCENE_OBJECT_NAME) != null;


      /// <summary>Shows a banner for "Online Check".</summary>
      public static void BannerOC()
      {
#if !CT_OC
         if (Util.Constants.SHOW_OC_BANNER)
         {
            GUILayout.BeginHorizontal();
            {
               EditorGUILayout.HelpBox("'Online Check' is not installed!" + System.Environment.NewLine + "For reliable Internet availability tests, please install or get it from the Unity AssetStore.", MessageType.Info);

               GUILayout.BeginVertical(GUILayout.Width(32));
               {
                  GUILayout.Space(4);

                  if (GUILayout.Button(new GUIContent(string.Empty, EditorHelper.Logo_Asset_OC, "Visit Online Check in the Unity AssetStore")))
                     OpenURL(Util.Constants.ASSET_OC);
               }
               GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
         }
#endif
      }

      /// <summary>Loads an image as Texture2D from 'Editor Default Resources'.</summary>
      /// <param name="logo">Logo to load.</param>
      /// <param name="fileName">Name of the image.</param>
      /// <returns>Image as Texture2D from 'Editor Default Resources'.</returns>
      private static Texture2D loadImage(ref Texture2D logo, string fileName)
      {
         if (logo == null)
         {
#if CT_DEVELOP
            logo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets" + EditorConfig.ASSET_PATH + "Icons/" + fileName, typeof(Texture2D));
#else
            logo = (Texture2D)EditorGUIUtility.Load("crosstales/TrueRandom/" + fileName);
#endif
            if (logo == null)
               Debug.LogWarning("Image not found: " + fileName);
         }

         return logo;
      }

      #endregion
   }
}
#endif
// © 2016-2021 crosstales LLC (https://www.crosstales.com)