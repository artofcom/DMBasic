using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;

namespace NOVNINE
{

public static partial class NNTool
{
    public static Texture2D CreateDummyTexture (int width, int height, Color color)
    {
        Texture2D texture2D = new Texture2D (width, height);
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                //Color color = ((j & i) <= 0) ? Color.gray : Color.white;
                texture2D.SetPixel (j, i, color);
            }
        }
        texture2D.Apply ();
        return texture2D;
    }

    public static Vector2 GetScreenPixelSize()
    {
#if UNITY_EDITOR
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null,null);
        return (Vector2)Res;
#else
        return new Vector2(Screen.width, Screen.height);
#endif
    }

    //public static GUITexture MakeGUITexture(Texture2D tex, float screenScaleHeight)
    //{
        //Dbg.Assert(tex != null);
      /*  GUITexture guiTex = GameObjectExt.CreateTemporaryGameObjectWithComponent<GUITexture>();
        Vector2 screen = GetScreenPixelSize();
        float screenAspect = screen.y / screen.x;
        float aspect = (float)tex.width / (float)tex.height;

        Vector3 scale = new Vector3(screenScaleHeight*aspect*screenAspect, screenScaleHeight, 1);
        guiTex.transform.localScale = scale;
        guiTex.transform.position = scale/2;
        guiTex.texture = tex;
        return guiTex;*/
    //}

    public static void MergeTexture2D(this Texture2D _this, IEnumerable<Texture2D> textures, int xCount, int yCount)
    {
        Debugger.Assert(_this != null);
        Debugger.Assert(textures != null);

        int width = _this.width;
        int height = _this.height;

        Texture2D mergeTexture = _this;

        Color32 []mergePixels = new Color32[width * height];

        int xPosition = 0;
        int yPosition = 0;
        foreach (Texture2D texture in textures) {
            Rect rect = new Rect(0, 0,  width / xCount, height / yCount);
            rect.x = xPosition * rect.width;
            rect.y = (yCount - yPosition - 1) * rect.height;

            xPosition += 1;
            if (xPosition >= xCount) {
                yPosition += 1;
                xPosition = 0;
            }

            Color32[] texturePixels = texture.GetPixels32();

            int textureWidth = texture.width;
            int textureHeight = texture.height;

            for (int y=0; y<rect.height; y++) {
                for (int x=0; x<rect.width; x++) {

                    int textureX = (int)(x * textureWidth / rect.width);
                    int textureY = (int)(y * textureHeight / rect.height);

                    mergePixels[(int)((x + rect.x) + (y + rect.y)* width)] = texturePixels[textureX + textureY * textureWidth];
                }
            }
        }

        mergeTexture.SetPixels32(mergePixels);
        mergeTexture.Apply();
    }

    public static void ConvertToNonTransparent(this Texture2D _this)
    {
        Debugger.Assert(_this != null);

        Texture2D texture = _this;
        Color32 []pixels = texture.GetPixels32();

        for (int i=0; i<pixels.Length; i++)
            pixels[i].a = 255;

        texture.SetPixels32(pixels);
        texture.Apply();
    }

}

}

