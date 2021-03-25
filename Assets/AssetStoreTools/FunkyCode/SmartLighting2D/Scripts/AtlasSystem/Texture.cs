using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightingSettings;

namespace AtlasSystem {
    
    public class Texture {
        private Texture2D texture;
        public int currentX = 1;
        public int currentY = 1;
        public int currentHeight = 0;

        public const int atlasSize = 1024;

        public int spriteCount = 0;

        public void Initialize() {
         
            Color resetColor = new Color32(0, 0, 0, 0);
            var pixels = texture.GetPixels(0, 0, texture.width, texture.height);
                        
            for(int x = 0; x < pixels.Length; x++) {
                pixels[x] = resetColor;
            }

            texture.SetPixels(0, 0, texture.width, texture.height, pixels);

            texture.Apply();
      
        }

        public Texture() {
            GetTexture();
        }

        public Texture2D GetTexture() {
            if (texture == null) {
                texture = new Texture2D(atlasSize, atlasSize);
            }
            return(texture);
        }
    }
}