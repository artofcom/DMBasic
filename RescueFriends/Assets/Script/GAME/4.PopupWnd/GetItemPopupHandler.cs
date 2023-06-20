using UnityEngine;
using System.Collections;
//using NOVNINE;
//using NOVNINE.Diagnostics;
using Spine.Unity;
using Spine;

public struct GetItemInfo
{
	public string Id;
	public int Count;
	public string Comment;
}

public class GetItemPopupHandler : BasePopupHandler 
{	
	readonly string[] SLOT = new string[]{ "Get_Daily_effect01","Get_Daily_effect02","Get_Daily_effect03","Get_Daily_effect04","eff_star" };
	public static bool GetItemPopupTouch = false;
	public SkeletonAnimation Effect;
	public tk2dTextMesh NameText;
	public tk2dTextMesh Comment;
	public tk2dTextMesh Count;
	public tk2dUIItem back = null;
	
    GetItemInfo _itemInfo;

	protected override void Start () 
	{
		base.Start();
		
		tk2dCamera cam = Camera.main.GetComponent<tk2dCamera>();
		
		Bone _bone = Effect.Skeleton.FindBone("curtain");
		if(_bone != null)
		{
			RegionAttachment _attachment = Effect.Skeleton.FindSlot("curtain").Attachment as RegionAttachment;

			float aspectFitHeight = cam.ScreenExtents.height / _attachment.Height + 50;
			float aspectFitWidth = cam.ScreenExtents.width / _attachment.Width + 50;

			_bone.ScaleX = aspectFitWidth;
			_bone.ScaleY = aspectFitHeight;	
		}
	}

	protected override void  OnEnter (object param) 
	{
		GetItemPopupHandler.GetItemPopupTouch = false;

        NNSoundHelper.Play("FX_item_earning");

        //bool isLoop             = false;
/*		if(param != null)
		{
			_itemInfo           = (GetItemInfo)param;
			string ribbon = "ribbon_bg";
			
          //  isLoop              = _itemInfo.isOnTutorial;
			Attachment _attachment0 = null;
			Attachment _attachment1 = null;
			Slot _slot = null;
			int index = 0;
			int token = 1;
			if(_itemInfo.IsRecip)
			{
				index = SA.Skeleton.FindSlotIndex("plate");
				_attachment0 = SA.Skeleton.GetAttachment(index, "plate");
				index = SA.Skeleton.FindSlotIndex("plate_mask");
				_attachment1 = SA.Skeleton.GetAttachment(index, "plate_mask");
				token = 2;	
			}
			else
			{
				ribbon = "get_ribbon_bg";
			}
			
			for(int i = 0; i < SLOT.Length; ++i)
			{
				index = Effect.Skeleton.FindSlotIndex(SLOT[i]);
				_slot = Effect.Skeleton.FindSlot(SLOT[i]);
				_slot.Attachment = Effect.Skeleton.GetAttachment(index, string.Format("{0}_{1}",SLOT[i],token));
			}
			
			index = SA.Skeleton.FindSlotIndex("get_ribbon_bg");
			_slot = SA.Skeleton.FindSlot("get_ribbon_bg");
			_slot.Attachment = SA.Skeleton.GetAttachment(index, ribbon);
			
			_slot = SA.Skeleton.FindSlot("plate");
			_slot.Attachment = _attachment0;
			_slot = SA.Skeleton.FindSlot("plate_mask");
			_slot.Attachment = _attachment1;
			
			index = SA.Skeleton.FindSlotIndex("item");
			_slot = SA.Skeleton.FindSlot("item");

            #if NO_GAME_SERVER
            _itemInfo.Id = "Ing_Sw01";
            #endif

			_slot.Attachment = SA.Skeleton.GetAttachment(index, _itemInfo.Id);
			
			string Name = _itemInfo.Comment;
			string Title = "";
			
			Count.text          = string.Format("x {0}", _itemInfo.Count);
			Count.Commit();
			
            if (_itemInfo.Id.Contains("Ing_"))
            {
                NOVNINE.IngrediantItem ingred = NOVNINE.Context.UncleBill.GetIngrediantItemByID(_itemInfo.Id);
                Name = ingred.name;
                Title = ingred.eType.ToString();
                Count.gameObject.SetActive(false);
            }
            else
            {
                Count.gameObject.SetActive(true);

//                if (_itemInfo.Id == "coin_bic")
//                {
//                    NOVNINE.Wallet.FireItemChangedEvent("coin", NOVNINE.Wallet.GetItemCount("coin"));
//                    WorldOverlayHandler handler = Scene.GetOverlayHandler<WorldOverlayHandler>();
//                    if (handler.gameObject.activeSelf)
//                        handler.PlayCoinEffect(transform.position);
//                }
            }
			
			Comment.text = Title;
			Comment.Commit();
			NameText.text = Name;
			NameText.Commit();
			
		}
		EventLock(false);
        PlayEffect("show",false);
        Play("show",false);*/
	}

