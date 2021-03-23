using UnityEngine;
using UnityEngine.Networking;
using Enumerable = System.Linq.Enumerable;

namespace Crosstales.TrueRandom.Module
{
   /// <summary>
   /// This module will generate true random integers in configurable intervals.
   /// </summary>
   public abstract class ModuleInteger : BaseModule
   {
      #region Variables

      //[Tooltip("List of the generated integers.")]
      private static System.Collections.Generic.List<int> result = new System.Collections.Generic.List<int>();

      private static bool isRunning;

      #endregion


      #region Events

      /// <summary>Event to get a message when generating integers has started.</summary>
      public static event GenerateIntegerStart OnGenerateStart;

      /// <summary>Event to get a message with the generated integers when finished.</summary>
      public static event GenerateIntegerFinished OnGenerateFinished;

      #endregion


      #region Static properties

      /// <summary>Returns the list of integers from the last generation.</summary>
      /// <returns>List of integers from the last generation.</returns>
      public static System.Collections.Generic.List<int> Result => new System.Collections.Generic.List<int>(result);

      #endregion


      #region Public methods

      /// <summary>Generates random integers.</summary>
      /// <param name="min">Smallest possible number (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">Biggest possible number (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many numbers you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="prng">Use Pseudo-Random-Number-Generator (default: false, optional)</param>
      /// <param name="silent">Ignore callbacks (default: false, optional)</param>
      /// <param name="id">id to identifiy the generated result (optional)</param>
      public static System.Collections.IEnumerator Generate(int min, int max, int number = 1, bool prng = false, bool silent = false, string id = "")
      {
         int _min = min;
         int _max = max;
         int _number;

         if (prng)
         {
            _number = Mathf.Clamp(number, 1, int.MaxValue);
         }
         else
         {
            if (number > 10000)
               Debug.LogWarning("'number' is larger than 10'000 - returning 10'000 integers.");

            _min = Mathf.Clamp(min, -1000000000, 1000000000);
            _max = Mathf.Clamp(max, -1000000000, 1000000000);
            _number = Mathf.Clamp(number, 1, 10000);
         }

         if (_min > _max)
         {
            Debug.LogWarning("'min' value is larger than 'max' value - switching values.");

            _min = max;
            _max = min;
         }

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
                  isRunning = true;

                  if (Util.Helper.isInternetAvailable)
                  {
                     if (Util.Config.DEBUG)
                        Debug.Log("Quota before: " + ModuleQuota.Quota);

                     if (ModuleQuota.Quota > 0)
                     {
                        string url = Util.Constants.GENERATOR_URL + "integers/?num=" + _number + "&min=" + _min + "&max=" + _max + "&col=1&base=10&format=plain&rnd=new";

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
                              foreach (string valueAsString in Enumerable.Where(_result, valueAsString => int.TryParse(valueAsString, out value)))
                              {
                                 result.Add(value);
                              }
                           }
                           else
                           {
                              onErrorInfo(www.error);
                              Debug.LogWarning("Could not read from url: " + www.error);

                              result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
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

                        result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
                     }
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

      /// <summary>Generates random integers with the C#-standard Pseudo-Random-Number-Generator.</summary>
      /// <param name="min">Smallest possible number</param>
      /// <param name="max">Biggest possible number</param>
      /// <param name="number">How many numbers you want to generate (default: 1, optional)</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      /// <returns>List with the generated integers.</returns>
      public static System.Collections.Generic.List<int> GeneratePRNG(int min, int max, int number = 1, int seed = 0)
      {
         System.Random rnd = seed == 0 ? new System.Random() : new System.Random(seed);
         int _number = Mathf.Abs(number);
         int _min = min;
         int _max = max;

         System.Collections.Generic.List<int> _result = new System.Collections.Generic.List<int>(_number);

         if (min > max)
         {
            Debug.LogWarning("'min' value is larger than 'max' value - switching values.");

            min = _max;
            max = _min;
         }

         for (int ii = 0; ii < _number; ii++)
         {
            _result.Add(rnd.Next(_min, _max + 1));
         }

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

      /// <summary>Generates random integers with the C#-standard Pseudo-Random-Number-Generator (Editor only).</summary>
      /// <param name="min">Smallest possible number (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">Biggest possible number (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many numbers you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="prng">Use Pseudo-Random-Number-Generator (default: false, optional)</param>
      /// <param name="id">id to identifiy the generated result (optional)</param>
      /// <returns>List with the generated integers.</returns>
      public static System.Collections.Generic.List<int> GenerateInEditor(int min, int max, int number = 1, bool prng = false, string id = "")
      {
         int _number = Mathf.Clamp(number, 1, 10000);
         int _min = Mathf.Clamp(min, -1000000000, 1000000000);
         int _max = Mathf.Clamp(max, -1000000000, 1000000000);

         onGenerateStart(id);

         if (_min > _max)
         {
            Debug.LogWarning("'min' value is larger than 'max' value - switching values.");

            _min = max;
            _max = min;
         }

         if (_min == _max)
         {
            result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
         }
         else
         {
#if UNITY_WSA || UNITY_XBOXONE
            result = GeneratePRNG(_min, _max, _number);
#else
            if (prng)
            {
               result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
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
                     System.Threading.Thread worker = new System.Threading.Thread(() => readData(_min, _max, _number));
                     worker.Start();

                     do
                     {
                        System.Threading.Thread.Sleep(50);
                     } while (worker.IsAlive);

                     if (Util.Config.SHOW_QUOTA)
                        ModuleQuota.GetQuotaInEditor();

                     /*
                     try
                     {
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = Util.Helper.RemoteCertificateValidationCallback;

                        using (System.Net.WebClient client = new Common.Util.CTWebClient(timeout * 1000))
                        {
                           string url = Util.Constants.GENERATOR_URL + "integers/?num=" + _number + "&min=" + _min + "&max=" + _max + "&col=1&base=10&format=plain&rnd=new";

                           if (Util.Config.DEBUG)
                              Debug.Log("URL: " + url);

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
                                 foreach (string valueAsString in _result)
                                 {
                                    if (int.TryParse(valueAsString, out value))
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

                        result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
                     }
                     */
                  }
                  else
                  {
                     Debug.LogError("Quota exceeded - using prng now!");

                     result = GeneratePRNG(_min, _max, _number, TRManager.Seed);
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

         onGenerateFinished(Result, id);

         return Result;
      }

#endif
#if !UNITY_WSA && !UNITY_XBOXONE
      private static void readData(int min, int max, int number)
      {
         try
         {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = Util.Helper.RemoteCertificateValidationCallback;

            using (System.Net.WebClient client = new Common.Util.CTWebClient(timeout * 1000))
            {
               string url = Util.Constants.GENERATOR_URL + "integers/?num=" + number + "&min=" + min + "&max=" + max + "&col=1&base=10&format=plain&rnd=new";

               if (Util.Config.DEBUG)
                  Debug.Log("URL: " + url);

               string content = client.DownloadString(url);

               if (Util.Config.DEBUG)
                  Debug.Log(content);

               result.Clear();
               string[] _result = System.Text.RegularExpressions.Regex.Split(content, "\r\n?|\n", System.Text.RegularExpressions.RegexOptions.Singleline);

               int value = 0;
               foreach (string valueAsString in Enumerable.Where(_result, valueAsString => int.TryParse(valueAsString, out value)))
               {
                  result.Add(value);
               }


               /*
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
                     foreach (string valueAsString in _result)
                     {
                        if (int.TryParse(valueAsString, out value))
                           result.Add(value);
                     }

                     if (Util.Config.SHOW_QUOTA)
                        ModuleQuota.GetQuotaInEditor();
                  }
               }
               */
            }
         }
         catch (System.Exception ex)
         {
            Debug.LogError("Error occured - using prng now: " + ex);

            result = GeneratePRNG(min, max, number, TRManager.Seed);
         }
      }
#endif
      #endregion
   }
}
// © 2016-2021 crosstales LLC (https://www.crosstales.com)