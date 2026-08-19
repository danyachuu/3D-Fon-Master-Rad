Shader "Custom/WallHackHighlight"
{
    Properties
    {
        _ColorA ("Color A (Red)", Color) = (1, 0, 0, 1)
        _ColorB ("Color B (Green)", Color) = (0, 1, 0, 1)
        _BlinkSpeed ("Blink Speed", Range(0.5, 10)) = 3.0
    }
    SubShader
    {
        Tags { "Queue"="Overlay+100" "RenderType"="Opaque" "IgnoreProjector"="True" }
        LOD 100

        ZTest Always   // Render over walls
        ZWrite Off    // Ignore depth buffer

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _ColorA;
            fixed4 _ColorB;
            float _BlinkSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                // Smooth sine wave normalized between 0.0 and 1.0
                float wave = (sin(_Time.y * _BlinkSpeed) + 1.0) * 0.5;

                // Blend color smoothly between Red (_ColorA) and Green (_ColorB)
                fixed4 col = lerp(_ColorA, _ColorB, wave);
                return col;
            }
            ENDCG
        }
    }
}