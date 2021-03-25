Shader "Light2D/Internal/Light" {
    Properties {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _LinearColor ("LinearColor", Float) = 0
    }

    Category {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend One OneMinusSrcAlpha
        ColorMask RGB
        Cull Off Lighting Off ZWrite Off

        SubShader {

            Pass {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
        
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float4 _MainTex_ST;

                float _LinearColor;

                struct appdata_t {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                v2f vert (appdata_t v) {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target {
                    fixed4 tex = tex2D(_MainTex, i.texcoord);

                    fixed4 col =  tex * 2.0f;
                    col.a = (1 - tex.a);

                    col.r *= i.color.a * 2;
                    col.g = col.r;
                    col.b = col.r;

                    col.rgb *= i.color;

                    if (_LinearColor) {
                        col.rgb = pow(col.rgb, 1/2.2);
                    }
                
                    return col;
                }
                ENDCG
            }
        }

    }
}