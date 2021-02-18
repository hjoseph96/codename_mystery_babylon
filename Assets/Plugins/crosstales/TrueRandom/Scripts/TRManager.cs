using UnityEngine;

namespace Crosstales.TrueRandom
{
   /// <summary>
   /// The TRManager is the manager for all modules.
   /// </summary>
   [ExecuteInEditMode]
   [DisallowMultipleComponent]
   [HelpURL("https://www.crosstales.com/media/data/assets/truerandom/api/class_crosstales_1_1_true_random_1_1_t_r_manager.html")]
   public class TRManager : Crosstales.Common.Util.Singleton<TRManager>
   {
      #region Variables

      [UnityEngine.Serialization.FormerlySerializedAsAttribute("PRNG")] [Header("Behaviour Settings"), Tooltip("Enable or disable the C#-standard Pseudo-Random-Number-Generator-mode (default: false)."), SerializeField]
      private bool prng;

      public readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>> AllIntegerResults = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>();
      public readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<float>> AllFloatResults = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<float>>();
      public readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>> AllSequenceResults = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>();
      public readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> AllStringResults = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();
      public readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Vector2>> AllVector2Results = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Vector2>>();
      public readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Vector3>> AllVector3Results = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Vector3>>();
      public readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Vector4>> AllVector4Results = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Vector4>>();

      private int generateCount;
      private static readonly System.Random rnd = new System.Random();

      #endregion


      #region Properties

      /// <summary>Enable or disable the C#-standard Pseudo-Random-Number-Generator-mode.</summary>
      public bool PRNG
      {
         get => prng;
         set => prng = value;
      }

      /// <summary>Returns the remaining quota in bits from the last check.</summary>
      /// <returns>Remaining quota in bits from the last check.</returns>
      public int CurrentQuota => Module.ModuleQuota.Quota;


      /// <summary>Returns the list of integers from the last generation.</summary>
      /// <returns>List of integers from the last generation.</returns>
      public System.Collections.Generic.List<int> CurrentIntegers => Module.ModuleInteger.Result;

      /// <summary>Returns the list of floats from the last generation.</summary>
      /// <returns>List of floats from the last generation.</returns>
      public System.Collections.Generic.List<float> CurrentFloats => Module.ModuleFloat.Result;

      /// <summary>Returns the sequence from the last generation.</summary>
      /// <returns>Sequence from the last generation.</returns>
      public System.Collections.Generic.List<int> CurrentSequence => Module.ModuleSequence.Result;

      /// <summary>Returns the list of strings from the last generation.</summary>
      /// <returns>List of strings from the last generation.</returns>
      public System.Collections.Generic.List<string> CurrentStrings => Module.ModuleString.Result;

      /// <summary>Returns the list of Vector2 from the last generation.</summary>
      /// <returns>List of Vector2 from the last generation.</returns>
      public System.Collections.Generic.List<Vector2> CurrentVector2 => Module.ModuleVector2.Result;

      /// <summary>Returns the list of Vector3 from the last generation.</summary>
      /// <returns>List of Vector3 from the last generation.</returns>
      public System.Collections.Generic.List<Vector3> CurrentVector3 => Module.ModuleVector3.Result;

      /// <summary>Returns the list of Vector4 from the last generation.</summary>
      /// <returns>List of Vector4 from the last generation.</returns>
      public System.Collections.Generic.List<Vector4> CurrentVector4 => Module.ModuleVector4.Result;

      /// <summary>Checks if True Random is generating numbers on this system.</summary>
      /// <returns>True if True Random is generating numbers on this system.</returns>
      public bool isGenerating => generateCount > 0;

      /// <summary>Returns a seed for the PRNG.</summary>
      /// <returns>Seed for the PRNG.</returns>
      public static int Seed => rnd.Next(int.MinValue, int.MaxValue);

      #endregion


      #region Events

