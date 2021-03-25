Shader "Light2D/Internal/NormalMapPixelToLight" {
    Properties
    {
        _MainTex ("Diffuse Texture", 2D) = "white" {}
        _Bump ("Bump", 2D) = "Bump" {}
        _SecTex ("CollisionTexture", 2D) = "white" {}

        _LightSize ("LightSize", Float) = 1

        _InvertX ("InvertX", Float) = 1
        _InvertY ("InvertY", Float) = 1
        _Depth ("Depth", Float) = 1

        _LightX ("LightX", Float) = 1
        _LightY ("LightY", Float) = 1
        _LightZ ("LightZ", Float) = 1
        _LightIntensity ("LightIntensity", Float) = 1
        _LightColor("LightColor", float) = 1
    
        _intensity ("Intensity", Range(0,300)) = 1
        _translucency ("Radius", Range(0,300)) = 30
		_textureSize("TextureSize", Range(32,4000)) = 2048
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {    
            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag 

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _Bump;
            sampler2D _SecTex;
  
            uniform float _LightSize;

            uniform float _InvertX;
            uniform float _InvertY;

            uniform float _LightX;
            uniform float _LightY;
            uniform float _LightZ;
            uniform float _LightIntensity;
            uniform float _LightColor;
            uniform float _Depth;

            float _intensity;
            float _translucency;
			float _textureSize;
            
            float4 _SecTex_ST;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 pos : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
            };

            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;

                output.pos = UnityObjectToClipPos(input.vertex);
                output.posWorld = mul(unity_ObjectToWorld, input.vertex);

                output.uv = float2(input.uv.xy);
                output.color = input.color;

                return output;
            }

            fixed4 Translucency (VertexOutput IN, float radius) {
                float2 pos = float2(IN.pos.x / _textureSize, IN.pos.y / _textureSize);
                float2 tc = pos;
                float resolution = 1000;
                float blur = radius / resolution / 4;     
                int size = 30;
                float constant = 1.0 / (size * 75) * 5;
                float raySize = 9;

                fixed4 sum = fixed4(0, 0, 0, 0);

                [unroll]
				for (int x = -raySize; x < raySize; x++) {

					[unroll]
					for (int y = -raySize + 1; y < raySize; y++) {
				
						float val = constant * sqrt(raySize - abs(x) + raySize - abs(y));

						sum += tex2D(_SecTex, float2(pos.x + x * blur, pos.y + y * blur)) * val;
					}
				}

                return float4(sum.rgb, 1);
            }

            float4 frag(VertexOutput input) : COLOR {
                float localTranslucency = input.color.a;
				float radius = _translucency * localTranslucency;
	
                float alpha = tex2D(_MainTex, input.uv).a;

                float3 normalDirection = (tex2D(_Bump, input.uv).xyz - 0.5f) * 2;

                float4 flat = float4(0, 0, -1, 1);

                float4 normalColor = lerp(flat, float4((normalDirection.xyz), 1),  _Depth);

                normalDirection = float3(mul(normalColor, unity_WorldToObject).xyz) ;

                normalDirection.x *= _LightX * _InvertX;
                normalDirection.y *= _LightY * _InvertY;
        

                normalDirection.z *= -1;
                normalDirection = normalize(normalDirection);

                float3 posWorld = float3(input.posWorld.xyz);
                posWorld.z = 0;

                float4 objectOrigin = mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0) );
           
                float3 vertexToLightSource = float3(0,0, -_LightZ) - posWorld.xyz;
     
                float distance = 1;
                float lightUV = 1 - ((distance - _LightSize) / _LightSize);

                float color = _LightColor;      
                lightUV *= color;


                float attenuation = sqrt(distance * distance) * _LightIntensity; 
                float3 lightDirection = normalize(vertexToLightSource);

                float normalDotLight = dot(normalDirection, lightDirection);
                float diffuseLevel = attenuation * max(0.0f, normalDotLight);

                float specularLevel;
                if (normalDotLight < 0.0f) {
                    specularLevel = 0.0f;
                } else {
                    specularLevel = attenuation * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), float3(0.0f, 0.0f, -1.0f))), 10);
                }

                float3 diffuseReflection = diffuseLevel * lightUV;
                float3 specularReflection = specularLevel * lightUV;

                fixed4 tex = float4(1.0, 1.0, 1.0, 1.0);

                if (radius > 0) {
                    tex = Translucency(input, radius);
                }

                return float4(diffuseReflection + specularReflection, alpha) * tex * _intensity;
             }

             ENDCG
        }
    }
}
