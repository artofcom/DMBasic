// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// unlit, vertex colour, alpha blended
// cull off

Shader "NOVNINE/DashLine" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,0)
	}
	
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off Lighting Off Cull Off Fog { Mode Off } Blend SrcAlpha OneMinusSrcAlpha
		LOD 110
		
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert_vct
			#pragma fragment frag_mult 
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float4 _Color;

			struct vin_vct 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f_vct
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			v2f_vct vert_vct(vin_vct v)
			{
				v2f_vct o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag_mult(v2f_vct i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
				return col;
			}
			
			ENDCG
		} 
	}
}
