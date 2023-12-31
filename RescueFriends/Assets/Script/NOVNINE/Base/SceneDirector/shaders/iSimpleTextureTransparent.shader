Shader "iPhone/Texture + No Light + Transparent" {
   Properties {
      _Color ("Main Color", Color) = (1,1,1,1)
      _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
   }
   Category {
      Tags { "Queue"="Transparent" "RenderType"="Transparent"}
      Blend SrcAlpha OneMinusSrcAlpha
      
      SubShader {
         Pass {
            Lighting Off
            SetTexture [_MainTex]
            {
               constantColor [_Color]
               Combine texture * constant, texture * constant
            }
         }
      }
   }
} 