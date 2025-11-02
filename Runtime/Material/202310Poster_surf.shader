Shader "Nomlas/202310Poster(Surface)"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("MainTexture", 2D) = "white" {}
        [NoScaleOffset] _SubTex ("SubTexture", 2D) = "white" {}
        _Transition("Transition", Range(0, 1)) = 1
        _Aspect("Aspect Raito", Range(0.001, 10)) = 1
        _MainTexAspect("MainTex Aspect", Range(0.001, 10)) = 1
        _SubTexAspect("SubTex Aspect", Range(0.001, 10)) = 1
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
            float _Aspect;
            float _MainTexAspect;
            float _SubTexAspect;

            float2 calcRaito(float i)
            {
                return i < 1 ? float2(1/i, 1) : float2(1, i);
            }

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
                float2 mainTexUV = i.uv;
                float2 subTexUV = i.uv;
                mainTexUV = calcRaito(_Aspect) * calcRaito(_MainTexAspect);
                subTexUV = calcRaito(_Aspect) * calcRaito(_SubTexAspect);
                fixed4 main = tex2D(_MainTex, (i.uv - 0.5) * mainTexUV + 0.5) * _Transition;
                if (abs(i.uv.x - 0.5) > 0.5 / mainTexUV.x || abs(i.uv.y - 0.5) > 0.5 / mainTexUV.y)
                {
                    main = 0;
                }
                fixed4 sub = tex2D(_SubTex, (i.uv - 0.5) * subTexUV + 0.5) * (1 - _Transition);
                if (abs(i.uv.x - 0.5) > 0.5 / subTexUV.x || abs(i.uv.y - 0.5) > 0.5 / subTexUV.y)
                {
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