      [Header("Events")] public OnGenerateCompleted OnGenerateCompleted;
      public OnQuotaUpdated OnQuotaUpdated;
      public OnError OnError;

      /// <summary>An event triggered whenever generating integers has started.</summary>
      public event GenerateIntegerStart OnGenerateIntegerStart;

      /// <summary>An event triggered whenever generating integers has finished.</summary>
      public event GenerateIntegerFinished OnGenerateIntegerFinished;

      /// <summary>An event triggered whenever generating floats has started.</summary>
      public event GenerateFloatStart OnGenerateFloatStart;

      /// <summary>An event triggered whenever generating floats has finished.</summary>
      public event GenerateFloatFinished OnGenerateFloatFinished;

      /// <summary>An event triggered whenever generating sequence has started.</summary>
      public event GenerateSequenceStart OnGenerateSequenceStart;

      /// <summary>An event triggered whenever generating sequence has finished.</summary>
      public event GenerateSequenceFinished OnGenerateSequenceFinished;

      /// <summary>An event triggered whenever generating strings has started.</summary>
      public event GenerateStringStart OnGenerateStringStart;

      /// <summary>An event triggered whenever generating strings has finished.</summary>
      public event GenerateStringFinished OnGenerateStringFinished;

      /// <summary>An event triggered whenever generating Vector2 has started.</summary>
      public event GenerateVector2Start OnGenerateVector2Start;

      /// <summary>An event triggered whenever generating Vector2 has finished.</summary>
      public event GenerateVector2Finished OnGenerateVector2Finished;

      /// <summary>An event triggered whenever generating Vector3 has started.</summary>
      public event GenerateVector3Start OnGenerateVector3Start;

      /// <summary>An event triggered whenever generating Vector3 has finished.</summary>
      public event GenerateVector3Finished OnGenerateVector3Finished;

      /// <summary>An event triggered whenever generating Vector4 has started.</summary>
      public event GenerateVector4Start OnGenerateVector4Start;

      /// <summary>An event triggered whenever generating Vector4 has finished.</summary>
      public event GenerateVector4Finished OnGenerateVector4Finished;

      /// <summary>An event triggered whenever the quota is updated.</summary>
      public event QuotaUpdate OnQuotaUpdate;

      /// <summary>An event triggered whenever an error occurs.</summary>
      public event ErrorInfo OnErrorInfo;

      #endregion


      #region MonoBehaviour methods

      protected override void Awake()
      {
         base.Awake();

         if (Instance == this)
         {
            Module.BaseModule.OnErrorInfo += onErrorInfo;
            Module.ModuleQuota.OnUpdateQuota += onQuotaUpdate;
            Module.ModuleInteger.OnGenerateStart += onGenerateIntegerStart;
            Module.ModuleInteger.OnGenerateFinished += onGenerateIntegerFinished;
            Module.ModuleFloat.OnGenerateStart += onGenerateFloatStart;
            Module.ModuleFloat.OnGenerateFinished += onGenerateFloatFinished;
            Module.ModuleSequence.OnGenerateStart += onGenerateSequenceStart;
            Module.ModuleSequence.OnGenerateFinished += onGenerateSequenceFinished;
            Module.ModuleString.OnGenerateStart += onGenerateStringStart;
            Module.ModuleString.OnGenerateFinished += onGenerateStringFinished;
            Module.ModuleVector2.OnGenerateStart += onGenerateVector2Start;
            Module.ModuleVector2.OnGenerateFinished += onGenerateVector2Finished;
            Module.ModuleVector3.OnGenerateStart += onGenerateVector3Start;
            Module.ModuleVector3.OnGenerateFinished += onGenerateVector3Finished;
            Module.ModuleVector4.OnGenerateStart += onGenerateVector4Start;
            Module.ModuleVector4.OnGenerateFinished += onGenerateVector4Finished;

            GetQuota();
         }
      }

