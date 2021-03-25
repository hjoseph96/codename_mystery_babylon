Shader "Light2D/UI/FogOfWar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

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

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 worldPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            sampler2D _MainTex;		
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

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.worldPos = mul (unity_ObjectToWorld, v.vertex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            bool InCamera (float2 pos, float2 rectPos, float2 rectSize) {
				rectPos -= rectSize / 2;
                return (pos.x < rectPos.x || pos.x > rectPos.x + rectSize.x || pos.y < rectPos.y || pos.y > rectPos.y + rectSize.y) == false;
            }

			float2 TransformToCamera(float2 pos, float rotation) {
				float c = cos(-rotation);
				float s = sin(-rotation);
		
				float x = pos.x;
				float y = pos.y;

				pos.x = x * c - y * s;
				pos.y = x * s + y * c;

                return(pos);
            }

            fixed4 LightColor(float id, float2 texcoord) {
				if (id < 0.5f) {
					return(tex2D (_Cam1_Texture, texcoord));
				} else if (id < 1.5f) {
					return(tex2D (_Cam2_Texture, texcoord));
				}
				
				return(fixed4(0, 0, 0, 0));
			}

            fixed4 InputColor(v2f IN) {
				fixed4 spritePixel = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				return(spritePixel);
			}

			fixed4 OutputColor(v2f IN, float2 posInCamera, float2 cameraSize, float id) {
				float2 texcoord = (posInCamera + cameraSize / 2) / cameraSize;
			
				fixed4 lightPixel = LightColor(id, texcoord);
				fixed4 spritePixel = InputColor(IN);

				float multiplier = (lightPixel.r + lightPixel.g + lightPixel.b) / 3;

				spritePixel.a *= multiplier;

				if (spritePixel.a > 1) {
					spritePixel.a = 1;
				}

				return spritePixel;
			}

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = InputColor(IN);

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

                #ifdef UNITY_UI_CLIP_RECT
                    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}