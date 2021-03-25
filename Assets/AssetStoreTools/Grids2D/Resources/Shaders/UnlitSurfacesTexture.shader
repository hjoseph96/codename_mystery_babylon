Shader "Grids 2D System/Unlit Surface Texture" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
    _MainTex ("Texture", 2D) = "black" {}
    _Offset ("Depth Offset", Int) = -1
}
 
SubShader {
    Tags {
        "Queue"="Geometry+1"
        "RenderType"="Transparent"
    }
    	
    Blend SrcAlpha OneMinusSrcAlpha
    Color [_Color]
   	ZTest Always
   	ZWrite Off
   	Offset [_Offset], [_Offset]
    Pass {
    	SetTexture [_MainTex] {
            Combine Texture * Primary, Texture * Primary
        }
    }
}
 
}
