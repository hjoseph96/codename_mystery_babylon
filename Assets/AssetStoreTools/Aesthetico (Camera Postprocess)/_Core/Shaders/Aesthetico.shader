Shader "ToucanSystems/Aesthetico"
{
	Properties 
	{
		[HideInInspector] _MainTex ("Main Texture", 2D) = "white" {}
		[HideInInspector] _ColorsMap ("Colors Map", 2D) = "white" {}
		[HideInInspector] _PixelCountU ("Pixel Count U", float) = 10
		[HideInInspector] _PixelCountV ("Pixel Count V", float) = 10
		[HideInInspector] _ColorsRange ("Lower Color Range", float) = 10
		[HideInInspector] _ColoringIntensity ("Coloring Intensity", Range(0,1)) = 1
		[HideInInspector] _EnablePixelation ("Enable Pixelation", int) = 1
		[HideInInspector] _EnableColoring ("Enable Coloring", int) = 1
		[HideInInspector] _ColorCorrection ("Color Correction", Range(0.1, 5)) = 1
	}

	SubShader 
	{
		Tags 
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}

		LOD 100
		
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha 
		
        Pass 
        {            
			CGPROGRAM 
			#pragma vertex vert
			#pragma fragment frag
							
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;	
			sampler2D _ColorsMap;
			float _PixelCountU;
			float _PixelCountV;
			float _ColorsRange;
			float _ColoringIntensity;
			int _EnablePixelation;
			int _EnableColoring;
			float _ColorCorrection;
					
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD1;
			};
			
			v2f vert(appdata_base v)
			{
				v2f o;			    
				o.uv = v.texcoord.xy;
				o.pos = UnityObjectToClipPos(v.vertex);
			    
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{   
				half2 uv = i.uv;

				if (_EnablePixelation == 1)
				{
					float pixelWidth = 1.0f / _PixelCountU;
					float pixelHeight = 1.0f / _PixelCountV;
				
					uv = half2(((int)(i.uv.x / pixelWidth) * pixelWidth) + pixelWidth / 2, ((int)(i.uv.y / pixelHeight) * pixelHeight) + pixelWidth / 2);	
				}

				half4 col = tex2D(_MainTex, uv);

				if (_EnableColoring == 1)
				{
					float colorBrightness = 1 - clamp(0.33 * col.r + 0.33 * col.g + 0.33 * col.b, 0.04, 0.96);
					half4 cm = tex2D(_ColorsMap, float2(colorBrightness, 0.5));

					col = lerp(col, cm, _ColoringIntensity);
					
					col = pow(col, 1 / _ColorCorrection);

					col.r = (float)((int)(col.r * _ColorsRange)) / _ColorsRange;
					col.g = (float)((int)(col.g * _ColorsRange)) / _ColorsRange;
					col.b = (float)((int)(col.b * _ColorsRange)) / _ColorsRange;	
				}

				col.a = 1;

				return col;
			}
			ENDCG
	  	}
	}
}