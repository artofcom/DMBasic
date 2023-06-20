using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;
using DG.Tweening;

public class RatePopupHandler : BasePopupHandler
{
#if UNITY_EDITOR
    public const bool _TESTMODE = false;// true;
#else
    public const bool _TESTMODE = false;
#endif

	public tk2dUIItem rateBTN;
	public tk2dUIItem feedbackeBTN;
    public tk2dUIItem noNeedWorkBTN;
	
    ButtonAnimation[] BTNAni = new ButtonAnimation[3];

	protected override void Awake () 
	{
		base.Awake();
	}

	protected override void Start () 
	{
		base.Start();
		if(rateBTN != null)     BTNAni[0] = rateBTN.GetComponent<ButtonAnimation>();
        if(feedbackeBTN != null)    BTNAni[1] = feedbackeBTN.GetComponent<ButtonAnimation>();
        if(noNeedWorkBTN!=null) BTNAni[2]= noNeedWorkBTN.GetComponent<ButtonAnimation>();
	}

	protected override void  OnEnter (object param) 
	{        
		base.OnEnter(param);
	}

    bool _alreadyRated()
    {
        return true;// (Root.Data.gameData.record.BaseData.Rated && false==_TESTMODE);
    }

	protected override void OnComplete (TrackEntry entry)
	{
		base.OnComplete(entry);

		if(entry.Animation.Name == "show")
        { 
			for(int i = 0; i < BTNAni.Length; ++i)
			{
				BTNAni[i].CLICK();
			}

            //if(true == _alreadyRated())
           //     rateBTN.GetComponent<ButtonAnimation>().SetColor( Color.gray );
		}

		if(entry.Animation.Name == "hide")
		{
			gameObject.SetActive(false);
		}
	}

	protected override void OnLeave()
	{
		base.OnLeave();
	}

	public override void CLICK (tk2dUIItem item) 
	{
        if(rateBTN==item && true==_alreadyRated())
            return;

		base.CLICK(item);

        OnEscape();
        return;

        /*
        if(rateBTN == item )
		{
			Root.Data.gameData.record.BaseData.Rated = true;
			Root.Data.gameData.SaveContext();
#if USE_UncleBill
			NOVNINE.Store.UncleBill.OpenStore(null, true);
#endif
		}
        else if (feedbackeBTN == item)
        {
            NOVNINE.NativeInterface.SendFeedback("nov9.support@nov9games.com");
        }*/
		
		OnEscape();
	}
}
