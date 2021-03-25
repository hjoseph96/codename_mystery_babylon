using System;
using UnityEngine;

namespace SBS
{
    [Serializable]
    public class PathProperty : PropertyBase
    {
        public bool autoFileNaming = true;
        public string fileName = "default";
        public string directoryPath = "";
    }
}
