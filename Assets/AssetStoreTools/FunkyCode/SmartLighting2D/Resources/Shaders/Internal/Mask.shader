Shader "Light2D/Internal/Mask" {

	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_SecTex ("CollisionTexture", 2D) = "white" {}

		_translucency ("Radius", float) = 0
		_intensity ("Intensity", float) = 1
		_textureSize("TextureSize", float) = 2048
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

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"
		
			sampler2D _MainTex;
			sampler2D _SecTex;

			//float4 _SecTex_ST;

			float _translucency;
			float _intensity;
			float _textureSize;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
		
				return OUT;
			}

			fixed4 Translucency (v2f IN, float radius) {
				float4 sum = float4(0.0, 0.0, 0.0, 0.0);
				float2 pos = float2(IN.vertex.x / _textureSize, IN.vertex.y / _textureSize);
				float blur = radius / 4000;     
				int size = 30;
				float constant = 1.0 / (size * 84) * 5;

				float raySize = 9;

				[unroll]
				for (int x = -raySize; x < raySize; x++) {

					[unroll]
					for (int y = -raySize + 1; y < raySize; y++) {
				
						float val = constant * sqrt(raySize - abs(x) + raySize - abs(y));

						sum += tex2D(_SecTex, float2(pos.x + x * blur, pos.y + y * blur)) * val;
					}
				}			
                return float4(sum.rgb, 1);
            }

			fixed4 frag(v2f IN) : SV_Target
			{
				float localTranslucency = IN.color.a;
				float radius = _translucency;

				fixed4 color = fixed4(1, 1, 1, tex2D(_MainTex, IN.texcoord).a);
				
				if (color.a < 0.75) {
					color.a = 0;
				}

				color *= IN.color;
				color.rgb *= color.a;

				if (radius > 0.01) {
					fixed4 tex = Translucency(IN, radius);
					if (tex.r < color.r) {
						color.r = tex.r;
					}
					
					color.r *= _intensity;
				}
		
				return color;
			}

			ENDCG
		}
	}
}