using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Crosstales.TrueRandom.Module
{
   /// <summary>
   /// This module will generate true random strings of various length and character compositions.
   /// </summary>
   public abstract class ModuleString : BaseModule
   {
      #region Variables

      private static System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();

      private static bool isRunning;

      #endregion


      #region Events

      /// <summary>Event to get a message when generating strings has started.</summary>
      public static event GenerateStringStart OnGenerateStart;

      /// <summary>Event to get a message with the generated strings when finished.</summary>
      public static event GenerateStringFinished OnGenerateFinished;

      #endregion


      #region Static properties

      /// <summary>Returns the list of strings from the last generation.</summary>
      /// <returns>List of strings from the last generation.</returns>
      public static System.Collections.Generic.List<string> Result => result.GetRange(0, result.Count);

      #endregion


      #region Public methods

      /// <summary>Generates random strings.</summary>
      /// <param name="length">How long the strings should be (range: 1 - 20)</param>
      /// <param name="number">How many strings you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="digits">Allow digits (0-9) (default: true, optional)</param>
      /// <param name="upper">Allow uppercase letters (default: true, optional)</param>
      /// <param name="lower">Allow lowercase letters (default: true, optional)</param>
      /// <param name="unique">String should be unique (default: false, optional)</param>
      /// <param name="prng">Use Pseudo-Random-Number-Generator (default: false, optional)</param>
      /// <param name="silent">Ignore callbacks (default: false, optional)</param>
      /// <param name="id">id to identify the generated result (optional)</param>
      public static System.Collections.IEnumerator Generate(int length, int number = 1, bool digits = true, bool upper = true, bool lower = true, bool unique = false, bool prng = false, bool silent = false, string id = "")
      {
         int _length = length;
         int _number;

         if (prng)
         {
            _number = Mathf.Clamp(number, 1, int.MaxValue);
         }
         else
         {
            _length = Mathf.Clamp(length, 1, 20);
            _number = calcMaxNumber(number, _length, digits, upper, lower, unique);

            if (_number < number)
               Debug.LogWarning("'number' is to large - returning " + _number + " strings.");
         }

         if (!digits && !upper && !lower)
         {
            onErrorInfo("'digits', 'upper' and 'lower' are 'false' - string generation not possible!");
         }
         else
         {
            if (!silent)
               onGenerateStart(id);

            if (prng)
            {
               result = GeneratePRNG(_length, _number, digits, upper, lower, unique, TRManager.Seed);
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
                        string url = Util.Constants.GENERATOR_URL + "strings/?num=" + _number + "&len=" + _length + "&digits=" + boolToString(digits) + "&upperalpha=" + boolToString(upper) + "&loweralpha=" + boolToString(lower) + "&unique=" + boolToString(unique) + "&format=plain&rnd=new";

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

                              foreach (string valueAsString in _result.Where(valueAsString => !string.IsNullOrEmpty(valueAsString)))
                              {
                                 result.Add(valueAsString);
                              }
                           }
                           else
                           {
                              onErrorInfo(www.error);
                              Debug.LogWarning("Could not read from url: " + www.error);

                              result = GeneratePRNG(_length, _number, digits, upper, lower, unique, TRManager.Seed);
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

                        result = GeneratePRNG(_length, _number, digits, upper, lower, unique, TRManager.Seed);
                     }
                  }
                  else
                  {
                     string msg = "No Internet access available - using standard prng now!";
                     Debug.LogWarning(msg);
                     onErrorInfo(msg);

                     result = GeneratePRNG(_length, _number, digits, upper, lower, unique, TRManager.Seed);
                  }

                  isRunning = false;
               }
               else
               {
                  Debug.LogWarning("There is already a request running - please try again later!");
               }
            }

            if (!silent)
               onGenerateFinished(Result, id);
         }
      }

      /// <summary>Generates random strings with the C#-standard Pseudo-Random-Number-Generator.</summary>
      /// <param name="length">How long the strings should be</param>
      /// <param name="number">How many strings you want to generate (default: 1, optional)</param>
      /// <param name="digits">Allow digits (0-9) (default: true, optional)</param>
      /// <param name="upper">Allow uppercase (A-Z) letters (default: true, optional)</param>
      /// <param name="lower">Allow lowercase (a-z) letters (default: true, optional)</param>
      /// <param name="unique">String should be unique (default: false, optional)</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      /// <returns>List with the generated strings.</returns>
      public static System.Collections.Generic.List<string> GeneratePRNG(int length, int number = 1, bool digits = true, bool upper = true, bool lower = true, bool unique = false, int seed = 0)
      {
         System.Random rnd = seed == 0 ? new System.Random() : new System.Random(seed);
         int _length = Mathf.Abs(length);
         int _number = calcMaxNumber(number, _length, digits, upper, lower, unique);
         System.Collections.Generic.List<string> _result = new System.Collections.Generic.List<string>(_number);

         string glyphs = string.Empty;

         if (upper)
         {
            glyphs += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
         }

         if (lower)
         {
            glyphs += "abcdefghijklmnopqrstuvwxyz";
         }

         if (digits)
         {
            glyphs += "1234567890";
         }

         for (int i = 0; i < _number; i++)
         {
            string s;
            if (unique)
            {
               bool isNotUnique;
               do
               {
                  isNotUnique = false;
                  s = string.Empty;
                  for (int ii = 0; ii < _length; ii++)
                  {
                     s += glyphs[rnd.Next(0, glyphs.Length)];
                  }

                  foreach (string str in result.Where(str => str == s))
                  {
                     isNotUnique = true;
                  }
               } while (isNotUnique);
            }
            else
            {
               s = string.Empty;
               for (int ii = 0; ii < _length; ii++)
               {
                  s += glyphs[rnd.Next(0, glyphs.Length)];
               }
            }

            _result.Add(s);
         }

         return _result;
      }

      #endregion


      #region Private methods

      private static int calcMaxNumber(int number, int length, bool digits, bool upper, bool lower, bool unique)
      {
         int _number = Mathf.Clamp(number, 1, 10000);

         if (unique && length > 0 && length <= 10)
         {
            double basis = 0d;

            if (digits)
            {
               basis += 10d;
            }

            if (upper)
            {
               basis += 26d;
            }

            if (lower)
            {
               basis += 26d;
            }

            if (basis > 0d)
            {
               long maxNumber = (long)System.Math.Pow(basis, length);

               if (maxNumber < number)
               {
                  Debug.LogWarning($"Too many numbers requested with 'unique' on - result reduced to {maxNumber} numbers!");
                  _number = (int)maxNumber;
               }
            }
         }

         return _number;
      }

      private static string boolToString(bool value)
      {
         return value ? "on" : "off";
      }

      #endregion


      #region Event-trigger methods

      private static void onGenerateStart(string id)
      {
         if (Util.Config.DEBUG)
            Debug.Log("onGenerateStart");

         OnGenerateStart?.Invoke(id);
      }

      private static void onGenerateFinished(System.Collections.Generic.List<string> _result, string id)
      {
         if (Util.Config.DEBUG)
            Debug.Log($"onGenerateFinished: {_result.Count}");

         OnGenerateFinished?.Invoke(_result, id);
      }

      #endregion


      #region Editor-only methods