      protected override void OnDestroy()
      {
         if (instance == this)
         {
            Module.BaseModule.OnErrorInfo -= onErrorInfo;
            Module.ModuleQuota.OnUpdateQuota -= onQuotaUpdate;
            Module.ModuleInteger.OnGenerateStart -= onGenerateIntegerStart;
            Module.ModuleInteger.OnGenerateFinished -= onGenerateIntegerFinished;
            Module.ModuleFloat.OnGenerateStart -= onGenerateFloatStart;
            Module.ModuleFloat.OnGenerateFinished -= onGenerateFloatFinished;
            Module.ModuleSequence.OnGenerateStart -= onGenerateSequenceStart;
            Module.ModuleSequence.OnGenerateFinished -= onGenerateSequenceFinished;
            Module.ModuleString.OnGenerateStart -= onGenerateStringStart;
            Module.ModuleString.OnGenerateFinished -= onGenerateStringFinished;
            Module.ModuleVector2.OnGenerateStart -= onGenerateVector2Start;
            Module.ModuleVector2.OnGenerateFinished -= onGenerateVector2Finished;
            Module.ModuleVector3.OnGenerateStart -= onGenerateVector3Start;
            Module.ModuleVector3.OnGenerateFinished -= onGenerateVector3Finished;
            Module.ModuleVector4.OnGenerateStart -= onGenerateVector4Start;
            Module.ModuleVector4.OnGenerateFinished -= onGenerateVector4Finished;
         }

         base.OnDestroy();
      }

      private void Start()
      {
         //do nothing, just allow to enable/disable the script
      }

      #endregion


      #region Public methods

      /// <summary>Resets this object.</summary>
      public static void ResetObject()
      {
         DeleteInstance();
      }

      /// <summary>
      /// Calculates needed bits (from the quota) for generating random floats.
      /// </summary>
      /// <param name="number">How many numbers (default: 1, optional)</param>
      /// <returns>Needed bits for generating the floats.</returns>
      public int CalculateFloat(int number = 1)
      {
         int bitsCounter = 32;

         bitsCounter *= Mathf.Abs(number);

         if (Util.Config.DEBUG)
         {
            Debug.Log(Mathf.Abs(number) + "x this number costs " + bitsCounter + " bits", this);

            int left = CurrentQuota - bitsCounter;
            int times = bitsCounter == 0 ? int.MaxValue : left / bitsCounter;

            Debug.Log("Quota left after generating float numbers: " + left, this);
            Debug.Log("You can generate these numbers " + times + "x in the next 24 hours", this);
         }

         return bitsCounter;
      }

      /// <summary>
      /// Calculates needed bits (from the quota) for generating random integers.
      /// </summary>
      /// <param name="max">Biggest allowed number</param>
      /// <param name="number">How many numbers (default: 1, optional)</param>
      /// <returns>Needed bits for generating the integers.</returns>
      public int CalculateInteger(int max, int number = 1)
      {
         int bitsCounter = 0;
         float tmp = Mathf.Abs(max);

         while (tmp >= 1)
         {
            if (Mathf.Abs(tmp % 2) < Util.Constants.FLOAT_TOLERANCE)
            {
               tmp /= 2;
            }
            else
            {
               tmp /= 2;
               tmp -= 0.5f;
            }

            bitsCounter++;
         }

         bitsCounter *= Mathf.Abs(number);

         if (Util.Config.DEBUG)
         {
            Debug.Log(Mathf.Abs(number) + "x this number costs " + bitsCounter + " bits", this);

            int left = CurrentQuota - bitsCounter;
            int times = bitsCounter == 0 ? int.MaxValue : left / bitsCounter;

            Debug.Log("Quota left after generating integer numbers: " + left, this);
            Debug.Log("You can generate these numbers " + times + "x in the next 24 hours", this);
         }

         return bitsCounter;
      }

