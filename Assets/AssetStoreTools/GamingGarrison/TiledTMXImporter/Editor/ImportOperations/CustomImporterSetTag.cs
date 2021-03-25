using System.Collections.Generic;
using UnityEngine;

namespace GamingGarrison
{
    public class CustomImporterSetTag : ITilemapImportOperation
    {
        public void HandleCustomProperties(GameObject gameObject, IDictionary<string, string> customProperties)
        {
            if (customProperties.ContainsKey("unity:tag"))
            {
                string tagValue = customProperties["unity:tag"];
                gameObject.tag = tagValue;
            }
        }
    }
}
