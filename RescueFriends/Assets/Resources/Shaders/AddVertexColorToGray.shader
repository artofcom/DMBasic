// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// unlit, vertex colour, alpha blended
// cull off

Shader "tk2d/AddVertexColorToGray" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	
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

			struct vin_vct 
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f_vct
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			v2f_vct vert_vct(vin_vct v)
			{
				v2f_vct o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 frag_mult(v2f_vct i) : COLOR
			{
//				fixed4 col = tex2D(_MainTex, i.texcoord);// * i.color;
				fixed4 col = tex2D(_MainTex, i.texcoord);


				float M = max(max(i.color.r, i.color.g), i.color.b);
				float m = min(min(i.color.r, i.color.g), i.color.b);

				float B = (M + m) * 0.5f;
				float L = 0.2126f * i.color.r + 0.7152f * i.color.g + 0.0722f * i.color.b;
				float S = 0.0f;

				if(L > 0.5f)
				{
					S = (M - m )/(2.0f - M - m);
				}
				else
				{
					S = (M - m) / (M + m);
				}

				col.rgb = lerp(col.rgb, dot( col.rgb, i.color.rgb), S) * B;

				if( S > 0.5f)
				{
					col.rgb += i.color.rgb * S;
					col.rgb -= i.color.rgb * B * 1.5f;
				}
//				else
//				{
//					col.rgb /= i.color.rgb;
//					col.rgb = lerp(col.rgb, dot( col.rgb, i.color.rgb), S) * B;
//				}

				col.a *= i.color.a;

				return col;
			}
			
			ENDCG
		} 
	}
 
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off Fog { Mode Off }
		LOD 100

		BindChannels 
		{
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
			Bind "Color", color
		}

		Pass 
		{
			Lighting Off
			SetTexture [_MainTex] { combine texture * primary } 
		}
	}
}
