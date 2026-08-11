Shader "Custom/SpriteTwoColors"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Sprite Tint", Color) = (1,1,1,1)

        _ColorDark ("Dark Color", Color) = (0.45, 0.75, 0.95, 1)
        _ColorLight ("Light Color", Color) = (0.85, 0.95, 1.0, 1)

        _Threshold ("Color Threshold", Range(0,1)) = 0.5
        _Contrast ("Brightness Contrast", Range(0.1,5)) = 1.0
        _Brightness ("Brightness Offset", Range(-1,1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;

            float4 _ColorDark;
            float4 _ColorLight;

            float _Threshold;
            float _Contrast;
            float _Brightness;

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample the sprite.
                half4 sprite = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    input.uv
                );

                // Preserve transparency.
                if (sprite.a <= 0.001)
                    discard;

                // Calculate perceived brightness.
                // This weights green more heavily because human vision
                // perceives green as brighter than red/blue.
                half brightness =
                    dot(sprite.rgb, half3(0.2126, 0.7152, 0.0722));

                // Apply contrast around the midpoint.
                brightness = (brightness - 0.5) * _Contrast + 0.5;

                // Apply brightness adjustment.
                brightness += _Brightness;

                brightness = saturate(brightness);

                // Choose one of the two palette colors.
                //
                // Darker source colors -> Dark Color
                // Lighter source colors -> Light Color
                half3 finalColor = brightness < _Threshold
                    ? _ColorDark.rgb
                    : _ColorLight.rgb;

                // Preserve sprite alpha and vertex tint alpha.
                return half4(
                    finalColor,
                    sprite.a * input.color.a
                );
            }

            ENDHLSL
        }
    }
}