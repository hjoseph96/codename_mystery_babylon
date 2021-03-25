Shader "Light2D/Internal/LightSprite"
{
	Properties
	{
		 _MainTex ("Sprite Texture", 2D) = "white" {}
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

		Blend SrcAlpha One

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;

				fixed4 data : TEXCOORD1;

				// data
				// x - radius
				// y - ratio
				// z - not used
				// w - not used
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				fixed4 data : TEXCOORD1;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				OUT.data = IN.data;
	
				return OUT;
			}

			fixed4 Blur(v2f IN) {
				float blurX = IN.data.x / 2000;
				float blurY = blurX * IN.data.y;
		
				float c = 1.0 / (1200);

				fixed4 a = (fixed4)0;

				[unroll]
				for (int x = -7; x < 8; x++) {

					[unroll]
					for (int y = -7; y < 8; y++) {

						a += tex2D(_MainTex, float2(IN.texcoord.x + x * blurX, IN.texcoord.y + y * blurY)) * (      c * sqrt(19 - abs(x) + 19 - abs(y))      );
					
					}
				}

				return(a);
			}

			fixed4 frag(v2f IN) : SV_Target {
				// Mask Type
				if (IN.data.z > 0) {
					if (IN.data.x > 0) {
						return(fixed4(1, 1, 1, 1) * Blur(IN).a * IN.color * 1.5);
					} else {
						return(fixed4(1, 1, 1, 1) * tex2D (_MainTex, IN.texcoord).a * IN.color * 1.5);
					}
				// Light Type
				} else {
					if (IN.data.x > 0) {
						return(Blur(IN) * IN.color * 1.5);
					} else {
						return(tex2D (_MainTex, IN.texcoord) * IN.color * 1.5);
					}
				}
				
			}
			
			ENDCG
		}
	}
}