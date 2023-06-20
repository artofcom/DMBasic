Shader "iPhone/Multiply" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue" = "Transparent" }
	Blend Zero SrcColor
	Lighting Off ZWrite Off Fog { Color (1,1,1,1) }
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	SubShader {
		Pass {
			SetTexture [_MainTex] {
				constantColor [_Color]
				combine texture lerp(texture) constant
			}
		}
	}
}
}