#if UNITY_EDITOR

      /// <summary>Generates random strings (Editor only).</summary>
      /// <param name="length">How long the strings should be (range: 1 - 20)</param>
      /// <param name="number">How many strings you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="digits">Allow digits (0-9) (default: true, optional)</param>
      /// <param name="upper">Allow uppercase letters (default: true, optional)</param>
      /// <param name="lower">Allow lowercase letters (default: true, optional)</param>
      /// <param name="unique">String should be unique (default: false, optional)</param>
      /// <param name="prng">Use Pseudo-Random-Number-Generator (default: false, optional)</param>
      /// <param name="id">id to identify the generated result (optional)</param>
      /// <returns>List with the generated strings.</returns>
      public static System.Collections.Generic.List<string> GenerateInEditor(int length, int number = 1, bool digits = true, bool upper = true, bool lower = true, bool unique = false, bool prng = false, string id = "")
      {
         int _length = Mathf.Clamp(length, 1, 20);
         int _number = calcMaxNumber(number, _length, digits, upper, lower, unique);

         onGenerateStart(id);

         if (!digits && !upper && !lower)
         {
            Debug.LogError("'digits', 'upper' and 'lower' are 'false' - string generation not possible!");
         }
         else
         {
            if (prng)
            {
               result = GeneratePRNG(_length, _number, digits, upper, lower, unique, TRManager.Seed);
            }
            else
            {
#if UNITY_WSA || UNITY_XBOXONE
               result = GeneratePRNG(_length, _number, digits, upper, lower, unique);
#else
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
                           string url = Util.Constants.GENERATOR_URL + "strings/?num=" + _number + "&len=" + _length + "&digits=" + boolToString(digits) + "&upperalpha=" + boolToString(upper) + "&loweralpha=" + boolToString(lower) + "&unique=" + boolToString(unique) + "&format=plain&rnd=new";

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

                                 foreach (string valueAsString in _result.Where(valueAsString => !string.IsNullOrEmpty(valueAsString)))
                                 {
                                    result.Add(valueAsString);
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

                        result = GeneratePRNG(_length, _number, digits, upper, lower, unique, TRManager.Seed);
                     }
                  }
                  else
                  {
                     Debug.LogWarning("Quota exceeded - using prng now!");

                     result = GeneratePRNG(_length, _number, digits, upper, lower, unique, TRManager.Seed);
                  }

                  isRunning = false;
               }
               else
               {
                  Debug.LogWarning("There is already a request running - please try again later!");
               }
#endif
            }
         }

         onGenerateFinished(Result, id);

         return Result;
      }

#endif

      #endregion
   }
}
// © 2016-2021 crosstales LLC (https://www.crosstales.com)