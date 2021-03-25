using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoValueChanger : MonoBehaviour {

	private PSXEffects psx;

	void Start() {
		psx = FindObjectOfType<PSXEffects>();
	}

	public void _ResolutionFactor(string input) {
		int val = int.Parse(input);
		psx.resolutionFactor = val;
		psx.UpdateProperties();
	}

	public void _FrameSkip(string input) {
		int val = int.Parse(input);
		psx.skipFrames = val;
		psx.UpdateProperties();
	}

	public void _AffineMapping(bool input) {
		psx.affineMapping = input;
		psx.UpdateProperties();
	}

	public void _DrawDistance(string input) {
		float val = float.Parse(input);
		psx.polygonalDrawDistance = val;
		psx.UpdateProperties();
	}

	public void _PolygonInaccuracy(string input) {
		int val = int.Parse(input);
		psx.polygonInaccuracy = val;
		psx.UpdateProperties();
	}

	public void _VertexInaccuracy(string input) {
		int val = int.Parse(input);
		psx.vertexInaccuracy = val;
		psx.UpdateProperties();
	}

	public void _WorldSpaceSnapping(bool input) {
		psx.worldSpaceSnapping = input;
		psx.UpdateProperties();
	}

	public void _CameraBasedSnapping(bool input) {
		psx.camSnapping = input;
		psx.UpdateProperties();
	}

	public void _SaturatedDiffuse(string input) {
		int val = int.Parse(input);
		psx.maxDarkness = val;
		psx.UpdateProperties();
	}

	public void _PostProcessing(bool input) {
		psx.postProcessing = input;
		psx.UpdateProperties();
	}

	public void _ColorDepth(string input) {
		int val = int.Parse(input);
		psx.colorDepth = val;
		psx.UpdateProperties();
	}

	public void _SubtractionFade(float input) {
		psx.subtractFade = (int)input;
		psx.UpdateProperties();
	}

	public void _FavorRed(string input) {
		float val = float.Parse(input);
		psx.favorRed = val;
		psx.UpdateProperties();
	}

	public void _Scanlines(bool input) {
		psx.scanlines = input;
		psx.UpdateProperties();
	}

	public void _Vertical(bool input) {
		psx.verticalScanlines = input;
		psx.UpdateProperties();
	}

	public void _ScanlineIntensity(string input) {
		int val = int.Parse(input);
		psx.scanlineIntensity = val;
		psx.UpdateProperties();
	}

	public void _Dithering(bool input) {
		psx.dithering = input;
		psx.UpdateProperties();
	}

	public void _DitherThreshold(string input) {
		float val = float.Parse(input);
		psx.ditherThreshold = val;
		psx.UpdateProperties();
	}

	public void _DitherIntensity(string input) {
		int val = int.Parse(input);
		psx.ditherIntensity = val;
		psx.UpdateProperties();
	}
}