    public IEnumerator processOnTutorial()
    {
     //   SA.AnimationState.TimeScale = 0.0f;
        //SA.AnimationState.
        PlayEffect("idle", true);
        do
        {
            if(null != TutorialOverlayHandler.getActiveTutorialId())
                yield return new WaitForSeconds(0.05f);
            else break;
        }while(true);
        
    //    SA.AnimationState.TimeScale = 1.0f;
    }

	float PlayEffect(string animationName,bool loop, float delay = 0.0f) 
	{
		if(Effect != null)
		{
			Spine.Animation ani = Effect.skeleton.Data.FindAnimation(animationName);
			if(ani != null)
			{
				float d = ani.Duration;
				TrackEntry _trackEntry = Effect.AnimationState.SetAnimation(0, animationName, loop);
				_trackEntry.Delay = delay;

				if(loop)
					return -1;
				else
					return d;
			}
		}

		return -2;
	}
	
	protected override void OnComplete (TrackEntry entry)
	{
		base.OnComplete(entry);
		if(entry.Animation.Name == "show" )
		{
			//PlayEffect("idle", true);
			OnEscape();
			return;
		}
		
		if(entry.Animation.Name == "hide" )
		{
			gameObject.SetActive(false);
			GetItemPopupHandler.GetItemPopupTouch = true;
			return;
		}
		
		return;
	}
	
	protected override void OnEvent (TrackEntry entry, Spine.Event e)
	{
		if(e.Data.Name == "Enable")
		{
			
			return;
		}
	}
	
	protected override void  OnLeave()
	{
		//Play("hide",false);

        if (_itemInfo.Id == "coin_bic")
        {
            NOVNINE.Wallet.FireItemChangedEvent("coin", NOVNINE.Wallet.GetItemCount("coin"));
            //WorldOverlayHandler handler = Scene.GetOverlayHandler<WorldOverlayHandler>();
            //if (handler.gameObject.activeSelf)
            //    handler.PlayCoinEffect(transform.position);
        }
        else if (_itemInfo.Id == "heart_bic")
        {
            //WorldOverlayHandler.ChargeStamina = 0; 

            //WorldOverlayHandler handler = Scene.GetOverlayHandler<WorldOverlayHandler>();
            //if (handler.gameObject.activeSelf)
            //    handler.PlayLifeEffect();
        }
		//PlayEffect("hide", false);
	}
	
	public override void CLICK (tk2dUIItem item) 
	{
        // note : 그냥 두면 자동으로 닫힘.
		base.CLICK(item);
		
		
		if(back==item)// && false==_itemInfo.isOnTutorial)
		{
			GetItemPopupHandler.GetItemPopupTouch = true;
			
			OnEscape();	
			//			Play("hide", false);
			//			PlayEffect("hide", false);
		}
	}
}