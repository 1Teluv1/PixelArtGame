Shader "Custom/SafeHitWaveShader_NoOutline"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WaveStrength ("Wave Strength", Float) = 0
        _WaveSpeed ("Wave Speed", Float) = 20
        _WaveFrequency ("Wave Frequency", Float) = 30
        _HitColor ("Hit Color", Color) = (1,0.5,0.5,1)
        _HitBlend ("Hit Blend", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
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
            float4 _Color;
            float _WaveStrength;
            float _WaveSpeed;
            float _WaveFrequency;
            float4 _HitColor;
            float _HitBlend;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                // 알파가 충분히 높을 때만 효과 적용 (경계 픽셀 보호)
                if (c.a < 0.5)
                    return c;
                // 파동 효과 (x축 변형 대신 색상만 살짝 변화)
                c.rgb = lerp(c.rgb, _HitColor.rgb, _HitBlend);
                return c;
            }
            ENDCG
        }
    }
}