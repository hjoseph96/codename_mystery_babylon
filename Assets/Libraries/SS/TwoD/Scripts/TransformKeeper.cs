using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
	public class TransformKeeper : MonoBehaviour
	{
		[SerializeField]
		bool m_KeepScale = true;
		[SerializeField]
		bool m_KeepPosition;

		Vector3 scale;
		Vector3 position;

		void Awake ()
		{
			scale = transform.localScale;
			position = transform.localPosition;
		}

		void LateUpdate ()
		{
			if (m_KeepScale) {
				transform.localScale = scale;
			}

			if (m_KeepPosition) {
				transform.localPosition = position;
			}
		}
	}
}