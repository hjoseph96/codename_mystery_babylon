using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamingGarrison
{
    public class ImportedTemplate
    {
        public TXTypes.Template m_template;
        public ImportedTileset m_importedTileset;

        public ImportedTemplate(TXTypes.Template template, ImportedTileset tileset)
        {
            m_template = template;
            m_importedTileset = tileset;
        }
    }
}
