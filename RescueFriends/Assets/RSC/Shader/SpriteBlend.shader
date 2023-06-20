// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SpriteBlend" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _SubTex ("Sub (RGB)", 2D) = "white" {}
        _Speed ("Speed", float) = 5.0 
    }

    SubShader {
        Tags { 
            "Queue" = "Transparent"
            "RenderType"="Transparent" 
        }
        LOD 200

        Pass {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha 

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _SubTex;
            float _Speed;

            struct Vertex {
                float4 vertex : POSITION;
                float2 uv_MainTex : TEXCOORD0;
            };

            struct Fragment {
                float4 vertex : POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_SubTex : TEXCOORD1;
            };

            Fragment vert (Vertex v) {
                Fragment o;

                float sinX = sin(-_Speed * _Time);
                float cosX = cos(-_Speed * _Time);
                float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv_MainTex = v.uv_MainTex;
                o.uv_SubTex = mul(v.uv_MainTex - float2(0.5, 0.5), rotationMatrix) * float2(0.7, 0.7) + float2(0.5, 0.5);

                return o;
            }

            float4 frag(Fragment IN) : COLOR {
                float4 main = tex2D(_MainTex, IN.uv_MainTex);
                float4 sub = tex2D(_SubTex, IN.uv_SubTex);
                float4 a = lerp(main, sub, main.a);
                float4 res = (main * (1 - sub.a)) + (sub * sub.a);
                res.a = main.a;
                return res;
            }

            ENDCG
        }
	} 
	FallBack "Diffuse"
}
