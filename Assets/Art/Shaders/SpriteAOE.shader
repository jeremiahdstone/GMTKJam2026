Shader "Custom/AOECircle"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.15, 0.05, 1)

        _Radius ("Circle Radius", Range(0.1, 0.5)) = 0.45

        [Header(Edge)]
        _BorderSize ("Solid Border (Pixels)", Float) = 1.0

        [Header(Inner Fade)]
        [Toggle] _EnableInnerFade ("Enable Inner Fade", Float) = 0
        _InnerPercent ("Inner Fade Percent", Range(0, 100)) = 20.0

        [Header(Outer Fade)]
        [Toggle] _EnableOuterFade ("Enable Outer Fade", Float) = 0
        _OuterPercent ("Outer Fade Percent", Range(0, 100)) = 20.0

        [Header(Pixelation)]
        _PixelSize ("Pixel Size", Range(1, 16)) = 1
        _FadeSteps ("Fade Steps", Range(1, 16)) = 4

        [Header(UV Wobble)]
        [Toggle] _EnableWobble ("Enable Wobble", Float) = 1
        _WobbleAmount ("Wobble Amount", Range(0, 0.1)) = 0.01
        _WobbleSpeed ("Wobble Speed", Range(0, 5)) = 0.5
        _WobbleFrequency ("Wobble Frequency", Range(1, 20)) = 6

        [Header(General)]
        _Alpha ("Alpha", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Cull Off
        ZWrite Off
        Lighting Off

        Blend SrcAlpha OneMinusSrcAlpha

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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;

            float _Radius;
            float _BorderSize;

            float _EnableInnerFade;
            float _InnerPercent;

            float _EnableOuterFade;
            float _OuterPercent;

            float _PixelSize;
            float _FadeSteps;

            float _EnableWobble;
            float _WobbleAmount;
            float _WobbleSpeed;
            float _WobbleFrequency;

            float _Alpha;


            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }


            // =========================================================
            // PIXELATED ALPHA
            // =========================================================

            float PixelateAlpha(float alpha)
            {
                alpha = saturate(alpha);

                if (_FadeSteps <= 1.0)
                {
                    return step(0.5, alpha);
                }

                return floor(alpha * _FadeSteps)
                    / _FadeSteps;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                // =====================================================
                // PIXEL GRID
                // =====================================================

                float2 uv = i.uv;

                if (_PixelSize > 1.0)
                {
                    float2 screenPixels =
                        _ScreenParams.xy;

                    float2 pixelGrid =
                        screenPixels / _PixelSize;

                    uv =
                        floor(uv * pixelGrid)
                        / pixelGrid;
                }

                // =====================================================
                // UV WOBBLE
                // =====================================================

                if (_EnableWobble > 0.5)
                {
                    float time = _Time.y * _WobbleSpeed;
                
                    float2 wobble;
                
                    wobble.x =
                        sin(
                            uv.y * _WobbleFrequency +
                            time
                        );
                        
                    wobble.y =
                        sin(
                            uv.x * (_WobbleFrequency * 0.83) -
                            time * 0.73
                        );
                        
                    uv += wobble * _WobbleAmount;
                }


                // =====================================================
                // CIRCLE DISTANCE
                // =====================================================

                float2 centeredUV = uv - 0.5;

                if (_EnableWobble > 0.5)
                {
                    float time = _Time.y * _WobbleSpeed;
                
                    centeredUV.x +=
                        sin(centeredUV.y * _WobbleFrequency + time)
                        * _WobbleAmount;
                
                    centeredUV.y +=
                        sin(centeredUV.x * _WobbleFrequency - time * 0.73)
                        * _WobbleAmount;
                }
                
                float distanceFromCenter =
                    length(centeredUV);


                // =====================================================
                // PIXEL SIZE
                // =====================================================

                float pixelSizeX =
                    fwidth(uv.x);

                float pixelSizeY =
                    fwidth(uv.y);

                float pixelSize =
                    max(
                        pixelSizeX,
                        pixelSizeY
                    );


                float borderSize =
                    _BorderSize * pixelSize;


                // =====================================================
                // OUTSIDE CIRCLE
                // =====================================================

                // The radius is ALWAYS the outermost point.
                //
                // Nothing is ever rendered outside it.

                if (distanceFromCenter > _Radius)
                {
                    return fixed4(0, 0, 0, 0);
                }


                // =====================================================
                // OUTER FADE
                // =====================================================

                if (_EnableOuterFade > 0.5)
                {
                    // Width of the fade measured inward
                    // from the circle's edge.

                    float outerFadeWidth =
                        _Radius *
                        (_OuterPercent / 100.0);


                    if (outerFadeWidth > 0.00001)
                    {
                        // Where the fade begins.

                        float outerFadeStart =
                            _Radius -
                            outerFadeWidth;


                        // -------------------------------------------------
                        // OUTER FADE REGION
                        // -------------------------------------------------

                        if (
                            distanceFromCenter >=
                            outerFadeStart
                        )
                        {
                            // 0 = transparent
                            // 1 = fully opaque at edge

                            float progress =
                                (
                                    distanceFromCenter -
                                    outerFadeStart
                                )
                                / outerFadeWidth;


                            float alpha =
                                PixelateAlpha(progress);


                            // -------------------------------------------------
                            // SHARP BORDER
                            //
                            // The border is centered at the ACTUAL
                            // radius and therefore never moves.
                            // -------------------------------------------------

                            float distanceToEdge =
                                _Radius -
                                distanceFromCenter;


                            if (
                                distanceToEdge <=
                                borderSize
                            )
                            {
                                alpha = 1.0;
                            }


                            alpha *=
                                _Color.a *
                                _Alpha;


                            return fixed4(
                                _Color.rgb,
                                alpha
                            );
                        }
                    }
                }


                // =====================================================
                // INNER FADE
                // =====================================================

                if (_EnableInnerFade > 0.5)
                {
                    float innerFadeWidth =
                        _Radius *
                        (_InnerPercent / 100.0);


                    if (
                        innerFadeWidth > 0.00001 &&
                        distanceFromCenter <
                        innerFadeWidth
                    )
                    {
                        // Center = opaque
                        // Edge of inner region = transparent

                        float progress =
                            distanceFromCenter /
                            innerFadeWidth;


                        float alpha =
                            1.0 -
                            PixelateAlpha(progress);


                        alpha *=
                            _Color.a *
                            _Alpha;


                        return fixed4(
                            _Color.rgb,
                            alpha
                        );
                    }
                }


                // =====================================================
                // TRANSPARENT MIDDLE
                // =====================================================

                return fixed4(0, 0, 0, 0);
            }

            ENDCG
        }
    }
}