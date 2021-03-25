using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering {
    public class LightBuffer {

        static public void Render(Light2D light) {
			float size = light.size;

			GL.PushMatrix();

            if (light.IsPixelPerfect()) {
                Camera camera = Camera.main;

                float cameraRotation = LightingPosition.GetCameraRotation(camera);
                Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, cameraRotation), Vector3.one);

                float sizeY = camera.orthographicSize;
                float sizeX = sizeY * ( (float)camera.pixelWidth / camera.pixelHeight );
                
                GL.LoadPixelMatrix( -sizeX, sizeX, -sizeY, sizeY );

            } else {

                GL.LoadPixelMatrix( -size, size, -size, size );
                
            }
			
			Rendering.Light.Main.Draw(light);

			GL.PopMatrix();
		}

         static public void RenderCollisions(Light2D light) {
			float size = light.size;

			GL.PushMatrix();

            if (light.IsPixelPerfect()) {
                Camera camera = Camera.main;

                float cameraRotation = LightingPosition.GetCameraRotation(camera);
                Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, cameraRotation), Vector3.one);

                float sizeY = camera.orthographicSize;
                float sizeX = sizeY * ( (float)camera.pixelWidth / camera.pixelHeight );
                
                GL.LoadPixelMatrix( -sizeX, sizeX, -sizeY, sizeY );

            } else {

                GL.LoadPixelMatrix( -size, size, -size, size );
                
            }
			
			Rendering.Light.Main.DrawCollisions(light);

			GL.PopMatrix();
		}


        static public void UpdateName(LightBuffer2D buffer) {
            string freeString = "";

            if (buffer.Free) {
                freeString = "free";
            } else {
                freeString = "taken";
            }

            if (buffer.renderTexture != null) {
                    
                buffer.name = "Buffer (Id: " + (LightBuffer2D.List.IndexOf(buffer) + 1) + ", Size: " + buffer.renderTexture.width + ", " + freeString + ")";

            } else {
                buffer.name = "Buffer (Id: " + (LightBuffer2D.List.IndexOf(buffer) + 1) + ", No Texture, " + freeString + ")";

            }
           
            if (Lighting2D.QualitySettings.HDR != LightingSettings.HDR.Off) {
                buffer.name = "HDR " + buffer.name;
            }
        }

        static public void InitializeRenderTexture(LightBuffer2D buffer, Vector2Int textureSize) {
            if (buffer.renderTexture != null) {
                //return;
            }

            RenderTextureFormat format = RenderTextureFormat.Default;

            // if (SystemInfo.SupportsTextureFormat(TextureFormat.RHalf)) {
        
            switch(Lighting2D.QualitySettings.HDR) {
                case LightingSettings.HDR.Half:
                    format = RenderTextureFormat.RHalf;
                break;

                case LightingSettings.HDR.Float:
                    format = RenderTextureFormat.DefaultHDR;
                break;

                case LightingSettings.HDR.Off:
                    format = RenderTextureFormat.R8;
                break;
            }

            buffer.renderTexture = new LightTexture(textureSize.x, textureSize.y, 0, format);
            buffer.renderTexture.renderTexture.filterMode = Lighting2D.Profile.qualitySettings.lightFilterMode;
 
            UpdateName(buffer);
        }

        static public void InitializeCollisionTexture(LightBuffer2D buffer, Vector2Int textureSize) {
            if (buffer.collisionTexture != null) {
                //return;
            }

            RenderTextureFormat format = RenderTextureFormat.Default;

            //if (SystemInfo.SupportsTextureFormat(TextureFormat.RHalf)) {
            //if (SystemInfo.SupportsTextureFormat(TextureFormat.R8)) {

            switch(Lighting2D.QualitySettings.HDR) {
                case LightingSettings.HDR.Half:
                    format = RenderTextureFormat.RHalf;
                break;

                case LightingSettings.HDR.Float:
                    format = RenderTextureFormat.DefaultHDR;
                break;

                case LightingSettings.HDR.Off:
                    format = RenderTextureFormat.R8;
                break;
            }

            buffer.collisionTexture = new LightTexture(textureSize.x, textureSize.y, 0, format);
            buffer.collisionTexture.renderTexture.filterMode = Lighting2D.Profile.qualitySettings.lightFilterMode;

            UpdateName(buffer);
        }
    }
}