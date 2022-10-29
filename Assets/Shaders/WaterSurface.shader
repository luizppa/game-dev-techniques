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
		_WaveSpeed("Wave Speed", float) = 1
		_WaveAmp("Wave Amp", float) = 0.2
		_ExtraHeight("Extra Height", float) = 0.0

		// Noise
		_WavesNoise("Waves Noise Texture", 2D) = "white" {}
		_FoamNoise("Foam Noise Texture", 2D) = "white" {}
		_RefractionNoiseHorizontal("Refraction Noise Texture 1", 2D) = "white" {}
		_RefractionNoiseVertical("Refraction Noise Texture 2", 2D) = "white" {}

		// Refraction
		_RefractionAmount("Refraction Amount", float) = 1
		_RefractionSpread("Refraction Spread", float) = 15
		_RefractionSpeed("Refraction Speed", float) = 15
		_RefractionDensity("Refraction Density", float) = 0.3
		[HDR] _RefractionColor("Refraction Color", Color) = (1, 1, 1, 1)

		// Reflection
		_ReflectionAmount("Reflection Amount", float) = 1
		_ReflectionSpread("Reflection Spread", float) = 15
		_ReflectionSpeed("Reflection Speed", float) = 15
		_ReflectionDensity("Reflection Density", float) = 0.3
		[HDR] _ReflectionColor("Reflection Color", Color) = (1, 1, 1, 1)
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

			struct vertexInput
			{
				float4 normal : NORMAL;
				float4 vertex : POSITION;
  			float4 texCoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};

			struct fragmentInput
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 texCoord : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float3 normal : TEXCOORD3;
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

			float _WaveSpeed;
			float _WaveAmp;
			float _ExtraHeight;

			sampler2D _WavesNoise;
			sampler2D _FoamNoise;
			sampler2D _RefractionNoiseHorizontal;
			sampler2D _RefractionNoiseVertical;

			float _RefractionAmount;
			float _RefractionSpread;
			float _RefractionSpeed;
			float _RefractionDensity;
			float4 _RefractionColor;

			float _ReflectionAmount;
			float _ReflectionSpread;
			float _ReflectionSpeed;
			float _ReflectionDensity;
			float4 _ReflectionColor;

			SamplerState _TrilinearRepeat;
			Texture2D<float4> _Skybox;

			bool shouldApplyFoam(float depth, float2 uv){
				fixed intersect = saturate((abs(depth)) / (_IntersectionThreshold));
				float foamFactor = pow(1 - intersect, 4) * _IntersectionPow;
				float sinTime = sin(_Time * _FoamAnimationSpeed) * 0.5 + 0.5;
				float timeFactor = 0.2 * sinTime;
				float inverseTimeFactor = 0.2 * (1 - sinTime);

				float noiseSample = tex2D(_FoamNoise, uv/2).r;

				float upperFoamBound = 0.3 + inverseTimeFactor + (0.3 * noiseSample);
				float middleFoamBound = timeFactor - (0.2 * noiseSample);
				float lowerFoamBound = 0.03 + (0.1 * noiseSample);

				return ((foamFactor > upperFoamBound) || (foamFactor < middleFoamBound && foamFactor > lowerFoamBound));
			}

			float4 applyFoam(float depth, float2 uv){
				if(shouldApplyFoam(depth, uv)){
					return _EdgeColor;
				}
				else {
					return float4(0, 0, 0, 0);
				}
			}

			float4 applySunRefraction(float3 worldPos, float3 worldNormal, float viewerDepth){
				float3 sunDir = _WorldSpaceLightPos0.xyz;
				float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
				float dotProd = saturate(dot(viewDir, sunDir));

				if(dotProd < 1 - _RefractionAmount){
					return float4(0, 0, 0, 0);
				}

				float timeFactor = _Time * _RefractionSpeed;
				float noiseSampleX = tex2D(_RefractionNoiseHorizontal, float2(worldPos.x + timeFactor, worldPos.z) / _RefractionSpread).r;
				float noiseSampleZ = tex2D(_RefractionNoiseHorizontal, float2(worldPos.x, worldPos.z + timeFactor) / _RefractionSpread).r;
				float noiseSample = noiseSampleX * noiseSampleZ;
				if(noiseSample > _RefractionDensity){
					float strength = saturate(1 - (viewerDepth * 2));
					return float4(_RefractionColor.rgb * strength, 1);
				}
				return float4(0, 0, 0, 0);
			}

			float4 applySunReflection(float3 worldPos, float3 worldNormal, float depth){
				float3 sunDir = _WorldSpaceLightPos0.xyz;
				float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
				float ref = reflect(-viewDir, worldNormal);
				float dotProd = saturate(dot(ref, sunDir));

				if(dotProd < 1 - _ReflectionAmount){
					return float4(0, 0, 0, 0);
				}
				
				float timeFactor = _Time * _ReflectionSpeed;
				float noiseSampleX = tex2D(_RefractionNoiseHorizontal, float2(worldPos.x + timeFactor, worldPos.z) / _ReflectionSpread).r;
				float noiseSampleZ = tex2D(_RefractionNoiseHorizontal, float2(worldPos.x, worldPos.z + timeFactor) / _ReflectionSpread).r;
				float noiseSample = noiseSampleX * noiseSampleZ;
				if(noiseSample > _ReflectionDensity){
					return float4(_ReflectionColor.rgb * depth, 1);
				}
				return float4(0, 0, 0, 0);
			}

			float getLightIncidence(float3 normal, float offset)
			{
				float3 sunDir = normalize(_WorldSpaceLightPos0.xyz);
				float lightIncidence = saturate(offset + saturate(dot(normal, sunDir)));
				return lightIncidence;
			}

			float4 upsideSurface(fragmentInput i, float depth){
				float4 col = float4(0, 0, 0, 0);
				fixed depthFading = saturate((abs(pow(depth, _DepthPow))) / _DepthFactor);
				col = lerp(_ShalowColor, _DeepColor, (depthFading * depthFading));

				col += applyFoam(depth, i.worldPos.xz);
				col *= getLightIncidence(i.normal, 0.35);
				col += applySunReflection(i.worldPos, i.normal, depth);

				return float4(col.rgb, depthFading);
			}

			float4 downsideSurface(fragmentInput i, float depth){
				float3 cameraPos = _WorldSpaceCameraPos;
				float chunkDepth = 32.0; // hardcoded for now

				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				float3 viewDir = normalize(i.worldPos - cameraPos);

				float viewDistance = length(i.worldPos - cameraPos);
				float viewerDepth = saturate(viewDistance / 32.0);
				float inverseDepthFactor = 1 - viewerDepth;

				float sinTimeX = sin(_Time + i.worldPos.x) * 0.5 + 0.5;
				float sinTimeZ = sin(_Time + i.worldPos.z) * 0.5 + 0.5;

				float4 col = unity_FogColor + pow(inverseDepthFactor, 2);
				// col += applyFoam(depth, i.worldPos.xz);
				col += applySunRefraction(i.worldPos, i.normal, viewerDepth);
				col *= getLightIncidence(-i.normal, 0.8);

				return float4(col.rgb, saturate(0.4 + viewerDepth));
			}

			fragmentInput vert (vertexInput v)
			{
				fragmentInput o;

				o.normal = normalize(v.normal);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.texCoord = v.texCoord;

				float noiseSample = tex2Dlod(_WavesNoise, float4(v.texCoord.xy, 0, 0));
  			o.vertex.y += sin((_Time * _WaveSpeed) + noiseSample) * _WaveAmp + _ExtraHeight;

				o.screenPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.screenPos.z);

				UNITY_TRANSFER_FOG(o, o.worldPos);

				return o;
			}

			float4 frag (fragmentInput i) : SV_Target
			{
				float dotProd = dot(i.normal, float3(0, 1.0, 0));
				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
				float depth = sceneZ - i.screenPos.z;

				if(dotProd > 0.1){
					return upsideSurface(i, depth);
				}
				else if (dotProd < -0.1){
					return downsideSurface(i, depth);
				}
				else{
					return float4(unity_FogColor.rgb, 1);
				}
			}
			ENDCG
		}
	}
}
