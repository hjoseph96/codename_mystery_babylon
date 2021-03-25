Shader "Grids 2D System/Canvas Background" {
	Properties {
    _MainTex ("Texture", 2D) = "black" {}
    _Color ("Color", Color) = (0,0,0,1)
	}
	SubShader {
        Offset 2, 2
        Tags { "Queue" = "Geometry-1" "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };
            
            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 color = tex2D(_MainTex, i.uv);
                return color * _Color;
            }
            ENDCG
        }
    }
}
