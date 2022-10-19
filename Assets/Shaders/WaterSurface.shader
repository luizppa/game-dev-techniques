Shader "Unlit/WaterSurface"
{
	Properties
	{
		// Depth
		[HDR] _ShalowColor("Shalow color", Color) = (1, 1, 1, 1)
		[HDR] _DeepColor("Deep color", Color) = (1, 1, 1, 1)
		_DepthFactor("Depth Factor", float) = 1.0
  	_DepthPow("Depth Pow", float) = 1.0

		// Foam
		[HDR] _EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
		_IntersectionThreshold("Intersection threshold", Float) = 1
		_IntersectionPow("Pow", Float) = 1
		_FoamAnimationSpeed("Foam animation speed", Float) = 10

		// Waves
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_WaveSpeed("Wave Speed", float) = 1
		_WaveAmp("Wave Amp", float) = 0.2
		_ExtraHeight("Extra Height", float) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass {
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"

			struct appdata
			{
				float3 normal : NORMAL;
				float4 vertex : POSITION;
  			float4 texCoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 texCoord : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
				float3 normal : NORMAL;
			};

			float4 _ShalowColor;
			float4 _DeepColor;
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			float _DepthFactor;
			float _DepthPow;

			float4 _EdgeColor;
			fixed _IntersectionThreshold;
			fixed _IntersectionPow;
			float _FoamAnimationSpeed;

			sampler2D _NoiseTex;
			float _WaveSpeed;
			float _WaveAmp;
			float _ExtraHeight;

			bool shouldApplyFoam(fixed foamFactor){
				float sinTime = sin(_Time * _FoamAnimationSpeed) * 0.5 + 0.5;
				float timeFactor = 0.2 * sinTime;
				float inverseTimeFactor = 0.2 * (1 - sinTime);

				return ((foamFactor > 0.3 + inverseTimeFactor) || (foamFactor < timeFactor && foamFactor > 0.05));
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				float noiseSample = tex2Dlod(_NoiseTex, float4(v.texCoord.xy, 0, 0));
  			o.vertex.y += sin((_Time * _WaveSpeed) + noiseSample) * _WaveAmp + _ExtraHeight;

				o.screenPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.screenPos.z);

				o.normal = normalize(mul(unity_ObjectToWorld, float4( v.normal, 0 )));
				// o.normal = v.normal;

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = fixed4(0, 0, 0, 0);
				float3 cameraPos = _WorldSpaceCameraPos;
				float dotProd = dot(i.normal, float3(0, 1.0, 0));

				// return float4(i.normal, 1);

				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
  			float depth = sceneZ - i.screenPos.z;
				fixed depthFading = saturate((abs(pow(depth, _DepthPow))) / _DepthFactor);
				col = lerp(_ShalowColor, _DeepColor, depthFading);

				float noiseSample = tex2Dlod(_NoiseTex, float4(i.texCoord.xz, 0, 0));
				fixed intersect = saturate((abs(depth)) / (_IntersectionThreshold - noiseSample));
				float foamFactor = pow(1 - intersect, 4) * _IntersectionPow;
				if(shouldApplyFoam(foamFactor)){
	  			col += _EdgeColor;
				}

				return float4(col.rgb, depthFading);
			}
			ENDCG
		}
	}
}
