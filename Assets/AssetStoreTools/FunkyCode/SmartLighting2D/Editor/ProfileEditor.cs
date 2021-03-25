using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightingSettings.Profile))]
public class ProfileEditor2 : Editor {

	override public void OnInspectorGUI() {
		LightingSettings.Profile profile = target as LightingSettings.Profile;

		ProfileEditor.DrawProfile(profile);
	}
}