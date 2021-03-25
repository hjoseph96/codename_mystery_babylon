using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightTilemapCollider;
using LightSettings;

namespace Rendering.Light {

    public static class NoSort {

        public static void Draw(Pass pass) {
            if (pass.drawShadows) {
                Shadows.Draw(pass);
            }

            if (pass.drawMask) {
                Masks.Draw(pass);
            }
        }

        public static class Shadows {

            public static void Draw(Pass pass) {

                switch(ShadowEngine.drawMode) {
                    case ShadowEngine.DRAW_MODE_SPRITEPROJECTION:
                        DrawCollider(pass);
                    break;

                    case ShadowEngine.DRAW_MODE_LEGACY_GPU:
                        Lighting2D.materials.GetLegacyGPUShadow().SetPass(0);

                        GL.Begin(GL.TRIANGLES);

                        DrawCollider(pass);
                        DrawTilemapCollider(pass);
                
                        GL.End();

                    break;

                    default:
                        ShadowEngine.GetMaterial().SetPass(0);

                        GL.Begin(GL.TRIANGLES);

                        DrawCollider(pass);
                        DrawTilemapCollider(pass);
                
                        GL.End();

                    break;

                }
                
            }

            private static void DrawCollider(Pass pass) {
                int colliderCount = pass.layerShadowList.Count;

                if (colliderCount < 1) {
                    return;
                }

                for(int id = 0; id < colliderCount; id++) {
                    LightCollider2D collider = pass.layerShadowList[id];

                    if (collider.ShadowDisabled()) {
                        continue;
                    }

                    // Shadow Projection Sprite
                    if (ShadowEngine.drawMode == ShadowEngine.DRAW_MODE_SPRITEPROJECTION) {
                        ShadowEngine.spriteProjection = collider.mainShape.spriteShape.GetOriginalSprite();
                        ShadowEngine.shadowDistanceMin = collider.shadowDistanceMin;
                        ShadowEngine.shadowDistanceMax = collider.shadowDistanceMax;
                    }
                   
                    Shadow.Shape.Draw(pass.light, collider);
                }
            }

            private static void DrawTilemapCollider(Pass pass) {
                #if UNITY_2017_4_OR_NEWER

                    for(int id = 0; id < pass.tilemapList.Count; id++) {
                        LightTilemapCollider2D tilemap = pass.tilemapList[id];

                        if (tilemap.shadowLayer != pass.layerID) {
                            continue;
                        }

                        bool shadowsDisabled = tilemap.ShadowsDisabled();
                        if (shadowsDisabled) {
                            continue;
                        }

                        if (tilemap.InLight(pass.light) == false) {
                           continue;
                        }

                        switch(tilemap.mapType) {

                            case MapType.UnityRectangle:
                            case MapType.UnityIsometric:
                            case MapType.UnityHexagon:

                                LightTilemapCollider.Base baseTilemap = tilemap.GetCurrentTilemap();

                                switch(baseTilemap.shadowType) {
                                    case ShadowType.SpritePhysicsShape:
                                    case ShadowType.Grid:
                                        Shadow.UnityTilemap.Draw(pass.light, tilemap);
                                    break;

                                    case ShadowType.CompositeCollider:
                                        // Only Rectangle?
                                        Shadow.TilemapCollider.Rectangle.Draw(pass.light, tilemap);
                                    break;
                                }
                                
                            break;

                            case MapType.SuperTilemapEditor:

                                 switch(tilemap.superTilemapEditor.shadowTypeSTE) {

                                    case SuperTilemapEditorSupport.TilemapCollider2D.ShadowType.Grid:
                                    case SuperTilemapEditorSupport.TilemapCollider2D.ShadowType.TileCollider:
                                         SuperTilemapEditorSupport.Light.Shadow.Grid.Draw(pass.light, tilemap);
                                    break;
                                     
                                    case SuperTilemapEditorSupport.TilemapCollider2D.ShadowType.Collider:
                                        SuperTilemapEditorSupport.Light.Shadow.Collider.Draw(pass.light, tilemap);
                                    break;
                                 }
                               
                            break;
                        }
                        
                        
                    }
                #endif 
            }
        }

        private static class Masks {

           static public void Draw(Pass pass) {
                Material maskMaterial = Lighting2D.materials.GetMask();
                maskMaterial.mainTexture = null;
                maskMaterial.SetPass(0);

                GL.Begin(GL.TRIANGLES);

                    DrawCollider(pass);

                    DrawTilemapCollider(pass);

                GL.End();

                DrawMesh(pass);

                DrawSprite(pass);

                DrawTilemapSprite(pass);
            }

            private static void DrawCollider(Pass pass) {
                int colliderCount = pass.layerMaskList.Count;

                if (colliderCount < 1) {
                    return;
                }

                for(int id = 0; id < colliderCount; id++) {
                    LightCollider2D collider = pass.layerMaskList[id];

                    switch(collider.mainShape.maskType) {
                        case LightCollider2D.MaskType.SpritePhysicsShape:
                        case LightCollider2D.MaskType.CompositeCollider2D:
                        case LightCollider2D.MaskType.Collider2D:
                        case LightCollider2D.MaskType.Collider3D:
                            Shape.Mask(pass.light, collider, pass.layer);
                        break;
                    }
                }
            }

