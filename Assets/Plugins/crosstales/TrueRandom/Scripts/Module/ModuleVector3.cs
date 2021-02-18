using UnityEngine;

namespace Crosstales.TrueRandom.Module
{
   /// <summary>
   /// This generator will generate true random Vector3 in configurable intervals.
   /// </summary>
   public abstract class ModuleVector3 : BaseModule
   {
      #region Variables

      private static System.Collections.Generic.List<Vector3> result = new System.Collections.Generic.List<Vector3>();

      private static bool isRunning;

      #endregion


      #region Events

      /// <summary>Event to get a message when generating Vector3 has started.</summary>
      public static event GenerateVector3Start OnGenerateStart;

      /// <summary>Event to get a message with the generated Vector3 when finished.</summary>
      public static event GenerateVector3Finished OnGenerateFinished;

      #endregion


      #region Static properties

      /// <summary>Returns the list of Vector3 from the last generation.</summary>
      /// <returns>List of Vector3 from the last generation.</returns>
      public static System.Collections.Generic.List<Vector3> Result => new System.Collections.Generic.List<Vector3>(result);

      #endregion


      #region Public methods

      /// <summary>Generates random Vector3.</summary>
      /// <param name="min">Smallest possible Vector3 (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">Biggest possible Vector3 (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many vectors you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="prng">Use Pseudo-Random-Number-Generator (default: false, optional)</param>
      /// <param name="silent">Ignore callbacks (default: false, optional)</param>
      /// <param name="id">id to identify the generated result (optional)</param>
      public static System.Collections.IEnumerator Generate(Vector3 min, Vector3 max, int number = 1, bool prng = false, bool silent = false, string id = "")
      {
         int _number;
         Vector3 _min = min;
         Vector3 _max = max;

         if (prng)
         {
            _number = Mathf.Clamp(number, 1, int.MaxValue);
         }
         else
         {
            if (number > 10000)
               Debug.LogWarning("'number' is larger than 10'000 - returning 10'000 Vector2.");

            _min = validateVector(min);
            _max = validateVector(max);
            _number = Mathf.Clamp(number, 1, 10000);
         }

         fixMinMax(ref _min, ref _max);

         if (!silent)
            onGenerateStart(id);

         if (_min == _max)
         {
            result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
         }
         else
         {
            if (prng)
            {
               result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
            }
            else
            {
               if (!isRunning)
               {
                  if (Util.Helper.isInternetAvailable)
                  {
                     isRunning = true;

                     System.Collections.Generic.List<Vector3> _result = new System.Collections.Generic.List<Vector3>(_number);
                     Vector3 value;

                     // x-axis
                     yield return ModuleFloat.Generate(_min.x, _max.x, _number, false, true);

                     int mfResultCount = ModuleFloat.Result.Count;

                     for (int ii = 0; ii < mfResultCount; ii++)
                     {
                        _result.Add(new Vector3(ModuleFloat.Result[ii], 0));
                     }

                     // y-axis
                     yield return ModuleFloat.Generate(_min.y, _max.y, _number, false, true);

                     int resultCount = _result.Count;
                     mfResultCount = ModuleFloat.Result.Count;

                     for (int ii = 0; ii < resultCount && ii < mfResultCount; ii++)
                     {
                        value = _result[ii];
                        value.y = ModuleFloat.Result[ii];
                        _result[ii] = value;
                     }

                     // z-axis
                     yield return ModuleFloat.Generate(_min.z, _max.z, _number, false, true);

                     mfResultCount = ModuleFloat.Result.Count;

                     for (int ii = 0; ii < resultCount && ii < mfResultCount; ii++)
                     {
                        value = _result[ii];
                        value.z = ModuleFloat.Result[ii];
                        _result[ii] = value;
                     }

                     result = _result;
                  }
                  else
                  {
                     string msg = "No Internet access available - using standard prng now!";
                     Debug.LogWarning(msg);
                     onErrorInfo(msg);

                     result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
                  }

                  isRunning = false;
               }
               else
               {
                  Debug.LogWarning("There is already a request running - please try again later!");
               }
            }
         }

         if (!silent)
            onGenerateFinished(Result, id);
      }

      /// <summary>Generates random Vector3 with the C#-standard Pseudo-Random-Number-Generator.</summary>
      /// <param name="min">Smallest possible Vector3</param>
      /// <param name="max">Biggest possible Vector3</param>
      /// <param name="number">How many Vector3 you want to generate (default: 1, optional)</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      /// <returns>List with the generated Vector3.</returns>
      public static System.Collections.Generic.List<Vector3> GeneratePRNG(Vector3 min, Vector3 max, int number = 1, int seed = 0)
      {
         System.Random rnd = seed == 0 ? new System.Random() : new System.Random(seed);
         int _number = Mathf.Abs(number);

         Vector3 _min = validateVector(min);
         Vector3 _max = validateVector(max);

         System.Collections.Generic.List<Vector3> _result = new System.Collections.Generic.List<Vector3>(_number);

         fixMinMax(ref _min, ref _max);

         for (int ii = 0; ii < _number; ii++)
         {
            _result.Add(new Vector3((float)(rnd.NextDouble() * (_max.x - _min.x) + _min.x), (float)(rnd.NextDouble() * (_max.y - _min.y) + _min.y), (float)(rnd.NextDouble() * (_max.z - _min.z) + _min.z)));
         }

         return _result;
      }

