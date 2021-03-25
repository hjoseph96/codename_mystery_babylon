using UnityEngine;
using System.Collections;

namespace Grids2D {
				public enum FADER_STYLE {
								FadeOut = 0,
								Blink = 1,
								Flash = 2
				}

				public class SurfaceFader : MonoBehaviour {

								Material fadeMaterial;
								float startTime, duration;
								Grid2D grid;
								Color color;
								Region region;
								Renderer _renderer;
								FADER_STYLE style;

								public static void Animate (FADER_STYLE style, Grid2D grid, GameObject surface, Region region, Material fadeMaterial, Color color, float duration) {
												SurfaceFader fader = surface.GetComponent<SurfaceFader> ();
												if (fader != null)
																DestroyImmediate (fader);
												fader = surface.AddComponent<SurfaceFader> ();
												fader.grid = grid;
												fader.startTime = Time.time;
												fader.duration = duration + 0.0001f;
												fader.color = color;
												fader.region = region;
												fader.fadeMaterial = fadeMaterial;
												fader.style = style;
								}

								void Start () {
												_renderer = GetComponent<Renderer> ();
												_renderer.sharedMaterial = fadeMaterial;
								}

								// Update is called once per frame
								void Update () {
												float elapsed = Time.time - startTime;
												switch (style) {
												case FADER_STYLE.FadeOut:
																UpdateFadeOut (elapsed);
																break;
												case FADER_STYLE.Blink:
																UpdateBlink (elapsed);
																break;
												case FADER_STYLE.Flash:
																UpdateFlash (elapsed);
																break;
												}
								}

								#region Fade Out effect

								void UpdateFadeOut (float elapsed) {
												float newAlpha = Mathf.Clamp01 (1.0f - elapsed / duration);
												SetAlpha (newAlpha);
												if (elapsed >= duration) {
																SetAlpha (0);
																region.customMaterial = null;
																DestroyImmediate (this);
												}
								}

								void SetAlpha (float newAlpha) {
												if (grid.highlightedObj == gameObject)
																return;
												Color newColor = new Color (color.r, color.g, color.b, newAlpha);
												fadeMaterial.color = newColor;
												if (_renderer.sharedMaterial != fadeMaterial) {
																fadeMaterial.mainTexture = _renderer.sharedMaterial.mainTexture;
																_renderer.sharedMaterial = fadeMaterial;
												}
								}

								#endregion

								#region Flash effect

								void UpdateFlash (float elapsed) {
												SetFlashColor (elapsed / duration);
												if (elapsed >= duration) {
																SetFlashColor (1f);
																if (region.customMaterial != null && _renderer != null)
																				_renderer.sharedMaterial = region.customMaterial;
																DestroyImmediate (this);
												}
								}

		
								void SetFlashColor (float t) {
												Color newColor = Color.Lerp (color, Misc.ColorTransparent, t);
												fadeMaterial.color = newColor;
												if (_renderer.sharedMaterial != fadeMaterial) {
																fadeMaterial.mainTexture = _renderer.sharedMaterial.mainTexture;
																_renderer.sharedMaterial = fadeMaterial;
												}
								}

								#endregion

								#region Blink effect

								void UpdateBlink (float elapsed) {
												SetBlinkColor (elapsed / duration);
												if (elapsed >= duration) {
																SetBlinkColor (0);
																if (region.customMaterial != null && _renderer != null)
																				_renderer.sharedMaterial = region.customMaterial;
																DestroyImmediate (this);
												}
								}

								void SetBlinkColor (float t) {
												Color newColor;
												if (t < 0.5f) {
																newColor = Color.Lerp (Misc.ColorTransparent, color, t * 2f);
												} else {
																newColor = Color.Lerp (color, Misc.ColorTransparent, (t - 0.5f) * 2f);
												}
												fadeMaterial.color = newColor;
												if (_renderer.sharedMaterial != fadeMaterial) {
																fadeMaterial.mainTexture = _renderer.sharedMaterial.mainTexture;
																_renderer.sharedMaterial = fadeMaterial;
												}
								}

								#endregion

				}

}