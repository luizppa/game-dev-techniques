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
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

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
                float opacity = sin((distance / _LaserPeriod) + time) * 0.5 + 0.5;
                // float opacity = ((time - distance) % _LaserPeriod)/_LaserPeriod;
                fixed4 col = fixed4(_Color.rgb, opacity);
                return col;
            }
            ENDCG
        }
    }
}
