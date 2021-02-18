using UnityEngine;
using UnityEditor;

namespace SS.IO
{
	public class FileUtil
	{
		public static string CopyFromTemplate(string templateFileName, string characterName, string surfix, string path, bool replaceExistFile = true)
		{
			string templatePath = SS.IO.File.GetPathTemplateFile(templateFileName);
			string directoryPath = System.IO.Path.Combine(Application.dataPath, path);
			string targetPath = System.IO.Path.Combine(directoryPath, characterName + surfix);

			if (!System.IO.Directory.Exists(directoryPath))
			{
				System.IO.Directory.CreateDirectory(directoryPath);
			}

			if (System.IO.File.Exists(targetPath))
			{
				if (replaceExistFile)
				{
					System.IO.File.Delete(targetPath);
				}
			}

			if (!System.IO.File.Exists(targetPath))
			{
				UnityEditor.FileUtil.CopyFileOrDirectory(templatePath, targetPath);
				AssetDatabase.Refresh();
			}

			return targetPath;
		}
	}
}
