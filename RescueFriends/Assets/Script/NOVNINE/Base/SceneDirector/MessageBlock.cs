using UnityEngine;
using System.Collections;
using NOVNINE;
using NOVNINE.Diagnostics;
using Spine.Unity;
using Spine;

public class MessageBlock : MonoBehaviour
{
	public tk2dTextMesh message;
	public SkeletonAnimation root;
    bool isEnd = true;
    Spine.AnimationState.TrackEntryDelegate completeDelegate;
    Bone _bone = null;

    void Awake()
    {
        completeDelegate = new Spine.AnimationState.TrackEntryDelegate(OnComplete);
        root.Initialize(true);
        _bone = root.Skeleton.FindBone("curtain");
        if(_bone != null)
        {
            tk2dCamera cam = Camera.main.GetComponent<tk2dCamera>();
            RegionAttachment _attachment = root.Skeleton.FindSlot("curtain").Attachment as RegionAttachment;
            Rect rect = cam.ScreenExtents;

            float aspectFitHeight = (rect.height * 1.48F) / _attachment.Height;
            float aspectFitWidth = (rect.width * 1.48F) / _attachment.Width;

            _bone.ScaleX = aspectFitWidth;
            _bone.ScaleY = aspectFitHeight; 
        }

    }
        
    void OnComplete (TrackEntry entry)
    {
        entry.Complete -= completeDelegate;

        root.AnimationState.ClearTracks();
        Scene.UnBlock(Camera.main);
        root.Initialize(true);

        if(_bone != null)
        {
            tk2dCamera cam = Camera.main.GetComponent<tk2dCamera>();
            RegionAttachment _attachment = root.Skeleton.FindSlot("curtain").Attachment as RegionAttachment;
            Rect rect = cam.ScreenExtents;

            float aspectFitHeight = (rect.height * 1.48F) / _attachment.Height;
            float aspectFitWidth = (rect.width * 1.48F) / _attachment.Width;

            _bone.ScaleX = aspectFitWidth;
            _bone.ScaleY = aspectFitHeight; 
        }
        isEnd = true;
        gameObject.SetActive(false);
    }

    public void Show (Camera cam, float delay = 0.0f) 
	{
        if (isEnd)
        {
    		Scene.Block(cam);
    		gameObject.SetActive(true);
    		
    		if(transform.parent != cam.transform)
    			transform.SetParent(cam.transform, false);

            if (delay > 0.0f)
            {
                message.gameObject.SetActive(false);
                DG.Tweening.DOVirtual.DelayedCall(1.0f, () => {
                        message.gameObject.SetActive(true);
                    });
            }


            TrackEntry entry = root.AnimationState.SetAnimation( 0, "play", true);
            entry.Delay = delay;
            isEnd = false;
        }
    }

    public void Hide () 
	{
        TrackEntry entry = root.AnimationState.SetAnimation(1, "hide", false);
        entry.Complete += completeDelegate;
    }
}
