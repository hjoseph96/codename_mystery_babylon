Shader "AkilliMum/Standard/D2WeatherEffects/Sprite/D2Rains"
{
    Properties
    {
        [HideInInspector][PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        [HideInInspector]_Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector]_TopFade("Top Fade", float) = 0.0
        [HideInInspector]_RightFade("Right Fade", float) = 0.0
        [HideInInspector]_BottomFade("Bottom Fade", float) = 0.0
        [HideInInspector]_LeftFade("Left Fade", float) = 0.0
        [HideInInspector]_FadeMultiplier("Fade Multiplier", float) = 0.1
        [HideInInspector]_CameraSpeedMultiplier("Camera Speed Multiplier", float) = 0.0
        [HideInInspector]_UVChangeX("UV Change X", float) = 1.0
        [HideInInspector]_UVChangeY("UV Change Y", float) = 1.0
        [HideInInspector]_Multiplier("Particle Multiplier", float) = 10
        [HideInInspector]_Size("Size", float) = 0.05
        [HideInInspector]_Tail("Tail", float) = 0.03
        [HideInInspector]_Speed("Speed", float) = 4
        [HideInInspector]_Zoom("Zoom", float) = 1.2
        [HideInInspector]_Direction("Direction", float) = 0.2
        [HideInInspector]_DarkMultiplier("Dark Multiplier", float) = 1  
    }
 
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
 
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
 
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert nofog keepalpha
        #pragma multi_compile _ PIXELSNAP_ON
        #pragma shader_feature ETC1_EXTERNAL_ALPHA
 
        sampler2D _MainTex;
        fixed4 _Color;
        sampler2D _AlphaTex;

        float _TopFade;
        float _RightFade;
        float _BottomFade;
        float _LeftFade;
        float _FadeMultiplier;
        float _Size;
        float _CameraSpeedMultiplier;
        float _UVChangeX;
        float _UVChangeY;
        float _Tail;
        float _Zoom;
        float _Speed;
        float _Direction;
        float _Multiplier;
        float _DarkMultiplier;
 
        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
            //fixed4 color;
        };
 
        void vert(inout appdata_full v, out Input o)
        {
#if defined(PIXELSNAP_ON)
            v.vertex = UnityPixelSnap(v.vertex);
#endif
            UNITY_INITIALIZE_OUTPUT(Input, o);
            //o.color = v.color * _Color;
        }
 
        fixed4 SampleSpriteTexture(float2 uv)
        {
            fixed4 color = tex2D(_MainTex, uv);
 
#if ETC1_EXTERNAL_ALPHA
            color.a = tex2D(_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA
 
            return color;
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
                
                //q += float2(q.y*w, _Speed*_Time.y / (1.0+_Zoom*0.03));
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
 
        void surf(Input IN, inout SurfaceOutput o)
        {
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

            //float2 fogUV = float2 (i.uv.x + _UVChangeX*_CameraSpeedMultiplier, i.uv.y + _UVChangeY*_CameraSpeedMultiplier);
            float2 fogUV = float2 (screenUV.x + _UVChangeX*_CameraSpeedMultiplier, screenUV.y + _UVChangeY*_CameraSpeedMultiplier);
            float m = calcSnow(fogUV) * _Color.a;
            
            //float2 fogUV = float2 (IN.uv_MainTex.x + _UVChangeX*_CameraSpeedMultiplier, IN.uv_MainTex.y + _UVChangeY*_CameraSpeedMultiplier);
            //float2 fogUV = float2 (screenUV.x + _UVChangeX*_CameraSpeedMultiplier, screenUV.y + _UVChangeY*_CameraSpeedMultiplier);
            //float f = fog(fogUV);
            //float m = min(f*_Density, 1.);
            //float m = f*_Density;
            
            //fixed top =    _TopFade    > 0 ? (1-IN.uv_MainTex.y*_TopFade)   : 1;
            //fixed right =  _RightFade  > 0 ? (1-IN.uv_MainTex.x*_RightFade) : 1;
            //fixed bottom = _BottomFade > 0 ?    IN.uv_MainTex.y*_BottomFade : 1;
            //fixed left =   _LeftFade   > 0 ?    IN.uv_MainTex.x*_LeftFade   : 1;

            half top =    _TopFade    > 0 ?
                ((1 - IN.uv_MainTex.y) < _FadeMultiplier ? (1 - IN.uv_MainTex.y) / _FadeMultiplier : 1)
                : 1;
            half right =  _RightFade  > 0 ?
                ((1 - IN.uv_MainTex.x) < _FadeMultiplier ? (1 - IN.uv_MainTex.x) / _FadeMultiplier : 1)
                : 1;
            half bottom = _BottomFade > 0 ?
                (IN.uv_MainTex.y < _FadeMultiplier ? IN.uv_MainTex.y / _FadeMultiplier : 1)
                : 1;
            half left =   _LeftFade   > 0 ?    
                (IN.uv_MainTex.x < _FadeMultiplier ? IN.uv_MainTex.x / _FadeMultiplier : 1)
                : 1;
            
            o.Albedo = top * right * bottom * left * m * _Color.rgb * _DarkMultiplier;
            o.Alpha =  top * right * bottom * left * m * _DarkMultiplier;
                //return tex*(1-m) + m*_Color;
            //}
        }
        ENDCG
    }

    Fallback "Transparent/VertexLit"
}
 