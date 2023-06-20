using UnityEngine;
using System.Collections;
//using Holoville.HOTween;
using DG.Tweening;
using Spine;
using Spine.Unity;

// [AI_MISSION]
public class chatBox : MonoBehaviour 
{
    public tk2dTextMesh         _txtText = null;

    tk2dSprite _sprBox          = null;
	void Awake()
	{
        Debug.Assert(null!=GetComponent<tk2dSprite>());
        Debug.Assert(null!=_txtText);
        transform.localScale    = Vector3.one*0.001f;
        _sprBox                 = GetComponent<tk2dSprite>();
	}
	
	void Start() {}
	
    public void displayChatBox(string msg, float duration, bool useRedBG=false)
    {
        transform.localScale    = Vector3.one*0.001f;
        _txtText.text           = msg;

        //NOVNINE.GameObjectExt.FadeInRecursively(gameObject, 0.1f);
        _sprBox.spriteName      = useRedBG ? "Red" : "Green";
        transform.DOScale(1.0f, 0.4f).SetEase(Ease.OutBack);
        DOVirtual.DelayedCall(duration, _hideChatBox).SetId(gameObject.GetInstanceID());
    }

    void _hideChatBox()
    {
        //NOVNINE.GameObjectExt.FadeOutRecursively(gameObject, 0.1f);
        transform.DOScale(0.001f, 0.4f).SetEase(Ease.OutBack).OnComplete( () =>
        {
            gameObject.SetActive(false);
        });
    }
    
}


