// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "GAPH Custom Shader/Integrated Particle/Integrated Particle (WithNormal)"{
	Properties{
		[HDR]_TintColor("Tint Color",Color) = (0.5,0.5,0.5,0.5)
		_AllColorFactor("All Color Factor",float) = 1.0
		_ColorRedFactor("Color Red Factor", float) = 1.0
		_ColorGreenFactor("Color Green Factor", float) = 1.0
		_ColorBlueFactor("Color Blue Factor",float) = 1.0
		_MainTex("Particle Texture", 2D) = "white" {}
        _NormalTex("Normal Texture", 2D) = "black" {}
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendSrc ("BlendSrc", float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendDst ("BlendDst", float) = 1
		_DistortFactor("Distort Factor",float)=1.0
		_InvFade("Soft Particle Factor", Range(0.01,3.0)) = 1.0
	}
		Category{
				Tags{"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
				Blend [_BlendSrc] [_BlendDst]
				ColorMask RGB
				Cull Off
				Lighting Off
				ZWrite Off

				SubShader {
					Pass{
						CGPROGRAM
						#pragma vertex vert
						#pragma fragment frag
						#pragma target 3.0
						#pragma multi_compile_particles
						#pragma multi_compile_fog

						#include "UnityCG.cginc"

						sampler2D _MainTex;
                        sampler2D _NormalTex;

						fixed4 _TintColor;
                        
						float _AllColorFactor;
						float _ColorRedFactor;
						float _ColorGreenFactor;
						float _ColorBlueFactor;
						float _DistortFactor;

						struct appdata_t {
							half4 vertex : POSITION;
							half4 color : COLOR;
							half2 texcoord : TEXCOORD0;
						};

						struct v2f {
							half4 vertex : SV_POSITION;
							half4 color : COLOR;
							half2 texcoord : TEXCOORD0;
							half2 texcoord2 : TEXCOORD1;
							UNITY_FOG_COORDS(2)
							#ifdef SOFTPARTICLES_ON
								half4 projPos : TEXCOORD3;
							#endif
						};

						float4 _MainTex_ST;
						float4 _NormalTex_ST;

						v2f vert(appdata_t v)
						{
							v2f o;
							o.vertex = UnityObjectToClipPos(v.vertex);
							
							#ifdef SOFTPARTICLES_ON
								o.projPos = ComputeScreenPos(o.vertex);
								COMPUTE_EYEDEPTH(o.projPos.z);
							#endif
							o.color = v.color;
							o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                            o.texcoord2 = TRANSFORM_TEX(v.texcoord, _NormalTex);
							UNITY_TRANSFER_FOG(o, o.vertex);
							return o;
						}
						
						sampler2D _CameraDepthTexture;
						half _InvFade;

						half4 frag(v2f i ) : SV_Target
						{
                            #ifdef SOFTPARTICLES_ON
								half sceneZ = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(i.projPos))));
								half partZ = i.projPos.z;
								half fade = saturate(_InvFade * (sceneZ - partZ));
                                i.color.a *= fade;
                            #endif

                            half2 distort = UnpackNormal(tex2D(_NormalTex, i.texcoord2)).rg;
                            half4 tex = tex2D(_MainTex,i.texcoord.xy + distort.xy * _DistortFactor); //Compose normal and tex
							half4 tex2 = tex2D(_MainTex, i.texcoord);

							tex = half4(//Compose each color data using color factors
                                (tex.r) * _ColorRedFactor * 0.75f,
                                (tex.g) * _ColorGreenFactor * 0.75f,
                                (tex.b) * _ColorBlueFactor* 0.75f, tex.a);

							tex.a = tex2.a; 

							half4 res = _AllColorFactor * i.color * _TintColor * tex;
							UNITY_APPLY_FOG_COLOR(i.fogCoord, res, half4(0, 0, 0, 0));
							return res;
						}
						ENDCG
				}
			}
	}
}