Shader "Unlit/LaserBeamShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _LaserOrigin ("Laser Origin", Vector) = (0,0,0)
        _LaserPeriod ("Laser Period", Float) = 1
        _LaserSpeed ("Laser Speed", Float) = 1
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 wsPosition : TEXCOORD0;
            };

            float4 _Color;
            float3 _LaserOrigin;
            float _LaserPeriod;
            float _LaserSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.wsPosition = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float time = _Time.y * _LaserSpeed;
                float distance = length(_LaserOrigin - i.wsPosition);
                float opacity = ((time - distance) % _LaserPeriod)/_LaserPeriod;
                fixed4 col = _Color + opacity;
                return col;
            }
            ENDCG
        }
    }
}
