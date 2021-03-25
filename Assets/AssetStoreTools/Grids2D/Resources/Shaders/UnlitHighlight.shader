Shader "Grids 2D System/Unlit Highlight" {
Properties {
    _Color ("Tint Color", Color) = (1,1,1,0.5)
    _Intensity ("Intensity", Range(0.0, 2.0)) = 1.0
    _Offset ("Depth Offset", Int) = -1    
}
SubShader {
    Tags {
        "Queue"="Transparent+5"
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

			fixed4 _Color;								
			fixed _Intensity;

			struct AppData {
				float4 vertex : POSITION;				
			};

			struct VertexToFragment {
				float4 pos : SV_POSITION;				
			};

			VertexToFragment vert(AppData v) {
				VertexToFragment o;						
				UNITY_INITIALIZE_OUTPUT(AppData,o);				
				o.pos = UnityObjectToClipPos(v.vertex);	
				return o;								
			}

			fixed4 frag(VertexToFragment i) : SV_Target {
				return _Color * _Intensity;		
			}

			ENDCG		

		}
	}	
}
