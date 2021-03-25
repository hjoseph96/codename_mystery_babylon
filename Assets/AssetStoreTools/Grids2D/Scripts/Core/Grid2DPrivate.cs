//#define HIGHLIGHT_NEIGHBOURS
//#define SHOW_DEBUG_GIZMOS

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Grids2D.Geom;
using Grids2D.Poly2Tri;

namespace Grids2D {

    [Serializable]
    [ExecuteInEditMode]
    public partial class Grid2D : MonoBehaviour {

        // internal fields
        const int MAP_LAYER = 5;
        const int IGNORE_RAYCAST = 2;
        const double MIN_VERTEX_DISTANCE = 0.002;
        const double SQR_MIN_VERTEX_DIST = 0.0002 * 0.0002;
        Rect canvasRect = new Rect(-0.5f, -0.5f, 1, 1);

        // Custom inspector stuff
        public const int MAX_TERRITORIES = 256;
        public const int MAX_CELLS = 10000;
        public const int MAX_CELLS_SQRT = 100;
        public bool isDirty;
        public const int MAX_CELLS_FOR_CURVATURE = 500;
        public const int MAX_CELLS_FOR_RELAXATION = 500;

        // Materials and resources
        Material territoriesMat, cellsMat, hudMatTerritoryOverlay, hudMatTerritoryGround, hudMatCellOverlay, hudMatCellGround;
        Material coloredMat, texturizedMat;
        Material cellLineMat;

        // Cell mesh data
        const string CELLS_LAYER_NAME = "Cells";
        Vector3[][] cellMeshBorders;
        int[][] cellMeshIndices;
        Dictionary<Segment, Region> cellNeighbourHit;
        bool recreateCells, recreateTerritories;
        Dictionary<int, Cell> cellTagged;
        bool needUpdateTerritories;

        // Territory mesh data
        const string TERRITORIES_LAYER_NAME = "Territories";
        Vector3[][] territoryMeshBorders;
        Dictionary<Segment, Region> territoryNeighbourHit;
        int[][] territoryMeshIndices;
        List<Segment> territoryFrontiers;
        List<Territory> _sortedTerritories;

        // Common territory & cell structures
        List<Vector3> frontiersPoints;
        Dictionary<Segment, bool> segmentHit;
        Dictionary<TriangulationPoint, int> surfaceMeshHit;
        Dictionary<Vector2, int> surfaceMeshHitVector2;
        List<Vector3> meshPoints;
        int[] triNew;
        int triNewIndex;
        int newPointsCount;

        // Placeholders and layers
        GameObject territoryLayer;
        GameObject _surfacesLayer;

        GameObject surfacesLayer {
            get {
                if (_surfacesLayer == null)
                    CreateSurfacesLayer();
                return _surfacesLayer;
            }
        }

        GameObject _highlightedObj;
        GameObject cellLayer;

        // Caches
        Dictionary<int, GameObject> surfaces;
        Dictionary<Cell, int> _cellLookup;
        int lastCellLookupCount = -1;
        Dictionary<Territory, int> _territoryLookup;
        int lastTerritoryLookupCount = -1;
        Dictionary<Color, Material> coloredMatCache;
        Color[] factoryColors;
        bool refreshCellMesh, refreshTerritoriesMesh;
        List<Cell> _sortedCells;

        // Interaction
        static Grid2D _instance;
        public bool mouseIsOver;
        Territory _territoryHighlighted;
        int _territoryHighlightedIndex = -1;
        Cell _cellHighlighted;
        int _cellHighlightedIndex = -1;
        float highlightFadeStart;
        int _territoryLastClickedIndex = -1, _cellLastClickedIndex = -1;

        // Misc
        int _lastVertexCount = 0;
        Color[] mask;
        bool useEditorRay;
        Ray editorRay;

        public int lastVertexCount { get { return _lastVertexCount; } }

        Dictionary<Cell, int> cellLookup {
            get {
                if (_cellLookup != null && cells.Count == lastCellLookupCount)
                    return _cellLookup;
                if (_cellLookup == null) {
                    _cellLookup = new Dictionary<Cell, int>();
                } else {
                    _cellLookup.Clear();
                }
                lastCellLookupCount = cells.Count;
                for (int k = 0; k < lastCellLookupCount; k++) {
                    _cellLookup.Add(cells[k], k);
                }
                return _cellLookup;
            }
        }


        List<Cell> sortedCells {
            get {
                if (_sortedCells == null || _sortedCells.Count != cells.Count) {
                    UpdateSortedCells();
                }
                return _sortedCells;
            }
        }


        int layerMask { get { return 1 << MAP_LAYER; } }

        List<Territory> sortedTerritories {
            get {
                if (_sortedTerritories == null || _sortedTerritories.Count != territories.Count) {
                    UpdateSortedTerritories();
                }
                return _sortedTerritories;
            }
        }

        Dictionary<Territory, int> territoryLookup {
            get {
                if (_territoryLookup != null && territories.Count == lastTerritoryLookupCount)
                    return _territoryLookup;
                if (_territoryLookup == null) {
                    _territoryLookup = new Dictionary<Territory, int>();
                } else {
                    _territoryLookup.Clear();
                }
                int terCount = territories.Count;
                for (int k = 0; k < terCount; k++) {
                    _territoryLookup.Add(territories[k], k);
                }
                lastTerritoryLookupCount = terCount;
                return _territoryLookup;
            }
        }



        #region Gameloop events

        void OnEnable() {
            if (cells == null || territories == null) {
                Init();
            }
            if (hudMatTerritoryOverlay != null && hudMatTerritoryOverlay.color != _territoryHighlightColor) {
                hudMatTerritoryOverlay.color = _territoryHighlightColor;
            }
            if (hudMatTerritoryGround != null && hudMatTerritoryGround.color != _territoryHighlightColor) {
                hudMatTerritoryGround.color = _territoryHighlightColor;
            }
            if (hudMatCellOverlay != null && hudMatCellOverlay.color != _cellHighlightColor) {
                hudMatCellOverlay.color = _cellHighlightColor;
            }
            if (hudMatCellGround != null && hudMatCellGround.color != _cellHighlightColor) {
                hudMatCellGround.color = _cellHighlightColor;
            }
            if (territoriesMat != null && territoriesMat.color != _territoryFrontierColor) {
                territoriesMat.color = _territoryFrontierColor;
            }
            if (cellsMat != null && cellsMat.color != _cellBorderColor) {
                cellsMat.color = _cellBorderColor;
            }
            UpdateMaterialProperties();
        }

        void OnDestroy() {
            DisposalManager.DisposeAll();
        }

        #endregion



        #region Initialization

