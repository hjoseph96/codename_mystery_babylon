Shader "NeatWolf/NW_Particle Rim AddSmooth 2S" {
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
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal xboxone ps4 psp2 n3ds wiiu 
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
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 node_504 = ((i.vertexColor.rgb*lerp(_RimColor.rgb,((_RimColor.rgb*_RimColor.a)*(_MainTex_var.rgb*_MainTex_var.a)),_RimTextureAmount))*i.vertexColor.a);
                float3 emissive = (node_504*_EmissionMultiplier);
                float3 finalColor = emissive + (node_504*(pow(1.0-max(0,dot(normalDirection, viewDirection)),_RimExponent)*_RimMultiplier));
                float4 finalRGBA = float4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Particles/Additive (Soft)"
}
