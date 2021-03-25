Shader "Grids 2D System/Unlit Highlight Texture" {
Properties {
    _Color ("Tint Color", Color) = (1,1,1,0.5)
    _Intensity ("Intensity", Range(0.0, 2.0)) = 1.0
    _Offset ("Depth Offset", Int) = -1    
    _MainTex("Texture", 2D) = "black" {}
}
SubShader {
    Tags {
        "Queue"="Transparent+230"
        "IgnoreProjector"="True"
        "RenderType"="Transparent"
    }
    
		Offset [_Offset], [_Offset]
		Cull Off			
		Lighting Off		
		ZWrite Off			
		ZTest Always		
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass {
			CGPROGRAM						
			#pragma target 2.0				
			#pragma fragment frag			
			#pragma vertex vert				
			#include "UnityCG.cginc"		

			sampler2D _MainTex;
			fixed4 _Color;								
			fixed _Intensity;

			struct AppData {
				float4 vertex : POSITION;	
				float2 texcoord: TEXCOORD0;			
			};

			struct VertexToFragment {
				float4 pos : SV_POSITION;	
				float2 uv: TEXCOORD0;			
			};

			VertexToFragment vert(AppData v) {
				VertexToFragment o;						
				UNITY_INITIALIZE_OUTPUT(AppData,o);				
				o.pos = UnityObjectToClipPos(v.vertex);	
				o.uv = v.texcoord;
				return o;								
			}

			fixed4 frag(VertexToFragment i) : SV_Target {
				fixed4 pixel = tex2D(_MainTex, i.uv);
				fixed4 color = _Color * _Intensity;
				fixed4 res = lerp(_Color, pixel + _Color, pixel.a);
				return fixed4(res.rgb, color.a);
			}

			ENDCG		

		}
	}	
}
