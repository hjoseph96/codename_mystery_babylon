using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightSettings;

namespace Rendering.Light {

    public struct UVRect {
        public float x0;
        public float y0;
        public float x1;
        public float y1;

        public UVRect(float x0, float y0, float x1, float y1) {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
        }
    }

    public static class ShadowEngine {
        public static Light2D light;

        public static Vector2 lightOffset = Vector2.zero;
        public static Vector2 drawOffset = Vector2.zero;

        public static float lightSize = 0;
        public static bool lightDrawAbove = false;

        public static Vector2 objectOffset = Vector2.zero;
        public static float shadowDistanceMax = 1;
        public static float shadowDistanceMin = 0;

        public static Sprite spriteProjection = null;

        public static bool perpendicularIntersection;
        public static int effectLayer = 0;

        // Layer Effect
        public static List<List<Polygon2>> effectPolygons = new List<List<Polygon2>>();

        // public static float shadowDistance;
        
        public static bool softShadow = false;

        public static int drawMode = 0;

        public const int DRAW_MODE_LEGACY_CPU = 0;
        public const int DRAW_MODE_LEGACY_GPU = 4;
        public const int DRAW_MODE_SOFT_CONVEX = 1;
        public const int DRAW_MODE_SOFT_VERTEX = 5;
        public const int DRAW_MODE_PERPENDICULAR = 2;
        public const int DRAW_MODE_SPRITEPROJECTION = 3;

        public static Material GetMaterial() {
            Material material;

            if (ShadowEngine.softShadow) {
                material = Lighting2D.materials.GetSoftShadow();
                material.SetFloat("_CoreSize", light.coreSize);

            } else {
                material = Lighting2D.materials.GetAtlasMaterial();
            }

            return(material);
        }
        
        public static void Draw(List<Polygon2> polygons, float shadowDistance, float shadowTranslucency) {
            switch(ShadowEngine.drawMode) {
                case DRAW_MODE_LEGACY_CPU:
                    Shadow.LegacyCPU.Draw(polygons, shadowDistance, shadowTranslucency);
                break;

                case DRAW_MODE_LEGACY_GPU:
                    Shadow.LegacyGPU.Draw(polygons, shadowDistance, shadowTranslucency);
                break;
            
                case DRAW_MODE_SOFT_CONVEX:
                case DRAW_MODE_SOFT_VERTEX:
                    // Does not support Shadow Distance
                    Shadow.Soft.Draw(polygons, shadowTranslucency); 
                break;

                case DRAW_MODE_PERPENDICULAR:
                    // Does not support Translucency + Shadow Distance after intersection)
                    Shadow.PerpendicularIntersection.Draw(polygons, shadowDistance);
                break;

                case DRAW_MODE_SPRITEPROJECTION:
                    Shadow.SpriteProjection.Draw(polygons, shadowDistance, shadowTranslucency);
                break;

            }
        }

        public static void SetPass(Light2D lightObject, LayerSetting layer) {
            light = lightObject;
            lightSize = Mathf.Sqrt(light.size * light.size + light.size * light.size);
            lightOffset = -light.transform2D.position;

            effectLayer = layer.shadowEffectLayer;

            objectOffset = Vector2.zero;

            effectPolygons.Clear();

            softShadow = layer.shadowEffect == LightLayerShadowEffect.SoftConvex || layer.shadowEffect == LightLayerShadowEffect.SoftVertex;

            if (lightObject.IsPixelPerfect()) {
                Camera camera = Camera.main;

                Vector2 pos = LightingPosition.GetPosition2D(-camera.transform.position);

                drawOffset = light.transform2D.position + pos;
            } else {
                drawOffset = Vector2.zero;
            }

            switch(layer.shadowEffect) {
                case LightLayerShadowEffect.PerpendicularProjection:
                    drawMode = DRAW_MODE_PERPENDICULAR;
                    GenerateEffectLayers();
                break;

                case LightLayerShadowEffect.SoftConvex:
                    drawMode = DRAW_MODE_SOFT_CONVEX;
                break;

                case LightLayerShadowEffect.SoftVertex:
                    drawMode = DRAW_MODE_SOFT_VERTEX;
                break;

                case LightLayerShadowEffect.SpriteProjection:
                    drawMode = DRAW_MODE_SPRITEPROJECTION;
                break;

                case LightLayerShadowEffect.LegacyCPU:
                    drawMode = DRAW_MODE_LEGACY_CPU;
                break;

                case LightLayerShadowEffect.LegacyGPU:
                    drawMode = DRAW_MODE_LEGACY_GPU;
                break;
            }
        }