        public void Init() {
#if UNITY_EDITOR
#if UNITY_2018_3_OR_NEWER
			UnityEditor.PrefabInstanceStatus prefabInstanceStatus = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(gameObject);
			if (prefabInstanceStatus != UnityEditor.PrefabInstanceStatus.NotAPrefab) {
            UnityEditor.EditorApplication.delayCall += () =>
			UnityEditor.PrefabUtility.UnpackPrefabInstance(gameObject, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
			}
#else
            UnityEditor.PrefabType prefabType = UnityEditor.PrefabUtility.GetPrefabType(gameObject);
            if (prefabType != UnityEditor.PrefabType.None && prefabType != UnityEditor.PrefabType.DisconnectedPrefabInstance && prefabType != UnityEditor.PrefabType.DisconnectedModelPrefabInstance) {
                UnityEditor.PrefabUtility.DisconnectPrefabInstance(gameObject);
            }
#endif
#endif


            gameObject.layer = MAP_LAYER;

            if (territoriesMat == null) {
                territoriesMat = Instantiate(Resources.Load<Material>("Materials/Territory"));
                territoriesMat.MarkForDisposal();
            }
            if (cellsMat == null) {
                cellsMat = Instantiate(Resources.Load<Material>("Materials/Cell"));
                cellsMat.MarkForDisposal();
            }
            if (hudMatTerritoryOverlay == null) {
                hudMatTerritoryOverlay = Instantiate(Resources.Load<Material>("Materials/HudTerritoryOverlay"));
                hudMatTerritoryOverlay.MarkForDisposal();
            }
            if (hudMatTerritoryGround == null) {
                hudMatTerritoryGround = Instantiate(Resources.Load<Material>("Materials/HudTerritoryGround"));
                hudMatTerritoryGround.MarkForDisposal();
            }
            if (hudMatCellOverlay == null) {
                hudMatCellOverlay = Instantiate(Resources.Load<Material>("Materials/HudCellOverlayTex"));
                hudMatCellOverlay.MarkForDisposal();
            }
            if (hudMatCellGround == null) {
                hudMatCellGround = Instantiate(Resources.Load<Material>("Materials/HudCellGroundTex"));
                hudMatCellGround.MarkForDisposal();
            }
            if (coloredMat == null) {
                coloredMat = Instantiate(Resources.Load<Material>("Materials/ColorizedRegion"));
                coloredMat.MarkForDisposal();
            }
            if (texturizedMat == null) {
                texturizedMat = Instantiate(Resources.Load<Material>("Materials/TexturizedRegion"));
                texturizedMat.MarkForDisposal();
            }
            coloredMatCache = new Dictionary<Color, Material>();
            UpdateMaterialProperties();

#if UNITY_5_5_OR_NEWER
            UnityEngine.Random.InitState(seed);
#else
			UnityEngine.Random.seed = seed;
#endif
            if (factoryColors == null || factoryColors.Length < MAX_CELLS) {
                factoryColors = new Color[MAX_CELLS];
                for (int k = 0; k < factoryColors.Length; k++)
                    factoryColors[k] = new Color(UnityEngine.Random.Range(0.0f, 0.5f), UnityEngine.Random.Range(0.0f, 0.5f), UnityEngine.Random.Range(0.0f, 0.5f));
            }

            if (textures == null || textures.Length == 0)
                textures = new Texture2D[32];

            ReadMaskContents();
            Redraw();

            if (territoriesTexture != null) {
                CreateTerritories(territoriesTexture, territoriesTextureNeutralColor);
            }
        }

        void CreateSurfacesLayer() {
            Transform t = transform.Find("Surfaces");
            if (t != null) {
                DestroyImmediate(t.gameObject);
            }
            _surfacesLayer = new GameObject("Surfaces");
            _surfacesLayer.transform.SetParent(transform, false);
            _surfacesLayer.transform.localPosition = Vector3.zero; // Vector3.back * 0.01f;
            _surfacesLayer.layer = gameObject.layer;
        }

        void DestroySurfaces() {
            HideTerritoryRegionHighlight();
            HideCellRegionHighlight();
            if (segmentHit != null)
                segmentHit.Clear();
            if (surfaces != null)
                surfaces.Clear();
            if (_surfacesLayer != null)
                DestroyImmediate(_surfacesLayer);
        }

        void ReadMaskContents() {
            if (_gridMask == null)
                return;
            try {
                mask = _gridMask.GetPixels();
            } catch {
                mask = null;
                Debug.Log("Mask texture is not readable. Check import settings.");
            }
        }


        #endregion

        #region Map generation

        Point[] centers;
        VoronoiFortune voronoi;

        void SetupIrregularGrid() {
            bool usesUserDefinedSites = false;
            if (_voronoiSites != null && _voronoiSites.Count > 0) {
                _numCells = _voronoiSites.Count;
                usesUserDefinedSites = true;
            }
            if (centers == null || centers.Length != _numCells) {
                centers = new Point[_numCells];
            }
            for (int k = 0; k < centers.Length; k++) {
                if (usesUserDefinedSites) {
                    Vector2 p = _voronoiSites[k];
                    centers[k] = new Point(p.x, p.y);
                } else {
                    centers[k] = new Point(UnityEngine.Random.Range(-0.49f, 0.49f), UnityEngine.Random.Range(-0.49f, 0.49f));
                }
            }

            if (voronoi == null) {
                voronoi = new VoronoiFortune();
            }
            for (int k = 0; k < goodGridRelaxation; k++) {
                voronoi.AssignData(centers);
                voronoi.DoVoronoi();
                if (k < goodGridRelaxation - 1) {
                    for (int j = 0; j < _numCells; j++) {
                        Point centroid = voronoi.cells[j].centroid;
                        centers[j] = (centers[j] + centroid) / 2;
                    }
                }
            }

            // Make cell regions: we assume cells have only 1 region but that can change in the future
            float curvature = goodGridCurvature;
            int cellCount = 0;
            for (int k = 0; k < voronoi.cells.Length; k++) {
                VoronoiCell voronoiCell = voronoi.cells[k];
                Cell cell = new Cell(voronoiCell.center.vector2);
                Region cr = new Region(cell);
                if (curvature > 0) {
                    cr.polygon = voronoiCell.GetPolygon(3, curvature);
                } else {
                    cr.polygon = voronoiCell.GetPolygon(1, 0);
                }
                if (cr.polygon != null) {
                    // Add segments
                    int segmentsCount = voronoiCell.segments.Count;
                    for (int i = 0; i < segmentsCount; i++) {
                        Segment s = voronoiCell.segments[i];
                        if (!s.deleted) {
                            if (curvature > 0) {
                                cr.segments.AddRange(s.subdivisions);
                            } else {
                                cr.segments.Add(s);
                            }
                        }
                    }
                    cell.region = cr;
                    cell.index = cellCount++;
                    cells.Add(cell);
                }
            }
        }

        void SetupBoxGrid(bool strictQuads) {

            int qx = _cellColumnCount;
            int qy = _cellRowCount;

            double stepX = 1.0 / qx;
            double stepY = 1.0 / qy;

            double halfStepX = stepX * 0.5;
            double halfStepY = stepY * 0.5;

            Segment[,,] sides = new Segment[qx, qy, 4]; // 0 = left, 1 = top, 2 = right, 3 = bottom
            int subdivisions = goodGridCurvature > 0 ? 3 : 1;
            int count = 0;
            for (int j = 0; j < qy; j++) {
                for (int k = 0; k < qx; k++) {
                    Point center = new Point((double)k / qx - 0.5 + halfStepX, (double)j / qy - 0.5 + halfStepY);
                    Cell cell = new Cell(new Vector2((float)center.x, (float)center.y));
                    cell.column = k;
                    cell.row = j;

                    Segment left = k > 0 ? sides[k - 1, j, 2] : new Segment(center.Offset(-halfStepX, -halfStepY), center.Offset(-halfStepX, halfStepY), true);
                    sides[k, j, 0] = left;

                    Segment top = new Segment(center.Offset(-halfStepX, halfStepY), center.Offset(halfStepX, halfStepY), j == qy - 1);
                    sides[k, j, 1] = top;

                    Segment right = new Segment(center.Offset(halfStepX, halfStepY), center.Offset(halfStepX, -halfStepY), k == qx - 1);
                    sides[k, j, 2] = right;

                    Segment bottom = j > 0 ? sides[k, j - 1, 1] : new Segment(center.Offset(halfStepX, -halfStepY), center.Offset(-halfStepX, -halfStepY), true);
                    sides[k, j, 3] = bottom;

                    Region cr = new Region(cell);
                    if (subdivisions > 1) {
                        cr.segments.AddRange(top.Subdivide(subdivisions, _gridCurvature));
                        cr.segments.AddRange(right.Subdivide(subdivisions, _gridCurvature));
                        cr.segments.AddRange(bottom.Subdivide(subdivisions, _gridCurvature));
                        cr.segments.AddRange(left.Subdivide(subdivisions, _gridCurvature));
                    } else {
                        cr.segments.Add(top);
                        cr.segments.Add(right);
                        cr.segments.Add(bottom);
                        cr.segments.Add(left);
                    }
                    Connector connector = new Connector();
                    connector.AddRange(cr.segments);
                    cr.polygon = connector.ToPolygon(); // FromLargestLineStrip();
                    if (cr.polygon != null) {
                        cell.region = cr;
                        cell.index = count++;
                        cells.Add(cell);
                    }
                }
            }
        }

        void SetupHexagonalGrid() {

            double qx = 1.0 + (_cellColumnCount - 1.0) * 3.0 / 4.0;
            double qy = _cellRowCount + 0.5;
            int qy2 = _cellRowCount;
            int qx2 = _cellColumnCount;

            double stepX = 1.0 / qx;
            double stepY = 1.0 / qy;

            double halfStepX = stepX * 0.5;
            double halfStepY = stepY * 0.5;
            int evenLayout = _evenLayout ? 1 : 0;

            Segment[,,] sides = new Segment[qx2, qy2, 6]; // 0 = left-up, 1 = top, 2 = right-up, 3 = right-down, 4 = down, 5 = left-down
            int subdivisions = goodGridCurvature > 0 ? 3 : 1;
            int cellCount = 0;
            for (int j = 0; j < qy2; j++) {
                for (int k = 0; k < qx2; k++) {
                    Point center = new Point((double)k / qx - 0.5 + halfStepX, (double)j / qy - 0.5 + stepY);
                    center.x -= k * halfStepX / 2;
                    Cell cell = new Cell(new Vector2((float)center.x, (float)center.y));
                    cell.column = k;
                    cell.row = j;

                    double offsetY = (k % 2 == evenLayout) ? 0 : -halfStepY;

                    Segment leftUp = (k > 0 && offsetY < 0) ? sides[k - 1, j, 3] : new Segment(center.Offset(-halfStepX, offsetY), center.Offset(-halfStepX / 2, halfStepY + offsetY), k == 0 || (j == qy2 - 1 && offsetY == 0));
                    sides[k, j, 0] = leftUp;

                    Segment top = new Segment(center.Offset(-halfStepX / 2, halfStepY + offsetY), center.Offset(halfStepX / 2, halfStepY + offsetY), j == qy2 - 1);
                    sides[k, j, 1] = top;

                    Segment rightUp = new Segment(center.Offset(halfStepX / 2, halfStepY + offsetY), center.Offset(halfStepX, offsetY), k == qx2 - 1 || (j == qy2 - 1 && offsetY == 0));
                    sides[k, j, 2] = rightUp;

                    Segment rightDown = (j > 0 && k < qx2 - 1 && offsetY < 0) ? sides[k + 1, j - 1, 0] : new Segment(center.Offset(halfStepX, offsetY), center.Offset(halfStepX / 2, -halfStepY + offsetY), (j == 0 && offsetY < 0) || k == qx2 - 1);
                    sides[k, j, 3] = rightDown;

                    Segment bottom = j > 0 ? sides[k, j - 1, 1] : new Segment(center.Offset(halfStepX / 2, -halfStepY + offsetY), center.Offset(-halfStepX / 2, -halfStepY + offsetY), true);
                    sides[k, j, 4] = bottom;

                    Segment leftDown;
                    if (offsetY < 0 && j > 0 && k > 0) {
                        leftDown = sides[k - 1, j - 1, 2];
                    } else if (offsetY == 0 && k > 0) {
                        leftDown = sides[k - 1, j, 2];
                    } else {
                        leftDown = new Segment(center.Offset(-halfStepX / 2, -halfStepY + offsetY), center.Offset(-halfStepX, offsetY), true);
                    }
                    sides[k, j, 5] = leftDown;

                    cell.center += Vector2.up * (float)offsetY;

                    Region cr = new Region(cell);
                    if (subdivisions > 1) {
                        if (!top.deleted)
                            cr.segments.AddRange(top.Subdivide(subdivisions, _gridCurvature));
                        if (!rightUp.deleted)
                            cr.segments.AddRange(rightUp.Subdivide(subdivisions, _gridCurvature));
                        if (!rightDown.deleted)
                            cr.segments.AddRange(rightDown.Subdivide(subdivisions, _gridCurvature));
                        if (!bottom.deleted)
                            cr.segments.AddRange(bottom.Subdivide(subdivisions, _gridCurvature));
                        if (!leftDown.deleted)
                            cr.segments.AddRange(leftDown.Subdivide(subdivisions, _gridCurvature));
                        if (!leftUp.deleted)
                            cr.segments.AddRange(leftUp.Subdivide(subdivisions, _gridCurvature));
                    } else {
                        if (!top.deleted)
                            cr.segments.Add(top);
                        if (!rightUp.deleted)
                            cr.segments.Add(rightUp);
                        if (!rightDown.deleted)
                            cr.segments.Add(rightDown);
                        if (!bottom.deleted)
                            cr.segments.Add(bottom);
                        if (!leftDown.deleted)
                            cr.segments.Add(leftDown);
                        if (!leftUp.deleted)
                            cr.segments.Add(leftUp);
                    }
                    Connector connector = new Connector();
                    connector.AddRange(cr.segments);
                    cr.polygon = connector.ToPolygon();
                    if (cr.polygon != null) {
                        cell.region = cr;
                        cell.index = cellCount++;
                        cells.Add(cell);
                    }
                }
            }
        }


        void CreateCells() {

#if UNITY_5_5_OR_NEWER
            UnityEngine.Random.InitState(seed);
#else
			UnityEngine.Random.seed = seed;
#endif

            _numCells = Mathf.Clamp(_numCells, Mathf.Max(_numTerritories, 2), MAX_CELLS);
            if (cells == null) {
                cells = new List<Cell>(_numCells);
            } else {
                cells.Clear();
            }
            if (cellTagged == null)
                cellTagged = new Dictionary<int, Cell>();
            else
                cellTagged.Clear();
            lastCellLookupCount = -1;

            switch (_gridTopology) {
                case GRID_TOPOLOGY.Box:
                    SetupBoxGrid(true);
                    break;
                case GRID_TOPOLOGY.Hexagonal:
                    SetupHexagonalGrid();
                    break;
                default:
                    SetupIrregularGrid();
                    break;
            }

            CellsFindNeighbours();
            CellsUpdateBounds();
            CellsApplyMask();

            // Update sorted cell list
            UpdateSortedCells();
            recreateCells = false;
            needRefreshRouteMatrix = true;
        }

        void UpdateSortedCells() {
            if (_sortedCells == null) {
                _sortedCells = new List<Cell>(cells);
            } else {
                _sortedCells.Clear();
                _sortedCells.AddRange(cells);
            }
            _sortedCells.Sort((cell1, cell2) => {
                return cell1.region.rect2DArea.CompareTo(cell2.region.rect2DArea);
            });
        }


        /// <summary>
        /// Takes the center of each cell and checks the alpha component of the mask to confirm visibility
        /// </summary>
        void CellsApplyMask() {
            int cellsCount = cells.Count;
            if (gridMask == null || mask == null) {
                for (int k = 0; k < cellsCount; k++)
                    cells[k].visible = true;
                return;
            }

            int tw = gridMask.width;
            int th = gridMask.height;

            for (int k = 0; k < cellsCount; k++) {
                Cell cell = cells[k];
                int pointCount = cell.region.points.Count;
                bool visible = false;
                for (int v = 0; v < pointCount; v++) {
                    Vector2 p = cell.region.points[v];
                    float y = p.y + 0.5f;
                    float x = p.x + 0.5f;
                    int ty = (int)(y * th);
                    int tx = (int)(x * tw);
                    if (ty >= 0 && ty < th && tx >= 0 && tx < tw && mask[ty * tw + tx].a > 0) {
                        visible = true;
                        break;
                    }
                }
                cell.visible = visible;
            }
            needRefreshRouteMatrix = true;
        }



        void CellsUpdateBounds() {
            // Update cells polygon
            for (int k = 0; k < cells.Count; k++) {
                CellUpdateBounds(cells[k]);
            }
        }


        void CellUpdateBounds(Cell cell) {
            if (cell.region.polygon.contours.Count == 0)
                return;
            List<Vector2> points = cell.region.polygon.contours[0].GetVector2Points();
            cell.region.points = points;
            // Update bounding rect
            float minx, miny, maxx, maxy;
            minx = miny = float.MaxValue;
            maxx = maxy = float.MinValue;
            int pointCount = points.Count;
            for (int p = 0; p < pointCount; p++) {
                Vector2 point = points[p];
                if (point.x < minx)
                    minx = point.x;
                if (point.x > maxx)
                    maxx = point.x;
                if (point.y < miny)
                    miny = point.y;
                if (point.y > maxy)
                    maxy = point.y;
            }
            float rectWidth = maxx - minx;
            float rectHeight = maxy - miny;
            cell.region.rect2D = new Rect(minx, miny, rectWidth, rectHeight);
            cell.region.rect2DArea = rectWidth * rectHeight;

            if (_sortedCells != null)
                _sortedCells.Clear();
        }


        /// <summary>
        /// Must be called after changing one cell geometry.
        /// </summary>
        void UpdateCellGeometry(Cell cell, Grids2D.Geom.Polygon poly) {
            // Copy new polygon definition
            cell.region.polygon = poly;
            // Update segments list
            cell.region.segments.Clear();
            List<Segment> segmentCache = new List<Segment>(cellNeighbourHit.Keys);
            Grids2D.Geom.Contour contour0 = poly.contours[0];
            int pointCount = contour0.points.Count;
            int segmentCacheCount = segmentCache.Count;
            for (int k = 0; k < pointCount; k++) {
                Segment s = contour0.GetSegment(k);
                bool found = false;
                // Search this segment in the segment cache
                for (int j = 0; j < segmentCacheCount; j++) {
                    Segment o = segmentCache[j];
                    if ((Point.EqualsBoth(o.start, s.start) && Point.EqualsBoth(o.end, s.end)) || (Point.EqualsBoth(o.end, s.start) && Point.EqualsBoth(o.start, s.end))) {
                        cell.region.segments.Add(o);
                        //						((Cell)cellNeighbourHit[o].entity).territoryIndex = cell.territoryIndex; // updates the territory index of this segment in the cache 
                        o.territoryIndex = cell.territoryIndex;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    cell.region.segments.Add(s);
            }
            // Refresh neighbours
            CellsUpdateNeighbours();
            // Refresh rect2D
            CellUpdateBounds(cell);

            // Refresh territories
            if (_enableTerritories) {
                FindTerritoryFrontiers();
                UpdateTerritoryBoundaries();
            }
        }

        void CellsUpdateNeighbours() {
            int cellCount = cells.Count;
            for (int k = 0; k < cellCount; k++) {
                cells[k].region.neighbours.Clear();
            }
            CellsFindNeighbours();
        }


        void CellsFindNeighbours() {

            if (cellNeighbourHit == null) {
                cellNeighbourHit = new Dictionary<Segment, Region>(50000);
            } else {
                cellNeighbourHit.Clear();
            }
            int cellsCount = cells.Count;
            for (int k = 0; k < cellsCount; k++) {
                Cell cell = cells[k];
                Region region = cell.region;
                int numSegments = region.segments.Count;
                for (int i = 0; i < numSegments; i++) {
                    Segment seg = region.segments[i];
                    if (cellNeighbourHit.ContainsKey(seg)) {
                        Region neighbour = cellNeighbourHit[seg];
                        if (neighbour != region) {
                            if (!region.neighbours.Contains(neighbour)) {
                                region.neighbours.Add(neighbour);
                                neighbour.neighbours.Add(region);
                            }
                        }
                    } else {
                        cellNeighbourHit.Add(seg, region);
                    }
                }
            }
        }


        void UpdateSortedTerritories() {
            if (_sortedTerritories == null) {
                _sortedTerritories = new List<Territory>(territories);
            } else {
                _sortedTerritories.Clear();
                _sortedTerritories.AddRange(territories);
            }
            _sortedTerritories.Sort(delegate (Territory x, Territory y) {
                return x.region.rect2DArea.CompareTo(y.region.rect2DArea);
            });
        }

        void FindTerritoryFrontiers() {

            if (territories == null || territories.Count == 0)
                return;

            if (territoryFrontiers == null) {
                territoryFrontiers = new List<Segment>(cellNeighbourHit.Count);
            } else {
                territoryFrontiers.Clear();
            }
            if (territoryNeighbourHit == null) {
                territoryNeighbourHit = new Dictionary<Segment, Region>(50000);
            } else {
                territoryNeighbourHit.Clear();
            }
            int terrCount = territories.Count;
            Connector[] connectors = new Connector[terrCount];
            for (int k = 0; k < terrCount; k++) {
                connectors[k] = new Connector();
                Territory territory = territories[k];
                territory.cells.Clear();
                if (territory.region == null) {
                    Region territoryRegion = new Region(territory);
                    territory.region = territoryRegion;
                }
                territories[k].region.neighbours.Clear();
            }

            int cellCount = cells.Count;
            for (int k = 0; k < cellCount; k++) {
                Cell cell = cells[k];
                if (cell.territoryIndex >= terrCount)
                    continue;
                bool validCell = cell.visible && cell.territoryIndex >= 0;
                if (validCell)
                    territories[cell.territoryIndex].cells.Add(cell);
                Region region = cell.region;
                int numSegments = region.segments.Count;
                for (int i = 0; i < numSegments; i++) {
                    Segment seg = region.segments[i];
                    if (seg.border) {
                        if (validCell) {
                            territoryFrontiers.Add(seg);
                            int territory1 = cell.territoryIndex;
                            connectors[territory1].Add(seg);
                            seg.territoryIndex = territory1;
                        }
                        continue;
                    }
                    //					if (territoryNeighbourHit.ContainsKey (seg)) {
                    //						Region neighbour = territoryNeighbourHit [seg];
                    //						Cell neighbourCell = (Cell)neighbour.entity;
                    //						if (neighbourCell.territoryIndex!=cell.territoryIndex) {
                    //							territoryFrontiers.Add (seg);
                    //							if (validCell) {
                    //								int territory1 = cell.territoryIndex;
                    //								connectors[territory1].Add (seg);
                    //							}
                    //							int territory2 = neighbourCell.territoryIndex;
                    //							if (territory2>=0) {
                    //								connectors[territory2].Add (seg);
                    //							}
                    //						}
                    //					} else {
                    //						territoryNeighbourHit.Add (seg, region);
                    //					}
                    if (territoryNeighbourHit.ContainsKey(seg)) {
                        Region neighbour = territoryNeighbourHit[seg];
                        Cell neighbourCell = (Cell)neighbour.entity;
                        int territory1 = cell.territoryIndex;
                        int territory2 = neighbourCell.territoryIndex;
                        if (territory2 != territory1) {
                            territoryFrontiers.Add(seg);
                            if (validCell) {
                                connectors[territory1].Add(seg);
                                seg.territoryIndex = (territory2 >= 0) ? -1 : territory1;   // if segment belongs to a visible cell and valid territory2, mark this segment as disputed. Otherwise make it part of territory1
                                if (seg.territoryIndex < 0) {
                                    // add territory neigbhours
                                    Region territory1Region = territories[territory1].region;
                                    Region territory2Region = territories[territory2].region;
                                    if (!territory1Region.neighbours.Contains(territory2Region)) {
                                        territory1Region.neighbours.Add(territory2Region);
                                    }
                                    if (!territory2Region.neighbours.Contains(territory1Region)) {
                                        territory2Region.neighbours.Add(territory1Region);
                                    }
                                }
                            }
                            if (territory2 >= 0) {
                                connectors[territory2].Add(seg);
                            }
                        }
                    } else {
                        territoryNeighbourHit[seg] = region;
                        seg.territoryIndex = cell.territoryIndex;
                    }
                }
            }

            for (int k = 0; k < terrCount; k++) {
                if (territories[k].region == null) {
                    territories[k].region = new Region(territories[k]);
                } else {
                    territories[k].region.entity = territories[k];
                }
                if (territories[k].cells.Count > 0) {
                    territories[k].region.polygon = connectors[k].ToPolygonFromLargestLineStrip();
                } else {
                    territories[k].region.polygon = null;
                }
            }

        }

        void GenerateCellsMesh() {

            if (segmentHit == null) {
                segmentHit = new Dictionary<Segment, bool>(50000);
            } else {
                segmentHit.Clear();
            }

            if (frontiersPoints == null) {
                frontiersPoints = new List<Vector3>(100000);
            } else {
                frontiersPoints.Clear();
            }

            int cellsCount = cells.Count;
            for (int k = 0; k < cellsCount; k++) {
                Cell cell = cells[k];
                if (cell.visible && cell.borderVisible) {
                    Region region = cell.region;
                    int numSegments = region.segments.Count;
                    for (int i = 0; i < numSegments; i++) {
                        Segment s = region.segments[i];
                        if (!segmentHit.ContainsKey(s)) {
                            segmentHit[s] = true;
                            frontiersPoints.Add(s.startToVector3);
                            frontiersPoints.Add(s.endToVector3);
                        }
                    }
                }
            }


            int meshGroups = (frontiersPoints.Count / 65000) + 1;
            int meshIndex = -1;
            if (cellMeshIndices == null || cellMeshIndices.GetUpperBound(0) != meshGroups - 1) {
                cellMeshIndices = new int[meshGroups][];
                cellMeshBorders = new Vector3[meshGroups][];
            }
            if (frontiersPoints.Count == 0) {
                cellMeshBorders[0] = new Vector3[0];
                cellMeshIndices[0] = new int[0];
            } else {
                for (int k = 0; k < frontiersPoints.Count; k += 65000) {
                    int max = Mathf.Min(frontiersPoints.Count - k, 65000);
                    ++meshIndex;
                    if (cellMeshBorders[meshIndex] == null || cellMeshBorders[0].GetUpperBound(0) != max - 1) {
                        cellMeshBorders[meshIndex] = new Vector3[max];
                        cellMeshIndices[meshIndex] = new int[max];
                    }
                    for (int j = 0; j < max; j++) {
                        cellMeshBorders[meshIndex][j] = frontiersPoints[j + k];
                        cellMeshIndices[meshIndex][j] = j;
                    }
                }
            }
        }

        void CreateTerritories() {

            _numTerritories = Mathf.Clamp(_numTerritories, 1, MAX_CELLS);

            if (!_enableTerritories) {
                if (territories != null)
                    territories.Clear();
                if (territoryLayer != null)
                    DestroyImmediate(territoryLayer);
                return;
            }

            if (territories == null) {
                territories = new List<Territory>(_numTerritories);
            } else {
                territories.Clear();
            }

            CheckCells();
            // Freedom for the cells!...
            for (int k = 0; k < cells.Count; k++) {
                cells[k].territoryIndex = -1;
            }

#if UNITY_5_5_OR_NEWER
            UnityEngine.Random.InitState(seed);
#else
			UnityEngine.Random.seed = seed;
#endif

            for (int c = 0; c < _numTerritories; c++) {
                Territory territory = new Territory(c.ToString());
                territory.fillColor = factoryColors[c];
                int territoryIndex = territories.Count;
                int p = UnityEngine.Random.Range(0, cells.Count);
                int z = 0;
                while ((cells[p].territoryIndex != -1 || !cells[p].visible) && z++ <= cells.Count) {
                    p++;
                    if (p >= cells.Count)
                        p = 0;
                }
                if (z > cells.Count)
                    break; // no more territories can be found - this should not happen
                Cell prov = cells[p];
                prov.territoryIndex = territoryIndex;
                territory.center = prov.center;
                territory.cells.Add(prov);
                territories.Add(territory);
            }

            // Continue conquering cells
            int[] territoryCellIndex = new int[territories.Count];

            // Iterate one cell per country (this is not efficient but ensures balanced distribution)
            bool remainingCells = true;
            while (remainingCells) {
                remainingCells = false;
                for (int k = 0; k < _numTerritories; k++) {
                    Territory territory = territories[k];
                    int territoryCellsCount = territory.cells.Count;
                    for (int p = territoryCellIndex[k]; p < territoryCellsCount; p++) {
                        Region cellRegion = territory.cells[p].region;
                        int cellRegionNeighboursCount = cellRegion.neighbours.Count;
                        for (int n = 0; n < cellRegionNeighboursCount; n++) {
                            Region otherRegion = cellRegion.neighbours[n];
                            Cell otherProv = (Cell)otherRegion.entity;
                            if (otherProv.territoryIndex == -1 && otherProv.visible) {
                                otherProv.territoryIndex = k;
                                territory.cells.Add(otherProv);
                                remainingCells = true;
                                p = territory.cells.Count;
                                break;
                            }
                        }
                        if (p < territoryCellsCount) // no free neighbours left for this cell
                            territoryCellIndex[k]++;
                    }
                }
            }
            FindTerritoryFrontiers();
            UpdateTerritoryBoundaries();

            UpdateSortedTerritories();
            recreateTerritories = false;
        }

        void UpdateTerritoryBoundaries() {
            if (territories == null)
                return;

            // Update territory region
            int terrCount = territories.Count;
            for (int k = 0; k < terrCount; k++) {
                Territory territory = territories[k];
                Region territoryRegion = territory.region; // new Region (territory);
                                                           //				territory.region = territoryRegion;

                //				if (territory.polygon == null) {
                if (territoryRegion.polygon == null) {
                    continue;
                }
                //				territoryRegion.points = territory.polygon.contours[0].GetVector2Points();
                //				List<Point> points = territory.polygon.contours[0].points;
                territoryRegion.points = territoryRegion.polygon.contours[0].GetVector2Points();
                List<Point> points = territoryRegion.polygon.contours[0].points;
                int pointCount = points.Count;
                for (int j = 0; j < pointCount; j++) {
                    Point p0 = points[j];
                    Point p1;
                    if (j == pointCount - 1) {
                        p1 = points[0];
                    } else {
                        p1 = points[j + 1];
                    }
                    territoryRegion.segments.Add(new Segment(p0, p1));
                }

                // Update bounding rect
                float minx, miny, maxx, maxy;
                minx = miny = float.MaxValue;
                maxx = maxy = float.MinValue;
                int terrPointCount = territoryRegion.points.Count;
                for (int p = 0; p < terrPointCount; p++) {
                    Vector2 point = territoryRegion.points[p];
                    if (point.x < minx)
                        minx = point.x;
                    if (point.x > maxx)
                        maxx = point.x;
                    if (point.y < miny)
                        miny = point.y;
                    if (point.y > maxy)
                        maxy = point.y;
                }
                float rectWidth = maxx - minx;
                float rectHeight = maxy - miny;
                territoryRegion.rect2D = new Rect(minx, miny, rectWidth, rectHeight);
                territoryRegion.rect2DArea = rectWidth * rectHeight;
            }

            if (_sortedTerritories != null)
                _sortedTerritories.Clear();
        }

        void GenerateTerritoriesMesh() {
            if (territories == null)
                return;

            if (segmentHit == null) {
                segmentHit = new Dictionary<Segment, bool>(5000);
            } else {
                segmentHit.Clear();
            }
            if (frontiersPoints == null) {
                frontiersPoints = new List<Vector3>(10000);
            } else {
                frontiersPoints.Clear();
            }

            if (territoryFrontiers == null)
                return;
            int territoryFrontiersCount = territoryFrontiers.Count;
            for (int k = 0; k < territoryFrontiersCount; k++) {
                Segment s = territoryFrontiers[k];
                if (s.territoryIndex >= 0 && !territories[s.territoryIndex].borderVisible)
                    continue;
                if (!s.border || _showTerritoriesOuterBorder) {
                    frontiersPoints.Add(s.startToVector3);
                    frontiersPoints.Add(s.endToVector3);
                }
            }

            int meshGroups = (frontiersPoints.Count / 65000) + 1;
            int meshIndex = -1;
            if (territoryMeshIndices == null || territoryMeshIndices.GetUpperBound(0) != meshGroups - 1) {
                territoryMeshIndices = new int[meshGroups][];
                territoryMeshBorders = new Vector3[meshGroups][];
            }
            for (int k = 0; k < frontiersPoints.Count; k += 65000) {
                int max = Mathf.Min(frontiersPoints.Count - k, 65000);
                ++meshIndex;
                if (territoryMeshBorders[meshIndex] == null || territoryMeshBorders[meshIndex].GetUpperBound(0) != max - 1) {
                    territoryMeshBorders[meshIndex] = new Vector3[max];
                    territoryMeshIndices[meshIndex] = new int[max];
                }
                for (int j = 0; j < max; j++) {
                    territoryMeshBorders[meshIndex][j] = frontiersPoints[j + k];
                    territoryMeshIndices[meshIndex][j] = j;
                }
            }
        }

        void UpdateRenderQueue(Material mat) {
            if (_renderQueue == RENDER_QUEUE.Opaque && mat.renderQueue >= 3000) {
                mat.renderQueue -= 1000;
            } else if (_renderQueue == RENDER_QUEUE.Transparent && mat.renderQueue < 3000) {
                mat.renderQueue += 1000;
            }

            // Ensures background material is appropriate
            Renderer r = GetComponent<Renderer>();
            Material bgMat = r.sharedMaterial;
            if (bgMat != null) {
                if (_renderQueue == RENDER_QUEUE.Transparent && bgMat.renderQueue < 3000) {
                    bgMat = new Material(Shader.Find("Grids 2D System/Canvas Background Transparent"));
                    r.sharedMaterial = bgMat;
                } else if (_renderQueue == RENDER_QUEUE.Opaque && bgMat.renderQueue >= 3000) {
                    bgMat = new Material(Shader.Find("Grids 2D System/Canvas Background"));
                    r.sharedMaterial = bgMat;
                }
            }
        }

        void UpdateMaterialProperties() {
            if (territories != null) {
                int terrCount = territories.Count;
                for (int c = 0; c < terrCount; c++) {
                    int cacheIndex = GetCacheIndexForTerritoryRegion(c);
                    if (surfaces.ContainsKey(cacheIndex)) {
                        GameObject surf = surfaces[cacheIndex];
                        if (surf != null) {
                            Material mat = surf.GetComponent<Renderer>().sharedMaterial;
                            if (mat != null) {
                                mat.SetInt("_Offset", _gridDepthOffset);
                                UpdateRenderQueue(mat);
                            }
                        }
                    }
                }
            }
            if (cells != null) {
                int cellsCount = cells.Count;
                for (int c = 0; c < cellsCount; c++) {
                    int cacheIndex = GetCacheIndexForCellRegion(c);
                    if (surfaces.ContainsKey(cacheIndex)) {
                        GameObject surf = surfaces[cacheIndex];
                        if (surf != null) {
                            Material mat = surf.GetComponent<Renderer>().sharedMaterial;
                            if (mat != null) {
                                mat.SetInt("_Offset", _gridDepthOffset);
                                UpdateRenderQueue(mat);
                            }
                        }
                    }
                }
            }
            cellsMat.SetFloat("_Offset", _gridDepthOffset / 10000.0f);
            UpdateRenderQueue(cellsMat);
            territoriesMat.SetFloat("_Offset", _gridDepthOffset / 10000.0f);
            UpdateRenderQueue(territoriesMat);
            hudMatCellOverlay.SetInt("_Offset", _gridDepthOffset);
            hudMatCellGround.SetInt("_Offset", _gridDepthOffset - 1);
            hudMatTerritoryOverlay.SetInt("_Offset", _gridDepthOffset);
            hudMatTerritoryGround.SetInt("_Offset", _gridDepthOffset - 1);
            UpdateRenderQueue(coloredMat);
            UpdateRenderQueue(texturizedMat);
        }


        #endregion

        #region Drawing stuff

        int GetCacheIndexForTerritoryRegion(int territoryIndex) {
            return territoryIndex; // * 1000 + regionIndex;
        }

        Material hudMatTerritory { get { return _overlayMode == OVERLAY_MODE.Overlay ? hudMatTerritoryOverlay : hudMatTerritoryGround; } }

        Material hudMatCell { get { return _overlayMode == OVERLAY_MODE.Overlay ? hudMatCellOverlay : hudMatCellGround; } }

        Material GetColoredTexturedMaterial(Color color, Texture2D texture) {
            if (texture == null && coloredMatCache.ContainsKey(color)) {
                return coloredMatCache[color];
            } else {
                Material customMat;
                if (texture != null) {
                    customMat = Instantiate(texturizedMat);
                    customMat.name = texturizedMat.name;
                    customMat.mainTexture = texture;
                } else {
                    customMat = Instantiate(coloredMat);
                    customMat.name = coloredMat.name;
                    coloredMatCache[color] = customMat;
                }
                customMat.color = color;
                customMat.MarkForDisposal();
                return customMat;
            }
        }

        void ApplyMaterialToSurface(GameObject obj, Material sharedMaterial) {
            if (obj != null) {
                Renderer r = obj.GetComponent<Renderer>();
                if (r != null)
                    r.sharedMaterial = sharedMaterial;
            }
        }


        void DrawColorizedTerritories() {
            if (territories == null)
                return;
            int terrCount = territories.Count;
            for (int k = 0; k < terrCount; k++) {
                Territory territory = territories[k];
                Region region = territory.region;
                if (region.customMaterial != null && region.customMaterial.HasProperty("_MainTex")) {
                    TerritoryToggle(k, true, region.customMaterial.color, (Texture2D)region.customMaterial.mainTexture, region.customTextureScale, region.customTextureOffset, region.customTextureRotation);
                } else {
                    Color fillColor = territories[k].fillColor;
                    fillColor.a *= colorizedTerritoriesAlpha;
                    TerritoryToggle(k, true, fillColor);
                }
            }
        }

        /// <summary>
        /// Resets all cells and territories to random or reloads territories from optional territories texture. This method is used internally by the editor to reset cells and also used during initialization.
        /// You should not call this method directly - use Redraw() instead - unless you want to reset cell and territories.
        /// </summary>
        public void GenerateMap() {
            recreateCells = true;
            recreateTerritories = true;
            if (cells != null)
                cells.Clear();
            if (territories != null)
                territories.Clear();
            lastTerritoryLookupCount = -1;
            Redraw();
            if (territoriesTexture != null) {
                CreateTerritories(territoriesTexture, territoriesTextureNeutralColor);
            }
            // Reload configuration if component exists
            Grid2DConfig[] configs = GetComponents<Grid2DConfig>();
            for (int k = 0; k < configs.Length; k++) {
                if (configs[k].enabled)
                    configs[k].LoadConfiguration();
            }
        }



        public void Redraw() {

            if (!gameObject.activeInHierarchy)
                return;

            // Initialize surface cache
            if (surfaces != null) {
                List<GameObject> cached = new List<GameObject>(surfaces.Values);
                int cachedCount = cached.Count;
                for (int k = 0; k < cachedCount; k++)
                    if (cached[k] != null)
                        DestroyImmediate(cached[k]);
            } else {
                surfaces = new Dictionary<int, GameObject>();
            }
            DestroySurfaces();

            refreshCellMesh = true;

            if (_regularHexagons && _gridTopology == GRID_TOPOLOGY.Hexagonal) {
                Vector3 pScale = new Vector3(1f + (_cellColumnCount - 1f) * 0.75f, _cellRowCount * 0.86602f, 1f); // cos(60), sqrt(3)/2
                pScale.x *= _hexSize;
                pScale.y *= _hexSize;
                transform.localScale = pScale;
            }
            _lastVertexCount = 0;
            CheckCells();
            if (_showCells) {
                DrawCellBorders();
            }
            DrawColorizedCells();

            refreshTerritoriesMesh = true;
            CheckTerritories();
            if (_showTerritories) {
                DrawTerritoryBorders();
            }
            if (_colorizeTerritories) {
                DrawColorizedTerritories();
            }
        }


        void CheckCells() {
            if (!_showCells && !_enableTerritories)
                return;
            if (cells == null || recreateCells) {
                CreateCells();
                refreshCellMesh = true;
            }
            if (refreshCellMesh) {
                GenerateCellsMesh();
                refreshCellMesh = false;
                refreshTerritoriesMesh = true;
            }
        }

        void DrawCellBorders() {

            if (cellLayer != null) {
                DestroyImmediate(cellLayer);
            } else {
                Transform t = transform.Find(CELLS_LAYER_NAME);
                if (t != null)
                    DestroyImmediate(t.gameObject);
            }
            if (cells.Count == 0)
                return;

            cellLayer = new GameObject(CELLS_LAYER_NAME);
            cellLayer.MarkForDisposal();
            cellLayer.transform.SetParent(transform, false);
            cellLayer.transform.localPosition = Vector3.back * 0.001f;

            for (int k = 0; k < cellMeshBorders.Length; k++) {
                GameObject flayer = new GameObject("flayer");
                flayer.MarkForDisposal();
                flayer.transform.SetParent(cellLayer.transform, false);
                flayer.transform.localPosition = Vector3.zero;
                flayer.transform.localRotation = Quaternion.Euler(Vector3.zero);

                Mesh mesh = new Mesh();
                mesh.vertices = cellMeshBorders[k];
                mesh.SetIndices(cellMeshIndices[k], MeshTopology.Lines, 0);

                mesh.RecalculateBounds();
                mesh.MarkForDisposal();

                MeshFilter mf = flayer.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                _lastVertexCount += mesh.vertexCount;

                MeshRenderer mr = flayer.AddComponent<MeshRenderer>();
                mr.receiveShadows = false;
                mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.sharedMaterial = cellsMat;

                mr.sortingOrder = _sortingOrder;
            }

            cellLayer.SetActive(_showCells);
        }


        void DrawColorizedCells() {
            int cellCount = cells.Count;
            for (int k = 0; k < cellCount; k++) {
                Cell cell = cells[k];
                Region region = cell.region;
                if (region.customMaterial != null && cell.visible) {
                    CellToggle(k, true, region.customMaterial.color, false, (Texture2D)region.customMaterial.mainTexture, region.customTextureScale, region.customTextureOffset, region.customTextureRotation, region.customTextureRotationInLocalSpace);
                }
            }
        }

        void CheckTerritories() {
            if (!enableTerritories)
                return;
            if (territories == null || recreateTerritories) {
                CreateTerritories();
                refreshTerritoriesMesh = true;
            } else if (needUpdateTerritories) {
                FindTerritoryFrontiers();
                UpdateTerritoryBoundaries();
                needUpdateTerritories = false;
                refreshTerritoriesMesh = true;
            }

            if (refreshTerritoriesMesh) {
                GenerateTerritoriesMesh();
                refreshTerritoriesMesh = false;
            }

        }


        void DrawTerritoryBorders() {

            if (territoryLayer != null) {
                DestroyImmediate(territoryLayer);
            } else {
                Transform t = transform.Find(TERRITORIES_LAYER_NAME);
                if (t != null)
                    DestroyImmediate(t.gameObject);
            }
            if (territories == null || territories.Count == 0)
                return;

            territoryLayer = new GameObject(TERRITORIES_LAYER_NAME);
            territoryLayer.MarkForDisposal();
            territoryLayer.transform.SetParent(transform, false);
            territoryLayer.transform.localPosition = Vector3.back * 0.001f;

            for (int k = 0; k < territoryMeshBorders.Length; k++) {
                GameObject flayer = new GameObject("flayer");
                flayer.MarkForDisposal();
                flayer.transform.SetParent(territoryLayer.transform, false);
                flayer.transform.localPosition = Vector3.back * 0.001f; // Vector3.zero;
                flayer.transform.localRotation = Quaternion.Euler(Vector3.zero);

                Mesh mesh = new Mesh();
                mesh.vertices = territoryMeshBorders[k];
                mesh.SetIndices(territoryMeshIndices[k], MeshTopology.Lines, 0);

                mesh.RecalculateBounds();
                mesh.MarkForDisposal();

                MeshFilter mf = flayer.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                _lastVertexCount += mesh.vertexCount;

                MeshRenderer mr = flayer.AddComponent<MeshRenderer>();
                mr.receiveShadows = false;
                mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.sharedMaterial = territoriesMat;

                mr.sortingOrder = _sortingOrder;
            }
            territoryLayer.SetActive(_showTerritories);

        }

        void PrepareNewSurfaceMesh(int pointCount) {
            if (meshPoints == null) {
                meshPoints = new List<Vector3>(pointCount);
            } else {
                meshPoints.Clear();
            }
            triNew = new int[pointCount];
            if (surfaceMeshHit == null)
                surfaceMeshHit = new Dictionary<TriangulationPoint, int>(2000);
            else
                surfaceMeshHit.Clear();
            if (surfaceMeshHitVector2 == null)
                surfaceMeshHitVector2 = new Dictionary<Vector2, int>(2000);
            else
                surfaceMeshHitVector2.Clear();

            triNewIndex = -1;
            newPointsCount = -1;
        }

        void AddPointToSurfaceMesh(TriangulationPoint p) {
            if (surfaceMeshHit.ContainsKey(p)) {
                triNew[++triNewIndex] = surfaceMeshHit[p];
            } else {
                Vector3 np = new Vector3(p.Xf - 2, p.Yf - 2, -p.Zf);
                meshPoints.Add(np);
                surfaceMeshHit.Add(p, ++newPointsCount);
                triNew[++triNewIndex] = newPointsCount;
            }
        }

        Poly2Tri.Polygon GetPolygon(Region region, bool reduce = false) {
            //			Connector connector = new Connector ();
            //			connector.AddRange (region.segments);
            //			Geom.Polygon surfacedPolygon = connector.ToPolygonFromLargestLineStrip ();
            Geom.Polygon surfacedPolygon = region.polygon;
            if (surfacedPolygon == null)
                return null;
            List<Point> surfacedPoints = surfacedPolygon.contours[0].points;

            int spCount = surfacedPoints.Count;
            List<PolygonPoint> ppoints = new List<PolygonPoint>(spCount);
            double midx = 0, midy = 0;
            for (int k = 0; k < spCount; k++) {
                double x = surfacedPoints[k].x + 2;
                double y = surfacedPoints[k].y + 2;
                if (!IsTooNearPolygon(x, y, ppoints)) {
                    ppoints.Add(new PolygonPoint(x, y, 0));
                    midx += x;
                    midy += y;
                }
            }
            int ppointsCount = ppoints.Count;
            if (ppointsCount > 0 && reduce) {
                midx /= ppointsCount;
                midy /= ppointsCount;
                for (int k = 0; k < ppointsCount; k++) {
                    PolygonPoint p = ppoints[k];
                    double DX = midx - p.X;
                    double DY = midy - p.Y;
                    ppoints[k] = new PolygonPoint(p.X + DX * 0.0001, p.Y + DY * 0.0001);
                }
            }
            return new Poly2Tri.Polygon(ppoints);
        }

        void AddPointToSurfaceMesh(Vector2 p) {
            if (surfaceMeshHitVector2.ContainsKey(p)) {
                triNew[++triNewIndex] = surfaceMeshHitVector2[p];
            } else {
                meshPoints.Add(p);
                surfaceMeshHitVector2.Add(p, ++newPointsCount);
                triNew[++triNewIndex] = newPointsCount;
            }
        }

        void AddHexagonalPoints(List<Vector2> points) {
            PrepareNewSurfaceMesh(4 * 3);
            // tri 1
            AddPointToSurfaceMesh(points[1]);
            AddPointToSurfaceMesh(points[0]);
            AddPointToSurfaceMesh(points[5]);
            // tri 2
            AddPointToSurfaceMesh(points[1]);
            AddPointToSurfaceMesh(points[5]);
            AddPointToSurfaceMesh(points[2]);
            // tri 3
            AddPointToSurfaceMesh(points[2]);
            AddPointToSurfaceMesh(points[5]);
            AddPointToSurfaceMesh(points[4]);
            // tri 4
            AddPointToSurfaceMesh(points[2]);
            AddPointToSurfaceMesh(points[4]);
            AddPointToSurfaceMesh(points[3]);
        }

        void AddBoxPoints(List<Vector2> points) {
            PrepareNewSurfaceMesh(2 * 3);
            // tri 1
            AddPointToSurfaceMesh(points[2]);
            AddPointToSurfaceMesh(points[1]);
            AddPointToSurfaceMesh(points[0]);
            // tri 2
            AddPointToSurfaceMesh(points[2]);
            AddPointToSurfaceMesh(points[0]);
            AddPointToSurfaceMesh(points[3]);
        }

        GameObject GenerateRegionSurface(Region region, int cacheIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace, bool useCanvasRect) {
            if (region.points == null)
                return null;
            int pointCount = region.points.Count;
            if (_gridTopology == GRID_TOPOLOGY.Hexagonal && pointCount == 6) {
                // Produce quick triangles based on hexagon
                AddHexagonalPoints(region.points);
            } else if (_gridTopology == GRID_TOPOLOGY.Box && pointCount == 4) {
                // Produce quick triangles based on box
                AddBoxPoints(region.points);
            } else {
                // Calculate region's surface points
                Poly2Tri.Polygon poly = GetPolygon(region);
                if (poly == null)
                    return null;

                // Support for internal territories
                if (_allowTerritoriesInsideTerritories && region.entity is Territory) {
                    int terrCount = territories.Count;
                    for (int ot = 0; ot < terrCount; ot++) {
                        Territory oter = territories[ot];
                        if (oter.region != region && region.Contains(oter.region)) {
                            Poly2Tri.Polygon oterPoly = GetPolygon(oter.region, true);
                            if (oterPoly != null)
                                poly.AddHole(oterPoly);
                        }
                    }
                }
                P2T.Triangulate(poly);

                // Calculate & optimize mesh data
                int triCount = poly.Triangles.Count;
                PrepareNewSurfaceMesh(triCount * 3);
                for (int k = 0; k < triCount; k++) {
                    DelaunayTriangle dt = poly.Triangles[k];
                    AddPointToSurfaceMesh(dt.Points[0]);
                    AddPointToSurfaceMesh(dt.Points[2]);
                    AddPointToSurfaceMesh(dt.Points[1]);
                }
            }

            string cacheIndexSTR = cacheIndex.ToString();
            Rect rect = (material != null && (useCanvasRect || (canvasTexture != null && material.mainTexture == canvasTexture))) ? canvasRect : region.rect2D;
            GameObject surf = Drawing.CreateSurface(cacheIndexSTR, meshPoints.ToArray(), triNew, material, rect, textureScale, textureOffset, textureRotation, rotateInLocalSpace, _sortingOrder);
            _lastVertexCount += meshPoints.Count;
            surf.transform.SetParent(surfacesLayer.transform, false);
            surf.transform.localPosition = Vector3.zero;
            surf.layer = gameObject.layer;
            if (surfaces.ContainsKey(cacheIndex)) {
                if (surfaces[cacheIndex] != null)
                    DestroyImmediate(surfaces[cacheIndex]);
                surfaces.Remove(cacheIndex);
            }
            surfaces.Add(cacheIndex, surf);
            return surf;
        }


        GameObject GenerateRegionSurfaceHexSprite(int cellIndex, Material material, Rect spriteRect) {
            if (cellIndex < 0 || cellIndex >= cells.Count)
                return null;
            Region region = cells[cellIndex].region;
            int cacheIndex = GetCacheIndexForCellRegion(cellIndex);
            if (region.points == null)
                return null;
            int pointCount = region.points.Count;
            if (_gridTopology != GRID_TOPOLOGY.Hexagonal || pointCount != 6) {
                return null;
            }

            // Produce quick triangles based on hexagon
            AddHexagonalPoints(region.points);

            string cacheIndexSTR = cacheIndex.ToString();
            Rect gridRect = region.rect2D;
            GameObject surf = Drawing.CreateSurface(cacheIndexSTR, meshPoints.ToArray(), triNew, material, gridRect, spriteRect);
            _lastVertexCount += meshPoints.Count;
            surf.transform.SetParent(surfacesLayer.transform, false);
            surf.transform.localPosition = Vector3.zero;
            surf.layer = gameObject.layer;
            if (surfaces.ContainsKey(cacheIndex)) {
                if (surfaces[cacheIndex] != null)
                    DestroyImmediate(surfaces[cacheIndex]);
                surfaces.Remove(cacheIndex);
            }
            surfaces.Add(cacheIndex, surf);
            return surf;
        }

        #endregion


        #region Internal API

        public string GetMapData() {
            return "";
        }

        int FastConvertToInt(string s) {
            int value = 0;
            int start, sign;
            if (s[0] == '-') {
                start = 1;
                sign = -1;
            } else {
                start = 0;
                sign = 1;
            }
            for (int i = start; i < s.Length; i++) {
                value = value * 10 + (s[i] - '0');
            }
            return value * sign;
        }



        #endregion


        #region Highlighting

        void OnMouseEnter() {
            mouseIsOver = true;
        }

        void OnMouseExit() {
            // Make sure it's outside of grid
            Vector3 mousePos = Input.mousePosition;
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 5000);
            if (hits.Length > 0) {
                for (int k = 0; k < hits.Length; k++) {
                    if (hits[k].collider.gameObject == gameObject)
                        return;
                }
            }
            mouseIsOver = false;
        }



        bool GetLocalHitFromMousePos(out Vector3 localPoint) {

            Ray ray;
            localPoint = Vector3.zero;

            if (useEditorRay && !Application.isPlaying) {
                ray = editorRay;
            } else {
                if (!mouseIsOver)
                    return false;
                Vector3 mousePos = Input.mousePosition;
                if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height) {
                    localPoint = Vector3.zero;
                    return false;
                }
                ray = mainCamera.ScreenPointToRay(mousePos);
            }
            RaycastHit[] hits = Physics.RaycastAll(ray, 5000);
            if (hits.Length > 0) {
                for (int k = 0; k < hits.Length; k++) {
                    if (hits[k].collider.gameObject == gameObject) {
                        localPoint = transform.InverseTransformPoint(hits[k].point);
                        return true;
                    }
                }
            }
            return false;
        }

        void CheckMousePos() {
            if (!Application.isPlaying && !useEditorRay)
                return;

            Vector3 localPoint;
            bool goodHit = GetLocalHitFromMousePos(out localPoint);
            if (!goodHit) {
                if (_highlightMode != HIGHLIGHT_MODE.None) {
                    HideTerritoryRegionHighlight();
                }
                if (Input.GetMouseButtonDown(0)) {
                    _cellLastClickedIndex = -1;
                    _territoryLastClickedIndex = -1;
                }
                return;
            }

            int overCellIndex = -1, overTerritoryIndex = -1;

            // verify if last highlighted territory remains active
            bool sameTerritoryHighlight = false;
            float sameTerritoryArea = float.MaxValue;
            if (_territoryHighlightedIndex >= 0) {
                if (_territoryHighlighted.visible && _territoryHighlighted.region.Contains(localPoint.x, localPoint.y)) {
                    sameTerritoryHighlight = true;
                    sameTerritoryArea = _territoryHighlighted.region.rect2DArea;
                    overTerritoryIndex = _territoryHighlightedIndex;
                }
            }
            int newTerritoryHighlightedIndex = -1;

            // mouse if over the grid - verify if hitPos is inside any territory polygon
            if (territories != null) {
                int terrCount = sortedTerritories.Count;
                for (int c = 0; c < terrCount; c++) {
                    Region sreg = _sortedTerritories[c].region;
                    if (sreg != null) {
                        if (sreg.Contains(localPoint.x, localPoint.y)) {
                            overTerritoryIndex = TerritoryGetIndex(_sortedTerritories[c]);
                            sameTerritoryHighlight = overTerritoryIndex == _territoryHighlightedIndex;
                            newTerritoryHighlightedIndex = overTerritoryIndex;
                            break;
                        }
                        if (sreg.rect2DArea > sameTerritoryArea)
                            break;
                    }
                }
            }

            // verify if last highlited cell remains active
            bool sameCellHighlight = false;
            if (_cellHighlighted != null) {
                if (_cellHighlighted.region.Contains(localPoint.x, localPoint.y)) {
                    overCellIndex = _cellHighlightedIndex;
                    sameCellHighlight = true;
                }
            }
            int newCellHighlightedIndex = -1;

            if (!sameCellHighlight) {
                if (_territoryHighlightedIndex >= 0) {
                    for (int p = 0; p < _territoryHighlighted.cells.Count; p++) {
                        Cell cell = _territoryHighlighted.cells[p];
                        if (cell.region.Contains(localPoint.x, localPoint.y)) {
                            overCellIndex = CellGetIndex(_cellHighlighted);
                            newCellHighlightedIndex = overCellIndex;
                            break;
                        }
                    }
                } else {
                    int sortedCellsCount = sortedCells.Count;
                    for (int p = 0; p < sortedCellsCount; p++) {
                        Cell cell = sortedCells[p];
                        if (cell.region.Contains(localPoint.x, localPoint.y)) {
                            overCellIndex = CellGetIndex(cell);
                            newCellHighlightedIndex = overCellIndex;
                            break;
                        }
                    }
                }
            }

            switch (_highlightMode) {
                case HIGHLIGHT_MODE.Territories:
                    if (!sameTerritoryHighlight) {
                        if (newTerritoryHighlightedIndex >= 0 && territories[newTerritoryHighlightedIndex].visible) {
                            HighlightTerritoryRegion(newTerritoryHighlightedIndex, false);
                        } else {
                            HideTerritoryRegionHighlight();
                        }
                    }
                    break;
                case HIGHLIGHT_MODE.Cells:
                    if (!sameCellHighlight) {
                        if (newCellHighlightedIndex >= 0 && (cells[newCellHighlightedIndex].visible || _cellHighlightNonVisible)) {
                            HighlightCellRegion(newCellHighlightedIndex, false);
                        } else {
                            HideCellRegionHighlight();
                        }
                    }
                    break;
            }

            // record last clicked cell/territory
            if (Input.GetMouseButtonDown(0)) {
                _cellLastClickedIndex = overCellIndex;
                _territoryLastClickedIndex = overTerritoryIndex;
            }

        }


        void UpdateHighlightFade() {
            if (_highlightFadeAmount == 0)
                return;

            if (_highlightedObj != null) {
                float newAlpha = 1.0f - Mathf.PingPong(Time.time - highlightFadeStart, _highlightFadeAmount);
                Material mat = _highlightedObj.GetComponent<Renderer>().sharedMaterial;
                Color color = mat.color;
                Color newColor = new Color(color.r, color.g, color.b, newAlpha);
                mat.color = newColor;
            }

        }


        void CheckUserInteraction() {

            if (_territoryLastClickedIndex >= 0 && OnTerritoryClick != null && Input.GetMouseButtonDown(0)) {
                OnTerritoryClick(_territoryLastClickedIndex);
            }
            if (_cellLastClickedIndex >= 0 && OnCellClick != null && Input.GetMouseButtonDown(0)) {
                OnCellClick(_cellLastClickedIndex);
            }
            if (_cellLastClickedIndex >= 0 && OnCellMouseUp != null && Input.GetMouseButtonUp(0)) {
                OnCellMouseUp(_cellLastClickedIndex);
            }
        }


        #endregion


        #region Geometric functions


        Vector3 GetWorldSpacePosition(Vector2 localPosition, float elevation = 0) {
            Vector3 p = transform.TransformPoint(localPosition);
            p -= transform.forward * elevation;
            return p;
        }


        #endregion



        #region Territory stuff

        void HideTerritoryRegionHighlight() {
            HideCellRegionHighlight();
            if (_territoryHighlighted == null)
                return;
            if (_highlightedObj != null) {
                if (_territoryHighlighted.region.customMaterial != null) {
                    ApplyMaterialToSurface(_highlightedObj, _territoryHighlighted.region.customMaterial);
                } else {
                    _highlightedObj.SetActive(false);
                }
                if (!_territoryHighlighted.visible) {
                    _highlightedObj.SetActive(false);
                }
                _highlightedObj = null;
            }
            if (OnTerritoryExit != null) {
                OnTerritoryExit(_territoryHighlightedIndex);
            }
            _territoryHighlighted = null;
            _territoryHighlightedIndex = -1;
        }

        /// <summary>
        /// Highlights the territory region specified. Returns the generated highlight surface gameObject.
        /// Internally used by the Map UI and the Editor component, but you can use it as well to temporarily mark a territory region.
        /// </summary>
        /// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
        public GameObject HighlightTerritoryRegion(int territoryIndex, bool refreshGeometry) {
            if (_highlightedObj != null)
                HideTerritoryRegionHighlight();
            if (territoryIndex < 0 || territoryIndex >= territories.Count)
                return null;

            if (OnTerritoryEnter != null)
                OnTerritoryEnter(territoryIndex);

            if (OnTerritoryHighlight != null) {
                bool cancelHighlight = false;
                OnTerritoryHighlight(territoryIndex, ref cancelHighlight);
                if (cancelHighlight)
                    return null;
            }

            int cacheIndex = GetCacheIndexForTerritoryRegion(territoryIndex);
            bool existsInCache = surfaces.ContainsKey(cacheIndex);
            if (refreshGeometry && existsInCache) {
                GameObject obj = surfaces[cacheIndex];
                surfaces.Remove(cacheIndex);
                DestroyImmediate(obj);
                existsInCache = false;
            }
            if (existsInCache) {
                _highlightedObj = surfaces[cacheIndex];
                if (_highlightedObj == null) {
                    surfaces.Remove(cacheIndex);
                } else {
                    if (!_highlightedObj.activeSelf)
                        _highlightedObj.SetActive(true);
                    Renderer rr = _highlightedObj.GetComponent<Renderer>();
                    if (rr.sharedMaterial != hudMatTerritory)
                        rr.sharedMaterial = hudMatTerritory;
                }
            } else {
                _highlightedObj = GenerateTerritoryRegionSurface(territoryIndex, hudMatTerritory, Vector2.one, Vector2.zero, 0);
            }

            _territoryHighlightedIndex = territoryIndex;
            _territoryHighlighted = territories[territoryIndex];

            return _highlightedObj;
        }


        GameObject GenerateTerritoryRegionSurface(int territoryIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation) {
            return GenerateTerritoryRegionSurface(territoryIndex, material, textureScale, textureOffset, textureRotation, false, false);
        }

        GameObject GenerateTerritoryRegionSurface(int territoryIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace, bool useCanvasRect) {
            if (territoryIndex < 0 || territoryIndex >= territories.Count)
                return null;
            Region region = territories[territoryIndex].region;
            int cacheIndex = GetCacheIndexForTerritoryRegion(territoryIndex);
            return GenerateRegionSurface(region, cacheIndex, material, textureScale, textureOffset, textureRotation, rotateInLocalSpace, useCanvasRect);
        }


        void UpdateColorizedTerritoriesAlpha() {
            if (territories == null)
                return;
            for (int c = 0; c < territories.Count; c++) {
                Territory territory = territories[c];
                int cacheIndex = GetCacheIndexForTerritoryRegion(c);
                if (surfaces.ContainsKey(cacheIndex)) {
                    GameObject surf = surfaces[cacheIndex];
                    if (surf != null) {
                        Color newColor = surf.GetComponent<Renderer>().sharedMaterial.color;
                        newColor.a = territory.fillColor.a * _colorizedTerritoriesAlpha;
                        surf.GetComponent<Renderer>().sharedMaterial.color = newColor;
                    }
                }
            }
        }

        Territory GetTerritoryAtPoint(Vector3 localPoint) {
            for (int p = 0; p < territories.Count; p++) {
                Territory territory = territories[p];
                if (territory.region.Contains(localPoint.x, localPoint.y)) {
                    return territory;
                }
            }
            return null;
        }

        void TerritoryAnimate(FADER_STYLE style, int territoryIndex, Color color, float duration) {
            if (territoryIndex < 0 || territoryIndex >= territories.Count)
                return;
            int cacheIndex = GetCacheIndexForTerritoryRegion(territoryIndex);
            GameObject territorySurface = null;
            if (surfaces.ContainsKey(cacheIndex)) {
                territorySurface = surfaces[cacheIndex];
            }
            if (territorySurface == null) {
                territorySurface = TerritoryToggle(territoryIndex, true, color);
                territories[territoryIndex].region.customMaterial = null;
            } else {
                territorySurface.SetActive(true);
            }
            Renderer renderer = territorySurface.GetComponent<Renderer>();
            Material oldMaterial = renderer.sharedMaterial;
            Material fadeMaterial = Instantiate(hudMatTerritory);
            fadeMaterial.color = color;
            fadeMaterial.mainTexture = oldMaterial.mainTexture;
            fadeMaterial.MarkForDisposal();
            fadeMaterial.name = oldMaterial.name;
            renderer.sharedMaterial = fadeMaterial;
            SurfaceFader.Animate(style, this, territorySurface, territories[territoryIndex].region, fadeMaterial, color, duration);
        }

        #endregion


        #region Cell stuff

        int GetCacheIndexForCellRegion(int cellIndex) {
            return 1000000 + cellIndex; // * 1000 + regionIndex;
        }

        /// <summary>
        /// Highlights the cell region specified. Returns the generated highlight surface gameObject.
        /// Internally used by the Map UI and the Editor component, but you can use it as well to temporarily mark a territory region.
        /// </summary>
        /// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
        public GameObject HighlightCellRegion(int cellIndex, bool refreshGeometry) {
#if HIGHLIGHT_NEIGHBOURS
			DestroySurfaces();
#endif
            if (_highlightedObj != null)
                HideCellRegionHighlight();
            if (cellIndex < 0 || cellIndex >= cells.Count)
                return null;

            if (OnCellEnter != null)
                OnCellEnter(cellIndex);

            bool cancelHighlight = false;
            if (OnCellHighlight != null) {
                OnCellHighlight(cellIndex, ref cancelHighlight);
                if (cancelHighlight)
                    return null;
            }

            int cacheIndex = GetCacheIndexForCellRegion(cellIndex);
            bool existsInCache = surfaces.ContainsKey(cacheIndex);
            if (refreshGeometry && existsInCache) {
                GameObject obj = surfaces[cacheIndex];
                surfaces.Remove(cacheIndex);
                DestroyImmediate(obj);
                existsInCache = false;
            }
            if (existsInCache) {
                _highlightedObj = surfaces[cacheIndex];
                if (_highlightedObj != null) {
                    _highlightedObj.SetActive(true);
                    _highlightedObj.GetComponent<Renderer>().sharedMaterial = hudMatCell;
                } else {
                    surfaces.Remove(cacheIndex);
                }
            } else {
                _highlightedObj = GenerateCellRegionSurface(cellIndex, hudMatCell, Vector2.one, Vector2.zero, 0);
            }
            // Reuse cell texture
            Cell cell = cells[cellIndex];
            if (cell.region.customMaterial != null) {
                hudMatCell.mainTexture = cell.region.customMaterial.mainTexture;
            } else {
                hudMatCell.mainTexture = null;
            }
            _cellHighlighted = cells[cellIndex];
            _cellHighlightedIndex = cellIndex;
            highlightFadeStart = Time.time;


#if HIGHLIGHT_NEIGHBOURS
			for (int k=0;k<cellRegionHighlighted.neighbours.Count;k++) {
				int  ni = GetCellIndex((Cell)cellRegionHighlighted.neighbours[k].entity);
				GenerateCellRegionSurface(ni, 0, hudMatTerritory);
			}
#endif

            return _highlightedObj;
        }

        void HideCellRegionHighlight() {
            if (_cellHighlighted == null)
                return;
            if (_highlightedObj != null) {
                if (cellHighlighted.region.customMaterial != null) {
                    ApplyMaterialToSurface(_highlightedObj, _cellHighlighted.region.customMaterial);
                } else if (_highlightedObj.GetComponent<SurfaceFader>() == null) {
                    _highlightedObj.SetActive(false);
                }
                if (!cellHighlighted.visible) {
                    _highlightedObj.SetActive(false);
                }
                _highlightedObj = null;
            }
            if (OnCellExit != null)
                OnCellExit(_cellHighlightedIndex);
            _cellHighlighted = null;
            _cellHighlightedIndex = -1;
        }


        float GetFirstPointInRow(float y, List<PolygonPoint> points) {
            int max = points.Count - 1;
            float minx = 1000;
            for (int k = 0; k <= max; k++) {
                PolygonPoint p0 = points[k];
                PolygonPoint p1;
                if (k == max) {
                    p1 = points[0];
                } else {
                    p1 = points[k + 1];
                }
                // if line crosses the horizontal line
                if (p0.Y >= y && p1.Y <= y || p0.Y <= y && p1.Y >= y) {
                    float x;
                    if (p1.Xf == p0.Xf) {
                        x = p0.Xf;
                    } else {
                        float a = (p1.Xf - p0.Xf) / (p1.Yf - p0.Yf);
                        x = p0.Xf + a * (y - p0.Yf);
                    }
                    if (x < minx)
                        minx = x;
                }
            }
            return minx - 2;
        }

        float GetLastPointInRow(float y, List<PolygonPoint> points) {
            int max = points.Count - 1;
            float maxx = -1000;
            for (int k = 0; k <= max; k++) {
                PolygonPoint p0 = points[k];
                PolygonPoint p1;
                if (k == max) {
                    p1 = points[0];
                } else {
                    p1 = points[k + 1];
                }
                // if line crosses the horizontal line
                if (p0.Yf >= y && p1.Yf <= y || p0.Yf <= y && p1.Yf >= y) {
                    float x;
                    if (p1.X == p0.Xf) {
                        x = p0.Xf;
                    } else {
                        float a = (p1.Xf - p0.Xf) / (p1.Yf - p0.Yf);
                        x = p0.Xf + a * (y - p0.Yf);
                    }
                    if (x > maxx)
                        maxx = x;
                }
            }
            return maxx - 2;
        }

        bool IsTooNearPolygon(double x, double y, List<PolygonPoint> points) {
            int pointCount = points.Count;
            for (int j = 0; j < pointCount; j++) {
                PolygonPoint p1 = points[j];
                if ((x - p1.X) * (x - p1.X) + (y - p1.Y) * (y - p1.Y) < SQR_MIN_VERTEX_DIST) {
                    return true;
                }
            }
            return false;
        }

        GameObject GenerateCellRegionSurface(int cellIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation) {
            return GenerateCellRegionSurface(cellIndex, material, textureScale, textureOffset, textureRotation, false, false);
        }

        GameObject GenerateCellRegionSurface(int cellIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace, bool useCanvasRect) {
            if (cellIndex < 0 || cellIndex >= cells.Count)
                return null;
            Region region = cells[cellIndex].region;
            int cacheIndex = GetCacheIndexForCellRegion(cellIndex);
            return GenerateRegionSurface(region, cacheIndex, material, textureScale, textureOffset, textureRotation, rotateInLocalSpace, useCanvasRect);
        }

        Cell GetCellAtPoint(Vector3 position, bool worldSpace) {

            // Compute local point
            if (worldSpace) {
                position = transform.InverseTransformPoint(position);
            }
            int cellsCount = cells.Count;
            for (int p = 0; p < cellsCount; p++) {
                Cell cell = cells[p];
                if (cell == null || cell.region == null || cell.region.points == null || !cell.visible)
                    continue;
                if (cell.region.Contains(position.x, position.y)) {
                    return cell;
                }
            }
            return null;
        }

        void CellAnimate(FADER_STYLE style, int cellIndex, Color color, float duration) {
            if (cellIndex < 0 || cellIndex >= cells.Count)
                return;
            int cacheIndex = GetCacheIndexForCellRegion(cellIndex);
            GameObject cellSurface = null;
            if (surfaces.ContainsKey(cacheIndex)) {
                cellSurface = surfaces[cacheIndex];
            }
            if (cellSurface == null) {
                cellSurface = CellToggle(cellIndex, true, color, false);
                cells[cellIndex].region.customMaterial = null;
            } else {
                cellSurface.SetActive(true);
            }
            Renderer renderer = cellSurface.GetComponent<Renderer>();
            Material oldMaterial = renderer.sharedMaterial;
            Material fadeMaterial = Instantiate(hudMatCell);
            fadeMaterial.color = Color.black;
            fadeMaterial.mainTexture = oldMaterial.mainTexture;
            fadeMaterial.MarkForDisposal();
            renderer.sharedMaterial = fadeMaterial;

            SurfaceFader.Animate(style, this, cellSurface, cells[cellIndex].region, fadeMaterial, color, duration);
        }

        #endregion

    }
}