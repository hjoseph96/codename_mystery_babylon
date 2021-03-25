using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamingGarrison
{
    /// <summary>
    /// Each Tiled layer or object can have custom properties.
    /// Whenever a layer or object is loaded, all implementations of this interface are triggered.
    /// This allows custom processing of the object based on these properties.
    /// </summary>
    public interface ITilemapImportOperation
    {
        /// <summary>
        /// Called whenever a Tiled layer or object is imported into the scene from the Tilemap TMX Importer
        /// </summary>
        /// <param name="gameObject">The gameobject that needs customising</param>
        /// <param name="customProperties">All properties associated with that object</param>
        void HandleCustomProperties(GameObject gameObject, IDictionary<string, string> customProperties);
    }
}

// Example usage (create your class in an editor folder)
/*
class CustomImporterAddComponent : GamingGarrison.ITilemapImportOperation
{
    public void HandleCustomProperties(UnityEngine.GameObject gameObject,
        IDictionary<string, string> props)
    {
        // Simply add a component to our GameObject
        if (props.ContainsKey("AddComp"))
        {
            gameObject.AddComponent(props["AddComp"]);
        }
    }
}

    This usage/property handling system is identical to that found in the Tiled2Unity project.  This is to allow user extensions to work with either tool.
*/
