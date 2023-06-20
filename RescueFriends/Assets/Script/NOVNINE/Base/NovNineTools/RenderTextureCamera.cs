using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class RenderTextureCamera : MonoBehaviour
{
    public bool useFullScreen = false;
    public int width = 256;
    public int height = 256;
    public bool useMipMap = false;

    void Awake ()
    {
        if (useFullScreen == true) {
            Resolution res = Screen.currentResolution;
            width = res.width;
            height = res.height;
        }
        GetComponent<Camera>().enabled = false;
    }

    public Texture2D BuildTexture2D()
    {
        var rt = RenderTexture.GetTemporary(width, height, 16);
        GetComponent<Camera>().targetTexture = rt;
        RenderTexture old = RenderTexture.active;
        RenderTexture.active = rt;

        GetComponent<Camera>().Render();

        Texture2D texture2d = new Texture2D(width, height, TextureFormat.ARGB32, useMipMap);
        texture2d.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2d.Apply();
        RenderTexture.active = old;

        RenderTexture.ReleaseTemporary(rt);
        return texture2d;
    }

    public void BuildTexture2D(ref Texture2D texture2d)
    {
        var rt = RenderTexture.GetTemporary(width, height, 16);
        GetComponent<Camera>().targetTexture = rt;
        RenderTexture old = RenderTexture.active;
        RenderTexture.active = rt;

        GetComponent<Camera>().Render();

        texture2d.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2d.Apply();
        RenderTexture.active = old;

        RenderTexture.ReleaseTemporary(rt);
    }

    public void Clear()
    {
        var rt = RenderTexture.GetTemporary(width, height, 16);
        GetComponent<Camera>().targetTexture = rt;
        RenderTexture old = RenderTexture.active;
        RenderTexture.active = rt;

        int mask = GetComponent<Camera>().cullingMask;
        GetComponent<Camera>().cullingMask = 0;
        GetComponent<Camera>().Render();
        Texture2D texture2d = new Texture2D(width, height, TextureFormat.ARGB32, useMipMap);
        texture2d.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2d.Apply();
        GetComponent<Camera>().cullingMask = mask;

        RenderTexture.active = old;
        RenderTexture.ReleaseTemporary(rt);
    }
}

