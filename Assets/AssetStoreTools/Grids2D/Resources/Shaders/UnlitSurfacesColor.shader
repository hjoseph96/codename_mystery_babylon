Shader "Grids 2D System/Unlit Surface Single Color" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
    _Offset ("Depth Offset", Int) = -1
}
 
SubShader {
    Color [_Color]
    Tags {
      "Queue"="Transparent+1"
      "RenderType"="Transparent"
  	}
  	Offset [_Offset], [_Offset]
  	Blend SrcAlpha OneMinusSrcAlpha
  	ZWrite Off
    Pass {
    }
}
 
}
