Shader "AkilliMum/Standard/D2WeatherEffects/Sprite/D2Fogs"
{
    Properties
    {
        [HideInInspector][PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        [HideInInspector]_Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector]_Size("Size", float) = 2.0
        [HideInInspector]_TopFade("Top Fade", float) = 0.0
        [HideInInspector]_RightFade("Right Fade", float) = 0.0
        [HideInInspector]_BottomFade("Bottom Fade", float) = 0.0
        [HideInInspector]_LeftFade("Left Fade", float) = 0.0
        [HideInInspector]_FadeMultiplier("Fade Multiplier", float) = 0.1
        [HideInInspector]_CameraSpeedMultiplier("Camera Speed Multiplier", float) = 1.0
        [HideInInspector]_UVChangeX("UV Change X", float) = 1.0
        [HideInInspector]_UVChangeY("UV Change Y", float) = 1.0
		[HideInInspector]_Speed("Horizontal Speed", float) = 0.2
		[HideInInspector]_VSpeed("Vertical Speed", float) = 0
        [HideInInspector]_Density("Density", float) = 1
        [HideInInspector]_DarkMode("Dark Mode", float) = 0
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

        float _Size;
        float _TopFade;
        float _RightFade;
        float _BottomFade;
        float _LeftFade;
        float _FadeMultiplier;
        float _CameraSpeedMultiplier;
        float _UVChangeX;
        float _UVChangeY;
        float _Speed;
        float _VSpeed;
        float _Density;
        float _DarkMode;
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

        float hash(float n) 
        { 
            return frac(sin(n)*753.5453123); 
        }

        float noise(in float3 x)
        {
            float3 p = floor(x);
            float3 f = frac(x);
            f = f*f*(3.0 - 2.0*f);

            float n = p.x + p.y*157.0 + 113.0*p.z;
            return lerp(
                        lerp(
                            lerp(hash(n + 0.0),   hash(n + 1.0),   f.x),
                            lerp(hash(n + 157.0), hash(n + 158.0), f.x), 
                            f.y),
                        lerp(
                            lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                            lerp(hash(n + 270.0), hash(n + 271.0), f.x), 
                            f.y),
                        f.z);
        }



        float fog(in float2 uv)
        {
            float direction = _Time.y * _Speed;
            float Vdirection = _Time.y * _VSpeed;
            float color = 0.0;
            float total = 0.0;
            float k = 0.0;

            for (float i=0; i<6; i++)
            {
                k = pow(2.0, i); 
                color += noise(float3((uv.x * _Size + direction * (i+1.0)*0.2) * k, 
                                (uv.y * _Size + Vdirection * (i + 1.0)*0.2) * k,
                                0.0)) 
                                / k; 
                total += 1.0/k;
            }
            color /= total;
            
            return clamp(color, 0.0, 1.0);

        }
 
        void surf(Input IN, inout SurfaceOutput o)
        {
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

            //float2 fogUV = float2 (IN.uv_MainTex.x + _UVChangeX*_CameraSpeedMultiplier, IN.uv_MainTex.y + _UVChangeY*_CameraSpeedMultiplier);
            float2 fogUV = float2 (screenUV.x + _UVChangeX*_CameraSpeedMultiplier, screenUV.y + _UVChangeY*_CameraSpeedMultiplier);
            float f = fog(fogUV);
            //float m = min(f*_Density, 1.);
            float m = f*_Density*_Color.a;
            
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
            
            o.Albedo = top * right * bottom * left * m * _Color.rgb;
            o.Alpha =  top * right * bottom * left * m;
                //return tex*(1-m) + m*_Color;
            //}
        }
        ENDCG
    }

    Fallback "Transparent/VertexLit"
}
 