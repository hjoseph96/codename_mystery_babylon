using System.Linq;
using UnityEngine;

namespace Crosstales.Common.Util
{
   /// <summary>Allows any Unity gameobject to survive a scene switch. This is especially useful to keep the music playing while loading a new scene.</summary>
   [DisallowMultipleComponent]
   public class SurviveSceneSwitch : Singleton<SurviveSceneSwitch>
   {
      #region Variables

      ///<summary>Objects which have to survive a scene switch.</summary>
      [Tooltip("Objects which have to survive a scene switch.")] public GameObject[] Survivors; //any object, like a RadioPlayer

      private const float ensureParentTime = 1.5f;
      private float ensureParentTimer;

      private Transform tf;

      #endregion


      #region MonoBehaviour methods

      private void Start()
      {
         ensureParentTimer = ensureParentTime;
         tf = transform;
      }

      private void Update()
      {
         ensureParentTimer += Time.deltaTime;

         if (Survivors != null && ensureParentTimer > ensureParentTime)
         {
            ensureParentTimer = 0f;

            foreach (GameObject _go in Survivors.Where(_go => _go != null))
            {
               _go.transform.SetParent(tf);
            }
         }
      }

      #endregion
   }
}
// © 2016-2021 crosstales LLC (https://www.crosstales.com)