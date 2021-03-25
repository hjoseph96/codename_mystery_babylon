using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightingSettings;
using LightSettings;

namespace Rendering.Night {
	
	public static class Main {

		static Pass pass = new Pass();

		public static void Draw(Camera camera, BufferPreset bufferPreset) {
			DarknessColor(camera, bufferPreset);

			LightingLayerSetting[] layerSettings = bufferPreset.lightLayers.Get();
			
			if (layerSettings == null) {
				return;
			}

			if (layerSettings.Length < 1) {
				return;
			}

			for(int i = 0; i < layerSettings.Length; i++) {
				LightingLayerSetting nightLayer = layerSettings[i];

				if (pass.Setup(nightLayer, camera) == false) {
					continue;
				}

				if (nightLayer.sorting == LayerSorting.None) {

					NoSort.Draw(pass);

				} else {

					pass.SortObjects();

					Sorted.Draw(pass);
				}
			}
		}

		private static void DarknessColor(Camera camera, BufferPreset bufferPreset) {
			Color color = bufferPreset.darknessColor;

			if (color.a > 0) {
				Material material = Lighting2D.materials.GetAlphaBlend();
				material.mainTexture = null;
				
				GLExtended.color = color;

				float cameraRotation = -LightingPosition.GetCameraRotation(camera);
				Vector2 size = LightingRender2D.GetSize(camera);

				Universal.Texture.Quad.Draw(material, Vector2.zero, size, cameraRotation, 0);
			}
		}		
	}
}