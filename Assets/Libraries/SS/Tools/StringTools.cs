using UnityEngine;
using System.Collections;

public class StringTools
{
	public static string MergeStringArray(string[] a, string link)
	{
		if (a.Length < 1)
			return string.Empty;

		string result = a[0];

		for (int i = 1; i < a.Length; i++)
		{
			result += (link + a[i]);
		}

		return result;
	}
}