Shader "NOVNINE/Mesh Depth Mask"
{
	SubShader
	{
		Tags {"Queue" = "Background"}
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
		ZWrite On
		ZTest Always
		Pass
		{
			Color(0,0,0,0)
		}
	}
}
