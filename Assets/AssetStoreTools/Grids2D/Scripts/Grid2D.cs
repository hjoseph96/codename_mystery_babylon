using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Grids2D.Geom;

namespace Grids2D {
	
	public enum HIGHLIGHT_MODE {
		None = 0,
		Territories = 1,
		Cells = 2
	}

	public enum OVERLAY_MODE {
		Overlay = 0,
		Ground = 1
	}

	public enum GRID_TOPOLOGY {
		Irregular = 0,
		Box = 1,
		//		Rectangular = 2,	// deprecated: use Box
		Hexagonal = 3
	}

    public enum RENDER_QUEUE {
        Opaque = 0,
        Transparent = 1
    }


	public partial class Grid2D : MonoBehaviour {

		public Texture2D canvasTexture;

		[SerializeField]
		Texture2D _gridMask;

		/// <summary>
		/// Gets or sets the grid mask. The alpha component of this texture is used to determine cells visibility (0 = cell invisible)
		/// </summary>
		public Texture2D gridMask {
			get { return _gridMask; }
			set {
				if (_gridMask != value) {
					_gridMask = value;
					isDirty = true;
					ReloadGridMask ();
				}
			}
		}

		[SerializeField]
		GRID_TOPOLOGY _gridTopology = GRID_TOPOLOGY.Irregular;

		/// <summary>
		/// The grid type (boxed, hexagonal or irregular)
		/// </summary>
		public GRID_TOPOLOGY gridTopology { 
			get { return _gridTopology; } 
			set {
				if (_gridTopology != value) {
					_gridTopology = value;
					GenerateMap ();
					isDirty = true;
				}
			}
		}


        [SerializeField]
        RENDER_QUEUE _renderQueue = RENDER_QUEUE.Opaque;

        /// <summary>
        /// Sets the default rendering queue for grid
        /// </summary>
        public RENDER_QUEUE renderQueue {
            get { return _renderQueue; }
            set { if (_renderQueue != value) {
                    _renderQueue = value;
                    UpdateMaterialProperties();
                }
            }
        }


        [SerializeField]
        int _sortingOrder = 0;

        /// <summary>
        /// Sets the sorting layer for the grid elements (only valid when rendering in transparent queue)
        /// </summary>
        public int sortingOrder {
            get { return _sortingOrder; }
            set {
                if (_sortingOrder != value) {
                    _sortingOrder = value;
                    Redraw();
                }
            }
        }

        [SerializeField]
		int _seed = 1;

		/// <summary>
		/// Randomize seed used to generate cells. Use this to control randomization.
		/// </summary>
		public int seed { 
			get { return _seed; } 
			set {
				if (_seed != value) {
					_seed = value;
					GenerateMap ();
					isDirty = true;
				}
			}
		}

		[SerializeField]
		public bool _regularHexagons;

		public bool regularHexagons {
			get { return _regularHexagons; }
			set {
				if (value != _regularHexagons) {
					_regularHexagons = value;
					isDirty = true;
					Redraw ();
				}
			}
		}

		[SerializeField]
		public float _hexSize = 1f;

		public float hexSize {
			get { return _hexSize; }
			set {
				if (value != _hexSize) {
					_hexSize = value;
					isDirty = true;
					Redraw ();
				}
			}
		}


		[SerializeField]
		int _gridRelaxation = 1;

		/// <summary>
		/// Sets the relaxation iterations used to normalize cells sizes in irregular topology.
		/// </summary>
		public int gridRelaxation { 
			get { return _gridRelaxation; } 
			set {
				if (_gridRelaxation != value) {
					_gridRelaxation = value;
					GenerateMap ();
					isDirty = true;
				}
			}
		}

		float goodGridRelaxation {
			get {
				if (_numCells >= MAX_CELLS_FOR_RELAXATION) {
					return 1;
				} else {
					return _gridRelaxation;
				}
			}
		}

		[SerializeField]
		float _gridCurvature = 0.0f;

		/// <summary>
		/// Gets or sets the grid's curvature factor.
		/// </summary>
		public float gridCurvature { 
			get { return _gridCurvature; } 
			set {
				if (_gridCurvature != value) {
					_gridCurvature = value;
					GenerateMap ();
					isDirty = true;
				}
			}
		}

		float goodGridCurvature {
			get {
				if (_numCells >= MAX_CELLS_FOR_CURVATURE) {
					return 0;
				} else {
					return _gridCurvature;
				}
			}
		}

		[SerializeField]
		HIGHLIGHT_MODE _highlightMode = HIGHLIGHT_MODE.Cells;

		public HIGHLIGHT_MODE highlightMode {
			get {
				return _highlightMode;
			}
			set {
				if (_highlightMode != value) {
					_highlightMode = value;
					isDirty = true;
					HideCellRegionHighlight ();
					HideTerritoryRegionHighlight ();
					CheckCells ();
					CheckTerritories ();
				}
			}
		}

		[SerializeField]
		float _highlightFadeAmount = 0.5f;

