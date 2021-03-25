using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightingSettings;

namespace Rendering {

    public class LightMainBuffer {

        public class Check {
                    
            static public void RenderTexture(LightMainBuffer2D buffer) {
                Vector2Int screen = GetScreenResolution(buffer);

                if (screen.x > 0 && screen.y > 0) {
                    Camera camera = buffer.cameraSettings.GetCamera();

                    if (buffer.renderTexture == null || screen.x != buffer.renderTexture.width || screen.y != buffer.renderTexture.height) {

                        switch(camera.cameraType) {
                            case CameraType.Game:
                                Rendering.LightMainBuffer.InitializeRenderTexture(buffer);
                            
                            break;

                            case CameraType.SceneView:
                                // Scene view pixel rect is constantly changing (Unity Bug?)
                                int differenceX = Mathf.Abs(screen.x - buffer.renderTexture.width);
                                int differenceY = Mathf.Abs(screen.y - buffer.renderTexture.height);
                                
                                if (differenceX > 5 || differenceY > 5) {
                                    Rendering.LightMainBuffer.InitializeRenderTexture(buffer);
                                }
                            
                            break;

                        }
                    }
                }
            }

            static public bool CameraSettings (LightMainBuffer2D buffer) {
                LightingManager2D manager = LightingManager2D.Get();
                int settingsID = buffer.cameraSettings.id;

                if (settingsID >= manager.cameraSettings.Length) {
                    return(false);
                }

                CameraSettings cameraSetting = manager.cameraSettings[settingsID];

                if (cameraSetting.Equals(buffer.cameraSettings) == false) {
                    return(false);
                }

                buffer.cameraSettings.renderMode = cameraSetting.renderMode;

                return(true);
            }

        }

        public static void Update(LightMainBuffer2D buffer) {
            BufferPreset bufferPreset = buffer.GetBufferPreset();

            if (bufferPreset == null) {
                buffer.DestroySelf();
                return;
            }

            if (Rendering.LightMainBuffer.Check.CameraSettings(buffer) == false) {
                buffer.DestroySelf();
                return;
            }
            
            Camera camera = buffer.cameraSettings.GetCamera();

            if (camera == null) {
                return;
            }

            Rendering.LightMainBuffer.Check.RenderTexture(buffer);
        }

        public static void DrawPost(LightMainBuffer2D buffer) {			
			if (buffer.cameraSettings.renderMode != CameraSettings.RenderMode.Draw) {
				return;
			}

            if (Lighting2D.RenderingMode != RenderingMode.OnPostRender) {
				return;
			}

			LightingRender2D.PostRender(buffer);
        }

        public static void DrawOn(LightMainBuffer2D buffer) {
			if (buffer.cameraSettings.renderMode != CameraSettings.RenderMode.Draw) {
				return;
			}
                
            switch(Lighting2D.RenderingMode) {
                case RenderingMode.OnRender:
                    LightingRender2D.OnRender(buffer);
                break;

                case RenderingMode.OnPreRender:
                    LightingRender2D.PreRender(buffer);
                break;
            }
        }

        public static void Render(LightMainBuffer2D buffer) {
            Camera camera = buffer.cameraSettings.GetCamera();
            if (camera == null) {
                return;
            }

            float cameraRotation = LightingPosition.GetCameraRotation(camera);
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, cameraRotation), Vector3.one);

            float sizeY = camera.orthographicSize;
            float sizeX = sizeY * ( (float)camera.pixelWidth / camera.pixelHeight );
            
            GL.LoadPixelMatrix( -sizeX, sizeX, -sizeY, sizeY );
            GL.MultMatrix(matrix);

            GL.PushMatrix();
          
            BufferPreset bufferPreset = buffer.GetBufferPreset();
            
            Rendering.Day.Main.Draw(camera, bufferPreset);
        
            Rendering.Night.Main.Draw(camera, bufferPreset);

            GL.PopMatrix();
        }

        static public Vector2Int GetScreenResolution(LightMainBuffer2D buffer) {
            BufferPreset bufferPreset = buffer.GetBufferPreset();

            if (bufferPreset == null) {
                return(Vector2Int.zero);
            }

            Camera camera = buffer.cameraSettings.GetCamera();

            if (camera == null) {
                return(Vector2Int.zero);
            }

            float resolution = bufferPreset.lightingResolution;

            int screenWidth = (int)(camera.pixelRect.width * resolution);
            int screenHeight = (int)(camera.pixelRect.height * resolution);

            return(new Vector2Int(screenWidth, screenHeight));
        }

        static public void InitializeRenderTexture(LightMainBuffer2D buffer) {
            Vector2Int screen = GetScreenResolution(buffer);
            
            if (screen.x > 0 && screen.y > 0) {
                string idName = "";

                int bufferID = buffer.cameraSettings.bufferID;
                
                if (bufferID < Lighting2D.BufferPresets.Length) {
                    idName = Lighting2D.BufferPresets[bufferID].name + ", ";
                }

                Camera camera = buffer.cameraSettings.GetCamera();

                buffer.name = "Camera Buffer (" + idName +"Id: " + (bufferID  + 1) + ", Camera: " + camera.name + " )";
    
                RenderTextureFormat format = RenderTextureFormat.Default;

                switch(Lighting2D.QualitySettings.HDR) {
                    case LightingSettings.HDR.Half:
                        format = RenderTextureFormat.RGB111110Float;
                    break;

                    case LightingSettings.HDR.Float:
                        format = RenderTextureFormat.DefaultHDR;
                    break;

                    case LightingSettings.HDR.Off:
                        if (SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.R5G6B5_UNormPack16, UnityEngine.Experimental.Rendering.FormatUsage.Render)) {
                            format = RenderTextureFormat.RGB565;
                        }
                    break;
                }
 
                buffer.renderTexture = new LightTexture (screen.x, screen.y, 0, format);
                buffer.renderTexture.Create ();
            }
        }
    }
}
