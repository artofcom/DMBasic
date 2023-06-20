// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NOVNINE/ScaleDown" {
	Properties {
		_Scale ("Scale", Float) = 0.8
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		ZWrite Off Lighting Off Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass 
		{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Scale;
            
            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                fixed4 col = _Color;
                col.a = _Scale;
                return col;
            }
            ENDCG         
		}
	}
}
