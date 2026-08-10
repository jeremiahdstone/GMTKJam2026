Shader "Custom/AOECircle"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.15, 0.05, 1)

        _Radius ("Circle Radius", Range(0.1, 0.5)) = 0.45

        _BorderSize ("Solid Border (Pixels)", Float) = 1.0

        _FadeSize ("Fade Into Circle (Pixels)", Float) = 3.0

        _OuterFade ("Fade Outside Circle (Pixels)", Float) = 0.0

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
            float _FadeSize;
            float _OuterFade;
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
                // -----------------------------------------------------
                // Center the UV coordinates
                // -----------------------------------------------------

                float2 centeredUV = i.uv - 0.5;


                // -----------------------------------------------------
                // Calculate circular distance
                // -----------------------------------------------------

                float distanceFromCenter = length(centeredUV);


                // -----------------------------------------------------
                // Get a CONSTANT pixel size
                //
                // IMPORTANT:
                // Do NOT use fwidth(distanceFromCenter).
                //
                // fwidth on the radial distance changes depending on
                // direction and causes the diamond-shaped artifact.
                //
                // Instead, measure the UV change across one screen
                // pixel directly.
                // -----------------------------------------------------

                float pixelSizeX = fwidth(i.uv.x);
                float pixelSizeY = fwidth(i.uv.y);

                float pixelSize = max(pixelSizeX, pixelSizeY);


                // -----------------------------------------------------
                // Convert pixel settings into UV distance
                // -----------------------------------------------------

                float borderSize =
                    _BorderSize * pixelSize;

                float fadeSize =
                    _FadeSize * pixelSize;

                float outerFadeSize =
                    _OuterFade * pixelSize;


                // -----------------------------------------------------
                // Distance from circle edge
                //
                // Negative = inside
                // Positive = outside
                // -----------------------------------------------------

                float edgeDistance =
                    distanceFromCenter - _Radius;


                // =====================================================
                // OUTSIDE CIRCLE
                // =====================================================

                if (edgeDistance > 0)
                {
                    if (_OuterFade <= 0)
                    {
                        return fixed4(0, 0, 0, 0);
                    }

                    float outerAlpha =
                        1.0 -
                        smoothstep(
                            0.0,
                            outerFadeSize,
                            edgeDistance
                        );

                    outerAlpha *= _Color.a;
                    outerAlpha *= _Alpha;

                    return fixed4(
                        _Color.rgb,
                        outerAlpha
                    );
                }


                // =====================================================
                // INSIDE CIRCLE
                // =====================================================

                float insideDistance =
                    -edgeDistance;


                // -----------------------------------------------------
                // Solid border
                // -----------------------------------------------------

                if (insideDistance <= borderSize)
                {
                    return fixed4(
                        _Color.rgb,
                        _Color.a * _Alpha
                    );
                }


                // -----------------------------------------------------
                // Fade inward toward the center
                // -----------------------------------------------------

                if (_FadeSize > 0)
                {
                    float fadeProgress =
                        (insideDistance - borderSize)
                        / fadeSize;

                    float alpha =
                        1.0 -
                        smoothstep(
                            0.0,
                            1.0,
                            fadeProgress
                        );

                    alpha *= _Color.a;
                    alpha *= _Alpha;

                    return fixed4(
                        _Color.rgb,
                        alpha
                    );
                }


                // -----------------------------------------------------
                // Fully transparent center
                // -----------------------------------------------------

                return fixed4(0, 0, 0, 0);
            }

            ENDCG
        }
    }
}