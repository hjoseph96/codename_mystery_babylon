using UnityEngine;

public class Reorderable : PropertyAttribute
{
	public string elementHeader { get; protected set; }
	public bool headerZeroIndex { get; protected set; }

	// <summary>
	/// Display a List/Array as a sortable list in the inspector
	/// </summary>
	public Reorderable()
	{
		elementHeader = string.Empty;
		headerZeroIndex = false;
	}

	// <summary>
	/// Display a List/Array as a sortable list in the inspector
	/// </summary>
	/// <param name="headerString">Customize the element name in the inspector</param>
	/// <param name="isZeroIndex">If false, start element list count from 1</param>
	/// <param name="isSingleLine">Try to fit the array elements in a single line</param>
	//public Reorderable(string headerString = "", bool isZeroIndex = true, bool isSingleLine = false)
	//{
	//	ElementHeader = headerString;
	//	HeaderZeroIndex = isZeroIndex;
	//	ElementSingleLine = isSingleLine;
	//}
}