      /// <summary>
      /// Calculates needed bits (from the quota) for generating a random sequence.
      /// </summary>
      /// <param name="min">Start of the interval</param>
      /// <param name="max">End of the interval</param>
      /// <returns>Needed bits for generating the sequence.</returns>
      public int CalculateSequence(int min, int max)
      {
         int _min = min;
         int _max = max;

         if (_min > _max)
         {
            _min = max;
            _max = min;
         }

         if (_min == 0 && _max == 0)
         {
            _max = 1;
         }

         int bitsCounter = 0;

         if (_min != _max)
         {
            bitsCounter = (_max - _min) * 31;
         }

         if (Util.Config.DEBUG)
         {
            Debug.Log(_max - _min + 1 + "x this sequence costs " + bitsCounter + " bits", this);

            int left = CurrentQuota - bitsCounter;
            int times = bitsCounter == 0 ? int.MaxValue : left / bitsCounter;

            Debug.Log("Quota left after generating sequence: " + left, this);
            Debug.Log("You can generate these sequence " + times + "x in the next 24 hours", this);
         }

         return bitsCounter;
      }

      /// <summary>
      /// Calculates needed bits (from the quota) for generating random strings.
      /// </summary>
      /// <param name="length">Length of the strings</param>
      /// <param name="number">How many strings (default: 1, optional)</param>
      /// <returns>Needed bits for generating the strings.</returns>
      public int CalculateString(int length, int number = 1)
      {
         int bitsCounter = Mathf.Abs(number) * Mathf.Abs(length) * 30;

         if (Util.Config.DEBUG)
         {
            Debug.Log("It's not possible to calculate exactly how many bits it costs. 1 char costs between 4 and 30 bits", this);

            Debug.Log("Generating these strings will use between: " + Mathf.Abs(number) * Mathf.Abs(length) * 4 + " and: " + Mathf.Abs(number) * Mathf.Abs(length) * 30 + " bits", this);

            int left = CurrentQuota - bitsCounter;
            int times = bitsCounter == 0 ? int.MaxValue : left / bitsCounter;

            Debug.Log("Quota left after generating strings: " + left, this);
            Debug.Log("You can generate these strings " + times + "x in the next 24 hours", this);
         }

         return bitsCounter;
      }

      /// <summary>
      /// Calculates needed bits (from the quota) for generating random Vector2.
      /// </summary>
      /// <param name="number">How many Vector2 (default: 1, optional)</param>
      /// <returns>Needed bits for generating the Vector2.</returns>
      public int CalculateVector2(int number = 1)
      {
         return CalculateFloat(number * 2);
      }

      /// <summary>
      /// Calculates needed bits (from the quota) for generating random Vector3.
      /// </summary>
      /// <param name="number">How many Vector3 (default: 1, optional)</param>
      /// <returns>Needed bits for generating the Vector3.</returns>
      public int CalculateVector3(int number = 1)
      {
         return CalculateFloat(number * 3);
      }

      /// <summary>
      /// Calculates needed bits (from the quota) for generating random Vector4.
      /// </summary>
      /// <param name="number">How many Vector4 (default: 1, optional)</param>
      /// <returns>Needed bits for generating the Vector4.</returns>
      public int CalculateVector4(int number = 1)
      {
         return CalculateFloat(number * 4);
      }

      /// <summary>Generates random integers.</summary>
      /// <param name="min">Smallest possible number (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">Biggest possible number (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many numbers you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="id">UID to identify the generated result (optional)</param>
      /// <returns>UID of the generator.</returns>
      public string GenerateInteger(int min, int max, int number = 1, string id = "")
      {
         string _id = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;

         if (Util.Helper.isEditorMode)
         {
#if UNITY_EDITOR
            generateIntegerInEditor(min, max, number);
#endif
         }
         else
         {
            StartCoroutine(Module.ModuleInteger.Generate(min, max, number, PRNG, false, _id));
         }

         return _id;
      }

