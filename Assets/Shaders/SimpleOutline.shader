Shader "Hidden/NewImageEffectShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 1, 1, 1)
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineSize("Outline Size", int) = 0
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Tags
        {
            "RenderType" = "Transparent"
        }

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

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            fixed4 _Color;
            int _OutlineSize;

            // Simple outline shader
            // Check neighbour pixels if at least one is fully transparent, draw outline using OutlineColor
            // Otherwise draw mainTexture tinted with Color
            fixed4 frag (v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.uv);

                if (_OutlineSize > 0 && col.a != 0)
                {
                    float totalAlpha = 1.0;

                    [unroll(16)]
                    for (int i = 1; i <= _OutlineSize; i++)
                    {
                        fixed4 pixelUp = tex2D(_MainTex, IN.uv + fixed2(0, i * _MainTex_TexelSize.y));
                        fixed4 pixelDown = tex2D(_MainTex, IN.uv - fixed2(0, i * _MainTex_TexelSize.y));
                        fixed4 pixelRight = tex2D(_MainTex, IN.uv + fixed2(i * _MainTex_TexelSize.x, 0));
                        fixed4 pixelLeft = tex2D(_MainTex, IN.uv - fixed2(i * _MainTex_TexelSize.x, 0));

                        totalAlpha *= pixelUp.a * pixelDown.a * pixelRight.a * pixelLeft.a;
                    }

                    if (totalAlpha == 0)
                    {
                        return fixed4(1, 1, 1, 1) * _OutlineColor;
                    }
                }

                return col * _Color;
            }
            ENDCG
        }
    }
}
