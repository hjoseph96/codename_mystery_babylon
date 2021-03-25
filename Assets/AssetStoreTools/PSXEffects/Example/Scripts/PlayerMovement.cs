using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public float moveSpeed = 10f;
	public Light dirLight;
	public GameObject museumText;

	private CharacterController cc;
	private PSXEffects psx;

	// Use this for initialization
	void Start () {
		cc = GetComponent<CharacterController>();
		psx = FindObjectOfType<PSXEffects>();
		dirLight.gameObject.SetActive(false);
		museumText.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		cc.Move((transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal")).normalized * moveSpeed * Time.deltaTime);
		if (!cc.isGrounded) {
			cc.Move(Vector3.up * Time.deltaTime * Physics.gravity.y);
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.transform.tag == "DemoRoom") {
			ToggleDemoMode(true);
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.transform.tag == "DemoRoom") {
			ToggleDemoMode(false);
		}
	}

	public void ToggleDemoMode(bool enabled) {
		if (enabled) {
			RenderSettings.fog = false;
			psx.favorRed = 0;
			psx.maxDarkness = 0;
			psx.resolutionFactor = 1;
			psx.polygonalDrawDistance = -1;
			psx.dithering = false;
			psx.colorDepth = 24;
			dirLight.gameObject.SetActive(true);
			museumText.SetActive(true);
			psx.UpdateProperties();
		} else {
			RenderSettings.fog = true;
			psx.favorRed = 1;
			psx.maxDarkness = 10;
			psx.resolutionFactor = 2;
			psx.polygonalDrawDistance = 30.61f;
			psx.dithering = false;
			psx.colorDepth = 24;
			dirLight.gameObject.SetActive(false);
			museumText.SetActive(false);
			psx.UpdateProperties();
		}
	}
}
