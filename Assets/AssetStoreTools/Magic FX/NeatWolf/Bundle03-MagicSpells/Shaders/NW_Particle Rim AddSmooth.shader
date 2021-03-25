// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NeatWolf/NW_Particle Rim AddSmooth" {
    Properties {
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _RimTextureAmount ("Rim Texture Amount", Range(0, 1)) = 1
        _RimExponent ("Rim Exponent", Float ) = 1
        _RimMultiplier ("Rim Multiplier", Float ) = 1
        _EmissionMultiplier ("Emission Multiplier", Float ) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            Cull Back
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 2.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _RimExponent;
            uniform float _RimMultiplier;
            uniform float4 _RimColor;
            uniform float _EmissionMultiplier;
            uniform float _RimTextureAmount;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
                float3 normalDirection =  i.normalDir;
////// Lighting:
////// Emissive:
                float4 node_502 = i.vertexColor;
                float2 node_520 = i.uv0;
                float4 node_10 = tex2D(_MainTex,TRANSFORM_TEX(node_520.rg, _MainTex));
                float3 node_504 = ((node_502.rgb*lerp(_RimColor.rgb,((_RimColor.rgb*_RimColor.a)*(node_10.rgb*node_10.a)),_RimTextureAmount))*node_502.a);
                float3 emissive = (node_504*_EmissionMultiplier);
                float3 finalColor = emissive + (node_504*(pow(1.0-max(0,dot(normalDirection, viewDirection)),_RimExponent)*_RimMultiplier));
/// Final Color:
                return float4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Particles/Additive (Soft)"
}
