Shader "Nomlas/202310Poster"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("MainTexture", 2D) = "white" {}
        [NoScaleOffset] _SubTex ("SubTexture", 2D) = "white" {}
        _Transition("Transition", Range(0, 1)) = 1
        _MainTexX("MainTexX", Range(0.001, 10)) = 1
        _MainTexY("MainTexY", Range(0.001, 10)) = 1
        _SubTexX("SubTexX", Range(0.001, 10)) = 1
        _SubTexY("SubTexY", Range(0.001, 10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
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
            sampler2D _SubTex;
            float4 _MainTex_ST;
            float4 _SubTex_ST;
            float _Transition;
            float _MainTexX;
            float _MainTexY;
            float _SubTexX;
            float _SubTexY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float u = ceil(i.uv.x) * 0.5;
                float v = ceil(i.uv.y) * 0.5;
                fixed4 main = tex2D(_MainTex, float2(lerp(u, i.uv.x, _MainTexX), lerp(v, i.uv.y, _MainTexY))) * _Transition;
                fixed4 sub = tex2D(_SubTex, float2(lerp(u, i.uv.x, _SubTexX), lerp(v, i.uv.y, _SubTexY))) * (1 - _Transition);
                if (i.uv.x < (1 - 1 / _MainTexX) / 2 || 1 - (1 - 1 / _MainTexX) / 2 < i.uv.x || i.uv.y < (1 - 1 / _MainTexY) / 2 || 1 - (1 - 1 / _MainTexY) / 2 < i.uv.y) {
                    main = 0;
                }
                if (i.uv.x < (1 - 1 / _SubTexX) / 2 || 1 - (1 - 1 / _SubTexX) / 2 < i.uv.x || i.uv.y < (1 - 1 / _SubTexY) / 2 || 1 - (1 - 1 / _SubTexY) / 2 < i.uv.y) {
                    sub = 0;
                }
                fixed4 col = main + sub;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
