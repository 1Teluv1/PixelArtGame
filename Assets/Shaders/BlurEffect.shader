Shader "Custom/BlurEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 10)) = 1.0
    }
    SubShader
    {
        // 투명도가 있는 오브젝트 지원
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

        CGINCLUDE
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
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;
        float _BlurSize;

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }

        // 단순 가우시안 블러 구현
        half4 fragHorizontal(v2f i) : SV_Target
        {
            float2 uvStep = float2(_BlurSize / _ScreenParams.x, 0);
            float2 uv = i.uv;
            half4 color = tex2D(_MainTex, uv) * 0.4;
            
            color += tex2D(_MainTex, uv + uvStep) * 0.15;
            color += tex2D(_MainTex, uv - uvStep) * 0.15;
            
            color += tex2D(_MainTex, uv + 2.0 * uvStep) * 0.1;
            color += tex2D(_MainTex, uv - 2.0 * uvStep) * 0.1;
            
            color += tex2D(_MainTex, uv + 3.0 * uvStep) * 0.05;
            color += tex2D(_MainTex, uv - 3.0 * uvStep) * 0.05;
            
            return color;
        }

        half4 fragVertical(v2f i) : SV_Target
        {
            float2 uvStep = float2(0, _BlurSize / _ScreenParams.y);
            float2 uv = i.uv;
            half4 color = tex2D(_MainTex, uv) * 0.4;
            
            color += tex2D(_MainTex, uv + uvStep) * 0.15;
            color += tex2D(_MainTex, uv - uvStep) * 0.15;
            
            color += tex2D(_MainTex, uv + 2.0 * uvStep) * 0.1;
            color += tex2D(_MainTex, uv - 2.0 * uvStep) * 0.1;
            
            color += tex2D(_MainTex, uv + 3.0 * uvStep) * 0.05;
            color += tex2D(_MainTex, uv - 3.0 * uvStep) * 0.05;
            
            return color;
        }
        ENDCG

        // 패스 1: 수평 블러
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragHorizontal
            ENDCG
        }

        // 패스 2: 수직 블러
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragVertical
            ENDCG
        }
    }
    FallBack "Diffuse"
} 