		public float highlightFadeAmount {
			get {
				return _highlightFadeAmount;
			}
			set {
				if (_highlightFadeAmount != value) {
					_highlightFadeAmount = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		OVERLAY_MODE _overlayMode = OVERLAY_MODE.Overlay;

		public OVERLAY_MODE overlayMode {
			get {
				return _overlayMode;
			}
			set {
				if (_overlayMode != value) {
					_overlayMode = value;
					isDirty = true;
				}
			}
		}

		
		[SerializeField]
		bool _evenLayout = false;

		/// <summary>
		/// Toggle even corner in hexagonal topology.
		/// </summary>
		public bool evenLayout { 
			get {
				return _evenLayout; 
			}
			set {
				if (value != _evenLayout) {
					_evenLayout = value;
					isDirty = true;
					GenerateMap ();
				}
			}
		}

		[SerializeField]
		int _gridDepthOffset = -1;

		public int gridDepthOffset { 
			get { return _gridDepthOffset; } 
			set {
				if (_gridDepthOffset != value) {
					_gridDepthOffset = value;
					UpdateMaterialProperties ();
					isDirty = true;
				}
			}
		}

		public Texture2D[] textures;

		
		[SerializeField]
		bool
			_respectOtherUI;

		/// <summary>
		/// When enabled, will prevent interaction if pointer is over an UI element
		/// </summary>
		public bool	respectOtherUI {
			get { return _respectOtherUI; }
			set {
				if (value != _respectOtherUI) {
					_respectOtherUI = value;
					isDirty = true;
				}
			}
		}


		[SerializeField]
		Camera _mainCamera;

		/// <summary>
		/// Main camera used for certain computations
		/// </summary>
		public Camera mainCamera {
			get {
				if (_mainCamera == null) {
					_mainCamera = Camera.main;
					if (_mainCamera == null) {
						Debug.LogError ("No camera tagged as Main has been found and no camera is assigned to Grid2D.");
					}
				}
				return _mainCamera; 
			}
			set {
				if (_mainCamera != value) {
					_mainCamera = value;
					isDirty = true;
				}
			}
		}



		#region State variables

		public static Grid2D instance {
			get {
				if (_instance == null) {
					GameObject o = GameObject.Find ("Grid2D");
					if (o != null) {
						_instance = o.GetComponentInChildren<Grid2D> ();
					} else {
						Debug.LogWarning ("Grid2D gameobject not found in the scene!");
					}
				}
				return _instance;
			}
		}

		/// <summary>
		/// Returns a reference of the currently highlighted gameobject (cell or territory)
		/// </summary>
		public GameObject highlightedObj { get { return _highlightedObj; } }


		#endregion


		#region Gameloop events

		void Update () {

			if (Input.touchSupported && Application.isMobilePlatform) {
				if (Input.touchCount == 0)
					return;
				if (Input.GetTouch (0).phase != TouchPhase.Began && Input.GetTouch (0).phase != TouchPhase.Ended)
					return;
			}

			// Check whether the points is on an UI element, then avoid user interaction
			if (_respectOtherUI) { //&& !Input.touchSupported) {
				bool canInteract = true;
				if (UnityEngine.EventSystems.EventSystem.current != null) {
					if (Input.touchSupported && Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (Input.GetTouch (0).fingerId)) {
						canInteract = false;
					} else if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (-1)) {
						canInteract = false;
					}
				}
				if (!canInteract && _highlightMode != HIGHLIGHT_MODE.None) {
					HideTerritoryRegionHighlight ();
					HideCellRegionHighlight ();
					return;
				}
			}

			CheckMousePos (); 		// Verify if mouse enter a territory boundary - we only check if mouse is inside the sphere of world
			CheckUserInteraction (); // Listen to pointer events
		}

		void LateUpdate () {
			UpdateHighlightFade (); 	// Fades current selection
		}


		#endregion

		#region Public API

		/// <summary>
		/// Used to cancel highlighting on a given gameobject. This call is ignored if go is not currently highlighted.
		/// </summary>
		public void HideHighlightedObject (GameObject go) {
			if (go != _highlightedObj)
				return;
			_cellHighlightedIndex = -1;
			_cellHighlighted = null;
			_territoryHighlightedIndex = -1;
			_territoryHighlighted = null;
			_highlightedObj = null;
			if (go != null) {
				go.SetActive(false);
			}
		}

		/// <summary>
		/// Issues a selection check based on a given ray. Used by editor to manipulate cells from Scene window.
		/// </summary>
		public void CheckRay (Ray ray) {
			useEditorRay = true;
			editorRay = ray;
			CheckMousePos ();
		}

		/// <summary>
		/// Draws a line over a cell side.
		/// </summary>
		/// <returns>The line.</returns>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="side">Side.</param>
		/// <param name="color">Color.</param>
		/// <param name="width">Width.</param>
		public GameObject DrawLine (int cellIndex, CELL_SIDE side, Color color, float width) {
			GameObject line = new GameObject ("Line");
			LineRenderer lr = line.AddComponent<LineRenderer> ();
			if (cellLineMat == null)
				cellLineMat = Resources.Load<Material> ("Materials/CellLine") as Material;
			Material mat = Instantiate (cellLineMat) as Material;
			mat.MarkForDisposal ();
			mat.color = color;
			lr.sharedMaterial = mat;
			lr.useWorldSpace = true;
			lr.positionCount = 2;
			lr.startWidth = width;
			lr.endWidth = width;
			int v1, v2;
			if (_gridTopology == GRID_TOPOLOGY.Hexagonal) {
				switch (side) {
				case CELL_SIDE.BottomLeft:
					v1 = 0;
					v2 = 1;
					break;
				case CELL_SIDE.Bottom:
					v1 = 1;
					v2 = 2;
					break;
				case CELL_SIDE.BottomRight:
					v1 = 2;
					v2 = 3;
					break;
				case CELL_SIDE.TopRight:
					v1 = 3;
					v2 = 4;
					break;
				case CELL_SIDE.Top:
					v1 = 4;
					v2 = 5;
					break;
				default: // BottomLeft
					v1 = 5;
					v2 = 0;
					break;
				}
			} else {
				// box
				switch (side) {
				case CELL_SIDE.Left:
					v1 = 3;
					v2 = 0;
					break;
				case CELL_SIDE.Bottom:
					v1 = 0;
					v2 = 1;
					break;
				case CELL_SIDE.Right:
					v1 = 1;
					v2 = 2;
					break;
				default: // top
					v1 = 2;
					v2 = 3;
					break;
				}
			}
			Vector3 offset = transform.forward * 0.05f;
			lr.SetPosition (0, CellGetVertexPosition (cellIndex, v1) - offset);
			lr.SetPosition (1, CellGetVertexPosition (cellIndex, v2) - offset);
			return line;
		}

		/// <summary>
		/// Draws a line connecting two cells centers
		/// </summary>
		/// <returns>The line.</returns>
		/// <param name="cellIndex1">Cell index1.</param>
		/// <param name="cellIndex2">Cell index2.</param>
		/// <param name="color">Color.</param>
		/// <param name="width">Width.</param>
		public GameObject DrawLine (int cellIndex1, int cellIndex2, Color color, float width) {

			GameObject line = new GameObject ("Line");
			LineRenderer lr = line.AddComponent<LineRenderer> ();
			if (cellLineMat == null)
				cellLineMat = Resources.Load<Material> ("Materials/CellLine") as Material;
			Material mat = Instantiate (cellLineMat) as Material;
			mat.MarkForDisposal ();
			mat.color = color;
			lr.sharedMaterial = mat;
			lr.useWorldSpace = true;
			lr.positionCount = 2;
			lr.startWidth = width;
			lr.endWidth = width;
			Vector3 offset = transform.forward * 0.05f;
			lr.SetPosition (0, CellGetPosition (cellIndex1) - offset);
			lr.SetPosition (1, CellGetPosition (cellIndex2) - offset);
			return line;
		}

		
		/// <summary>
		/// Moves a given game object from current position to the center of a destination cell specified by row and column
		/// </summary>
		/// <param name="o">The game object</param>
		/// <param name="cellIndex">Index of the destination cell</param>
		/// <param name="duration">Duration in seconds</param>
		/// <param name="elevation">Optional offset from the grid surface</param>
		public Grid2DMove MoveTo (GameObject o, int row, int column, float duration, float elevation = 0) {
			int destinationCellIndex = CellGetIndex (row, column);
			return MoveTo (o, destinationCellIndex, duration, elevation);
		}

		/// <summary>
		/// Moves a given game object from current position to the center of a destination cell specified by index
		/// </summary>
		/// <param name="o">The game object</param>
		/// <param name="cellIndex">Index of the destination cell</param>
		/// <param name="duration">Duration in seconds</param>
		/// <param name="elevation">Optional offset from the grid surface</param>
		public Grid2DMove MoveTo (GameObject o, int cellIndex, float duration, float elevation = 0) {
			List<int> positions = new List<int> ();
			positions.Add (cellIndex);
			return MoveTo (o, positions, duration, elevation);
		}

		/// <summary>
		/// Moves a given game object from current position to the center of a destination cell specified by row and column
		/// </summary>
		/// <param name="o">The game object</param>
		/// <param name="cellIndex">Index of the destination cell</param>
		/// <param name="duration">Duration in seconds</param>
		/// <param name="elevation">Optional offset from the grid surface</param>
		public Grid2DMove MoveTo (GameObject o, List<int> positions, float duration, float elevation = 0) {
			Grid2DMove mv = o.AddComponent<Grid2DMove> ();
			mv.grid = this;
			mv.positions = positions;
			mv.duration = duration;
			mv.elevation = elevation;
			return mv;
		}



		public void ReloadGridMask () {
			ReadMaskContents (); 
			CellsApplyMask ();
			recreateTerritories = true;
			Redraw ();
			if (territoriesTexture != null) {
				CreateTerritories (territoriesTexture, territoriesTextureNeutralColor);
			}
		}

		#endregion


	
	}
}

