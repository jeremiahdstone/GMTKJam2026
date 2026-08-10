Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size (Pixels)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 sprite = tex2D(_MainTex, i.uv);
            
                float2 pixel = _MainTex_TexelSize.xy;
                float2 offset = pixel * round(_OutlineSize);
            
                float aLeft = tex2D(_MainTex, i.uv + float2(-offset.x, 0)).a;
                float aRight = tex2D(_MainTex, i.uv + float2(offset.x, 0)).a;
                float aDown = tex2D(_MainTex, i.uv + float2(0, -offset.y)).a;
            
                float outline = max(max(aLeft, aRight), aDown);
            
                outline = saturate(outline - sprite.a);
            
                fixed4 result = sprite * i.color;
            
                float outlineAlpha = outline * _OutlineColor.a;
            
                result.rgb *= result.a;
            
                result.rgb += _OutlineColor.rgb * outlineAlpha;
                result.a = saturate(result.a + outlineAlpha);
            
                return result;
            }
            ENDCG
        }
    }
}