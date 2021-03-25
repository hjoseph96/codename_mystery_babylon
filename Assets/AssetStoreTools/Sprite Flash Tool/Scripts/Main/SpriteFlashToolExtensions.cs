using UnityEngine;
using System.IO;


namespace SpriteFlashTools.Extensions
{
    public static class SpriteFlashToolExtensions
    {

        public static void CopyToClipboard(this string _s)
        {
            TextEditor te = new TextEditor { text = _s };
            te.SelectAll();
            te.Copy();
        }

        public static void OpenFile(string _filePath)
        {
            _filePath = _filePath.Replace(@"/", @"\");
            System.Diagnostics.Process.Start("explorer.exe", "/open," + _filePath);
        }

        public static void OpenFileExplorer(string _filePath)
        {
            _filePath = _filePath.Replace(@"/", @"\");
            System.Diagnostics.Process.Start("explorer.exe", "/select," + _filePath);
        }

        public static void ClearTextFile(string _filePath)
        {
            File.Delete(_filePath);
            File.Move(Path.GetTempFileName(), _filePath);
        }

        public static void AddNewLineToFile(string _filePath, string _newLine)
        {
            StreamWriter writer = new StreamWriter(_filePath, true);
            writer.WriteLine(_newLine);
            writer.Close();
        }

        public static bool FileExists(string _filePath)
        {
            if (File.Exists(_filePath))
                return true;
            return false;
        }

        public static void CreateFile(string _filePath)
        {
            File.Move(Path.GetTempFileName(), _filePath);
        }

        public static string GetFullTextFileInString(string _filePath, bool _mustBeColor)
        {
            string finalString = "";
            using (var sr = new StreamReader(_filePath))
            {
                string line;
                Color placeholderColor;
                while ((line = sr.ReadLine()) != null)
                {
                    if (_mustBeColor && ColorUtility.TryParseHtmlString(line, out placeholderColor))
                        finalString += line + "\n";
                    else
                        finalString += line + "\n";
                };
            }
            return finalString;
        }

        public static void DeleteLineInFile(string _filePath, string _lineToDelete)
        {
            string tempFile = Path.GetTempFileName();
            using (var sr = new StreamReader(_filePath))
            using (var sw = new StreamWriter(tempFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != _lineToDelete)
                        sw.WriteLine(line);
                }
            }
            File.Delete(_filePath);
            File.Move(tempFile, _filePath);
        }


        public static int GetFileLinesCount(string _filePath)
        {
            using (var sr = new StreamReader(_filePath))
            {
                int _count = 0;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != "")
                        _count++;
                };
                return _count;
            }
        }

        public static bool LineAlreadyExist(string _filePath, string _line, string _logMessage = "")
        {
            using (var sr = new StreamReader(_filePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == _line)
                    {
                        Debug.Log(_logMessage);
                        return true;
                    }
                };
                return false;
            }
        }


    }
}
