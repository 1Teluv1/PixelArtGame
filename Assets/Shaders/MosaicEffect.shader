Shader "Custom/MosaicEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Range(1, 128)) = 16
    }
    SubShader
    {
        // 모든 플랫폼에서 동작
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
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
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PixelSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 모자이크 효과: 픽셀 사이즈에 따라 UV 좌표를 양자화
                float2 pixelatedUV = floor(i.uv * _PixelSize) / _PixelSize;
                
                // 양자화된 UV를 사용하여 색상 샘플링
                fixed4 col = tex2D(_MainTex, pixelatedUV);
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
} 