      #endregion


      #region Private methods

      private static Vector3 validateVector(Vector3 vec)
      {
         return new Vector3(Mathf.Clamp(vec.x, -1000000000f, 1000000000f), Mathf.Clamp(vec.y, -1000000000f, 1000000000f), Mathf.Clamp(vec.z, -1000000000f, 1000000000f));
      }

      private static void fixMinMax(ref Vector3 vectorMin, ref Vector3 vectorMax)
      {
         Vector3 _vectorMin = vectorMin;
         Vector3 _vectorMax = vectorMax;

         if (vectorMin.x > vectorMax.x)
         {
            vectorMin.x = _vectorMax.x;
            vectorMax.x = _vectorMin.x;
         }

         if (vectorMin.y > vectorMax.y)
         {
            vectorMin.y = _vectorMax.y;
            vectorMax.y = _vectorMin.y;
         }

         if (vectorMin.z > vectorMax.z)
         {
            vectorMin.z = _vectorMax.z;
            vectorMax.z = _vectorMin.z;
         }
      }

      #endregion


      #region Event-trigger methods

      private static void onGenerateStart(string id)
      {
         if (Util.Config.DEBUG)
            Debug.Log("onGenerateStart");

         OnGenerateStart?.Invoke(id);
      }

      private static void onGenerateFinished(System.Collections.Generic.List<Vector3> _result, string id)
      {
         if (Util.Config.DEBUG)
            Debug.Log("onGenerateFinished: " + _result.Count);

         OnGenerateFinished?.Invoke(_result, id);
      }

      #endregion


      #region Editor-only methods

#if UNITY_EDITOR

      /// <summary>Generates random Vector3 (Editor only).</summary>
      /// <param name="min">Smallest possible Vector3 (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">Biggest possible Vector3 (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many Vector3 you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="prng">Use Pseudo-Random-Number-Generator (default: false, optional)</param>
      /// <param name="id">id to identify the generated result (optional)</param>
      /// <returns>List with the generated Vector3.</returns>
      public static System.Collections.Generic.List<Vector3> GenerateInEditor(Vector3 min, Vector3 max, int number = 1, bool prng = false, string id = "")
      {
         int _number = Mathf.Clamp(number, 1, 10000);
         Vector3 _min = validateVector(min);
         Vector3 _max = validateVector(max);

         fixMinMax(ref _min, ref _max);

         onGenerateStart(id);

         if (_min == _max)
         {
            result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
         }
         else
         {
            if (prng)
            {
               result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
            }
            else
            {
               if (!isRunning)
               {
                  isRunning = true;

                  System.Collections.Generic.List<Vector3> _result = new System.Collections.Generic.List<Vector3>(_number);
                  Vector3 value;

                  // x-axis
                  ModuleFloat.GenerateInEditor(_min.x, _max.x, _number);

                  int mfResultCount = ModuleFloat.Result.Count;

                  for (int ii = 0; ii < mfResultCount; ii++)
                  {
                     _result.Add(new Vector3(ModuleFloat.Result[ii], 0));
                  }

                  // y-axis
                  ModuleFloat.GenerateInEditor(_min.y, _max.y, _number);

                  int resultCount = _result.Count;
                  mfResultCount = ModuleFloat.Result.Count;

                  for (int ii = 0; ii < resultCount && ii < mfResultCount; ii++)
                  {
                     value = _result[ii];
                     value.y = ModuleFloat.Result[ii];
                     _result[ii] = value;
                  }

                  // z-axis
                  ModuleFloat.GenerateInEditor(_min.z, _max.z, _number);

                  mfResultCount = ModuleFloat.Result.Count;

                  for (int ii = 0; ii < resultCount && ii < mfResultCount; ii++)
                  {
                     value = _result[ii];
                     value.z = ModuleFloat.Result[ii];
                     _result[ii] = value;
                  }

                  result = _result;

                  isRunning = false;
               }
               else
               {
                  Debug.LogWarning("There is already a request running - please try again later!");
               }
            }
         }

         onGenerateFinished(Result, id);

         return Result;
      }

#endif

      #endregion
   }
}
// © 2017-2021 crosstales LLC (https://www.crosstales.com)