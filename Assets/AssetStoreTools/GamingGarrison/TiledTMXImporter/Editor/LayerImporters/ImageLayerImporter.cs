using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamingGarrison
{
    internal class ImageLayerImporter
    {
        internal static bool ImportImageLayer(TMX.ImageLayer imageLayer, GameObject parent, int sortingLayer, out GameObject newImageLayer)
        {
            newImageLayer = new GameObject(imageLayer.name, typeof(SpriteRenderer));
            newImageLayer.transform.SetParent(parent.transform, false);
            TiledTMXImporter.SetupLayerOffset(newImageLayer, imageLayer.offsetx, imageLayer.offsety);
            if (imageLayer.image != null)
            {
                string relativeSource = imageLayer.image.source;
                Sprite importedSprite = TiledUtils.ImportPathAsSprite(TiledTMXImporter.s_tmxParentFolder, relativeSource, TiledTMXImporter.s_imageLayerSpriteDir, TiledTMXImporter.s_pixelsPerUnit);
                SpriteRenderer renderer = newImageLayer.GetComponent<SpriteRenderer>();
                renderer.sprite = importedSprite;
                renderer.sortingOrder = sortingLayer;
            }
            return true;
        }
    }
}