      /// <summary>Generates random floats.</summary>
      /// <param name="min">Smallest possible number (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">Biggest possible number (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many numbers you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="id">UID to identify the generated result (optional)</param>
      /// <returns>UID of the generator.</returns>
      public string GenerateFloat(float min, float max, int number = 1, string id = "")
      {
         string _id = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;

         if (Util.Helper.isEditorMode)
         {
#if UNITY_EDITOR
            generateFloatInEditor(min, max, number);
#endif
         }
         else
         {
            StartCoroutine(Module.ModuleFloat.Generate(min, max, number, PRNG, false, string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id));
         }

         return _id;
      }

      /// <summary>Generates random sequence.</summary>
      /// <param name="min">Start of the interval (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">End of the interval (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many numbers you have in the result (max range: max - min, optional)</param>
      /// <param name="id">UID to identify the generated result (optional)</param>
      /// <returns>UID of the generator.</returns>
      public string GenerateSequence(int min, int max, int number = 0, string id = "")
      {
         string _id = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;

         if (Util.Helper.isEditorMode)
         {
#if UNITY_EDITOR
            generateSequenceInEditor(min, max, number);
#endif
         }
         else
         {
            StartCoroutine(Module.ModuleSequence.Generate(min, max, number, PRNG, false, string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id));
         }

         return _id;
      }

      /// <summary>Generates random strings.</summary>
      /// <param name="length">How long the strings should be (range: 1 - 20)</param>
      /// <param name="number">How many strings you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="digits">Allow digits (0-9) (default: true, optional)</param>
      /// <param name="upper">Allow uppercase (A-Z) letters (default: true, optional)</param>
      /// <param name="lower">Allow lowercase (a-z) letters (default: true, optional)</param>
      /// <param name="unique">String should be unique in the result (default: false, optional)</param>
      /// <param name="id">UID to identify the generated result (optional)</param>
      /// <returns>UID of the generator.</returns>
      public string GenerateString(int length, int number = 1, bool digits = true, bool upper = true, bool lower = true, bool unique = false, string id = "")
      {
         string _id = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;

         if (Util.Helper.isEditorMode)
         {
#if UNITY_EDITOR
            generateStringInEditor(length, number, digits, upper, lower, unique);
#endif
         }
         else
         {
            StartCoroutine(Module.ModuleString.Generate(length, number, digits, upper, lower, unique, PRNG, false, string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id));
         }

         return _id;
      }

      /// <summary>Generates random Vector2.</summary>
      /// <param name="min">Smallest possible Vector2 (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">Biggest possible Vector2 (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many Vector2 you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="id">UID to identify the generated result (optional)</param>
      /// <returns>UID of the generator.</returns>
      public string GenerateVector2(Vector2 min, Vector2 max, int number = 1, string id = "")
      {
         string _id = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;

         if (Util.Helper.isEditorMode)
         {
#if UNITY_EDITOR
            generateVector2InEditor(min, max, number);
#endif
         }
         else
         {
            StartCoroutine(Module.ModuleVector2.Generate(min, max, number, PRNG, false, string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id));
         }

         return _id;
      }

      /// <summary>Generates random Vector3.</summary>
      /// <param name="min">Smallest possible Vector3 (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">Biggest possible Vector3 (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many Vector3 you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="id">UID to identify the generated result (optional)</param>
      /// <returns>UID of the generator.</returns>
      public string GenerateVector3(Vector3 min, Vector3 max, int number = 1, string id = "")
      {
         string _id = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;

         if (Util.Helper.isEditorMode)
         {
#if UNITY_EDITOR
            GenerateVector3InEditor(min, max, number);
#endif
         }
         else
         {
            StartCoroutine(Module.ModuleVector3.Generate(min, max, number, PRNG, false, string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id));
         }

         return _id;
      }

