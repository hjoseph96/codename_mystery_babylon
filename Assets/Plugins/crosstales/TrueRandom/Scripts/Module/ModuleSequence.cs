using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Crosstales.TrueRandom.Module
{
   /// <summary>
   /// This module will randomize a given interval of integers, i.e. arrange them in random order.
   /// </summary>
   public abstract class ModuleSequence : BaseModule
   {
      #region Variables

      private static System.Collections.Generic.List<int> result = new System.Collections.Generic.List<int>();

      private static bool isRunning;

      #endregion


      #region Events

      /// <summary>Event to get a message when generating sequence has started.</summary>
      public static event GenerateSequenceStart OnGenerateStart;

      /// <summary>Event to get a message with the generated sequence when finished.</summary>
      public static event GenerateSequenceFinished OnGenerateFinished;

      #endregion


      #region Static properties

      /// <summary>Returns the sequence from the last generation.</summary>
      /// <returns>Sequence from the last generation.</returns>
      public static System.Collections.Generic.List<int> Result => new System.Collections.Generic.List<int>(result);

      #endregion


      #region Public methods

      /// <summary>Generates random sequence.</summary>
      /// <param name="min">Start of the interval (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">End of the interval (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many numbers you have in the result (max range: max - min, optional)</param>
      /// <param name="prng">Use Pseudo-Random-Number-Generator (default: false, optional)</param>
      /// <param name="silent">Ignore callbacks (default: false, optional)</param>
      /// <param name="id">id to identify the generated result (optional)</param>
      public static System.Collections.IEnumerator Generate(int min, int max, int number = 0, bool prng = false, bool silent = false, string id = "")
      {
         int _min = Mathf.Clamp(min, -1000000000, 1000000000);
         int _max = Mathf.Clamp(max, -1000000000, 1000000000);

         if (_min > _max)
         {
            Debug.LogWarning("'min' value is larger than 'max' value - switching values.");

            _min = max;
            _max = min;
         }

         if (_max - _min >= 10000)
         {
            onErrorInfo("Sequence range ('max' - 'min') is larger than 10'000 elements: " + (_max - _min + 1));
         }
         else
         {
            if (!silent)
               onGenerateStart(id);

            if (_min == _max)
            {
               result = GeneratePRNG(_min, _max, TRManager.Seed);
            }
            else
            {
               if (prng)
               {
                  result = GeneratePRNG(_min, _max, TRManager.Seed);
               }
               else
               {
                  if (!isRunning)
                  {
                     isRunning = true;

                     if (Util.Helper.isInternetAvailable)
                     {
                        if (Util.Config.DEBUG)
                           Debug.Log("Quota before: " + ModuleQuota.Quota);

                        if (ModuleQuota.Quota > 0)
                        {
                           string url = Util.Constants.GENERATOR_URL + "sequences/?min=" + _min + "&max=" + _max + "&col=1&format=plain&rnd=new";

                           if (Util.Config.DEBUG)
                              Debug.Log("URL: " + url);

                           using (UnityWebRequest www = UnityWebRequest.Get(url))
                           {
                              www.timeout = timeout;
                              www.downloadHandler = new DownloadHandlerBuffer();
                              yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                              if (www.result != UnityWebRequest.Result.ProtocolError && www.result != UnityWebRequest.Result.ConnectionError)
#else
                              if (!www.isHttpError && !www.isNetworkError)
#endif
                              {
                                 result.Clear();
                                 string[] _result = System.Text.RegularExpressions.Regex.Split(www.downloadHandler.text, "\r\n?|\n", System.Text.RegularExpressions.RegexOptions.Singleline);

                                 int value = 0;
                                 foreach (string valueAsString in _result.Where(valueAsString => int.TryParse(valueAsString, out value)))
                                 {
                                    result.Add(value);
                                 }
                              }
                              else
                              {
                                 onErrorInfo(www.error);
                                 Debug.LogWarning("Could not read from url: " + www.error);

                                 result = GeneratePRNG(_min, _max, TRManager.Seed);
                              }
                           }

                           if (Util.Config.DEBUG)
                              Debug.Log("Quota after: " + ModuleQuota.Quota);
                        }
                        else
                        {
                           string msg = "Quota exceeded - using standard prng now!";
                           Debug.LogWarning(msg);
                           onErrorInfo(msg);

                           result = GeneratePRNG(_min, _max, TRManager.Seed);
                        }
                     }
                     else
                     {
                        string msg = "No Internet access available - using standard prng now!";
                        Debug.LogWarning(msg);
                        onErrorInfo(msg);

                        result = GeneratePRNG(_min, _max, TRManager.Seed);
                     }

                     isRunning = false;
                  }
                  else
                  {
                     Debug.LogWarning("There is already a request running - please try again later!");
                  }
               }
            }

            if (number > 0 && number < result.Count)
               result = result.GetRange(0, number);

            if (!silent)
               onGenerateFinished(Result, id);
         }
      }

      /// <summary>Generates a random sequence with the C#-standard Pseudo-Random-Number-Generator.</summary>
      /// <param name="min">Start of the interval</param>
      /// <param name="max">End of the interval</param>
      /// <param name="number">How many numbers you have in the result (max range: max - min, optional)</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      /// <returns>List with the generated sequence.</returns>
      public static System.Collections.Generic.List<int> GeneratePRNG(int min, int max, int number = 0, int seed = 0)
      {
         int _min = min;
         int _max = max;
         System.Collections.Generic.List<int> _result = new System.Collections.Generic.List<int>(max - min + 1);

         if (_min > _max)
            Debug.LogWarning("'min' value is larger than 'max' value - switching values.");

         for (int ii = min; ii <= max; ii++)
         {
            _result.Add(ii);
         }

         _result.CTShuffle(seed);

         if (number > 0 && number < _result.Count)
            return _result.GetRange(0, number);

         return _result;
      }

      #endregion


      #region Event-trigger methods

      private static void onGenerateStart(string id)
      {
         if (Util.Config.DEBUG)
            Debug.Log("onGenerateStart");

         OnGenerateStart?.Invoke(id);
      }

      private static void onGenerateFinished(System.Collections.Generic.List<int> _result, string id)
      {
         if (Util.Config.DEBUG)
            Debug.Log($"onGenerateFinished: {_result.Count}");

         OnGenerateFinished?.Invoke(_result, id);
      }

      #endregion


      #region Editor-only methods

#if UNITY_EDITOR

      /// <summary>Generates random sequence (Editor only).</summary>
      /// <param name="min">Start of the interval (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">End of the interval (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many numbers you have in the result (max range: max - min, optional)</param>
      /// <param name="prng">Use Pseudo-Random-Number-Generator (default: false, optional)</param>
      /// <param name="id">id to identify the generated result (optional)</param>
      /// <returns>List with the generated sequence.</returns>
      public static System.Collections.Generic.List<int> GenerateInEditor(int min, int max, int number = 0, bool prng = false, string id = "")
      {
         int _min = Mathf.Clamp(min, -1000000000, 1000000000);
         int _max = Mathf.Clamp(max, -1000000000, 1000000000);

         onGenerateStart(id);

         if (_min > _max)
         {
            Debug.LogWarning("'min' value is larger than 'max' value - switching values.");

            _min = max;
            _max = min;
         }

         if (_max - _min >= 10000)
         {
            Debug.LogError("Sequence range ('max' - 'min') is larger than 10'000 elements: " + (_max - _min + 1));
         }
         else
         {
            if (_min == _max)
            {
               result = GeneratePRNG(_min, _max, TRManager.Seed);
            }
            else
            {
#if UNITY_WSA || UNITY_XBOXONE
               result = GeneratePRNG(_min, _max);
#else
               if (prng)
               {
                  result = GeneratePRNG(_min, _max, TRManager.Seed);
               }
               else
               {
                  if (!isRunning)
                  {
                     isRunning = true;

                     if (Util.Config.DEBUG)
                        Debug.Log("Quota before: " + ModuleQuota.Quota);

                     if (ModuleQuota.Quota > 0)
                     {
                        try
                        {
                           System.Net.ServicePointManager.ServerCertificateValidationCallback = Util.Helper.RemoteCertificateValidationCallback;

                           using (System.Net.WebClient client = new Common.Util.CTWebClient(timeout * 1000))
                           {
                              string url = Util.Constants.GENERATOR_URL + "sequences/?min=" + _min + "&max=" + _max + "&col=1&format=plain&rnd=new";

                              using (System.IO.Stream stream = client.OpenRead(url))
                              {
                                 using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                                 {
                                    string content = reader.ReadToEnd();

                                    if (Util.Config.DEBUG)
                                       Debug.Log(content);

                                    result.Clear();
                                    string[] _result = System.Text.RegularExpressions.Regex.Split(content, "\r\n?|\n", System.Text.RegularExpressions.RegexOptions.Singleline);

                                    int value = 0;
                                    foreach (string valueAsString in _result.Where(valueAsString => int.TryParse(valueAsString, out value)))
                                    {
                                       result.Add(value);
                                    }

                                    if (Util.Config.SHOW_QUOTA)
                                       ModuleQuota.GetQuotaInEditor();
                                 }
                              }
                           }
                        }
                        catch (System.Exception ex)
                        {
                           Debug.LogError("Error occured - using prng now: " + ex);

                           result = GeneratePRNG(_min, _max, TRManager.Seed);
                        }
                     }
                     else
                     {
                        Debug.LogWarning("Quota exceeded - using standard prng now!");

                        result = GeneratePRNG(_min, _max, TRManager.Seed);
                     }

                     isRunning = false;
                  }
                  else
                  {
                     Debug.LogWarning("There is already a request running - please try again later!");
                  }
               }

#endif
            }
         }

         if (number > 0 && number < result.Count)
            result = result.GetRange(0, number);

         onGenerateFinished(Result, id);

         return Result;
      }

#endif

      #endregion
   }
}
// © 2016-2021 crosstales LLC (https://www.crosstales.com)