using UnityEngine;
using System.Collections;

namespace Grids2D {

	[ExecuteInEditMode]
	public class Grid2DConfig : MonoBehaviour {

		[Tooltip("Help")]
		[TextArea]
		public string info = "To load this configuration, just activate this component or call LoadConfiguration() method of this script.";

		[Tooltip("User-defined name for this configuration")]
		[TextArea]
		public string title = "Optionally name this configuration editing this text.";

		[HideInInspector]
		public string config;

		[HideInInspector]
		public Texture2D[] textures;

		// Use this for initialization
		void OnEnable() {
			if (!Application.isPlaying) LoadConfiguration();
		}

		void Start() {
			LoadConfiguration();
		}

		/// <summary>
		/// Call this method to force a configuration load.
		/// </summary>
		public void LoadConfiguration() {
			if (config == null) return;

			Grid2D grid = GetComponent<Grid2D>();
			if (grid==null) {
				Debug.Log ("Grid2D not found in this game object!");
				return;
			}
			grid.textures = textures;
			grid.CellSetConfigurationData(config);
		}

	}

}