      /// <summary>Generates random Vector4.</summary>
      /// <param name="min">Smallest possible Vector4 (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="max">Biggest possible Vector4 (range: -1'000'000'000 - 1'000'000'000)</param>
      /// <param name="number">How many Vector4 you want to generate (range: 1 - 10'000, default: 1, optional)</param>
      /// <param name="id">UID to identify the generated result (optional)</param>
      /// <returns>UID of the generator.</returns>
      public string GenerateVector4(Vector4 min, Vector4 max, int number = 1, string id = "")
      {
         string _id = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;

         if (Util.Helper.isEditorMode)
         {
#if UNITY_EDITOR
            generateVector4InEditor(min, max, number);
#endif
         }
         else
         {
            StartCoroutine(Module.ModuleVector4.Generate(min, max, number, PRNG, false, string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id));
         }

         return _id;
      }

      /// <summary>Gets the remaining quota in bits from the server.</summary>
      public void GetQuota()
      {
         if (Util.Helper.isEditorMode)
         {
#if UNITY_EDITOR
            getQuotaInEditor();
#endif
         }
         else
         {
            StartCoroutine(Module.ModuleQuota.GetQuota());
         }
      }

      /// <summary>Generates random integers with the C#-standard Pseudo-Random-Number-Generator.</summary>
      /// <param name="min">Smallest possible number</param>
      /// <param name="max">Biggest possible number</param>
      /// <param name="number">How many numbers you want to generate (default: 1, optional)</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      /// <returns>List with the generated integers.</returns>
      public System.Collections.Generic.List<int> GenerateIntegerPRNG(int min, int max, int number = 1, int seed = 0)
      {
         return Module.ModuleInteger.GeneratePRNG(min, max, number, seed);
      }

      /// <summary>Generates random floats with the C#-standard Pseudo-Random-Number-Generator.</summary>
      /// <param name="min">Smallest possible number</param>
      /// <param name="max">Biggest possible number</param>
      /// <param name="number">How many numbers you want to generate (default: 1, optional)</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      /// <returns>List with the generated floats.</returns>
      public System.Collections.Generic.List<float> GenerateFloatPRNG(float min, float max, int number = 1, int seed = 0)
      {
         return Module.ModuleFloat.GeneratePRNG(min, max, number, seed);
      }

