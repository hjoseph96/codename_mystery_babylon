using UnityEngine;

namespace Crosstales.TrueRandom
{
   public delegate void GenerateIntegerStart(string id);

   public delegate void GenerateIntegerFinished(System.Collections.Generic.List<int> result, string id);

   public delegate void GenerateFloatStart(string id);

   public delegate void GenerateFloatFinished(System.Collections.Generic.List<float> result, string id);

   public delegate void GenerateSequenceStart(string id);

   public delegate void GenerateSequenceFinished(System.Collections.Generic.List<int> result, string id);

   public delegate void GenerateStringStart(string id);

   public delegate void GenerateStringFinished(System.Collections.Generic.List<string> result, string id);

   public delegate void GenerateVector2Start(string id);

   public delegate void GenerateVector2Finished(System.Collections.Generic.List<Vector2> result, string id);

   public delegate void GenerateVector3Start(string id);

   public delegate void GenerateVector3Finished(System.Collections.Generic.List<Vector3> result, string id);

   public delegate void GenerateVector4Start(string id);

   public delegate void GenerateVector4Finished(System.Collections.Generic.List<Vector4> result, string id);

   public delegate void ErrorInfo(string error);

   public delegate void QuotaUpdate(int quota);
}
// © 2020-2021 crosstales LLC (https://www.crosstales.com)