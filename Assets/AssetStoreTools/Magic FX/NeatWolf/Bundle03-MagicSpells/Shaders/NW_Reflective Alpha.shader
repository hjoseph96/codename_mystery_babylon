Shader "NeatWolf/Reflective Alpha" {
 Properties {
 _Color ("Main Color", Color) = (0.5,0.5,0.5,1)
 _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
 //_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
 _MainTex ("Base (RGB) TransGloss (A)", 2D) = "white" {}
 //_BumpMap ("Normalmap", 2D) = "bump" {}
 //_Cube ("Reflection Cubemap", Cube) = "black" { TexGen CubeReflect }
 _Cube ("Cubemap", Cube) = "black" { /* used to be TexGen CubeReflect */ }
 
 }
 SubShader {
 Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
 Blend SrcAlpha OneMinusSrcAlpha
 //Pass {
 // Color [_Color]
 // SetTexture [_MainTex] {
 // combine texture * primary
 // }
 // }
 LOD 300
 
 CGPROGRAM
 //#pragma surface surf BlinnPhong decal:add nolightmap
 #pragma surface surf BlinnPhong alpha
 //#pragma target 3.0
 sampler2D _MainTex;
 //sampler2D _BumpMap;
 samplerCUBE _Cube;
 //half _Shininess;
 
 fixed4 _ReflectColor;
 fixed4 _Color;
 
 struct Input {
 float3 worldRefl;
 float2 uv_MainTex;
 //float2 uv_BumpMap;
 INTERNAL_DATA
 };
 
 void surf (Input IN, inout SurfaceOutput o) {
 fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
 //o.Albedo = 1;
 //o.Gloss = 0;
 o.Albedo = tex.rgb * _Color.rgb;
 o.Gloss = tex.a;
 //o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
 
 //o.Specular = _Shininess;
 float3 worldRefl = WorldReflectionVector (IN, o.Normal);
 fixed4 reflcol = texCUBE (_Cube, IN.worldRefl);
 reflcol *= tex.a;
 o.Emission = reflcol.rgb * _ReflectColor.rgb;
 //o.Alpha = reflcol.a * _ReflectColor.a;
 o.Alpha = tex.a * _Color.a;
 
 }
 ENDCG
 }
 FallBack "Transparent/VertexLit"
}