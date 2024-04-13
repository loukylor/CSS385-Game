Shader "loukylor/OrbShader"
{
    Properties
    {
        _Color ("Color", Color) = (0.5, 0.5, 0.5, 1)
        _EmissionStrength ("Emission Strength", Float) = 1
        _Wave ("Wave", 2D) = "white" {}
        _WaveStrength ("Wave Strength", Range(0, 4)) = 0.333 
        _WaveOffset ("Wave Offset", Float) = 0
        _DistortSpeed ("Distort Speed", Float) = 1
    }
    SubShader
    {
        Tags { 
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent" 
        }
        Blend One One
        ZWrite Off
        Cull Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            float4 _Color;
            float _EmissionStrength;
            sampler2D _Wave;
            float4 _Wave_ST;
            float _WaveStrength;
            float _WaveOffset;
            float _DistortSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);

                float4 time = _Time;

                // Get the position on _Wave we want to sample
                float2 wavePos = o.uv + (time.y * _DistortSpeed);

                // Get the color of the _Wave texture at wavePos
                float4 waveSample = tex2Dlod(_Wave, float4(TRANSFORM_TEX(wavePos, _Wave), 0, 0));

                // Add the color to the vertex position away from the center
                v.vertex.xyz += (waveSample.x + _WaveOffset) * v.normal * _WaveStrength;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // lerp between 0 and 1 based on the dot product of the 
                // difference in camera direction and normal direction
                float angle = dot(i.normal, normalize(_WorldSpaceCameraPos - i.worldPos));
                float val = lerp(0, 1, angle);
                
                fixed4 col = _Color * val * _EmissionStrength;
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}
