using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Grids2D {

				static class DisposalManager {

								static List<Object> disposeObjects;

								static DisposalManager () {
												disposeObjects = new List<Object> ();
								}

								public static void DisposeAll () {
												int c = disposeObjects.Count;
												for (int k = 0; k < c; k++) {
																Object o = disposeObjects [k];
																if (o != null) {
																				Object.DestroyImmediate (o);
																}
												}
												disposeObjects.Clear ();
								}

								public static void MarkForDisposal(this Object o) {
												o.hideFlags = HideFlags.DontSave;
												disposeObjects.Add(o);
								}


				}

}