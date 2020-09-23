// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/bg_scroller"
{
    Properties
    {
        _Overlay ("Color", Color) = (1,1,1,1)
        //_OutColor("Ouside overlay", Color) = (1,1,1,1)
        //_AreaSize("Area size", float) = 100
        //_BorderSize("Border size", float) = 0.1
        _Scale ("Scale", float) = 1
        _MainTex ("Texture", 2D) = "white" {}
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
            //float4 _OutColor;
            //float _AreaSize;
            //float _BorderSize;
            float _Scale;
            float4 _MainTex_ST;

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
                fixed4 tex = tex2D(_MainTex, i.uv * _Scale);

                fixed4 col = _Overlay;
                fixed4 res = tex * col;

                // --- border shader ---
                /*fixed4 res;
                float2 pos = i.worldPos.xy;
                float2 center = 0;
                int blend = 0;

                if (distance(center, pos) > _AreaSize)
                {
                    if (round(frac(pos.x + pos.y)) == 1 || distance(center, pos) < _AreaSize + _BorderSize)
                    {
                        blend = 1;
                    }
                }

                fixed4 col = _OutColor;
                if (blend == 1) 
                {
                    col = _OutColor;
                    res = (tex + col) * 0.5;
                }
                else 
                {
                    col = _Overlay;
                    res = tex * col; 
                }*/

                return res;
            }
            ENDCG
        }
    }
}
