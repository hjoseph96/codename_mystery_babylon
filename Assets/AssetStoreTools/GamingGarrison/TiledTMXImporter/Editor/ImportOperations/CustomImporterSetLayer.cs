using System.Collections.Generic;
using UnityEngine;

namespace GamingGarrison
{
    public class CustomImporterSetLayer : ITilemapImportOperation
    {
        public void HandleCustomProperties(GameObject gameObject, IDictionary<string, string> customProperties)
        {
            if (customProperties.ContainsKey("unity:layer"))
            {
                string layerName = customProperties["unity:layer"];
                int layerID = LayerMask.NameToLayer(layerName);
                if (layerID >= 0)
                {
                    gameObject.layer = layerID;
                }
                else
                {
                    Debug.LogError("The TMX map is expecting a layer called " + layerName + " to exist, but it is not configured in your Unity project");
                }
            }
        }
    }
}
