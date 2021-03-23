using UnityEngine;

namespace Crosstales.TrueRandom.Util
{
   /// <summary>Various helper functions.</summary>
   public abstract class Helper : Common.Util.BaseHelper
   {
      #region Static properties

      /// <summary>Checks if the current platform is supported.</summary>
      /// <returns>True if the current platform is supported.</returns>
      public static bool isSupportedPlatform => true;

      #endregion


      #region Static methods
#if !UNITY_WSA && !UNITY_WEBGL && !UNITY_XBOXONE
      /// <summary>Save generated results as text-file.</summary>
      /// <param name="filePath">Path for the file</param>
      /// <param name="results">Results to save</param>
      public static void SaveAsText<T>(string filePath, System.Collections.Generic.List<T> results)
      {
         saveAsText(filePath, results.CTDump(), results.Count);
      }

      /// <summary>Save generated Vector2 as text-file.</summary>
      /// <param name="filePath">Path for the file</param>
      /// <param name="results">Results to save</param>
      public static void SaveAsText(string filePath, System.Collections.Generic.List<Vector2> results)
      {
         saveAsText(filePath, results.CTDump(), results.Count);
      }

      /// <summary>Save generated Vector3 as text-file.</summary>
      /// <param name="filePath">Path for the file</param>
      /// <param name="results">Results to save</param>
      public static void SaveAsText(string filePath, System.Collections.Generic.List<Vector3> results)
      {
         saveAsText(filePath, results.CTDump(), results.Count);
      }

      /// <summary>Save generated Vector4 as text-file.</summary>
      /// <param name="filePath">Path for the file</param>
      /// <param name="results">Results to save</param>
      public static void SaveAsText(string filePath, System.Collections.Generic.List<Vector4> results)
      {
         saveAsText(filePath, results.CTDump(), results.Count);
      }

      private static void saveAsText(string path, string content, int number)
      {
         try
         {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
               file.WriteLine("# " + Util.Constants.ASSET_NAME + " " + Util.Constants.ASSET_VERSION);
               file.WriteLine("# © 2016-2021 by " + Util.Constants.ASSET_AUTHOR + " (" + Util.Constants.ASSET_AUTHOR_URL + ")");
               file.WriteLine("#");
               file.WriteLine("# Number of generated of random results: " + number);
               file.WriteLine("# Created: " + System.DateTime.Now.ToString("dd.MM.yyyy"));
               file.WriteLine(content);
            }
            //System.IO.File.WriteAllText(path, content);
         }
         catch (System.Exception ex)
         {
            Debug.LogError("Could not save file: " + path + System.Environment.NewLine + ex);
         }
      }
#endif
      #endregion
   }
}
// © 2016-2021 crosstales LLC (https://www.crosstales.com)