Shader "Hidden/CornerCuttingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "black" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            static const int CELL_SIZE = 16;

            sampler2D _MainTex;
            sampler2D _MaskTex;

            float4 _MainTex_TexelSize;

            // Shader used to remove/add triangles from the highlighting based on Mask texture value
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 maskColor = tex2D(_MaskTex, i.uv); // Get mask color
                int maskValue = round(maskColor.r * 255); // Conver mask value from [0..1] float to [0..255] int
                int sign = round(maskColor.a) * 2 - 1; // Alpha channel is used to determine if we should remove (0) or add (1) pixels from the texture

                int2 pixelCoords = i.uv / _MainTex_TexelSize.xy;
                int2 cellCoords = fmod(pixelCoords, CELL_SIZE); // Position of pixel inside cell. Cell is CELL_SIZE x CELL_SIZE pixels
                // Example: pixel at (36, 18) will have cellCoords = (36 % 16, 18 % 16) = (4, 2)

                float value = tex2D(_MainTex, i.uv).a;
                value += sign * (maskValue & 1) * (cellCoords.x + cellCoords.y >= CELL_SIZE); // Remove/add TOP-RIGHT triangle
                value += sign * (maskValue & 2) * (cellCoords.x <= cellCoords.y); // Remove/add TOP-LEFT triangle
                value += sign * (maskValue & 4) * (cellCoords.x + cellCoords.y <= CELL_SIZE); // Remove/add BOTTOM-LEFT triangle
                value += sign * (maskValue & 8) * (cellCoords.x >= cellCoords.y); // Remove/add BOTTOM-RIGHT triangle

                return fixed4(1, 1, 1, 1) * value;
            }
            ENDCG
        }
    }
}
