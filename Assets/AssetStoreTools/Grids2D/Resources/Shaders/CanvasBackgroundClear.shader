Shader "Grids 2D System/Canvas Background Clear" {
	Properties {
	}
	SubShader {
		GrabPass {  "_BackgroundTexture" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }

            sampler2D _BackgroundTexture;

            half4 frag(v2f i) : SV_Target
            {
                return tex2Dproj(_BackgroundTexture, i.grabPos);
            }
            ENDCG
        }
    }
}
