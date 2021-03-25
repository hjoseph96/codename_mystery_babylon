using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Day {

    public static class Shadow {

        public static float direction;
        public static float directionCos;
        public static float directionSin;
        public static float shadowDistance;

        public static Vector2 pointA, pointB, pointAOffset, pointBOffset;

        static public void Begin() {
            Lighting2D.materials.GetDayCPUShadow().SetPass(0);
            GL.Begin(GL.QUADS);

            direction = -Lighting2D.DayLightingSettings.direction * Mathf.Deg2Rad;
            shadowDistance = Lighting2D.DayLightingSettings.height;

            directionCos = Mathf.Cos(direction);
            directionSin = Mathf.Sin(direction);
        }

        static public void End() {
            GL.End();
        }

        static public void Draw(DayLightCollider2D id, Vector2 position) {
            if (id.mainShape.height <= 0 || id.shadowTranslucency >= 1) {
                return;
            }
        
            if (id.InAnyCamera() == false) {
                return;
            }

            GL.Color(new Color(0, 0, 0, 1 - id.shadowTranslucency));

            float distance = shadowDistance * id.mainShape.height;
            float cosShadow = directionCos * distance;
            float sinShadow = directionSin * distance;

            int shapeCount = id.shapes.Count;
            for(int s = 0; s < shapeCount; s++) {
                DayLightColliderShape shape = id.shapes[s];
                
                List<Polygon2> polygons = shape.GetPolygonsWorld();

                int polygonCount = polygons.Count;
                for(int p = 0; p < polygonCount; p++) {
                    Polygon2 polygon = polygons[p];

                    int pointsCount = polygon.points.Length;
                    for(int i = 0; i < pointsCount; i++ ) {
                        pointA = polygon.points[i];
                        pointA.x += position.x;
                        pointA.y += position.y;

                        pointB = polygon.points[(i + 1) % pointsCount];
                        pointB.x += position.x;
                        pointB.y += position.y;

                        pointAOffset.x = pointA.x + cosShadow;
                        pointAOffset.y = pointA.y + sinShadow;
       
                        pointBOffset.x = pointB.x + cosShadow;
                        pointBOffset.y = pointB.y + sinShadow;

                        GL.Vertex3(pointA.x, pointA.y, 0);
                        GL.Vertex3(pointAOffset.x, pointAOffset.y, 0);
                        GL.Vertex3(pointBOffset.x, pointBOffset.y, 0);
                        GL.Vertex3(pointB.x, pointB.y, 0);
                    }   
                }
            }
        }

        static public void DrawTilemap(DayLightTilemapCollider2D id, Vector2 position) {
            //if (id.InAnyCamera() == false) {
            //     continue;
            //}

            if (id.height <= 0) {
                return;
            }

            float distance = shadowDistance * id.height;
            float cosShadow = directionCos * distance;
            float sinShadow = directionSin * distance;

            GL.Color(new Color(0, 0, 0, 1));

            foreach(DayLightingTile dayTile in id.dayTiles) {
                if (dayTile.InAnyCamera() == false) {
                    continue;
                }
                
                List<Polygon2> polygons = dayTile.polygons;
                foreach(Polygon2 polygon in polygons) {
                    int pointsCount = polygon.points.Length;

                    for(int i = 0; i < pointsCount; i++ ) {
                        pointA = polygon.points[i];
                        pointA.x += position.x;
                        pointA.y += position.y;

                        pointB = polygon.points[(i + 1) % pointsCount];
                        pointB.x += position.x;
                        pointB.y += position.y;

                        pointAOffset.x = pointA.x + cosShadow;
                        pointAOffset.y = pointA.y + sinShadow;
       
                        pointBOffset.x = pointB.x + cosShadow;
                        pointBOffset.y = pointB.y + sinShadow;

                        GL.Vertex3(pointA.x, pointA.y, 0);
                        GL.Vertex3(pointAOffset.x, pointAOffset.y, 0);
                        GL.Vertex3(pointBOffset.x, pointBOffset.y, 0);
                        GL.Vertex3(pointB.x, pointB.y, 0);
                    }   
                }
            }
        }
    }
}