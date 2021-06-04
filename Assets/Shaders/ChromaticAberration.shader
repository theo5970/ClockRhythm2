Shader "Unlit/ChromaticAberration"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Amount("Amount", Range(-0.1, 0.1)) = 0.0005
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Amount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 색수차 쉐이더 코드
                fixed4 col;
                col.r = tex2D(_MainTex, float2(i.uv.x - _Amount, i.uv.y + _Amount)).r;
                col.g = tex2D(_MainTex, i.uv).g;
                col.b = tex2D(_MainTex, float2(i.uv.x + _Amount, i.uv.y - _Amount)).b;
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}
