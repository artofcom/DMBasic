// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NOVNINE/DistFont/Outline" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,0)
		_AlphaMin ("Alpha Min", Float) = 0.49
		_AlphaMax ("Alpha Max", Float) = 0.54
        _OutColor ("Outline Color", Color) = (0.3,0.3,0.3,0)
		_OutAlphaMin ("Outline Alpha Min", Float) = 0.28
		_OutAlphaMax ("Outline Alpha Max", Float) = 0.54
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off Lighting Off Cull Off Fog { Mode Off } Blend SrcAlpha OneMinusSrcAlpha
        LOD 110

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _Color;
			float _AlphaMin;
			float _AlphaMax;
			float4 _OutColor;
			float _OutAlphaMin;
			float _OutAlphaMax;
			sampler2D _MainTex;

			//Unity-required vars
			float4 _MainTex_ST;
			struct vin_vct 
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv: TEXCOORD0;
				float4 color : COLOR;
			};

			v2f vert (vin_vct v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float4 base = tex2D(_MainTex, i.uv);
				_Color.a *= smoothstep(_AlphaMin, _AlphaMax, base.w);
				_OutColor.a *= smoothstep(_OutAlphaMin, _OutAlphaMax, base.w);
                return lerp(_OutColor, _Color, _Color.a) * i.color;
			}

			ENDCG
		}
	} 
}
