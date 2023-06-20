// unlit, vertex colour, alpha blended
// cull off

Shader "tk2d/BlendVertexMatColor" 
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
		
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off


		BindChannels 
		{
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
			Bind "Color", color
		}

		Pass 
		{
			Lighting Off
			SetTexture [_MainTex] { 
                constantColor[_Color]
                combine texture * constant } 
		}
	}
}
