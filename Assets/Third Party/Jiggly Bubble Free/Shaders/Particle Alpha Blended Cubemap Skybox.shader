//Written by ifurkend of Moonflower Carnivore in 2017. All rights reserved.
Shader "Particles/Alpha Blended Cubemap Skybox" {
	Properties {
		_TintColor ("Tint Color", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGB) Mask (A)", 2D) = "white" {}
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
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
					float3 normal : NORMAL;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					half3 worldRefl : TEXCOORD1;
					float4 color : COLOR;
				};
				
				half4 _TintColor;
				sampler2D _MainTex;
				half4 _MainTex_ST;
				half _DLightPow;
				half _Glow;
				
				v2f vert (appdata v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv,_MainTex);
					o.color = v.color;
					o.color.rgb += _LightColor0.rgb * _DLightPow;
					o.color.rgb += _Glow;
					
					float3 worldNormal = UnityObjectToWorldNormal(float3(
					v.normal.xy,
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
					fixed4 cubemap = UNITY_SAMPLE_TEXCUBE (unity_SpecCube0, i.worldRefl);
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