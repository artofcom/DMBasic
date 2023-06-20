using UnityEngine;
using DG.Tweening;
using NOVNINE;

public class GUISceneFader : SingletonMonoBehaviour<GUISceneFader>
{
    public Color fadeColor = new Color(0,0,0,0.3f);
    GUIStyle backgroundStyle;

    void Start()
    {
        backgroundStyle = new GUIStyle();
        backgroundStyle.normal.background = NNTool.CreateDummyTexture(32,32,Color.black);
    }

    public static void ValidateInstance()
    {
        if(GUISceneFader.Instance == null)
            GameObjectExt.CreateTemporaryGameObjectWithComponent<GUISceneFader>();
    }

    public static void FadeIn(float sec)
    {
        ValidateInstance();
		DOTween.To(()=> GUISceneFader.Instance.fadeColor, x=> GUISceneFader.Instance.fadeColor = x, new Color(0,0,0,0), sec);
    }

    public static void FadeOut(float sec, float alpha)
    {
        ValidateInstance();
		DOTween.To(()=> GUISceneFader.Instance.fadeColor, x=> GUISceneFader.Instance.fadeColor = x, new Color(0,0,0,alpha), sec);
    }

    void OnGUI()
    {
        if (fadeColor.a > 0) {
            GUI.color = fadeColor;
            GUI.depth = -1000;
            GUI.Label(new Rect(-10, -10, Screen.width + 10, Screen.height + 10), (Texture2D)null, backgroundStyle);
        }
    }
}

