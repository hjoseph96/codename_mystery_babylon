using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Rendering.Light.Shadow {

    public static class LegacyCPU {

        public static Pair2 pair = Pair2.Zero();
        public static Vector2 projectedMiddle, projectedLeft, projectedRight, outerLeft, outerRight;
        public static Vector2 edgeAWorld, edgeBWorld, edgeALocal, edgeBLocal;
        public static Vector2 closestPoint;
        public static Vector2 edgeAVector, edgeBVector;

        public static float angleA, angleB;
        public static float rotA, rotB;

        public static Vector2 middle;

        public static float sqrt;
 
        public static void Draw(List<Polygon2> polygons, float distance, float translucency) {
            if (polygons == null) {
                return;
            }

            Light2D light = ShadowEngine.light;
            Vector2 position = ShadowEngine.lightOffset + ShadowEngine.objectOffset;

            float shadowDistance = ShadowEngine.lightSize;
            float lightSizeSquare = (ShadowEngine.lightSize * ShadowEngine.lightSize) * 0.5f;

            float outerAngle = Mathf.Deg2Rad * light.outerAngle;
            bool drawInside = (ShadowEngine.lightDrawAbove == false);
            //bool culling = true;

            UVRect penumbraRect = ShadowEngine.Penumbra.uvRect;
            UVRect fillRect = ShadowEngine.FillBlack.uvRect;

            int PolygonCount = polygons.Count;

            Vector2 draw = ShadowEngine.drawOffset;

            if (translucency > 0) {
                GL.Color(new Color(0, 0, 0, 1 - translucency));
                
                fillRect = ShadowEngine.FillWhite.uvRect;
                outerAngle = 0;
            }

            if (distance > 0) {
                shadowDistance = distance;
                outerAngle = 0;
                //culling = false;
            }

            for(int i = 0; i < PolygonCount; i++) {
                Vector2[] pointsList = polygons[i].points;
                int pointsCount = pointsList.Length;
            
                for(int x = 0; x < pointsCount; x++) {
                    int next = (x + 1) % pointsCount;
                    
                    pair.A = pointsList[x];
                    pair.B = pointsList[next];

                    edgeALocal.x = pair.A.x;
                    edgeALocal.y = pair.A.y;

                    edgeBLocal.x = pair.B.x;
                    edgeBLocal.y = pair.B.y;

                    edgeAWorld.x = edgeALocal.x + position.x;
                    edgeAWorld.y = edgeALocal.y + position.y;

                    edgeBWorld.x = edgeBLocal.x + position.x;
                    edgeBWorld.y = edgeBLocal.y + position.y;

                    // Need to test it
                    closestPoint = Math2D.ClosestPointOnLine(edgeAWorld, edgeBWorld);
                
                    if (closestPoint.x * closestPoint.x > lightSizeSquare || closestPoint.y * closestPoint.y > lightSizeSquare) {
                        continue;
                    }

                    middle.x = (edgeAWorld.x + edgeBWorld.x) / 2;
                    middle.y = (edgeAWorld.y + edgeBWorld.y) / 2;

                    // Edge Rotation to the light
                    //float lightDirection = (float)Math.Atan2(middle.x, middle.y) * Mathf.Rad2Deg;
                    //float EdgeDirection = (float)Math.Atan2(edgeALocal.y - edgeBLocal.y, edgeALocal.x - edgeBLocal.x) * Mathf.Rad2Deg - 180;

                    //lightDirection -= EdgeDirection;
                    //lightDirection = (lightDirection + 720) % 360;
                    
                    // Culling only if outside
                    /*
                    if (culling && drawInside == false) {
                        if (lightDirection < 180) {
                            // Failing
                            //continue;
                        }
                    }
                    */

                    sqrt = Mathf.Sqrt(edgeAWorld.sqrMagnitude);
                    edgeAVector.x = edgeAWorld.x / sqrt;
                    edgeAVector.y = edgeAWorld.y / sqrt;

                    sqrt = Mathf.Sqrt(edgeBWorld.sqrMagnitude);
                    edgeBVector.x = edgeBWorld.x / sqrt;
                    edgeBVector.y = edgeBWorld.y / sqrt;

                    projectedRight.x = edgeAWorld.x + edgeAVector.x * shadowDistance;
                    projectedRight.y = edgeAWorld.y + edgeAVector.y * shadowDistance;

                    projectedLeft.x = edgeBWorld.x + edgeBVector.x * shadowDistance;
                    projectedLeft.y = edgeBWorld.y + edgeBVector.y * shadowDistance;

                    if (outerAngle > 0) {
                        angleA = (float)System.Math.Atan2 (edgeAWorld.y, edgeAWorld.x);
                        angleB = (float)System.Math.Atan2 (edgeBWorld.y, edgeBWorld.x);

                        rotA = angleA - outerAngle;
                        rotB = angleB + outerAngle; 

                        outerRight.x = edgeAWorld.x + Mathf.Cos(rotA) * shadowDistance;
                        outerRight.y = edgeAWorld.y + Mathf.Sin(rotA) * shadowDistance;

                        outerLeft.x = edgeBWorld.x + Mathf.Cos(rotB) * shadowDistance;
                        outerLeft.y = edgeBWorld.y + Mathf.Sin(rotB) * shadowDistance;

                        // Right Penumbra
                        GL.TexCoord3(penumbraRect.x0, penumbraRect.y0, 0);
                        GL.Vertex3(draw.x + edgeAWorld.x, draw.y + edgeAWorld.y, 0);

                        GL.TexCoord3(penumbraRect.x1, penumbraRect.y0, 0);
                        GL.Vertex3(draw.x + outerRight.x, draw.y + outerRight.y, 0);
                        
                        GL.TexCoord3(penumbraRect.x0, penumbraRect.y1, 0);
                        GL.Vertex3(draw.x + projectedRight.x, draw.y + projectedRight.y, 0);
                        
                        // Left Penumbra
                        GL.TexCoord3(penumbraRect.x0, penumbraRect.y0, 0);
                        GL.Vertex3(draw.x + edgeBWorld.x, draw.y + edgeBWorld.y, 0);

                        GL.TexCoord3(penumbraRect.x1, penumbraRect.y0, 0);
                        GL.Vertex3(draw.x + outerLeft.x, draw.y + outerLeft.y, 0);
                        
                        GL.TexCoord3(penumbraRect.x0, penumbraRect.y1, 0);
                        GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);
                    }

                    GL.TexCoord3(fillRect.x0, fillRect.y0, 0);

                    // Right Fin
                    GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);
                    GL.Vertex3(draw.x + projectedRight.x, draw.y + projectedRight.y, 0);
                    GL.Vertex3(draw.x + edgeAWorld.x, draw.y + edgeAWorld.y, 0);
                    
                    // Left Fin
                    GL.Vertex3(draw.x + edgeAWorld.x, draw.y + edgeAWorld.y, 0);
                    GL.Vertex3(draw.x + edgeBWorld.x, draw.y + edgeBWorld.y, 0);
                    GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);

                    // Detailed Shadow
                    closestPoint = Math2D.ClosestPointOnLine(projectedLeft, projectedRight);

                    sqrt = Mathf.Sqrt(closestPoint.x * closestPoint.x + closestPoint.y * closestPoint.y);
                    closestPoint.x = closestPoint.x / sqrt;
                    closestPoint.y = closestPoint.y / sqrt;

                    projectedMiddle.x = middle.x + closestPoint.x * shadowDistance;
                    projectedMiddle.y = middle.y + closestPoint.y * shadowDistance;                        
                                
                    // Middle Fin
                    GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);
                    GL.Vertex3(draw.x + projectedRight.x, draw.y + projectedRight.y, 0);
                    GL.Vertex3(draw.x + projectedMiddle.x, draw.y + projectedMiddle.y, 0); 
                }
            }

            if (translucency > 0) {
                GL.Color(Color.white);
            }
        }
    }
}