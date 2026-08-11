Shader "Custom/SpriteColorSwap"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Sprite Tint", Color) = (1,1,1,1)

        _Color1 ("Darkest Color", Color) = (0.25, 0.55, 0.80, 1)
        _Color2 ("Dark Color", Color) = (0.40, 0.70, 0.90, 1)
        _Color3 ("Light Color", Color) = (0.65, 0.85, 0.95, 1)
        _Color4 ("Lightest Color", Color) = (0.85, 0.95, 1.00, 1)

        _Threshold1 ("Threshold 1", Range(0,1)) = 0.25
        _Threshold2 ("Threshold 2", Range(0,1)) = 0.50
        _Threshold3 ("Threshold 3", Range(0,1)) = 0.75

        _Contrast ("Brightness Contrast", Range(0.1,5)) = 1.0
        _Brightness ("Brightness Offset", Range(-1,1)) = 0.0

        [Header(Frozen Corner)]

        _FrozenColor ("Frozen Color", Color) = (1,1,1,1)
        _FrozenStrength ("Frozen Strength", Range(0,1)) = 1.0
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
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;

            float4 _Color;

            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _Color4;

            float _Threshold1;
            float _Threshold2;
            float _Threshold3;

            float _Contrast;
            float _Brightness;

            float4 _FrozenColor;
            float _FrozenStrength;


            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;
                output.color = input.color * _Color;

                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                // ---------------------------------------------------------
                // SAMPLE SPRITE
                // ---------------------------------------------------------

                half4 sprite = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    input.uv
                );

                if (sprite.a <= 0.001)
                    discard;


                // ---------------------------------------------------------
                // FOUR COLOR PALETTE
                // ---------------------------------------------------------

                half brightness = dot(
                    sprite.rgb,
                    half3(
                        0.2126,
                        0.7152,
                        0.0722
                    )
                );

                brightness =
                    (brightness - 0.5)
                    * _Contrast
                    + 0.5;

                brightness += _Brightness;

                brightness = saturate(brightness);


                half3 finalColor;

                if (brightness < _Threshold1)
                {
                    finalColor = _Color1.rgb;
                }
                else if (brightness < _Threshold2)
                {
                    finalColor = _Color2.rgb;
                }
                else if (brightness < _Threshold3)
                {
                    finalColor = _Color3.rgb;
                }
                else
                {
                    finalColor = _Color4.rgb;
                }


                // ---------------------------------------------------------
                // FROZEN TOP-RIGHT CORNER
                // ---------------------------------------------------------

                float2 texel = _MainTex_TexelSize.xy;


                // ---------------------------------------------------------
                // CHECK ABOVE - 2 PIXELS
                // ---------------------------------------------------------

                half alphaAbove1 = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    input.uv + float2(0, texel.y)
                ).a;

                half alphaAbove2 = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    input.uv + float2(0, texel.y * 2.0)
                ).a;


                // ---------------------------------------------------------
                // CHECK RIGHT - 2 PIXELS
                // ---------------------------------------------------------

                half alphaRight1 = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    input.uv + float2(texel.x, 0)
                ).a;

                half alphaRight2 = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    input.uv + float2(texel.x * 2.0, 0)
                ).a;


                // ---------------------------------------------------------
                // DETERMINE TOP EXPOSURE
                // ---------------------------------------------------------

                float exposedTop =
                    step(alphaAbove1, 0.001) +
                    step(alphaAbove2, 0.001);

                exposedTop = saturate(exposedTop);


                // ---------------------------------------------------------
                // DETERMINE RIGHT EXPOSURE
                // ---------------------------------------------------------

                float exposedRight =
                    step(alphaRight1, 0.001) +
                    step(alphaRight2, 0.001);

                exposedRight = saturate(exposedRight);


                // ---------------------------------------------------------
                // BOTH CONDITIONS MUST BE TRUE
                // ---------------------------------------------------------

                float frozenMask =
                    exposedTop * exposedRight;


                // ---------------------------------------------------------
                // APPLY FROZEN COLOR
                // ---------------------------------------------------------

                finalColor = lerp(
                    finalColor,
                    _FrozenColor.rgb,
                    frozenMask * _FrozenStrength
                );


                return half4(
                    finalColor,
                    sprite.a * input.color.a
                );
            }

            ENDHLSL
        }
    }
}