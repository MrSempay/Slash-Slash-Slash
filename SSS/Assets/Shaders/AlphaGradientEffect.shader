Shader "TextMeshPro/Custom/AlphaGradientEffect"
{
    Properties
    {
        // Основные свойства TextMesh Pro
        _FaceTex ("Face Texture", 2D) = "white" {}
        _FaceColor ("Face Color", Color) = (1,1,1,1)
        _FaceDilate ("Face Dilate", Range(-1,1)) = 0

        [Space]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineTex ("Outline Texture", 2D) = "white" {}
        _OutlineWidth ("Outline Thickness", Range(0, 1)) = 0
        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0

        [Space]
        _WeightNormal ("Weight Normal", float) = 0
        _WeightBold ("Weight Bold", float) = 0.5

        // Наши новые свойства для градиента
        _GradientTex ("Gradient Texture", 2D) = "white" {}
        _GradientScale ("Gradient Scale", Float) = 1.0
        _GradientOffset ("Gradient Offset", Float) = 0.0

        // Стандартные свойства TMPro
        _ShaderFlags ("Flags", float) = 0
        _ScaleRatioA ("Scale RatioA", float) = 1
        _ScaleRatioB ("Scale RatioB", float) = 1
        _ScaleRatioC ("Scale RatioC", float) = 1

        _MainTex ("Font Atlas", 2D) = "white" {}
        _TextureWidth ("Texture Width", float) = 512
        _TextureHeight ("Texture Height", float) = 512
        _GradientScale ("Gradient Scale", float) = 5.0
        _ScaleX ("Scale X", float) = 1.0
        _ScaleY ("Scale Y", float) = 1.0
        _PerspectiveFilter ("Perspective Correction", Range(0, 1)) = 0.875
        _Sharpness ("Sharpness", Range(-1,1)) = 0

        [HideInInspector] _VertexOffsetX ("Vertex OffsetX", float) = 0
        [HideInInspector] _VertexOffsetY ("Vertex OffsetY", float) = 0
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True" 
            "PreviewType"="Plane" 
        }
        
        LOD 200

        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Свойства TextMesh Pro
            sampler2D _FaceTex;
            float4 _FaceTex_ST;
            fixed4 _FaceColor;
            float _FaceDilate;

            sampler2D _OutlineTex;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineSoftness;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Наши свойства градиента
            sampler2D _GradientTex;
            float _GradientScale;
            float _GradientOffset;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD2;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = v.texcoord0;
                o.texcoord1 = v.texcoord1;
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Базовый цвет текста из основного атласа шрифта
                fixed4 texColor = tex2D(_MainTex, i.texcoord0);
                fixed4 faceColor = _FaceColor * i.color;
                
                // Вычисление градиента по Y-координате в мировом пространстве
                float gradientValueY = saturate(i.worldPos.y * _GradientScale + _GradientOffset);
                fixed4 gradientSample = tex2D(_GradientTex, float2(0.5, gradientValueY));
                
                // Применение альфа-канала градиента к альфа-каналу текста
                float sourceAlpha = texColor.a * faceColor.a;
                float finalAlpha = sourceAlpha * gradientSample.a;
                
                // Финальный цвет
                fixed4 finalColor = faceColor;
                finalColor.rgb *= texColor.rgb;
                finalColor.a = finalAlpha;
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "TextMeshPro/Mobile/Distance Field"
    CustomEditor "TMPro.EditorUtilities.TMP_SDFShaderGUI"
}