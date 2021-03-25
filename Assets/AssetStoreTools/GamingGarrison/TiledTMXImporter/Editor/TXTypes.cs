using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace GamingGarrison
{
    /// <summary>
    /// TX is the Object Template file type
    /// </summary>
    namespace TXTypes
    {
        [XmlRoot(ElementName = "template")]
        public class Template
        {
            [XmlElement(ElementName = "tileset")]
            public TMX.TilesetReference tileset;

            [XmlElement(ElementName = "object")]
            public TMX.Object templateObject;
        }
    }
}
