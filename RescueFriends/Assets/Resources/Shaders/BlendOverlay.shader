// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/**
* @file BlendOverlay.shader
* @brief Simpler Overlay for Photoshop
* @author amugana (amugana@gmail.com)
* @version 1.0
* @date 2012-07-11
*/
Shader "tk2d/BlendOverlay" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
	}

	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		
		ZWrite Off Cull Off Lighting Off
		Blend DstColor SrcAlpha
        BlendOp Add

		Pass 
		{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            
            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            float4 _MainTex_ST;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                fixed4 col;
                fixed4 tex = tex2D(_MainTex, i.texcoord);
                col = i.color * tex;
                return col;
            }
            ENDCG 
		}
	}

	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		
		ZWrite Off
		Blend DstColor SrcAlpha
        BlendOp Add
		Cull Off
        Lighting Off
	    //Color [_Color]

		BindChannels 
		{
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
			Bind "Color", color
		}

		Pass 
		{
			SetTexture [_MainTex] { combine texture * primary } 
		}
	}
}
