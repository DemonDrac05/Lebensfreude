Shader "Unlit/GradientReveal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GreyColor ("Grey Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _WhiteColor ("White Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _RevealAmount ("Reveal Amount", Range(0, 1)) = 0.0
        _Direction ("Reveal Direction", Vector) = (0, 0, 0, 0)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _GreyColor;
            float4 _WhiteColor;
            float _RevealAmount;
            float4 _Direction;
            float _Smoothness;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize the direction vector
                float2 dir = normalize(_Direction.xy);
                
                // Calculate the gradient based on the UV coordinates and direction
                float gradient = dot(i.uv - 0.5, dir) + 0.5;

                // Apply reveal amount and smoothness for the transition
                float reveal = smoothstep(_RevealAmount - _Smoothness, _RevealAmount, gradient);

                // Blend between grey and white colors
                fixed4 color = lerp(_WhiteColor, _GreyColor, reveal);

                // Sample the texture
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Combine the texture color with the blended color
                fixed4 finalColor = texColor * color;

                return finalColor;
            }
            ENDCG
        }
    }
}
