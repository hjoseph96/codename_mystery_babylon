using UnityEngine;

namespace NeatWolf.ParticleFX03
{
    public class Rotating : MonoBehaviour {
		public Vector3 rotationSpeed=Vector3.zero;
		public Space relativeTo=Space.Self;
		
		// Update is called once per frame
		void Update () {
			transform.Rotate(rotationSpeed * Time.deltaTime, relativeTo);
		}

		public void ForcedRotation(float time)
		{
			transform.Rotate(rotationSpeed * time, relativeTo);
		}
	}
}