// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/CartoonStyle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BorderThreshold ("Border Threshold", Range(0, 1)) = 0.3
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)
        [Toggle] _UseGrayScale ("Use Gray Scale", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _BorderThreshold;
            float4 _EdgeColor;
            bool _UseGrayScale;

			// Mesh dara: vertex position, vertex normal, UVs, tangents, colors
            struct VertexShaderInput
            {
                float4 vertex : POSITION;
				// float4 color : COLOR;
				float2 uv0 : TEXCOORD0;
				// float2 uv1 : TEXCOORD1;
				float4 normal: NORMAL;
            };

            struct FragmentShaderInput
            {
                float2 uv0 : TEXCOORD0;
                float4 clipPos : SV_POSITION;
				float3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            FragmentShaderInput vert (VertexShaderInput v)
            {
                FragmentShaderInput o;
                o.clipPos = UnityObjectToClipPos(v.vertex);
                o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
				o.normal = v.normal;
                return o;
            }

            fixed4 rgbToGs(float4 rgb){
                float shade = (rgb.x + rgb.y + rgb.z)/3;
                float4 color = float4(shade, shade, shade, 1);
                return color;
            }

            fixed4 frag (FragmentShaderInput i) : SV_Target
            {
                float4 objPos = mul(unity_ObjectToWorld, float4(0,0,0,1));
                float3 viewDir = normalize(objPos - _WorldSpaceCameraPos);
				float3 normal = i.normal;
                float3 worldNormal = mul(unity_ObjectToWorld, float4( normal, 0.0 )).xyz;
                float angle = dot(viewDir, worldNormal);

                float boderTreshhold = _BorderThreshold;

                if(angle <= boderTreshhold && angle >= -boderTreshhold){
                    return _EdgeColor;
                }
                else{
                    float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                    angle = dot(lightDirection, worldNormal);

                    float4 color = tex2D(_MainTex, i.uv0);
                    if(_UseGrayScale){
                        color = rgbToGs(color);
                    }

                    if(angle <= 0.4)
                    {
                        return color * 0.5;
                    }
                    if(angle <= 0.7)
                    {
                        return color * 0.7;
                    }
                    else if(angle <= 0.96)
                    {
                        return color * 0.9;
                    }
                    else
                    {
                        return color * 1;
                    }
                }

            }
            ENDCG
        }
    }
}
