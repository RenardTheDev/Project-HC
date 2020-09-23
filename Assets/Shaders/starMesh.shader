Shader "Unlit/starMesh"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Spectrum("Spectrum", 2D) = "white" {}
        _rndA("rndA", float) = 12.9898
        _rndB("rndB", float) = 78.233
        _rndC("rndC", float) = 45.5432
        _rndD("rndD", float) = 43758.5453
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _Spectrum;
            float4 _Spectrum_ST;

            float _rndA;
            float _rndB;
            float _rndC;
            float _rndD;

            float rand(float3 co)
            {
                return frac(sin(dot(co.xyz, float3(_rndA, _rndB, _rndC))) * _rndD);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
