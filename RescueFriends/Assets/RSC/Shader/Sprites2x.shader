﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Sprites2x" {
    Properties {   
        _MainTex ("Sprite Texture", 2D) = "white" {}
    }   

    SubShader {   
        Tags {  
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }   

        Cull Off 
        Lighting Off 
        ZWrite Off 
        Fog { Mode Off }
        Blend One OneMinusSrcAlpha

        Pass {   
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile DUMMY PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t {   
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };  

            struct v2f {   
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
            };  

            v2f vert(appdata_t IN) {   
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
#ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
#endif

                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                c.rgb = c.rgb * 2 * c.a;
                return c;
            }

            ENDCG
        }
    }
}
