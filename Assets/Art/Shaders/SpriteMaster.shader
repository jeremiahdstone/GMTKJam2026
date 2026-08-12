Shader "Custom/SpriteMaster"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Sprite Tint", Color) = (1,1,1,1)

        // ---------------------------------------------------------
        // ENABLE / DISABLE
        // ---------------------------------------------------------

        [Header(Effects)]

        [Toggle] _EnableColorSwap ("Enable Color Swap", Float) = 1
        [Toggle] _EnableOutline ("Enable Outline", Float) = 1


        // ---------------------------------------------------------
        // COLOR SWAP
        // ---------------------------------------------------------

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

        _FrozenStrength ("Frozen Strength", Range(0,1)) = 1.0
        _FrozenPixelSize ("Frozen Pixel Size", Range(1,4)) = 2


        // ---------------------------------------------------------
        // OUTLINE
        // ---------------------------------------------------------

        [Header(Outline)]

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size (Pixels)", Float) = 1
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

        Cull Off
        Lighting Off
        ZWrite Off

        Blend SrcAlpha OneMinusSrcAlpha

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

            // Enable / Disable
            float _EnableColorSwap;
            float _EnableOutline;


            // ---------------------------------------------------------
            // COLOR SWAP VARIABLES
            // ---------------------------------------------------------

            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _Color4;

            float _Threshold1;
            float _Threshold2;
            float _Threshold3;

            float _Contrast;
            float _Brightness;

            float _FrozenStrength;
            float _FrozenPixelSize;


            // ---------------------------------------------------------
            // OUTLINE VARIABLES
            // ---------------------------------------------------------

            float4 _OutlineColor;
            float _OutlineSize;


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


                // ---------------------------------------------------------
                // SPRITE COLOR
                // ---------------------------------------------------------

                half3 finalColor = sprite.rgb;


                // ---------------------------------------------------------
                // COLOR SWAP
                // ---------------------------------------------------------

                if (_EnableColorSwap > 0.5)
                {
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


                    half3 paletteColor;

                    int paletteIndex;


                    if (brightness < _Threshold1)
                    {
                        paletteColor = _Color1.rgb;
                        paletteIndex = 0;
                    }
                    else if (brightness < _Threshold2)
                    {
                        paletteColor = _Color2.rgb;
                        paletteIndex = 1;
                    }
                    else if (brightness < _Threshold3)
                    {
                        paletteColor = _Color3.rgb;
                        paletteIndex = 2;
                    }
                    else
                    {
                        paletteColor = _Color4.rgb;
                        paletteIndex = 3;
                    }


                    finalColor = paletteColor;


                    // -----------------------------------------------------
                    // FROZEN TOP-RIGHT CORNER
                    // -----------------------------------------------------

                    float2 texel = _MainTex_TexelSize.xy;

                    int pixelSize = (int)round(_FrozenPixelSize);


                    // -----------------------------------------------------
                    // CHECK TOP EXPOSURE
                    // -----------------------------------------------------

                    float exposedTop = 0.0;

                    for (int i = 1; i <= 4; i++)
                    {
                        if (i > pixelSize)
                            break;

                        half alphaAbove = SAMPLE_TEXTURE2D(
                            _MainTex,
                            sampler_MainTex,
                            input.uv + float2(
                                0,
                                texel.y * i
                            )
                        ).a;

                        if (alphaAbove <= 0.001)
                        {
                            exposedTop = 1.0;
                            break;
                        }
                    }


                    // -----------------------------------------------------
                    // CHECK RIGHT EXPOSURE
                    // -----------------------------------------------------

                    float exposedRight = 0.0;

                    for (int i = 1; i <= 4; i++)
                    {
                        if (i > pixelSize)
                            break;

                        half alphaRight = SAMPLE_TEXTURE2D(
                            _MainTex,
                            sampler_MainTex,
                            input.uv + float2(
                                texel.x * i,
                                0
                            )
                        ).a;

                        if (alphaRight <= 0.001)
                        {
                            exposedRight = 1.0;
                            break;
                        }
                    }


                    // -----------------------------------------------------
                    // BOTH CONDITIONS MUST BE TRUE
                    // -----------------------------------------------------

                    float frozenMask =
                        exposedTop * exposedRight;


                    // -----------------------------------------------------
                    // SHIFT ONE COLOR BRIGHTER
                    // -----------------------------------------------------

                    half3 brighterColor = finalColor;

                    if (paletteIndex == 0)
                    {
                        brighterColor = _Color2.rgb;
                    }
                    else if (paletteIndex == 1)
                    {
                        brighterColor = _Color3.rgb;
                    }
                    else if (paletteIndex == 2)
                    {
                        brighterColor = _Color4.rgb;
                    }
                    else
                    {
                        brighterColor = _Color4.rgb;
                    }


                    // -----------------------------------------------------
                    // APPLY FROZEN HIGHLIGHT
                    // -----------------------------------------------------

                    finalColor = lerp(
                        finalColor,
                        brighterColor,
                        frozenMask * _FrozenStrength
                    );
                }


                // ---------------------------------------------------------
                // OUTLINE
                // ---------------------------------------------------------

                float outline = 0.0;

                if (_EnableOutline > 0.5)
                {
                    float2 pixel = _MainTex_TexelSize.xy;
                    float2 offset = pixel * round(_OutlineSize);


                    float aLeft = SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(-offset.x, 0)
                    ).a;


                    float aRight = SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(offset.x, 0)
                    ).a;


                    float aDown = SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(0, -offset.y)
                    ).a;


                    outline = max(
                        max(aLeft, aRight),
                        aDown
                    );


                    // Only show the outline where the current pixel
                    // itself is transparent.
                    outline = saturate(
                        outline - sprite.a
                    );
                }


                // ---------------------------------------------------------
                // SPRITE + OUTLINE
                // ---------------------------------------------------------

                // This is the original sprite's alpha, including
                // SpriteRenderer tint.
                float spriteAlpha =
                    sprite.a * input.color.a;


                float outlineAlpha =
                    outline * _OutlineColor.a;


                // The outline is composited AFTER the color swap.
                // Therefore _OutlineColor is never passed through
                // the palette swap.
                half3 resultColor = finalColor;


                // Match the behavior of the original outline shader:
                //
                // sprite contribution
                // +
                // outline contribution
                //
                // Then convert back to normal alpha blending.

                float resultAlpha =
                    saturate(
                        spriteAlpha + outlineAlpha
                    );


                if (resultAlpha > 0.001)
                {
                    resultColor =
                        (
                            finalColor * spriteAlpha
                            +
                            _OutlineColor.rgb * outlineAlpha
                        )
                        / resultAlpha;
                }


                return half4(
                    resultColor,
                    resultAlpha
                );
            }

            ENDHLSL
        }
    }
}