Shader "Light2D/Sprites/LitGreyScale"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Lit ("Lit", Range(0,1)) = 1

		// _LinearColor ("LinearColor", Float) = 0

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

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass {

		CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color    : COLOR;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 texcoord  : TEXCOORD1;
				fixed4 color    : COLOR;
                float2 worldPos : TEXCOORD0;
			};
			
			fixed4 _Color;
			float _Lit;
			sampler2D _MainTex;

			// float _LinearColor;
			
            // Cam 1
			sampler2D _Cam1_Texture;
            float _Cam1_Position_X;
            float _Cam1_Position_Y;
            float _Cam1_Size_X;
            float _Cam1_Size_Y;
            float _Cam1_Size_Rotation;

			 // Cam 2
			sampler2D _Cam2_Texture;
			float _Cam2_Position_X;
            float _Cam2_Position_Y;
            float _Cam2_Size_X;
            float _Cam2_Size_Y;
            float _Cam2_Size_Rotation;

			v2f vert(appdata_t IN)
			{
				v2f OUT;

				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
                
                OUT.worldPos = mul (unity_ObjectToWorld, IN.vertex);

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
				fixed4 spritePixel = tex2D (_MainTex, IN.texcoord) * IN.color; 
                float greyscale = (spritePixel.r + spritePixel.g + spritePixel.b) / 3;
                float4 color = float4(greyscale, greyscale, greyscale, spritePixel.a);
				return(color);
			}

			fixed4 OutputColor(v2f IN, float2 posInCamera, float2 cameraSize, float id) {
				float2 texcoord = (posInCamera + cameraSize / 2) / cameraSize;
			
				fixed4 lightPixel = LightColor(id, texcoord);
				fixed4 spritePixel = InputColor(IN);

				lightPixel = lerp(lightPixel, fixed4(1, 1, 1, 1), 1 - _Lit);

				spritePixel = spritePixel * lightPixel;
				spritePixel.rgb *= spritePixel.a; 
				
				//spritePixel.rgb =  pow(spritePixel.rgb, 1/2.2);
				
				return spritePixel;
			}
        
			fixed4 frag(v2f IN) : SV_Target
			{
				if (_Cam1_Size_X > 0) {
					float2 camera_1_Size = float2(_Cam1_Size_X, _Cam1_Size_Y);
					float2 posInCamera1 = TransformToCamera(IN.worldPos - float2(_Cam1_Position_X, _Cam1_Position_Y),  _Cam1_Size_Rotation);

					if (InCamera(posInCamera1, float2(0, 0), camera_1_Size)) {
                    	return OutputColor(IN, posInCamera1, camera_1_Size, 0);
					}
				}

				if (_Cam2_Size_X > 0) {
					float2 camera_2_Size = float2(_Cam2_Size_X, _Cam2_Size_Y);
					float2 posInCamera2 = TransformToCamera(IN.worldPos - float2(_Cam2_Position_X, _Cam2_Position_Y),  _Cam2_Size_Rotation);
					
					if (InCamera(posInCamera2, float2(0, 0), camera_2_Size)) {
						return OutputColor(IN, posInCamera2, camera_2_Size, 1);
					}
                }
					
				fixed4 spritePixel = InputColor(IN);
				spritePixel.rgb *= spritePixel.a; 
				return spritePixel;
			}

		    ENDCG
		}
	}
}