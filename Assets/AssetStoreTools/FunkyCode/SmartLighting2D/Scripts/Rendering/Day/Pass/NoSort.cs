using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightingSettings;
using LightSettings;

namespace Rendering.Day {

    public static class NoSort {

        static public void Draw(Pass pass) {
            bool drawShadows = pass.layer.type != LayerType.MaskOnly;
            bool drawMask = pass.layer.type != LayerType.ShadowsOnly;

            if (drawShadows) {
                Day.Shadow.Begin();

                Shadow.DrawCollider(pass);
                Shadow.DrawTilemapCollider(pass);
                
                Day.Shadow.End();

                Shadow.DrawSprite(pass);
            }

            if (drawMask) {
                Mask.DrawCollider(pass);

                Mask.DrawTilemap(pass);
            }
        }

        public static class Mask {

            public static void DrawCollider(Pass pass) {
                // Regular Sprites
                SpriteRenderer2D.currentTexture = null;

                for(int i = 0; i < pass.colliderCount; i++) {
                    DayLightCollider2D id = pass.colliderList[i];

                    if (id.maskLayer != pass.layerId) {
                        continue;
                    }

                    switch(id.mainShape.maskType) {
                        case DayLightCollider2D.MaskType.Sprite:
                            SpriteRenderer2D.Draw(id, pass.offset);
                        break;
                    }
                }

                if (SpriteRenderer2D.currentTexture != null) {
                    GL.End();

                    SpriteRenderer2D.currentTexture = null;
                }

                // Bumped Sprites
                for(int i = 0; i < pass.colliderCount; i++) {
                    DayLightCollider2D id = pass.colliderList[i];

                    if (id.maskLayer != pass.layerId) {
                        continue;
                    }

                    switch(id.mainShape.maskType) {
                        case DayLightCollider2D.MaskType.BumpedSprite:
                            SpriteRenderer2D.DrawBumped(id, pass.offset);
                        break;
                    }
                }
            }

            public static void DrawTilemap(Pass pass) {
                for(int i = 0; i < pass.tilemapColliderCount; i++) {
                    DayLightTilemapCollider2D id = pass.tilemapColliderList[i];

                    if (id.maskLayer != pass.layerId) {
                        continue;
                    }

                    SpriteRenderer2D.DrawTilemap(id, pass.offset);
                }
            }

        }

        public static class Shadow {
            public static void DrawCollider(Pass pass) {
                for(int i = 0; i < pass.colliderCount; i++) {
                    DayLightCollider2D id = pass.colliderList[i];
                    
                    if (id.shadowLayer != pass.layerId) {
                        continue;
                    }

                    switch(id.mainShape.shadowType) {
                        case DayLightCollider2D.ShadowType.SpritePhysicsShape:
                        case DayLightCollider2D.ShadowType.Collider2D:
                            Day.Shadow.Draw(id, pass.offset);  
                        break;
                    }             
                }
            }

            public static void DrawTilemapCollider(Pass pass) {
                for(int i = 0; i < pass.tilemapColliderCount; i++) {
                    DayLightTilemapCollider2D id = pass.tilemapColliderList[i];
                    
                    if (id.shadowLayer != pass.layerId) {
                        continue;
                    }

                    Day.Shadow.DrawTilemap(id, pass.offset);                
                }
            }

            public static void DrawSprite(Pass pass) {
                SpriteRendererShadow.Begin(pass.offset);

                for(int i = 0; i < pass.colliderCount; i++) {
                    DayLightCollider2D id = pass.colliderList[i];

                    if (id.shadowLayer != pass.layerId) {
                        continue;
                    }

                    switch(id.mainShape.shadowType) {
                        case DayLightCollider2D.ShadowType.SpriteProjection:
                            SpriteRendererShadow.DrawProjection(id);
                        break;

                        case DayLightCollider2D.ShadowType.SpriteProjectionShape:
                        case DayLightCollider2D.ShadowType.SpriteProjectionCollider:
                            SpriteRendererShadow.DrawProjectionShape(id);
                        break;

                        case DayLightCollider2D.ShadowType.SpriteOffset:
                            SpriteRendererShadow.Draw(id);
                        break;
                    }
                }

                if (SpriteRendererShadow.currentTexture != null) {
                    SpriteRendererShadow.End();
                }
            }
        }
    }
}