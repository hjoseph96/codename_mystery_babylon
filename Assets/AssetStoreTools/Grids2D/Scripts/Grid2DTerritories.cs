using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Grids2D.Geom;

namespace Grids2D
{

	/* Event definitions */
	public delegate void OnTerritoryEvent (int territoryIndex);
	public delegate void OnTerritoryHighlight (int territoryIndex, ref bool cancelHighlight);


	public partial class Grid2D : MonoBehaviour
	{

		public event OnTerritoryEvent OnTerritoryEnter;
		public event OnTerritoryEvent OnTerritoryExit;
		public event OnTerritoryEvent OnTerritoryClick;
		public event OnTerritoryHighlight OnTerritoryHighlight;

		[NonSerialized]
		public List<Territory> territories;

		public Texture2D territoriesTexture;
		public Color territoriesTextureNeutralColor;


		[SerializeField]
		bool _enableTerritories = true;

		/// <summary>
		/// Enables territories functionality.
		/// </summary>
		public bool enableTerritories { 
			get { return _enableTerritories; } 
			set {
				if (_enableTerritories != value) {
					_enableTerritories = value;
					GenerateMap ();
					isDirty = true;
				}
			}
		}


		[SerializeField]
		int _numTerritories = 3;

		/// <summary>
		/// Gets or sets the number of territories.
		/// </summary>
		public int numTerritories { 
			get { return _numTerritories; } 
			set {
				if (_numTerritories != value) {
					_numTerritories = Mathf.Clamp (value, 1, MAX_TERRITORIES);
					GenerateMap ();
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool
			_showTerritories = false;

		/// <summary>
		/// Toggle frontiers visibility.
		/// </summary>
		public bool showTerritories { 
			get {
				return _showTerritories; 
			}
			set {
				if (value != _showTerritories) {
					_showTerritories = value;
					isDirty = true;
					if (!_showTerritories && territoryLayer != null) {
						territoryLayer.SetActive (false);
					} else {
						Redraw ();
					}
				}
			}
		}

		[SerializeField]
		bool _colorizeTerritories = false;

		/// <summary>
		/// Toggle colorize countries.
		/// </summary>
		public bool colorizeTerritories { 
			get {
				return _colorizeTerritories; 
			}
			set {
				if (value != _colorizeTerritories) {
					_colorizeTerritories = value;
					isDirty = true;
					if (!_colorizeTerritories && surfacesLayer != null) {
						DestroySurfaces ();
					} else {
						Redraw ();
					}
				}
			}
		}

		[SerializeField]
		float _colorizedTerritoriesAlpha = 0.7f;

		public float colorizedTerritoriesAlpha { 
			get { return _colorizedTerritoriesAlpha; } 
			set {
				if (_colorizedTerritoriesAlpha != value) {
					_colorizedTerritoriesAlpha = value;
					isDirty = true;
					UpdateColorizedTerritoriesAlpha ();
				}
			}
		}


		[SerializeField]
		Color
			_territoryHighlightColor = new Color (1, 0, 0, 0.7f);

		/// <summary>
		/// Fill color to use when the mouse hovers a territory's region.
		/// </summary>
		public Color territoryHighlightColor {
			get {
				return _territoryHighlightColor;
			}
			set {
				if (value != _territoryHighlightColor) {
					_territoryHighlightColor = value;
					isDirty = true;
					if (hudMatTerritoryOverlay != null && _territoryHighlightColor != hudMatTerritoryOverlay.color) {
						hudMatTerritoryOverlay.color = _territoryHighlightColor;
					}
					if (hudMatTerritoryGround != null && _territoryHighlightColor != hudMatTerritoryGround.color) {
						hudMatTerritoryGround.color = _territoryHighlightColor;
					}
				}
			}
		}

		
		[SerializeField]
		Color
			_territoryFrontierColor = new Color (0, 1, 0, 1.0f);

		/// <summary>
		/// Territories border color
		/// </summary>
		public Color territoryFrontiersColor {
			get {
				if (territoriesMat != null) {
					return territoriesMat.color;
				} else {
					return _territoryFrontierColor;
				}
			}
			set {
				if (value != _territoryFrontierColor) {
					_territoryFrontierColor = value;
					isDirty = true;
					if (territoriesMat != null && _territoryFrontierColor != territoriesMat.color) {
						territoriesMat.color = _territoryFrontierColor;
					}
				}
			}
		}


		public float territoryFrontiersAlpha {
			get {
				return _territoryFrontierColor.a;
			}
			set {
				if (_territoryFrontierColor.a != value) {
					_territoryFrontierColor = new Color (_territoryFrontierColor.r, _territoryFrontierColor.g, _territoryFrontierColor.b, value);
				}
			}
		}

		[SerializeField]
		bool _showTerritoriesOuterBorder = true;

		/// <summary>
		/// Shows perimetral/outer border of territories?
		/// </summary>
		/// <value><c>true</c> if show territories outer borders; otherwise, <c>false</c>.</value>
		public bool showTerritoriesOuterBorders {
			get { return _showTerritoriesOuterBorder; }
			set {
				if (_showTerritoriesOuterBorder != value) {
					_showTerritoriesOuterBorder = value;
					isDirty = true;
					Redraw ();
				}
			}
		}

		
		[SerializeField]
		bool _allowTerritoriesInsideTerritories = false;

		/// <summary>
		/// Set this property to true to allow territories to be surrounded by other territories.
		/// </summary>
		public bool allowTerritoriesInsideTerritories {
			get { return _allowTerritoriesInsideTerritories; }
			set {
				if (_allowTerritoriesInsideTerritories != value) {
					_allowTerritoriesInsideTerritories = value;
					isDirty = true;
				}
			}
		}

	
		#region State variables

		/// <summary>
		/// Returns Territory under mouse position or null if none.
		/// </summary>
		public Territory territoryHighlighted { get { return _territoryHighlighted; } }

		/// <summary>
		/// Returns currently highlighted territory index in the countries list.
		/// </summary>
		public int territoryHighlightedIndex { get { return _territoryHighlightedIndex; } }

		/// <summary>
		/// Returns Territory index which has been clicked
		/// </summary>
		public int territoryLastClickedIndex { get { return _territoryLastClickedIndex; } }

		#endregion


		#region Public Territories API

		/// <summary>
		/// Returns the_numCellsrovince in the cells array by its reference.
		/// </summary>
		public int TerritoryGetIndex (Territory territory)
		{
			if (territory == null)
				return -1;
			if (territoryLookup.ContainsKey (territory))
				return _territoryLookup [territory];
			else
				return -1;
		}

		/// <summary>
		/// Returns the territory object under position in local coordinates
		/// </summary>
		public Territory TerritoryGetAtPosition(Vector3 localPosition) {
			return GetTerritoryAtPoint(localPosition);
		}


		/// <summary>
		/// Gets the territory's center position in world space.
		/// </summary>
		public Vector3 TerritoryGetPosition(int territoryIndex, float elevation = 0) {
			if (territories == null || territoryIndex < 0 || territoryIndex >= territories.Count)
				return Misc.Vector3zero;
			Vector2 territoryGridCenter = territories[territoryIndex].center;
			return GetWorldSpacePosition(territoryGridCenter, elevation);
		}


		/// <summary>
		/// Returns the rect enclosing the territory in world space
		/// </summary>
		public Bounds TerritoryGetRectWorldSpace(int territoryIndex) {
			if (territories == null || territoryIndex < 0 || territoryIndex >= territories.Count)
				return new Bounds(Misc.Vector3zero, Misc.Vector3zero);
			Rect rect = territories[territoryIndex].region.rect2D;
			Vector3 min = GetWorldSpacePosition(rect.min);
			Vector3 max = GetWorldSpacePosition(rect.max);
			Bounds bounds = new Bounds((min + max) * 0.5f, max - min);
			return bounds;
		}


		/// <summary>
		/// Uncolorize/hide specified territory by index in the territories collection.
		/// </summary>
		public void TerritoryHideRegionSurface (int territoryIndex)
		{
			if (territories == null || territoryIndex < 0 || territoryIndex >= territories.Count)
				return;
			if (_territoryHighlightedIndex != territoryIndex) {
				int cacheIndex = GetCacheIndexForTerritoryRegion (territoryIndex);
				if (surfaces.ContainsKey (cacheIndex)) {
					if (surfaces [cacheIndex] == null) {
						surfaces.Remove (cacheIndex);
					} else {
						surfaces [cacheIndex].SetActive (false);
					}
				}
			}
			territories [territoryIndex].region.customMaterial = null;
		}

		public GameObject TerritoryToggle (int territoryIndex, bool visible, Color color)
		{
			return TerritoryToggle (territoryIndex, visible, color, null, Misc.Vector2one, Misc.Vector2zero, 0);
		}

		
		/// <summary>
		/// Colorize specified region of a territory by indexes.
		/// </summary>
		public GameObject TerritoryToggle (int territoryIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture, bool useCanvasRect = false)
		{
			return TerritoryToggle (territoryIndex, visible, color, texture, Misc.Vector2one, Misc.Vector2zero, 0, useCanvasRect);
		}

		/// <summary>
		/// Colorize specified region of a territory by indexes.
		/// </summary>
		public GameObject TerritoryToggle (int territoryIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool useCanvasRect = false)
		{
			if (territories == null || territoryIndex < 0 || territoryIndex >= territories.Count)
				return null;

			if (!visible) {
				TerritoryHideRegionSurface (territoryIndex);
				return null;
			}
			GameObject surf = null;
			Region region = territories [territoryIndex].region;
			if (region == null)
				return null;

			int cacheIndex = GetCacheIndexForTerritoryRegion (territoryIndex);
			// Checks if current cached surface contains a material with a texture, if it exists but it has not texture, destroy it to recreate with uv mappings
			if (surfaces.ContainsKey (cacheIndex) && surfaces [cacheIndex] != null)
				surf = surfaces [cacheIndex];
			
			// Should the surface be recreated?
			Material surfMaterial;
			if (surf != null) {
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if (texture != null && (region.customMaterial == null || textureScale != region.customTextureScale || textureOffset != region.customTextureOffset ||
				                textureRotation != region.customTextureRotation || !region.customMaterial.name.Equals (texturizedMat.name))) {
					surfaces.Remove (cacheIndex);
					DestroyImmediate (surf);
					surf = null;
				}
			}
			// If it exists, activate and check proper material, if not create surface
			bool isHighlighted = territoryHighlightedIndex == territoryIndex;
			if (surf != null) {
				if (!surf.activeSelf)
					surf.SetActive (true);
				// Check if material is ok
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if ((texture == null && !surfMaterial.name.Equals (coloredMat.name)) || (texture != null && !surfMaterial.name.Equals (texturizedMat.name))
				                || (surfMaterial.color != color && !isHighlighted) || (texture != null && region.customMaterial.mainTexture != texture)) {
					Material goodMaterial = GetColoredTexturedMaterial (color, texture);
					region.customMaterial = goodMaterial;
					ApplyMaterialToSurface (surf, goodMaterial);
				}
			} else {
				surfMaterial = GetColoredTexturedMaterial (color, texture);
				surf = GenerateTerritoryRegionSurface (territoryIndex, surfMaterial, textureScale, textureOffset, textureRotation, false, useCanvasRect);
				region.customMaterial = surfMaterial;
				region.customTextureOffset = textureOffset;
				region.customTextureScale = textureScale;
				region.customTextureRotation = textureRotation;
			}
			// If it was highlighted, highlight it again
			if (region.customMaterial != null && isHighlighted) {
				if (region.customMaterial != null) {
					hudMatTerritory.mainTexture = region.customMaterial.mainTexture;
				} else {
					hudMatTerritory.mainTexture = null;
				}
				surf.GetComponent<Renderer> ().sharedMaterial = hudMatTerritory;
				_highlightedObj = surf;
			}
			return surf;
		}

		/// <summary>
		/// Specifies if a given cell is visible.
		/// </summary>
		public void TerritorySetVisible (int territoryIndex, bool visible)
		{
			if (territoryIndex < 0 || territoryIndex >= territories.Count)
				return;
			territories [territoryIndex].visible = visible;
		}

		/// <summary>
		/// Returns true if territory is visible
		/// </summary>
		public bool TerritoryIsVisible (int territoryIndex)
		{
			if (territoryIndex < 0 || territoryIndex >= territories.Count)
				return false;
			return territories [territoryIndex].visible;
		}

		/// <summary>
		/// Specifies if a given cell is visible.
		/// </summary>
		public void TerritorySetBorderVisible (int territoryIndex, bool visible) {
			if (territoryIndex < 0 || territoryIndex >= territories.Count)
				return;
			territories [territoryIndex].borderVisible = visible;
		}



		/// <summary>
		/// Returns a list of neighbour cells for specificed cell index.
		/// </summary>
		public List<Territory> TerritoryGetNeighbours (int territoryIndex)
		{
			List<Territory> neighbours = new List<Territory> ();
			Region region = territories [territoryIndex].region;
			for (int k = 0; k < region.neighbours.Count; k++) {
				neighbours.Add ((Territory)region.neighbours [k].entity);
			}
			return neighbours;
		}

		/// <summary>
		/// Colors a territory and fades it out during "duration" in seconds.
		/// </summary>
		public void TerritoryFadeOut (int territoryIndex, Color color, float duration)
		{
			TerritoryAnimate (FADER_STYLE.FadeOut, territoryIndex, color, duration);
		}

		/// <summary>
		/// Flashes a territory with "color" and "duration" in seconds.
		/// </summary>
		public void TerritoryFlash (int territoryIndex, Color color, float duration)
		{
			TerritoryAnimate (FADER_STYLE.Flash, territoryIndex, color, duration);
		}

		/// <summary>
		/// Blinks a territory with "color" and "duration" in seconds.
		/// </summary>
		public void TerritoryBlink (int territoryIndex, Color color, float duration)
		{
			TerritoryAnimate (FADER_STYLE.Blink, territoryIndex, color, duration);
		}

		/// <summary>
		/// Automatically generates territories based on the different colors included in the texture.
		/// </summary>
		/// <param name="neutral">This color won't generate any texture.</param>
		public void CreateTerritories (Texture2D texture, Color neutral)
		{

			if (texture == null || cells == null)
				return;

			List<Color> dsColors = new List<Color> ();
			int cellCount = cells.Count;
			Color[] colors = texture.GetPixels ();
			for (int k = 0; k < cellCount; k++) {
				if (!cells [k].visible)
					continue;
				Vector2 uv = cells [k].center;
				uv.x += 0.5f;
				uv.y += 0.5f;

				int x = (int)(uv.x * texture.width);
				int y = (int)(uv.y * texture.height);
				int pos = y * texture.width + x;
				if (pos < 0 || pos > colors.Length)
					continue;
				Color pixelColor = colors [pos];
				int territoryIndex = dsColors.IndexOf (pixelColor);
				if (territoryIndex < 0) {
					dsColors.Add (pixelColor);
					territoryIndex = dsColors.Count - 1;
				}
				CellSetTerritory (k, territoryIndex);
				if (territoryIndex >= 255)
					break;
			}
			if (dsColors.Count > 0) {
				_numTerritories = dsColors.Count;
				_showTerritories = true;

				if (territories == null) {
					territories = new List<Territory> (_numTerritories);
				} else {
					territories.Clear ();
				}
				for (int c = 0; c < _numTerritories; c++) {
					Territory territory = new Territory (c.ToString ());
					Color territoryColor = dsColors [c];
					if (territoryColor.r != neutral.r || territoryColor.g != neutral.g || territoryColor.b != neutral.b) {
						territory.fillColor = territoryColor;
					} else {
						territory.fillColor = new Color (0, 0, 0, 0);
						territory.visible = false;
					}
					territories.Add (territory);
				}
				lastTerritoryLookupCount = -1;
				isDirty = true;
				Redraw ();
			}
		}

		#endregion
	}
}

