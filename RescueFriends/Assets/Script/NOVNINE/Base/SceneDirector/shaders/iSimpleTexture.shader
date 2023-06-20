Shader "iPhone/Texture + No Light" {
   Properties {
      _MainTex ("Base (RGB)", 2D) = "white"
   }
   SubShader {
      Pass {
         SetTexture [_MainTex]
      }
   }
}