using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Grids2D {
    public static class Drawing {

        static Rect dummyRect = new Rect();

        public static GameObject CreateSurface(string name, Vector3[] surfPoints, int[] indices, Material material, int sortingOrder) {
            return CreateSurface(name, surfPoints, indices, material, dummyRect, Vector2.one, Vector2.zero, 0, false, sortingOrder);
        }

        /// <summary>
        /// Rotates one point around another
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="centerPoint">The centre point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, float angleInDegrees) {
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            float cosTheta = Mathf.Cos(angleInRadians);
            float sinTheta = Mathf.Sin(angleInRadians);
            return new Vector2(cosTheta * (pointToRotate.x - centerPoint.x) - sinTheta * (pointToRotate.y - centerPoint.y) + centerPoint.x,
                            sinTheta * (pointToRotate.x - centerPoint.x) + cosTheta * (pointToRotate.y - centerPoint.y) + centerPoint.y);
        }

        public static GameObject CreateSurface(string name, Vector3[] surfPoints, int[] indices, Material material, Rect rect, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace, int sortingOrder) {

            GameObject hexa = new GameObject(name);
            MeshFilter mf = hexa.AddComponent<MeshFilter>();
            MeshRenderer mr = hexa.AddComponent<MeshRenderer>();
            hexa.MarkForDisposal(); // | HideFlags.HideInHierarchy;
            hexa.hideFlags |= HideFlags.HideInHierarchy;

            Mesh mesh = new Mesh();
            mesh.MarkForDisposal();
            mesh.vertices = surfPoints;
            mesh.triangles = indices;
            // uv mapping
            if (material.HasProperty("_MainTex") && material.mainTexture != null) {
                Vector2[] uv = new Vector2[surfPoints.Length];
                for (int k = 0; k < uv.Length; k++) {
                    Vector2 coor = surfPoints[k];
                    Vector2 normCoor;
                    if (rotateInLocalSpace) {
                        normCoor = new Vector2((coor.x - rect.xMin) / rect.width, (coor.y - rect.yMin) / rect.height);
                        if (textureRotation != 0) {
                            normCoor = RotatePoint(normCoor, Misc.Vector2half, textureRotation);
                        }
                        normCoor.x = 0.5f + (normCoor.x - 0.5f) / textureScale.x;
                        normCoor.y = 0.5f + (normCoor.y - 0.5f) / textureScale.y;
                        normCoor -= textureOffset;
                    } else {
                        coor.x /= textureScale.x;
                        coor.y /= textureScale.y;
                        if (textureRotation != 0) {
                            coor = RotatePoint(coor, Vector2.zero, textureRotation);
                        }
                        coor -= textureOffset;
                        normCoor = new Vector2((coor.x - rect.xMin) / rect.width, (coor.y - rect.yMin) / rect.height);
                    }
                    uv[k] = normCoor;
                }
                mesh.uv = uv;
            }
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.mesh = mesh;
            mr.sharedMaterial = material;
            mr.sortingOrder = sortingOrder;
            return hexa;

        }

        public static GameObject CreateSurface(string name, Vector3[] surfPoints, int[] indices, Material material, Rect targetRect, Rect sourceRect) {

            GameObject hexa = new GameObject(name);
            MeshFilter mf = hexa.AddComponent<MeshFilter>();
            MeshRenderer mr = hexa.AddComponent<MeshRenderer>();
            hexa.MarkForDisposal();

            Mesh mesh = new Mesh();
            mesh.MarkForDisposal();
            mesh.vertices = surfPoints;
            mesh.triangles = indices;
            // uv mapping
            if (material.mainTexture != null) {
                sourceRect.xMin /= material.mainTexture.width;
                sourceRect.yMin /= material.mainTexture.height;
                sourceRect.xMax /= material.mainTexture.width;
                sourceRect.yMax /= material.mainTexture.height;
                Vector2[] uv = new Vector2[surfPoints.Length];
                for (int k = 0; k < uv.Length; k++) {
                    Vector2 coor = surfPoints[k];
                    Vector2 normCoor = new Vector2((sourceRect.xMin / sourceRect.width) + ((coor.x - targetRect.xMin) / targetRect.width) / sourceRect.width, (sourceRect.yMin / sourceRect.height) + ((coor.y - targetRect.yMin + sourceRect.yMin) / targetRect.height) / sourceRect.height);
                    uv[k] = normCoor;
                }
                mesh.uv = uv;
            }
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
#if !UNITY_5_5_OR_NEWER
			mesh.Optimize ();
#endif

            mf.mesh = mesh;
            mr.sharedMaterial = material;
            return hexa;

        }
    }


}



