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

            float _Alpha;


            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                // =====================================================
                // CIRCLE
                // =====================================================

                float2 centeredUV = i.uv - 0.5;

                float distanceFromCenter =
                    length(centeredUV);

                float edgeDistance =
                    _Radius - distanceFromCenter;


                // =====================================================
                // NOTHING OUTSIDE THE CIRCLE
                // =====================================================

                if (edgeDistance < 0.0)
                {
                    return fixed4(0, 0, 0, 0);
                }


                // =====================================================
                // PIXEL SIZE
                // =====================================================

                float pixelSizeX = fwidth(i.uv.x);
                float pixelSizeY = fwidth(i.uv.y);

                float pixelSize =
                    max(pixelSizeX, pixelSizeY);


                float borderSize =
                    _BorderSize * pixelSize;


                // =====================================================
                // OUTER FADE + BORDER
                // =====================================================

                if (_EnableOuterFade > 0.5)
                {
                    float outerDistance =
                        _Radius *
                        (_OuterPercent / 100.0);


                    // Make sure the outer fade has a valid size.

                    if (outerDistance > 0.00001)
                    {
                        float fadeStart =
                            _Radius - outerDistance;


                        // -------------------------------------------------
                        // SOLID BORDER
                        // -------------------------------------------------

                        if (edgeDistance <= borderSize)
                        {
                            return fixed4(
                                _Color.rgb,
                                _Color.a * _Alpha
                            );
                        }


                        // -------------------------------------------------
                        // OUTER FADE
                        // -------------------------------------------------

                        if (distanceFromCenter >= fadeStart)
                        {
                            float progress =
                                (distanceFromCenter - fadeStart)
                                / outerDistance;


                            float alpha =
                                smoothstep(
                                    0.0,
                                    1.0,
                                    progress
                                );


                            alpha *= _Color.a;
                            alpha *= _Alpha;


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
                    float innerDistance =
                        _Radius *
                        (_InnerPercent / 100.0);


                    if (innerDistance > 0.00001 &&
                        distanceFromCenter < innerDistance)
                    {
                        float progress =
                            distanceFromCenter /
                            innerDistance;


                        float alpha =
                            1.0 -
                            smoothstep(
                                0.0,
                                1.0,
                                progress
                            );


                        alpha *= _Color.a;
                        alpha *= _Alpha;


                        return fixed4(
                            _Color.rgb,
                            alpha
                        );
                    }
                }


                // =====================================================
                // TRANSPARENT
                // =====================================================

                return fixed4(0, 0, 0, 0);
            }

            ENDCG
        }
    }
}