            private static void DrawMesh(Pass pass) {
                int colliderCount = pass.layerMaskList.Count;

                if (colliderCount < 1) {
                    return;
                }

                for(int id = 0; id < colliderCount; id++) {
                    LightCollider2D collider = pass.layerMaskList[id];

                    switch(collider.mainShape.maskType) {
                        case LightCollider2D.MaskType.MeshRenderer:
                            Mesh.Mask(pass.light, collider, pass.materialMask, pass.layer);
                        break;

                        case LightCollider2D.MaskType.BumpedMeshRenderer:
                            Material material = collider.bumpMapMode.SelectMaterial(pass.materialNormalMap_PixelToLight, pass.materialNormalMap_ObjectToLight);
                            Mesh.MaskNormalMap(pass.light, collider, material, pass.layer);
                        break;

                        case LightCollider2D.MaskType.SkinnedMeshRenderer:
                            SkinnedMesh.Mask(pass.light, collider, pass.materialMask, pass.layer);
                        break;
                    }
                }
            }

            private static void DrawSprite(Pass pass) {
                int colliderCount = pass.layerMaskList.Count;

                if (colliderCount < 1) {
                    return;
                }

                SpriteRenderer2D.currentTexture = null;

                for(int id = 0; id < colliderCount; id++) {
                    LightCollider2D collider = pass.layerMaskList[id];

                    if (collider.mainShape.maskType != LightCollider2D.MaskType.Sprite) {
                        continue;
                    }

                    SpriteRenderer2D.Mask(pass.light, collider, pass.materialMask, pass.layer);
                }

                if (SpriteRenderer2D.currentTexture != null) {
                    GL.End();

                    pass.materialMask.mainTexture = null;

                    SpriteRenderer2D.currentTexture = null;
                }

                for(int id = 0; id < colliderCount; id++) {
                    LightCollider2D collider = pass.layerMaskList[id];

                    if (collider.mainShape.maskType != LightCollider2D.MaskType.BumpedSprite) {
                        continue;
                    }

                    Material material = collider.bumpMapMode.SelectMaterial(pass.materialNormalMap_PixelToLight, pass.materialNormalMap_ObjectToLight);
                
                    SpriteRenderer2D.MaskBumped(pass.light, collider, material, pass.layer);
                }

            }

            private static void DrawTilemapCollider(Pass pass) {
                #if UNITY_2017_4_OR_NEWER
                    for(int id = 0; id < pass.tilemapList.Count; id++) {
                        LightTilemapCollider2D tilemap = pass.tilemapList[id];

                        if (tilemap.maskLayer != pass.layerID) {
                            continue;
                        }

                        if (tilemap.MasksDisabled()) {
                            continue;
                        }
                        
                        if (tilemap.InLight(pass.light) == false) {
                           continue;
                        }

                        // Tilemap In Range
                        switch(tilemap.mapType) {
                            case MapType.UnityRectangle:
                            case MapType.UnityIsometric:
                            case MapType.UnityHexagon:

                                LightTilemapCollider.Base baseTilemap = tilemap.GetCurrentTilemap();

                                switch(baseTilemap.maskType) {
                                    case MaskType.Grid:
                                    case MaskType.SpritePhysicsShape:
                                        UnityTilemap.MaskShape(pass.light, tilemap, pass.layer);
                                    break;
                                }

                            break;

                            case MapType.SuperTilemapEditor:
                                SuperTilemapEditorSupport.Light.Mask.Grid.Draw(pass.light, tilemap);
                            break;
                        }
                    }
                #endif
            }

            private static void DrawTilemapSprite(Pass pass) {
                #if UNITY_2017_4_OR_NEWER
                    for(int id = 0; id < pass.tilemapList.Count; id++) {
                        LightTilemapCollider2D tilemap = pass.tilemapList[id];

                        if (tilemap.maskLayer != pass.layerID) {
                            continue;
                        }

                        if (tilemap.MasksDisabled()) {
                            continue;
                        }

                        if (tilemap.InLight(pass.light) == false) {
                            continue;
                        }

                        // Tilemap In Range

                            switch(tilemap.mapType) {
                                case MapType.UnityRectangle:
                                case MapType.UnityIsometric:
                                case MapType.UnityHexagon:

                                    LightTilemapCollider.Base baseTilemap = tilemap.GetCurrentTilemap();
                            
                                switch(baseTilemap.maskType) {
                                    case LightTilemapCollider.MaskType.Sprite:
                                        
                                        UnityTilemap.Sprite(pass.light, tilemap, pass.materialMask, pass.layer);
                                    
                                    break;

                                    case MaskType.BumpedSprite:
                                        Material material = tilemap.bumpMapMode.SelectMaterial(pass.materialNormalMap_PixelToLight, pass.materialNormalMap_ObjectToLight);
                            
                                        UnityTilemap.BumpedSprite(pass.light, tilemap, material, pass.layer);
                                    break;
                                }
                                
                            break;

                            case MapType.SuperTilemapEditor:

                                switch(tilemap.superTilemapEditor.maskTypeSTE) {
                                    case SuperTilemapEditorSupport.TilemapCollider.MaskType.Sprite:

                                        SuperTilemapEditorSupport.Light.Mask.SpriteRenderer2D.Sprite(pass.light, tilemap, pass.materialMask);
                                    
                                    break;
                                    
                                    case SuperTilemapEditorSupport.TilemapCollider.MaskType.BumpedSprite:
                                        Material material = tilemap.bumpMapMode.SelectMaterial(pass.materialNormalMap_PixelToLight, pass.materialNormalMap_ObjectToLight);
                            
                                        SuperTilemapEditorSupport.Light.Mask.SpriteRenderer2D.BumpedSprite(pass.light, tilemap, material);
                                
                                    break;
                                }
      
                            break;
                        }                   
                    }
                #endif
            }
        }
    }
}