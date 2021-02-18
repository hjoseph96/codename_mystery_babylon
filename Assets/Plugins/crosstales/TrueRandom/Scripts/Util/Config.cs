namespace Crosstales.TrueRandom.Util
{
   /// <summary>Configuration for the asset.</summary>
   public static class Config
   {
      #region Variables

      /// <summary>Enable or disable debug logging for the asset.</summary>
      public static bool DEBUG = Constants.DEFAULT_DEBUG || Constants.DEV_DEBUG;

      /// <summary>Shows the quota inside the editor components.</summary>
      public static bool SHOW_QUOTA = Constants.DEFAULT_SHOW_QUOTA;

      /// <summary>Is the configuration loaded?</summary>
      public static bool isLoaded;

      #endregion

#if UNITY_EDITOR

      #region Public static methods

      /// <summary>Resets all changeable variables to their default value.</summary>
      public static void Reset()
      {
         if (!Constants.DEV_DEBUG)
            DEBUG = Constants.DEFAULT_DEBUG;

         SHOW_QUOTA = Constants.DEFAULT_SHOW_QUOTA;
      }

      /// <summary>Loads the all changeable variables.</summary>
      public static void Load()
      {
         if (!Constants.DEV_DEBUG)
         {
            if (Common.Util.CTPlayerPrefs.HasKey(Constants.KEY_DEBUG))
               DEBUG = Common.Util.CTPlayerPrefs.GetBool(Constants.KEY_DEBUG);
         }
         else
         {
            DEBUG = Constants.DEV_DEBUG;
         }

         if (Common.Util.CTPlayerPrefs.HasKey(Constants.KEY_SHOW_QUOTA))
            SHOW_QUOTA = Common.Util.CTPlayerPrefs.GetBool(Constants.KEY_SHOW_QUOTA);

         isLoaded = true;
      }

      /// <summary>Saves the all changeable variables.</summary>
      public static void Save()
      {
         if (!Constants.DEV_DEBUG)
            Common.Util.CTPlayerPrefs.SetBool(Constants.KEY_DEBUG, DEBUG);

         Common.Util.CTPlayerPrefs.SetBool(Constants.KEY_SHOW_QUOTA, SHOW_QUOTA);

         Common.Util.CTPlayerPrefs.Save();
      }

      #endregion

#endif
   }
}
// © 2017-2021 crosstales LLC (https://www.crosstales.com)