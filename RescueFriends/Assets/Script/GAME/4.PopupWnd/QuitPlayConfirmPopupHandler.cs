using UnityEngine;
using System.Collections;
using NOVNINE;
using Spine.Unity;
using Spine;

public enum QUITPLAY_RESULT { RESUME, GIVE_UP };

public class QuitPlayConfirmPopupHandler : BasePopupHandler
{
	public tk2dUIItem playOnBTN;
	public tk2dUIItem giveUpBTN;

	ButtonAnimation[] BTNAni = new ButtonAnimation[2];

    int _countAutoEventBlk      = 0;
    int ClickBTNIndex = 0;


	protected override void Start () 
	{
		base.Start();
		if(playOnBTN != null)
			BTNAni[0] = playOnBTN.GetComponent<ButtonAnimation>();

		if(giveUpBTN != null)
			BTNAni[1] = giveUpBTN.GetComponent<ButtonAnimation>();
	}
	
	protected override void OnEnter (object param) 
	{
		//=if (NNSoundHelper.EnableBGM && NNSoundHelper.IsPlaying("play_bgm")) 
		//=	NNSoundHelper.StopBGM(0, false);

        JMFRelay.ON_PACKET_RES_CLOSE_POPUP += OnPacketResClosePopup;
        JMFRelay.ON_PACKET_RES_CANCEL += OnPacketCancel;
		base.OnEnter(param);
        ClickBTNIndex = 0;
       // bReceive = false;
        _countAutoEventBlk      = (int)param;
	}

	protected override void OnComplete (TrackEntry entry)
	{
		base.OnComplete(entry);

		if(entry.Animation.Name == "show")
		{
			for(int i = 0; i < BTNAni.Length; ++i)
			{
				if(BTNAni[i] != null)
					BTNAni[i].CLICK();
			}
		}

		if(entry.Animation.Name == "hide")
		{
            if (ClickBTNIndex == 0) // palyBtn
            {
                JMFUtils.GM.Resume();
            }
                
			gameObject.SetActive(false);
		}
	}

    protected override void OnLeave()
	{
        JMFRelay.ON_PACKET_RES_CLOSE_POPUP -= OnPacketResClosePopup;
        JMFRelay.ON_PACKET_RES_CANCEL -= OnPacketCancel;
		base.OnLeave();
	}

	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);

        if (playOnBTN == item)
        {
            ClickBTNIndex = 0;
            //Scene.ClosePopup(QUITPLAY_RESULT.RESUME);
            OnEscape();
        }
		else if(giveUpBTN == item)
        {
         /*   if (_countAutoEventBlk > 0)
            {
                Scene.AddPopup("TastyCollectorContinuePopup", true, _countAutoEventBlk, (_param) =>
                    {
                        EventLock(true);
                        bool isContinue = (bool)_param;
                        if (false == isContinue)
                        {
                            ClickBTNIndex = 1;
                            StartCoroutine("_coWaitResultPacket");
                        }
                    });
            }
            else*/
            {
                ClickBTNIndex = 1;
                StartCoroutine("_coWaitResultPacket");

            }
        }
	}
        
    void OnPacketResClosePopup()
    {        
     //   bReceive = true;
        OnEscape();
    }

    IEnumerator _coWaitResultPacket()
    {
     //   bReceive = false;
        byte failReason = (byte)E_PUZZLE_GAMERESULT_FAIL_REASON.E_PUZZLE_GAMERESULT_FAIL_REASON_GIVEUP;
       // if(JMFUtils.GM.CountinueCount > 0)
       //     failReason = (byte)E_PUZZLE_GAMERESULT_FAIL_REASON.E_PUZZLE_GAMERESULT_FAIL_REASON_OUTOFMOVE_GIVEUP;

        
        OnPacketResClosePopup();

        PlaySceneHandler.ShowFailPopup(JMFUtils.GM.Score ,failReason);

        yield break;
      //  while (!bReceive)
        {
      //      yield return null;
        }
    }

    void OnPacketCancel()
    {
        ClickBTNIndex = 0;
      //  bReceive = true;
        StopCoroutine("_coWaitResultPacket");
        OnEscape();
    }
}
