// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AkilliMum/Standard/D2WeatherEffects/Post/D2Rains" {
    Properties{
        [HideInInspector]_CameraSpeedMultiplier("Camera Speed Multiplier", float) = 1.0
        [HideInInspector]_UVChangeX("UV Change X", float) = 1.0
        [HideInInspector]_UVChangeY("UV Change Y", float) = 1.0
        [HideInInspector]_Multiplier("Particle Multiplier", float) = 10
        [HideInInspector]_Size("Size", float) = 0.05
        [HideInInspector]_Tail("Tail", float) = 0.03
        [HideInInspector]_Speed("Speed", float) = 4
        [HideInInspector]_Zoom("Zoom", float) = 1.2
        [HideInInspector]_Direction("Direction", float) = 0.2
        [HideInInspector]_DarkMode("Dark Mode", float) = 0
        [HideInInspector]_DarkMultiplier("Dark Multiplier", float) = 1  
        [HideInInspector]_LuminanceAdd("Luminance Add", float) = 0.001  
        [HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
        [HideInInspector]_Color("Color", Color) = (1, 1, 1, 1)
        
    }

    Subshader{

        Pass{
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            //Tags{ "Queue" = "Opaque" }

            CGPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Size;
            float _CameraSpeedMultiplier;
            float _UVChangeX;
            float _UVChangeY;
            float _Tail;
            float _Zoom;
            float _Speed;
            float _Direction;
            float _Multiplier;
            float _DarkMode;
            float _DarkMultiplier;
            float _LuminanceAdd;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vertexOutput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            vertexOutput vert(appdata v)
            {
                vertexOutput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float mod(float a, float b)
            {
                return a - floor(a / b) * b;
            }
            float2 mod(float2 a, float2 b)
            {
                return a - floor(a / b) * b;
            }
            float3 mod(float3 a, float3 b)
            {
                return a - floor(a / b) * b;
            }
            float4 mod(float4 a, float4 b)
            {
                return a - floor(a / b) * b;
            } 

            float calcSnow(float2 uv)
            {
                const float3x3 p = float3x3(13.323122,23.5112,21.71123,21.1212,
                    28.7312,11.9312,21.8112,14.7212,61.3934);
                
                float snow = 0.;
                for (float i=0.; i < _Multiplier; i++)
                {
                    float2 q = uv *float2(1,_Tail) * i*_Zoom;

                    //float w = _Direction * mod(i*7.238917,1.0)-_Direction*0.1*sin(_Time.y+i);
                    float w = (_Direction+0.01) * mod(i*7.238917,1.0);// + _Direction*0.1*sin(_Time.y+i);
                
                    //q += float2(q.y*w, _Speed*_Time.y / (1.0+i*_Zoom*0.03));
                    q += float2(q.y*w, _Speed*_Time.y / (1.0+i*_Zoom*0.03));

                    float3 n = float3(floor(q),31.189+i);
                    float3 m = floor(n)*0.00001 + frac(n);
                    float3 mp = (31314.9+m) / frac(mul(p,m));
                    float3 r = frac(mp);
                    float2 s = abs(mod(q,1.0) -0.5 +0.9*r.xy -0.45);
                    s += 0.01*abs(2.0*frac(10.*q.yx)-1.); 
                    float d = 0.6*max(s.x-s.y,s.x+s.y)+max(s.x,s.y)-.01;
                    snow += smoothstep(_Size,-_Size,d)*(r.x/(1.+.02*i*_Zoom));
                }
                return snow;
            }

            half4 frag(vertexOutput i) : SV_Target
            {
                float2 fogUV = float2 (i.uv.x + _UVChangeX*_CameraSpeedMultiplier, i.uv.y + _UVChangeY*_CameraSpeedMultiplier);
                float snow = calcSnow(fogUV);
                float4 tex = tex2D(_MainTex, i.uv);
                if(_DarkMode==1){
                    half lum = tex.r*.3 + tex.g*.59 + tex.b*.11;
                    return tex*(1-snow)+(lum+_LuminanceAdd)*snow*_Color*_DarkMultiplier;
                } 
                else{
                    return tex*(1-snow)+snow*_Color*_DarkMultiplier;
                }
            }
            ENDCG
        }
    }
}