Shader "Grids 2D System/Unlit Single Color Cell Thin Line" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Offset ("Depth Offset", float) = -0.01  
}
 
SubShader {
    Tags {
      "Queue"="Geometry+2"
      "RenderType"="Opaque"
  	}
    Blend SrcAlpha OneMinusSrcAlpha
  	ZWrite Off
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#include "UnityCG.cginc"

		float _Offset;
		fixed4 _Color;

		//Data structure communication from Unity to the vertex shader
		//Defines what inputs the vertex shader accepts
		struct AppData {
			float4 vertex : POSITION;
		};

		//Data structure for communication from vertex shader to fragment shader
		//Defines what inputs the fragment shader accepts
		struct VertexToFragment {
			float4 pos : SV_POSITION;	
		};
		
		//Vertex shader
		VertexToFragment vert(AppData v) {
			VertexToFragment o;		
			UNITY_INITIALIZE_OUTPUT(AppData,o);				
			o.pos = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
			o.pos.z-=_Offset;
			#else
			o.pos.z+=_Offset;
			#endif
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			return _Color;
		}
			
		ENDCG
    }
    
}
}
