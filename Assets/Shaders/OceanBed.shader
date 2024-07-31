Shader "Custom/OceanBed"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        // _BiomeMap ("Biome Map", 2D) = "white" {}
        _Caustics ("Caustics", 2D) = "white" {}
        _CausticsStart ("Caustics Start", Range(0,100)) = 28
        _CausticsEnd ("Caustics End", Range(0,100)) = 15
        _CausticsSpeed ("Caustics Speed", Range(0,100)) = 10
        _CausticsIntensity ("Caustics Intensity", Range(0,1)) = 1

        _BiomeMap1 ("Biome Map 1", 2D) = "white" {}
        _BiomeMap2 ("Biome Map 2", 2D) = "white" {}
        _BiomeMap3 ("Biome Map 3", 2D) = "white" {}
        _BiomeMap4 ("Biome Map 4", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        sampler2D _MainTex;
        sampler2D _BiomeMap1;
        sampler2D _BiomeMap2;
        sampler2D _BiomeMap3;
        sampler2D _BiomeMap4;
        sampler2D _Caustics;
        struct Input
        {
            float3 normal;
            float2 uv_MainTex;
            float3 wPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _CausticsStart;
        float _CausticsEnd;
        float _CausticsSpeed;
        float _CausticsIntensity;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input,data);
            data.normal = mul(unity_ObjectToWorld, float4( v.normal, 0 ));
            data.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        }

        float invLerp(float from, float to, float value){
          return (value - from) / (to - from);
        }

        float4 applyCaustics(Input IN){
            float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
            float time = _Time.x * _CausticsSpeed;
            float causticsX = tex2D (_Caustics, float2(IN.wPos.x + time, IN.wPos.z) * 0.08).r;
            float causticsY = tex2D (_Caustics, float2(IN.wPos.x, IN.wPos.z + time) * 0.08).r;
            float caustics = pow((causticsX + causticsY) * 0.5 * _CausticsIntensity, 2);
            float depth = IN.wPos.y;
            float causticsFactor = saturate(invLerp(_CausticsEnd, _CausticsStart, depth)) * caustics;

            return float4(causticsFactor, causticsFactor, causticsFactor, 0) * angle;
        }

        // ====================== Biome Functions ====================== //

        float4 rockyMeadowsSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.93){
              return float4(angle * 0.75, angle * 0.68, angle * 0.2, 1.0);
          }
          else{
            return float4(angle * 0.9, .2, 1 - (angle2 * 0.9), 1.0);
          }
        }

        float4 deepSeaSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float3 color = float3(angle * 0.9, .2, 1 - (angle * angle * 0.7));
          return float4(color.rgb, 1);
        }

        float4 cavernsSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.93){
            return float4(0.2, angle2 * 0.1, 0.0, 1.0);
          }
          else{
            return float4(angle * 0.7, angle * 0.1, angle * 0.8, 1.0);
          }
        }

        float4 serenityFieldsSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          return float4(angle2 * 0.3, angle * 0.5, angle2 * 0.2, 1.0);
        }

        float4 canyonsSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.93){
            return float4(angle * 0.8, angle * 0.4, 0.1, 1.0);
          }
          else{
            return float4(angle * 0.8, angle * 0.7, angle2 * 0.4, 1.0);
          }
        }

        float4 steamingValleySurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.93){
            return float4(angle * 0.4, angle * 0.4, 0.6, 1.0);
          }
          else{
            return float4(angle * 0.1, angle * 0.1, angle * 0.1, 1.0);
          }
        }

        float4 wastelandSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.93){
            return float4(angle * 0.8, angle * 0.4, 0.1, 1.0);
          }
          else{
            return float4(angle * 0.8, angle * 0.7, angle2 * 0.4, 1.0);
          }
        }

        float4 bloomingHillsSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.93){
            return float4(angle * 0.8, angle * 0.4, 0.9, 1.0);
          }
          else{
            return float4(angle2 * 0.4, angle2 * 0.5, angle2 * 0.9, 1.0);
          }
        }

        float4 underwaterTundraSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.93){
            return float4(angle2 * 0.6, angle2 * 0.7, angle2 * 0.9, 1.0);
          }
          else{
            return float4(angle2 * 0.4, angle2 * 0.5, angle2 * 0.9, 1.0);
          }
        }

        float4 tropicalIslandsSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.93){
            return float4(angle2 * 0.3, angle * 0.65, angle2 * 0.2, 1.0);
          }
          else{
            return float4(angle * 0.6, angle * 0.6, angle2 * 0.3, 1.0);
          }
        }

        float4 deadlandsSurface(Input IN){
          return float4(0.1, 0.0, 0.0, 1);
        }

        float4 stoneValleySurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.8){
            return float4(angle * 0.1, angle2 * 0.9, 0.2, 1.0);
          }
          else{
            return float4(angle * 0.2, angle * 0.2, angle * 0.2, 1.0);
          }
        }

        float4 acidPlateauSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.8){
            return float4(angle * 0.1, angle2 * 0.7, 0.2, 1.0);
          }
          else{
            return float4(angle * 0.3, angle * 0.3, angle * 0.3, 1.0);
          }
        }

        float4 darkDeepsSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;
          return float4(angle2 * 0.1, angle2 * 0.1, angle2 * 0.1, 1);
        }

        float4 nowhereSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;
          return float4(angle2 * 0.1, angle2 * 0.1, angle2 * 0.1, 1);
        }

        float4 scarletVeilSurface(Input IN){
          float angle = (dot(float3(0, 1, 0), IN.normal) + 1) / 2;
          float angle2 = angle * angle;

          if(angle > 0.8){
            return float4(angle * 1.0, angle2 * 0.2, angle * 0.2, 1.0);
          }
          else{
            return float4(angle * 0.7, angle2 * 0.2, angle2 * 0.2, 1.0);
          }
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 biomeValues[4] = {
              tex2D(_BiomeMap1, IN.wPos.xz),
              tex2D(_BiomeMap2, IN.wPos.xz),
              tex2D(_BiomeMap3, IN.wPos.xz),
              tex2D(_BiomeMap4, IN.wPos.xz)
            };
            float4 color = float4(0.0, 0.0, 0.0, 1);

            color += (rockyMeadowsSurface(IN) * biomeValues[0].r);
            color += (deepSeaSurface(IN) * biomeValues[0].g);
            color += (cavernsSurface(IN) * biomeValues[0].b);
            color += (serenityFieldsSurface(IN) * biomeValues[0].a);

            color += (canyonsSurface(IN) * biomeValues[1].r);
            color += (steamingValleySurface(IN) * biomeValues[1].g);
            color += (wastelandSurface(IN) * biomeValues[1].b);
            color += (bloomingHillsSurface(IN) * biomeValues[1].a);

            color += (underwaterTundraSurface(IN) * biomeValues[2].r);
            color += (tropicalIslandsSurface(IN) * biomeValues[2].g);
            color += (deadlandsSurface(IN) * biomeValues[2].b);
            color += (stoneValleySurface(IN) * biomeValues[2].a);

            color += (acidPlateauSurface(IN) * biomeValues[3].r);
            color += (darkDeepsSurface(IN) * biomeValues[3].g);
            color += (nowhereSurface(IN) * biomeValues[3].b);
            color += (scarletVeilSurface(IN) * biomeValues[3].a);
            // color = (biomeValues[0] + biomeValues[1] + biomeValues[2] + biomeValues[3]) / 4;

            color = tex2D (_MainTex, IN.uv_MainTex) * color;

            if(IN.wPos.y <= _CausticsStart && IN.wPos.y >= _CausticsEnd){
              color += applyCaustics(IN);
            }
            
            o.Albedo = color.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
