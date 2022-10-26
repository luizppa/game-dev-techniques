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

		// Phong
		_SpecularAmount("Specular Amount", float) = 0.2
		_SpecularSpread("Specular Spread", float) = 0.4
		_SpecularSpeed("Specular Speed", float) = 3
		_SpecularDensity("Specular Density", float) = 3
		[HDR] _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
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

			sampler2D _NoiseTex;
			float _WaveSpeed;
			float _WaveAmp;
			float _ExtraHeight;

			float _SpecularAmount;
			float _SpecularSpread;
			float _SpecularSpeed;
			float _SpecularDensity;
			float4 _SpecularColor;

			SamplerState _TrilinearRepeat;
			Texture2D<float4> _Skybox;

			bool shouldApplyFoam(float depth, float2 uv){
				fixed intersect = saturate((abs(depth)) / (_IntersectionThreshold));
				float foamFactor = pow(1 - intersect, 4) * _IntersectionPow;
				float sinTime = sin(_Time * _FoamAnimationSpeed) * 0.5 + 0.5;
				float timeFactor = 0.2 * sinTime;
				float inverseTimeFactor = 0.2 * (1 - sinTime);

				float noiseSample = tex2D(_NoiseTex, uv/2).r;

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

				if(dotProd < _SpecularAmount){
					return float4(0, 0, 0, 0);
				}

				float noiseSample = tex2D(_NoiseTex, (worldPos.xz/_SpecularSpread) + (_Time.x * _SpecularSpeed)).r;
				if(noiseSample > _SpecularDensity){
					float strength = saturate(1 - (viewerDepth * 2));
					return float4(_SpecularColor.rgb * strength, 1);
				}
				return float4(0, 0, 0, 0);
			}

			float4 applySunReflection(float3 worldPos, float3 worldNormal, float depth){
				float3 sunDir = _WorldSpaceLightPos0.xyz;
				float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
				float ref = reflect(-viewDir, worldNormal);
				float dotProd = saturate(dot(ref, sunDir));
				if(dotProd < 1 - _SpecularAmount){
					return float4(0, 0, 0, 0);
				}
				float noiseSample = tex2D(_NoiseTex, (worldPos.xz/_SpecularSpread) + (_Time.x * _SpecularSpeed)).r;
				if(noiseSample > _SpecularDensity){
					return float4(_SpecularColor.rgb * depth, 1);
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

				float noiseSample = tex2Dlod(_NoiseTex, float4(i.texCoord.xz, 0, 0));
				col += applyFoam(depth, i.worldPos.xz);
				col *= getLightIncidence(i.normal, 0.35);
				// col += applySunReflection(i.worldPos, i.normal, depth);

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

				float4 col = unity_FogColor + (inverseDepthFactor * inverseDepthFactor);
				// col += applyFoam(depth, i.worldPos.xz);
				col += applySunRefraction(i.worldPos, i.normal, viewerDepth);
				col *= getLightIncidence(-i.normal, 0.8);

				return float4(col.rgb, viewerDepth);
			}

			fragmentInput vert (vertexInput v)
			{
				fragmentInput o;

				o.normal = normalize(v.normal);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.texCoord = v.texCoord;

				float noiseSample = tex2Dlod(_NoiseTex, float4(v.texCoord.xy, 0, 0));
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
