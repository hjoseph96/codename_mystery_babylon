using UnityEngine;

namespace Crosstales.TrueRandom
{
   /// <summary>Various extension methods.</summary>
   public static class ExtensionMethods
   {
      /// <summary>
      /// Extension method for Vector3.
      /// Convert it to a Quaternion.
      /// </summary>
      /// <param name="eulerAngle">Vector3-instance to convert.</param>
      /// <returns>Quaternion from euler angles.</returns>
      public static Quaternion ToQuaternion(this Vector3 eulerAngle)
      {
         return Quaternion.Euler(eulerAngle);
      }

      /// <summary>
      /// Extension method for Vector4.
      /// Convert it to a Quaternion.
      /// </summary>
      /// <param name="angle">Vector4-instance to convert.</param>
      /// <returns>Quaternion from Vector4.</returns>
      public static Quaternion ToQuaternion(this Vector4 angle)
      {
         return new Quaternion(angle.x, angle.y, angle.z, angle.w);
      }

      /// <summary>
      /// Extension method for Vector3.
      /// Convert it to a Color.
      /// </summary>
      /// <param name="rgb">Vector3-instance to convert (RGB = xyz).</param>
      /// <param name="alpha">Alpha-value of the color (default: 1, optional).</param>
      /// <returns>Color from RGB.</returns>
      public static Color ToColorRGB(this Vector3 rgb, float alpha = 1f)
      {
         return new Color(Mathf.Clamp01(rgb.x), Mathf.Clamp01(rgb.y), Mathf.Clamp01(rgb.z), Mathf.Clamp01(alpha));
      }

      /// <summary>
      /// Extension method for Vector4.
      /// Convert it to a Color.
      /// </summary>
      /// <param name="rgba">Vector4-instance to convert (RGBA = xyzw).</param>
      /// <returns>Color from RGBA.</returns>
      public static Color ToColorRGBA(this Vector4 rgba)
      {
         return new Color(Mathf.Clamp01(rgba.x), Mathf.Clamp01(rgba.y), Mathf.Clamp01(rgba.z), Mathf.Clamp01(rgba.w));
      }
   }
}
// © 2016-2021 crosstales LLC (https://www.crosstales.com)