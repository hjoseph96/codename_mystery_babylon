using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace GamingGarrison
{
    public class ImportUtils
    {
        public static bool s_validationMode = false;
        static bool s_hadValidationWarningsOnImport = false;

        public static T ReadXMLIntoObject<T>(string path)
        {
            StreamReader streamReader = File.OpenText(path);
            if (streamReader == null)
            {
                return default(T);
            }

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.DTD;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;

            XmlReader xmlReader = XmlTextReader.Create(streamReader, settings);

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            if (s_validationMode)
            {
                serializer.UnknownNode += Serializer_UnknownNode;
                serializer.UnknownElement += Serializer_UnknownElement;
                serializer.UnknownAttribute += Serializer_UnknownAttribute;
                serializer.UnreferencedObject += Serializer_UnreferencedObject;
            }

            try
            {
                s_hadValidationWarningsOnImport = false;
                object deserialized = serializer.Deserialize(xmlReader);
                streamReader.Close();
                if (s_hadValidationWarningsOnImport)
                {
                    throw new Exception("<b>Stopping due to validation mode finding something unrecognised by this version of TMX Importer</b>");
                }
                return (T)deserialized;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                streamReader.Close();
                return default(T);
            }
        }

        private static void Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Debug.LogError("Unknown node when parsing XML = " + e.ObjectBeingDeserialized + "->" + e.LocalName);
            s_hadValidationWarningsOnImport = true;
        }

        private static void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            Debug.LogError("Unknown element when parsing XML = " + e.ObjectBeingDeserialized + "->" + e.Element.LocalName);
            s_hadValidationWarningsOnImport = true;
        }

        private static void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            Debug.LogError("Unknown attribute when parsing XML = " + e.ObjectBeingDeserialized + "->" + e.Attr.Name);
            s_hadValidationWarningsOnImport = true;
        }

        private static void Serializer_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {
            Debug.LogError("Unreferenced object when parsing XML = " + e.UnreferencedObject);
            s_hadValidationWarningsOnImport = true;
        }

        public static bool CreateAssetFolderIfMissing(string path, bool askPermission)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                bool ok = (askPermission ? EditorUtility.DisplayDialog(path + " not found", "Create new directory?", "OK", "Cancel") : true);
                if (ok)
                {
                    ok = CreateAssetFolder(path);
                    if (!ok)
                    {
                        Debug.LogError("Target directory " + path + " could not be created!");
                        return false;
                    }
                }
                else
                {
                    Debug.LogError("Permission not given to create folder by user");
                    return false;
                }
            }
            return true;
        }

        static bool CreateAssetFolder(string path)
        {
            DirectoryInfo parent = Directory.GetParent(path);
            string folderName = Path.GetFileName(path);
            string guid = AssetDatabase.CreateFolder(parent.ToString(), folderName);
            if (guid == null || guid.Length == 0)
            {
                return false;
            }
            return true;
        }

        public static T[] GetObjectsThatImplementInterface<T>()
        {
            List<T> typeList = new List<T>();

            Type type = typeof(T);
            IEnumerable<object> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass && type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => Activator.CreateInstance(x));

            foreach (object t in types)
            {
                typeList.Add((T)t);
            }
            return typeList.ToArray();
        }

        public static Vector2[] PointsFromString(string pointsString, Vector2 scale)
        {
            string[] pointsSplit = pointsString.Split(' ');
            Vector2[] points = new Vector2[pointsSplit.Length];
            for (int i = 0; i < points.Length; i++)
            {
                string[] vectorComponents = pointsSplit[i].Split(',');
                Debug.Assert(vectorComponents.Length == 2, "This string should have 2 components separated by a comma: " + pointsSplit[i]);
                points[i] = new Vector2(float.Parse(vectorComponents[0]), float.Parse(vectorComponents[1]));
                points[i] = Vector2.Scale(scale, points[i]);
            }
            return points;
        }

        public static T CreateOrReplaceAsset<T> (T asset, string path) where T:UnityEngine.Object
        {
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                existingAsset = asset;
            }
            else
            {
                EditorUtility.CopySerialized(asset, existingAsset);
            }

            return existingAsset;
        }

        static byte[] DecompressFromStream(Stream stream)
        {
            MemoryStream output = new MemoryStream();
            byte[] buffer = new byte[16 * 1024];
            int amountRead;
            while ((amountRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, amountRead);
            }

            byte[] uncompressed = output.ToArray();
            return uncompressed;
        }

        public static byte[] DecompressZLib(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data, 2, data.Length - 2)) // Skip zlib header
            {
                using (System.IO.Compression.DeflateStream deflateStream = new System.IO.Compression.DeflateStream(memoryStream, System.IO.Compression.CompressionMode.Decompress))
                {
                    return DecompressFromStream(deflateStream);
                }
            }
        }

        public static byte[] DecompressGZip(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (System.IO.Compression.GZipStream gZipStream = new System.IO.Compression.GZipStream(memoryStream, System.IO.Compression.CompressionMode.Decompress))
                {
                    return DecompressFromStream(gZipStream);
                }
            }
        }
    }
}
