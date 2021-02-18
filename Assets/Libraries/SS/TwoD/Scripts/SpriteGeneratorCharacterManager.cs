using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
	public class SpriteGeneratorCharacterManager : MonoBehaviour
	{
		public string prefixName = "Char";
		public int indexPadLeftZero = 4;
		public bool includeAI = true;
		public bool movable = true;
		public string prefabPath = "Prefabs/Gen";
		public Animator[] characters;
		public GameObject[] outputPrefabs;
		public bool[] willGenerate;

		public void DeactivateAllCharacters()
		{
			for (int i = 0; i < characters.Length; i++)
			{
				characters[i].gameObject.SetActive(false);
			}
		}

		public void ActivateCharacter(int index)
		{
			DeactivateAllCharacters();

			if (index < characters.Length)
			{
				characters[index].gameObject.SetActive(true);
			}
		}
	}
}