        public static void GenerateEffectLayers() {
            int layerID = (int)ShadowEngine.effectLayer;

            foreach(LightCollider2D c in LightCollider2D.GetShadowList((layerID))) {
                List<Polygon2> polygons = c.mainShape.GetPolygonsWorld();

                if (polygons == null) {
                    continue;
                }
    
                if (c.InLight(light)) {
                    effectPolygons.Add(polygons);
                }
            }
        }
        
        public static void Prepare(Light2D light) {
            FillWhite.Calculate();
            FillBlack.Calculate();

            Penumbra.Calculate();

            lightDrawAbove = (light.whenInsideCollider == Light2D.WhenInsideCollider.DrawAbove);
        }

      
        static public class Penumbra {
            static public UVRect uvRect = new UVRect();
            static public Vector2 size;
    
            static Sprite sprite = null;

            public static void Calculate() {
                LightingManager2D manager = LightingManager2D.Get();
                
                sprite = Lighting2D.materials.GetAtlasPenumbraSprite();

                if (sprite == null || sprite.texture == null) {
                    return;
                }

                Rect spriteRect = sprite.textureRect;
                int atlasSize = AtlasSystem.Texture.atlasSize / 2;

                uvRect.x0 = spriteRect.x / sprite.texture.width;
                uvRect.y0 = spriteRect.y / sprite.texture.height;

                size.x = ((float)spriteRect.width) / sprite.texture.width;
                size.y = ((float)spriteRect.height) / sprite.texture.height;

                uvRect.x1 = spriteRect.width / 2 / sprite.texture.width;
                uvRect.y1 = spriteRect.height / 2 / sprite.texture.height;
                uvRect.x1 += uvRect.x0;
                uvRect.y1 += uvRect.y0;

                uvRect.x0 += 1f / atlasSize;
                uvRect.y0 += 1f / atlasSize;
                uvRect.x1 -= 1f / atlasSize;
                uvRect.y1 -= 1f / atlasSize;
            }
        }

        public class FillWhite {
            static public UVRect uvRect = new UVRect();

            static public void Calculate() {
                LightingManager2D manager = LightingManager2D.Get();
                
                Sprite fillSprite = Lighting2D.materials.GetAtlasWhiteMaskSprite();

                if (fillSprite != null) {
                    Rect spriteRect = fillSprite.textureRect;

                    uvRect.x0 = spriteRect.x / fillSprite.texture.width;
                    uvRect.y0 = spriteRect.y / fillSprite.texture.height;
                    uvRect.x1 = spriteRect.width / fillSprite.texture.width;
                    uvRect.y1 = spriteRect.height / fillSprite.texture.height;

                    uvRect.x0 += uvRect.x1 / 2;
                    uvRect.y0 += uvRect.x1 / 2;
                }
            }
        }

        public class FillBlack {
            static public UVRect uvRect = new UVRect();

            static public void Calculate() {
                LightingManager2D manager = LightingManager2D.Get();
                
                Sprite fillSprite = Lighting2D.materials.GetAtlasBlackMaskSprite();

                if (fillSprite != null) {
                    Rect spriteRect = fillSprite.textureRect;

                    uvRect.x0 = spriteRect.x / fillSprite.texture.width;
                    uvRect.y0 = spriteRect.y / fillSprite.texture.height;
                    uvRect.x1 = spriteRect.width / fillSprite.texture.width;
                    uvRect.y1 = spriteRect.height / fillSprite.texture.height;

                    uvRect.x0 += uvRect.x1 / 2;
                    uvRect.y0 += uvRect.x1 / 2;
                }
            }
        }   
    }
}