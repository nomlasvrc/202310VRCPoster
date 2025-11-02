Shader "Nomlas/202310Poster(Surface)"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("MainTexture", 2D) = "white" {}
        [NoScaleOffset] _SubTex ("SubTexture", 2D) = "white" {}
        _Transition("Transition", Range(0, 1)) = 1
        _Aspect("Aspect Ratio", Range(0.001, 10)) = 1
        _MainTexAspect("MainTex Aspect", Range(0.001, 10)) = 1
        _SubTexAspect("SubTex Aspect", Range(0.001, 10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 100

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows alpha:fade
            #pragma target 3.0

            struct Input
            {
                float2 uv_MainTex;
            };

            sampler2D _MainTex;
            sampler2D _SubTex;
            float _Transition;
            float _Aspect;
            float _MainTexAspect;
            float _SubTexAspect;

            float2 calcRatio(float i)
            {
                return i < 1 ? float2(1/i, 1) : float2(1, i);
            }

            void surf (Input IN, inout SurfaceOutputStandard o)
            {
                float2 mainTexUV = calcRatio(_Aspect) * calcRatio(_MainTexAspect);
                float2 subTexUV = calcRatio(_Aspect) * calcRatio(_SubTexAspect);
                fixed4 main = tex2D(_MainTex, (IN.uv_MainTex - 0.5) * mainTexUV + 0.5) * _Transition;
                if (abs(IN.uv_MainTex.x - 0.5) > 0.5 / mainTexUV.x || abs(IN.uv_MainTex.y - 0.5) > 0.5 / mainTexUV.y)
                {
                    main = 0;
                }
                fixed4 sub = tex2D(_SubTex, (IN.uv_MainTex - 0.5) * subTexUV + 0.5) * (1 - _Transition);
                if (abs(IN.uv_MainTex.x - 0.5) > 0.5 / subTexUV.x || abs(IN.uv_MainTex.y - 0.5) > 0.5 / subTexUV.y)
                {
                    sub = 0;
                }
                fixed4 col = main + sub;
                o.Albedo = col.rgb;
                o.Alpha = col.a;
            }
            ENDCG
    }
}
