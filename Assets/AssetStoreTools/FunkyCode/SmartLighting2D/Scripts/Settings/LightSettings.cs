using UnityEngine;
using UnityEngine.Events;
using LightSettings;

namespace LightSettings {
		
	// Light 2D
	public class LightEvent : UnityEvent <Light2D> {} // ??

	public enum MaskLit {Lit, Unlit, Isometric, Custom}

	public enum LightLayerType {ShadowAndMask, ShadowOnly, MaskOnly}

	public enum LightLayerSorting {None, SortingLayerAndOrder, DistanceToLight, YDistanceToLight, YAxisLower, YAxisHigher, ZAxisLower, ZAxisHigher, Isometric};
	public enum LightLayerSortingIgnore {None, IgnoreAbove};

	public enum LightLayerShadowEffect {LegacyCPU, LegacyGPU, PerpendicularProjection, SoftConvex, SoftVertex, SpriteProjection};
	public enum LightLayerMaskLit {AlwaysLit, AboveLit, NeverLit};

	public enum LayerSorting {None, ZAxisLower, ZAxisHigher, YAxisLower, YAxisHigher};
	public enum LayerType {ShadowsAndMask, ShadowsOnly, MaskOnly}

	public enum NormalMapTextureType {
		Texture,
		Sprite,
		SecondaryTexture
	}

	public enum NormalMapType {
		PixelToLight,
		ObjectToLight
	}

	public enum LightEventState {
		OnCollision, 
		OnCollisionEnter, 
		OnCollisionExit, 
		None
	}

}