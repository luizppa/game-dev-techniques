Shader "Unlit/WaterSurface"
{
	Properties
	{
		// Depth
		[HDR] _Color("Color", Color) = (1, 1, 1, 1)
		_DepthFactor("Depth Factor", float) = 1.0
  	_DepthPow("Depth Pow", float) = 1.0

		// Foam
		[HDR] _EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
		_IntersectionThreshold("Intersection threshold", Float) = 1
		_IntersectionPow("Pow", Float) = 1

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

			struct appdata
			{
				float4 vertex : POSITION;
  			float4 texCoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 texCoord : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			float4 _Color;
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			float _DepthFactor;
			float _DepthPow;

			float4 _EdgeColor;
			fixed _IntersectionThreshold;
			fixed _IntersectionPow;

			sampler2D _NoiseTex;
			float _WaveSpeed;
			float _WaveAmp;
			float _ExtraHeight;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				float noiseSample = tex2Dlod(_NoiseTex, float4(v.texCoord.xy, 0, 0));
  			o.vertex.y += sin(_Time * _WaveSpeed * noiseSample) * _WaveAmp + _ExtraHeight;

				o.screenPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.screenPos.z);

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _Color;
				float3 cameraPos = _WorldSpaceCameraPos;

				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
  			float depth = sceneZ - i.screenPos.z;
				fixed depthFading = saturate((abs(pow(depth, _DepthPow))) / _DepthFactor);
				col *= depthFading;

				fixed intersect = saturate((abs(depth)) / _IntersectionThreshold);
  			col += _EdgeColor * pow(1 - intersect, 4) * _IntersectionPow;

				return float4(col.rgb, 1);
			}
			ENDCG
		}
	}
}
