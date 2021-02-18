using UnityEngine;
using System.Collections;

namespace SS.IO
{
	public class Path
	{
		public static string GetRalativePath(string absolutePath)
		{
			string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
			return absolutePath.Replace(projectPath, string.Empty);
		}
	}
}
