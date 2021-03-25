Shader "Light2D/Misc/MultiplyBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _radius ("Radius", float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend Zero SrcColor

        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
			float _radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 Blur (v2f IN) {
				float4 sum = float4(0.0, 0.0, 0.0, 0.0);
				float blur = _radius / 4000;     
				float constant = 1.0 / 55.0;
				float raySize = 3;
                float ratio = 9.0 / 16.0;

				[unroll]
				for (int x = -raySize; x < raySize; x++) {

					[unroll]
					for (int y = -raySize + 1; y < raySize; y++) {
				
						float val = constant * sqrt(raySize - abs(x) + raySize - abs(y));

						sum += tex2D(_MainTex, float2(IN.uv.x + x * blur * ratio, IN.uv.y + y * blur)) * val;
					}
				}			
                return sum;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, float2(i.uv.x, i.uv.y)); // Translucency(IN, _translucency);
                tex = Blur(i);

                return(tex);
            }
            ENDCG
        }
    }
}