      /// <summary>Generates a random sequence with the C#-standard Pseudo-Random-Number-Generator.</summary>
      /// <param name="min">Start of the interval</param>
      /// <param name="max">End of the interval</param>
      /// <param name="number">How many numbers you have in the result (max range: max - min, optional)</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      /// <returns>List with the generated sequence.</returns>
      public System.Collections.Generic.List<int> GenerateSequencePRNG(int min, int max, int number = 0, int seed = 0)
      {
         return Module.ModuleSequence.GeneratePRNG(min, max, number, seed);
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
      public System.Collections.Generic.List<string> GenerateStringPRNG(int length, int number = 1, bool digits = true, bool upper = true, bool lower = true, bool unique = false, int seed = 0)
      {
         return Module.ModuleString.GeneratePRNG(length, number, digits, upper, lower, unique, seed);
      }

      /// <summary>Generates random Vector2 with the C#-standard Pseudo-Random-Number-Generator.</summary>
      /// <param name="min">Smallest possible Vector2</param>
      /// <param name="max">Biggest possible Vector2</param>
      /// <param name="number">How many Vector2 you want to generate (default: 1, optional)</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      /// <returns>List with the generated Vector2.</returns>
      public System.Collections.Generic.List<Vector2> GenerateVector2PRNG(Vector2 min, Vector2 max, int number = 1, int seed = 0)
      {
         return Module.ModuleVector2.GeneratePRNG(min, max, number, seed);
      }

      /// <summary>Generates random Vector3 with the C#-standard Pseudo-Random-Number-Generator.</summary>
      /// <param name="min">Smallest possible Vector3</param>
      /// <param name="max">Biggest possible Vector3</param>
      /// <param name="number">How many Vector3 you want to generate (default: 1, optional)</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      /// <returns>List with the generated Vector3.</returns>
      public System.Collections.Generic.List<Vector3> GenerateVector3PRNG(Vector3 min, Vector3 max, int number = 1, int seed = 0)
      {
         return Module.ModuleVector3.GeneratePRNG(min, max, number, seed);
      }

      /// <summary>Generates random Vector4 with the C#-standard Pseudo-Random-Number-Generator.</summary>
      /// <param name="min">Smallest possible Vector4</param>
      /// <param name="max">Biggest possible Vector4</param>
      /// <param name="number">How many Vector4 you want to generate (default: 1, optional)</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      /// <returns>List with the generated Vector4.</returns>
      public System.Collections.Generic.List<Vector4> GenerateVector4PRNG(Vector4 min, Vector4 max, int number = 1, int seed = 0)
      {
         return Module.ModuleVector4.GeneratePRNG(min, max, number, seed);
      }

      #endregion


      #region Event-trigger methods

      private void onGenerateIntegerStart(string id)
      {
         generateCount++;

         OnGenerateIntegerStart?.Invoke(id);
      }

      private void onGenerateIntegerFinished(System.Collections.Generic.List<int> result, string id)
      {
         generateCount--;

         if (!string.IsNullOrEmpty(id) && !AllIntegerResults.ContainsKey(id))
            AllIntegerResults.Add(id, result);

         if (!Util.Helper.isEditorMode)
            OnGenerateCompleted?.Invoke(id, "int");

         OnGenerateIntegerFinished?.Invoke(result, id);
      }

      private void onGenerateFloatStart(string id)
      {
         generateCount++;

         OnGenerateFloatStart?.Invoke(id);
      }

      private void onGenerateFloatFinished(System.Collections.Generic.List<float> result, string id)
      {
         generateCount--;

         if (!string.IsNullOrEmpty(id) && !AllFloatResults.ContainsKey(id))
            AllFloatResults.Add(id, result);

         if (!Util.Helper.isEditorMode)
            OnGenerateCompleted?.Invoke(id, "float");

         OnGenerateFloatFinished?.Invoke(result, id);
      }

      private void onGenerateSequenceStart(string id)
      {
         generateCount++;

         OnGenerateSequenceStart?.Invoke(id);
      }

      private void onGenerateSequenceFinished(System.Collections.Generic.List<int> result, string id)
      {
         generateCount--;

         if (!string.IsNullOrEmpty(id) && !AllSequenceResults.ContainsKey(id))
            AllSequenceResults.Add(id, result);

         if (!Util.Helper.isEditorMode)
            OnGenerateCompleted?.Invoke(id, "sequence");

         OnGenerateSequenceFinished?.Invoke(result, id);
      }

      private void onGenerateStringStart(string id)
      {
         generateCount++;

         OnGenerateStringStart?.Invoke(id);
      }

      private void onGenerateStringFinished(System.Collections.Generic.List<string> result, string id)
      {
         generateCount--;

         if (!string.IsNullOrEmpty(id) && !AllStringResults.ContainsKey(id))
            AllStringResults.Add(id, result);

         if (!Util.Helper.isEditorMode)
            OnGenerateCompleted?.Invoke(id, "string");

         OnGenerateStringFinished?.Invoke(result, id);
      }

      private void onGenerateVector2Start(string id)
      {
         generateCount++;

         OnGenerateVector2Start?.Invoke(id);
      }

      private void onGenerateVector2Finished(System.Collections.Generic.List<Vector2> result, string id)
      {
         generateCount--;

         if (!string.IsNullOrEmpty(id) && !AllVector2Results.ContainsKey(id))
            AllVector2Results.Add(id, result);

         if (!Util.Helper.isEditorMode)
            OnGenerateCompleted?.Invoke(id, "Vector2");

         OnGenerateVector2Finished?.Invoke(result, id);
      }

      private void onGenerateVector3Start(string id)
      {
         generateCount++;

         OnGenerateVector3Start?.Invoke(id);
      }

      private void onGenerateVector3Finished(System.Collections.Generic.List<Vector3> result, string id)
      {
         generateCount--;

         if (!string.IsNullOrEmpty(id) && !AllVector3Results.ContainsKey(id))
            AllVector3Results.Add(id, result);

         if (!Util.Helper.isEditorMode)
            OnGenerateCompleted?.Invoke(id, "Vector3");

         OnGenerateVector3Finished?.Invoke(result, id);
      }

      private void onGenerateVector4Start(string id)
      {
         generateCount++;

         OnGenerateVector4Start?.Invoke(id);
      }

      private void onGenerateVector4Finished(System.Collections.Generic.List<Vector4> result, string id)
      {
         generateCount--;

         if (!string.IsNullOrEmpty(id) && !AllVector4Results.ContainsKey(id))
            AllVector4Results.Add(id, result);

         if (!Util.Helper.isEditorMode)
            OnGenerateCompleted?.Invoke(id, "Vector4");

         OnGenerateVector4Finished?.Invoke(result, id);
      }

      private void onQuotaUpdate(int quota)
      {
         if (!Util.Helper.isEditorMode)
            OnQuotaUpdated?.Invoke(quota);

         OnQuotaUpdate?.Invoke(quota);
      }

      private void onErrorInfo(string info)
      {
         if (!Util.Helper.isEditorMode)
            OnError?.Invoke(info);

         OnErrorInfo?.Invoke(info);
      }

      #endregion


      #region Editor-only methods

#if UNITY_EDITOR

      private void generateIntegerInEditor(int min, int max, int number)
      {
         if (Util.Helper.isEditorMode)
            Module.ModuleInteger.GenerateInEditor(min, max, number, PRNG);
      }

      private void generateFloatInEditor(float min, float max, int number)
      {
         if (Util.Helper.isEditorMode)
            Module.ModuleFloat.GenerateInEditor(min, max, number, PRNG);
      }

      private void generateSequenceInEditor(int min, int max, int number)
      {
         if (Util.Helper.isEditorMode)
         {
            System.Threading.Thread worker = new System.Threading.Thread(() => Module.ModuleSequence.GenerateInEditor(min, max, number, PRNG));
            worker.Start();
         }
      }

      private void generateStringInEditor(int length, int number, bool digits, bool upper, bool lower, bool unique)
      {
         if (Util.Helper.isEditorMode)
         {
            System.Threading.Thread worker = new System.Threading.Thread(() => Module.ModuleString.GenerateInEditor(length, number, digits, upper, lower, unique, PRNG));
            worker.Start();
         }
      }

      private void generateVector2InEditor(Vector2 min, Vector2 max, int number)
      {
         if (Util.Helper.isEditorMode)
            Module.ModuleVector2.GenerateInEditor(min, max, number, PRNG);
      }

      private void GenerateVector3InEditor(Vector3 min, Vector3 max, int number)
      {
         if (Util.Helper.isEditorMode)
            Module.ModuleVector3.GenerateInEditor(min, max, number, PRNG);
      }

      private void generateVector4InEditor(Vector4 min, Vector4 max, int number = 1)
      {
         if (Util.Helper.isEditorMode)
            Module.ModuleVector4.GenerateInEditor(min, max, number, PRNG);
      }

      private void getQuotaInEditor()
      {
         if (Util.Helper.isEditorMode)
         {
            System.Threading.Thread worker = new System.Threading.Thread(Module.ModuleQuota.GetQuotaInEditor);
            worker.Start();
         }
      }

#endif

      #endregion
   }

   [System.Serializable]
   public class OnGenerateCompleted : UnityEngine.Events.UnityEvent<string, string>
   {
   }

   [System.Serializable]
   public class OnQuotaUpdated : UnityEngine.Events.UnityEvent<int>
   {
   }

   [System.Serializable]
   public class OnError : UnityEngine.Events.UnityEvent<string>
   {
   }
}
// © 2016-2021 crosstales LLC (https://www.crosstales.com)