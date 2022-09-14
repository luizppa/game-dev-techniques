//Written by ifurkend of Moonflower Carnivore in 2017. All rights reserved.
//In case you really want to support Shader Model 1 which is completely antique, comment lines 7, 49 and 80 which involves _Bias, then uncomment line 79.
Shader "Particles/Alpha Blended Cubemap" {
	Properties {
		_TintColor ("Tint Color", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGB) Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _Cube ("Cubemap (6 frames layout)", Cube) = "grey" {}
		_Bias ("Cubemap Bias (blur)", Range(0,8)) = 0//The greater bias, the more blur is the cubemap. This requires the cubemap has mip maps generated in texture settings.
		//_AmbientPow ("Ambient Power", Range(0,1)) = 0.5
		_DLightPow ("Dir Light Power", Range(0,10)) = 0.5//How much the main directional light affects the material.
		_Glow ("Intensity", Range(0, 10)) = 0//Only increase the _Glow/Intensity when HDR mode is enabled in your rendering camera.
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "PreviewType" = "Plane"}
		Cull Back
		ZTest Off
		ZWrite Off
		Lighting On
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				
				#pragma multi_compile_particles
				
				#include "UnityCG.cginc"
				#include "UnityLightingCommon.cginc"

				struct appdata {
					float4 vertex : POSITION;
					half2 uv : TEXCOORD0;
					half4 color : COLOR;
					float3 normal : NORMAL;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 uv : TEXCOORD0;
					half3 worldRefl : TEXCOORD1;
					half4 color : COLOR;
				};
				
				half4 _TintColor;
				sampler2D _MainTex;
				half4 _MainTex_ST;
				samplerCUBE _Cube;
				half _Bias;
				//half _AmbientPow;
				half _DLightPow;
				half _Glow;
				
				v2f vert (appdata v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv,_MainTex);
					o.color = v.color;
					//o.color.rgb += ShadeSH9(half4(UnityObjectToWorldNormal(v.normal),1)) * _AmbientPow;
					o.color.rgb += _LightColor0.rgb * _DLightPow;
					o.color.rgb += _Glow;
					
					float3 worldNormal = UnityObjectToWorldNormal(float3(
					v.normal.xy,
					//v.normal.z
					v.normal.z*2-1
					));
					o.worldRefl = reflect(float3(
					v.uv.x-0.5,
					v.uv.y-0.5,
					0.5
					), worldNormal);
					
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target {
					//fixed4 cubemap = texCUBE (_Cube, i.worldRefl) * i.color * _TintColor;
					fixed4 cubemap = texCUBEbias (_Cube, half4(i.worldRefl,_Bias));
					fixed4 maintex2d = tex2D (_MainTex, i.uv);
					fixed4 col;
					col.rgb = cubemap.rgb * maintex2d.rgb * i.color.rgb * _TintColor.rgb;
					col.a = (cubemap.r + cubemap.g + cubemap.b) * 0.33 * cubemap.a * maintex2d.a * i.color.a * _TintColor.a;
					return col;
				}
			ENDCG
		}
	}
}