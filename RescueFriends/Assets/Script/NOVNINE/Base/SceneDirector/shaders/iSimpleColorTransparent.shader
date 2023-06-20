Shader "iPhone/Simple Color + No Light + Transparent" {
	
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
}

SubShader {
    Tags {"RenderType"="Transparent" "Queue"="Transparent"}

    Pass {
        ColorMask 0
    }

    Pass {
        ZWrite Off
        //Blend DstColor SrcColor
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Color [_Color]
    }
}
}