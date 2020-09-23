// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/bg_dots"
{
    Properties
    {
        _Overlay ("Color", Color) = (1,1,1,1)
        _Scale ("Scale", float) = 1
        _MainTex("Texture", 2D) = "white" {}


        _lower("Lower", float) = 1
        _upper("Upper", float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

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
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _Overlay;
            float _Scale;
            float _lower;
            float _upper;
            float4 _MainTex_ST;

            float2 rand(float2 co) {
                return float2(
                    frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453),
                    frac(cos(dot(co.yx, float2(80.64947, 45.097))) * 23758.5453)
                ) * 2.0 - 1.0;
            }

            /*
                Dot distance field.
            */
            float dots(float2 uv)
            {
                // Consider the integer component of the UV coordinate
                // to be an ID of a local coordinate space.
                float2 g = floor(uv);
                // "What 'local coordinate space'?" you say? Why the one
                // implicitly defined by the fractional component of
                // the UV coordinate. Here we translate the origin to the
                // center.
                float2 f = frac(uv) * 2.0 - 1.0;

                // Get a random value based on the "ID" of the coordinate
                // system. This value is invariant across the entire region.
                float2 r = rand(g) * .5;

                // Return the distance to that point.
                return length(f + r);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldPos = worldPos;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _Overlay;
                fixed4 res = 1.0-smoothstep(_lower, _upper, dots(i.uv * _Scale)).x * col;
                return res;
            }
            ENDCG
        }
    }
}
