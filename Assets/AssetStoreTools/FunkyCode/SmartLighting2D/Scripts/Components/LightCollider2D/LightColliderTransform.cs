using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightColliderTransform {

	private bool update = true;
	public bool UpdateNeeded {
		get => update;
		set => update = value;
	}

	public Vector2 position = Vector2.zero;
	public Vector2 scale = Vector3.zero;
	public float rotation = 0;
	public float shadowHeight = 0;
	public float shadowTranslucency = 0;
	public float maskTranslucency = 1;

	private Vector3 position3D = Vector3.zero;
	private bool flipX = false;
	private bool flipY = false;
	private Vector2 size = Vector2.one;

	LightColliderShape shape;

	public void SetShape(LightColliderShape shape) {
		this.shape = shape;
	}

	public void Reset() {
		position = Vector2.zero;
		rotation = 0;
		scale = Vector3.zero;
	}

	void UpdateTransform(Transform transform) {
		Vector3 newPosition3D = transform.position;
		Vector2 position2D = LightingPosition.GetPosition2D(transform.position);

		Vector2 scale2D = transform.lossyScale;
		float rotation2D = LightingPosition.GetRotation2D(transform);

		if (scale != scale2D) {
			scale = scale2D;

			update = true;
		}

		if (rotation != rotation2D) {
			rotation = rotation2D;

			update = true;
		}

			
		if (position3D != newPosition3D) {
			position3D = newPosition3D;

			update = true;
		}

		if (position != position2D) {
			position = position2D;

			update = true;
		}
	}

	public void Update(bool force) {
		if (shape == null) {
			return;
		}

		Transform transform = shape.transform;
		
		if (transform == null) {
			return;
		}

		if (transform.hasChanged || force) {
			transform.hasChanged = false;

			UpdateTransform(transform);
		}

		if (shadowTranslucency != shape.shadowTranslucency) {
			shadowTranslucency = shape.shadowTranslucency;

			update = true;
		}

		if (maskTranslucency != shape.maskTranslucency) {
			maskTranslucency = shape.maskTranslucency;

			update = true;
		}		

		bool checkShapeSprite = shape.maskType == LightCollider2D.MaskType.SpritePhysicsShape || shape.shadowType == LightCollider2D.ShadowType.SpritePhysicsShape;
		bool checkMaskSprite = shape.maskType == LightCollider2D.MaskType.Sprite || shape.maskType == LightCollider2D.MaskType.BumpedSprite;

		if (checkShapeSprite || checkMaskSprite) {
			SpriteRenderer spriteRenderer = shape.spriteShape.GetSpriteRenderer();

			if (spriteRenderer != null) {

				if (spriteRenderer.size != size) {
					size = spriteRenderer.size;
					
					update = true;
				}

				if (spriteRenderer.flipX != flipX || spriteRenderer.flipY != flipY) {
					flipX = spriteRenderer.flipX;
					flipY = spriteRenderer.flipY;

					shape.ResetWorld();

					update = true;
				}
				
				if (shape.spriteShape.GetOriginalSprite() != spriteRenderer.sprite) {
					shape.ResetLocal();

					update = true;
				}
			}
		}

		bool checkShapeMesh = shape.maskType == LightCollider2D.MaskType.MeshRenderer || shape.shadowType == LightCollider2D.ShadowType.MeshRenderer;

		if (checkShapeMesh) {
			MeshFilter meshFilter = shape.meshShape.GetMeshFilter();

			if (meshFilter != null) {

				if (meshFilter.sharedMesh != shape.meshShape.mesh) {
					shape.ResetLocal();
					
					update = true;
				}
			}
		}
	}
}