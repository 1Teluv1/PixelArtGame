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
        _EffectEnabled ("Effect Enabled", Float) = 1
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
            float _EffectEnabled;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_EffectEnabled < 0.5)
                {
                    fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                    return c;
                }
                float wave = sin(i.uv.x * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveStrength;
                float2 uv = i.uv;
                uv.y += wave;
                fixed4 c = tex2D(_MainTex, uv) * _Color;
                if (c.a < 0.5)
                    return c;
                c.rgb = lerp(c.rgb, _HitColor.rgb, _HitBlend);
                return c;
            }
            ENDCG
        }
    }
}