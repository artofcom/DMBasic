// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NOVNINE/ScaleUp" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		ZWrite Off Lighting Off Cull Back
		Blend DstColor One

		Pass 
		{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            fixed4 _Color;
            
            struct appdata_t {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                return _Color;
            }
            ENDCG         
		}
	}
}
