Shader "Light2D/TextMeshPro/BitmapFogOfWar" {

Properties {
	_MainTex		("Font Atlas", 2D) = "white" {}
	_FaceTex		("Font Texture", 2D) = "white" {}
	[HDR]_FaceColor	("Text Color", Color) = (1,1,1,1)

	_VertexOffsetX	("Vertex OffsetX", float) = 0
	_VertexOffsetY	("Vertex OffsetY", float) = 0
	_MaskSoftnessX	("Mask SoftnessX", float) = 0
	_MaskSoftnessY	("Mask SoftnessY", float) = 0

	_ClipRect("Clip Rect", vector) = (-32767, -32767, 32767, 32767)

	_StencilComp("Stencil Comparison", Float) = 8
	_Stencil("Stencil ID", Float) = 0
	_StencilOp("Stencil Operation", Float) = 0
	_StencilWriteMask("Stencil Write Mask", Float) = 255
	_StencilReadMask("Stencil Read Mask", Float) = 255

	_CullMode("Cull Mode", Float) = 0
	_ColorMask("Color Mask", Float) = 15
	
	 // Camera 1
	[HideInInspector] _Cam1_Texture ("Camera 1 Texture", 2D) = "white" {}
	[HideInInspector] _Cam1_Position_X ("Camera 1 Position X", Float) = 0
	[HideInInspector] _Cam1_Position_Y ("Camera 1 Position Y", Float) = 0
	[HideInInspector] _Cam1_Size_X ("Camera 1 Size X", Float) = 1
	[HideInInspector] _Cam1_Size_Y ("Camera 1 Size Y", Float) = 1
	[HideInInspector] _Cam1_Size_Rotation ("Camera 1 Size Rotation", Float) = 0

	// Camera 2
	[HideInInspector] _Cam2_Texture ("Camera 2 Texture", 2D) = "white" {}
	[HideInInspector] _Cam2_Position_X ("Camera 2 Position X", Float) = 0
	[HideInInspector] _Cam2_Position_Y ("Camera 2 Position Y", Float) = 0
	[HideInInspector] _Cam2_Size_X ("Camera 2 Size X", Float) = 1
	[HideInInspector] _Cam2_Size_Y ("Camera 2 Size Y", Float) = 1
	[HideInInspector] _Cam2_Size_Rotation ("Camera 2 Size Rotation", Float) = 0
}

SubShader{

	Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

	Stencil
	{
		Ref[_Stencil]
		Comp[_StencilComp]
		Pass[_StencilOp]
		ReadMask[_StencilReadMask]
		WriteMask[_StencilWriteMask]
	}


	Lighting Off
	Cull [_CullMode]
	ZTest [unity_GUIZTestMode]
	ZWrite Off
	Fog { Mode Off }
	Blend SrcAlpha OneMinusSrcAlpha
	ColorMask[_ColorMask]

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		#pragma multi_compile __ UNITY_UI_ALPHACLIP


		#include "UnityCG.cginc"

		struct appdata_t {
			float4 vertex		: POSITION;
			fixed4 color		: COLOR;
			float2 texcoord0	: TEXCOORD0;
			float2 texcoord1	: TEXCOORD1;
		};

		struct v2f {
			float4	vertex		: SV_POSITION;
			fixed4	color		: COLOR;
			float2	texcoord0	: TEXCOORD0;
			float2	texcoord1	: TEXCOORD1;
			float4	mask		: TEXCOORD2;
			float2 worldPos : TEXCOORD3;
		};

		uniform	sampler2D 	_MainTex;
		uniform	sampler2D 	_FaceTex;
		uniform float4		_FaceTex_ST;
		uniform	fixed4		_FaceColor;

		uniform float		_VertexOffsetX;
		uniform float		_VertexOffsetY;
		uniform float4		_ClipRect;
		uniform float		_MaskSoftnessX;
		uniform float		_MaskSoftnessY;

		sampler2D _Cam1_Texture;
		sampler2D _Cam2_Texture;

		// Cam 1
		float _Cam1_Position_X;
		float _Cam1_Position_Y;
		float _Cam1_Size_X;
		float _Cam1_Size_Y;
		float _Cam1_Size_Rotation;

		// Cam 2
		float _Cam2_Position_X;
		float _Cam2_Position_Y;
		float _Cam2_Size_X;
		float _Cam2_Size_Y;
		float _Cam2_Size_Rotation;

		float2 UnpackUV(float uv)
		{
			float2 output;
			output.x = floor(uv / 4096);
			output.y = uv - 4096 * output.x;

			return output * 0.001953125;
		}

		bool InCamera (float2 pos, float2 rectPos, float2 rectSize) {
			rectPos -= rectSize / 2;
			return (pos.x < rectPos.x || pos.x > rectPos.x + rectSize.x || pos.y < rectPos.y || pos.y > rectPos.y + rectSize.y) == false;
		}

		float2 TransformToCamera(float2 pos, float rotation) {
			float angle = atan2(pos.y,  pos.x) - rotation;
			float dist = sqrt(pos.x *  pos.x +  pos.y *  pos.y);

			pos.x = cos(angle) * dist;
			pos.y = sin(angle) * dist;

			return(pos);
		}

		fixed4 GetPixelByID(float id, float2 texcoord) {
			if (id < 0.5f) {
				return(tex2D (_Cam1_Texture, texcoord));
			} else if (id < 1.5f) {
				return(tex2D (_Cam2_Texture, texcoord));
			}
			
			return(fixed4(0, 0, 0, 0));
		}

		fixed4 OutputColor(v2f IN, float2 posInCamera, float2 cameraSize, float id) {
			float2 texcoord = (posInCamera + cameraSize / 2) / cameraSize;
		
			fixed4 lightPixel = GetPixelByID(id, texcoord);

			fixed4 spritePixel = tex2D(_MainTex, IN.texcoord0);
			spritePixel = fixed4 (tex2D(_FaceTex, IN.texcoord1).rgb * IN.color.rgb, IN.color.a * spritePixel.a);

			float multiplier = (lightPixel.r + lightPixel.g + lightPixel.b) / 3;

			spritePixel.a *= multiplier;

			if (spritePixel.a > 1) {
				spritePixel.a = 1;
			}

			return spritePixel;
		}

		v2f vert (appdata_t v)
		{
			float4 vert = v.vertex;
			vert.x += _VertexOffsetX;
			vert.y += _VertexOffsetY;

			vert.xy += (vert.w * 0.5) / _ScreenParams.xy;

			float4 vPosition = UnityPixelSnap(UnityObjectToClipPos(vert));

			fixed4 faceColor = v.color;
			faceColor *= _FaceColor;

			v2f OUT;
			OUT.vertex = vPosition;
			OUT.color = faceColor;
			OUT.texcoord0 = v.texcoord0;
			OUT.texcoord1 = TRANSFORM_TEX(UnpackUV(v.texcoord1), _FaceTex);
			OUT.worldPos = mul (unity_ObjectToWorld, v.vertex);

			float2 pixelSize = vPosition.w;
			pixelSize /= abs(float2(_ScreenParams.x * UNITY_MATRIX_P[0][0], _ScreenParams.y * UNITY_MATRIX_P[1][1]));

			// Clamp _ClipRect to 16bit.
			float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
			OUT.mask = float4(vert.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_MaskSoftnessX, _MaskSoftnessY) + pixelSize.xy));

			return OUT;
		}

		fixed4 frag (v2f IN) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, IN.texcoord0);
			col = fixed4 (tex2D(_FaceTex, IN.texcoord1).rgb * IN.color.rgb, IN.color.a * col.a);
			fixed4 c = col;

			if (_Cam1_Size_X > 0) {
				float2 camera_1_Size = float2(_Cam1_Size_X, _Cam1_Size_Y);
				float2 posInCamera1 = TransformToCamera(IN.worldPos - float2(_Cam1_Position_X, _Cam1_Position_Y),  _Cam1_Size_Rotation);

				if (InCamera(posInCamera1, float2(0, 0), camera_1_Size)) {
					c = OutputColor(IN, posInCamera1, camera_1_Size, 0);
				}
			}

			if (_Cam2_Size_X > 0) {
				float2 camera_2_Size = float2(_Cam2_Size_X, _Cam2_Size_Y);
				float2 posInCamera2 = TransformToCamera(IN.worldPos - float2(_Cam2_Position_X, _Cam2_Position_Y),  _Cam2_Size_Rotation);
				
				if (InCamera(posInCamera2, float2(0, 0), camera_2_Size)) {
					c = OutputColor(IN, posInCamera2, camera_2_Size, 1);
				}
			}

			fixed4 color = c;
		
			// Alternative implementation to UnityGet2DClipping with support for softness.
			#if UNITY_UI_CLIP_RECT
				half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
				color *= m.x * m.y;
			#endif

			#if UNITY_UI_ALPHACLIP
				clip(color.a - 0.001);
			#endif

			return color;
		}
		ENDCG
	}
}

	CustomEditor "TMPro.EditorUtilities.TMP_BitmapShaderGUI"
}
