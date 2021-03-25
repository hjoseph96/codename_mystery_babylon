using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D {
				public class Demo18 : MonoBehaviour {

								Grid2D grid;
								public Texture2D tex;
								public Texture2D colorRamp;
								[Range(0,0.99f)]
								public float waterLevel = 0.55f;

								void Start () {
												// Get a reference to Grids 2D System's API
												grid = Grid2D.instance;

												Color[] noise = tex.GetPixels();
												Color[] ramp = colorRamp.GetPixels();
												int width = tex.width;
												int height = tex.height;
												int cellCount = grid.cells.Count;
												for (int k=0;k<cellCount;k++) {
																Vector2 center = grid.cells[k].center;
																int tw = (int)((center.x + 0.5f) * width);
																int th = (int)((center.y + 0.5f) * height);
																float elevation = noise[th * width + tw].r;
																if (elevation<waterLevel) {
																				// water
																				grid.cells[k].visible = false;
																} else {
																				int pos = (int)(colorRamp.width * (elevation - waterLevel) / (1f - waterLevel));
																				Color color = ramp[pos];
																				grid.CellToggle(k, true, color);
																}
												}
												grid.Redraw();

								}

	
				}
}
