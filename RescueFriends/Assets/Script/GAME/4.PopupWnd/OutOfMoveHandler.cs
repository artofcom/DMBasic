using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;
using Data;
using Spine.Unity;
using Spine;

public enum CONTINUE_RESULT { CLOSE, CONTINUE, GIVE_UP };
public enum MISSION_TYPE { NONE=-1, COLLECT, FILL, DEFEAT, FIND, CLEAR,SCORE, MAX_TYPE };    

public class OutOfMoveHandler : BasePopupHandler
{	
	readonly static float[,] ITEM_POSX = new float[,]{{-2.07F , 1.89F,0.0f}, {-3.4F, 0.1F, 3.51F }};

    //public struct PARAM
    //{
    //    public Level level;
    //    public int countAutoBlk;
    //};

	//public tk2dBaseSprite[] items;	
	public tk2dTextMesh coinCount;
	public tk2dUIItem playOnBTN;
	public tk2dUIItem closeBTN;
	
    public tk2dTextMesh txtMessage;

    int coinCost = 0;
    //int levelIndex = -1;
	ButtonAnimation[] BTNAni = new ButtonAnimation[2];

    int _countAutoEventBlk      = 0;

	protected override void  OnEnter (object param) 
	{
        //PARAM needParam         = (PARAM)param;
		//Level level             = needParam.level;
        //_countAutoEventBlk      = needParam.countAutoBlk;

        //levelIndex              = level.Index;
        coinCost                = InfoLoader.GetItemCost("PlayOn");
		if(coinCount != null)
		{
            coinCount.text = coinCost.ToString();
			coinCount.Commit();
		}

        //ActiveContinueItems(level);

        // NNSoundHelper.StopBGM(0, true, 0.15f);
		NNSoundHelper.Play("FX_outofmove");
		base.OnEnter(param);

        
        if(JMFUtils.GM.CurrentLevel.isTimerGame)
        {
            int nExtraTimes     = InfoLoader.GetDefaultPlayOnAddTime(0);
            txtMessage.text     = string.Format("Add {0} seconds to Continue!", nExtraTimes);
            txtMessage.Commit();
        }
        else
        {
            int nExtraTurns     = InfoLoader.GetDefaultPlayOnMoveCount(JMFUtils.GM.CountinueCount);
            txtMessage.text     = string.Format("Add {0} moves to Continue!", nExtraTurns);
            txtMessage.Commit();
        }

        //Add 5 moves to Continue!
    }
	
	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		if(closeBTN == item)
			Scene.ClosePopup(CONTINUE_RESULT.GIVE_UP);
		else if(playOnBTN == item)
		{
#if UNITY_EDITOR
			if(LevelEditorSceneHandler.EditorMode)
			{
				//ContinueGame();
                Scene.ClosePopup(CONTINUE_RESULT.CONTINUE);
				return;
			}
#endif			
            if(Wallet.GetItemCount("coin") < coinCost)
            {
                // Rubby buy popup needed
                MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
                data2.isOkOnly      = false;
                data2.strMessage    = "You don't have \nenough rubbys.\nWanna Buy some?";
                data2.emotion       = MessagePopupHandler.EMOTIONS.SAD;
                Scene.ShowPopup("MessagePopup", data2, (param) =>
                {
                    if(null != param)
                    {
                        MessagePopupHandler.RET ret = (MessagePopupHandler.RET)param;
                        if(ret == MessagePopupHandler.RET.OK)
                            Scene.ShowPopup("BuyRubyPopup", null, null);
                    }
                });
            }
            else 
                Scene.ClosePopup(CONTINUE_RESULT.CONTINUE);
		}
	}
	
	void ContinueGame()
	{
        Debug.Log("Continue Game....");
        
	}

    void ActiveContinueItems (Data.Level level)
	{
        /*Debugger.Assert(level != null, "ContinuePopupHandler.ActiveContinueItems : Level is null.");

		bool addItem = JMFUtils.GM.CountinueCount > 0;
		
		items[2].gameObject.SetActive(addItem);
		int index = Convert.ToInt32(addItem);
		float size = 1f;
		if(addItem)
			size = 0.8f;
		
        for(int i = 0; i < items.Length - 1 + index; ++i)
		{
			Vector3 pos = items[i].transform.localPosition;
			pos.x = ITEM_POSX[index,i];
			items[i].transform.localPosition = pos;
			items[i].scale = Vector3.one * size;
		}
		
		if(level.missionType == (int)MISSION_TYPE.DEFEAT)
		{
            transform.FindChild("Form/popup/Text").GetComponent<TextMesh>().text    = "Do Not Give Up!";
			items[0].SetSprite("remove5spell");
			comment.text = "Remove -5 Spell to Continue!";

            string token = GetSpriteItem(InfoLoader.GetDefeatPlayOnBooster(JMFUtils.GM.CountinueCount));
            if(token != null)
                items[1].SetSprite(token);
            token = GetSpriteItem(InfoLoader.GetDefeatPlayOnItem(JMFUtils.GM.CountinueCount));
            if(token != null)
                items[2].SetSprite(token);
		}
		else
		{
            transform.FindChild("Form/popup/Text").GetComponent<TextMesh>().text    = "Out of Moves";
			comment.text = "Add 5 Moves to Continue!";
			items[0].SetSprite("extra5move");
            string token = GetSpriteItem(InfoLoader.GetDefaultPlayOnBooster(JMFUtils.GM.CountinueCount));
            if(token != null)
                items[1].SetSprite(token);
            token = GetSpriteItem(InfoLoader.GetDefaultPlayOnItem(JMFUtils.GM.CountinueCount));
            if(token != null)
                items[2].SetSprite(token);
		}*/
		        
//        bool didGameOverByTimeBomb = JMFUtils.GM.DidGameOverByTimeBomb();
//        int posXGapIndex = 0;
    } 

    string GetSpriteItem(string token)
    {
        if (token == "mixedbooster")
            return "boost_bic_01";

        if (token == "triplebooster")
            return "boost_bic_02";
        
        if (token == "specialbooster")
            return "boost_bic_03";
        
        if (token == "hammer")
            return "btn_hammer";
        
        if (token == "firecracker")
            return "btn_firecracker";
        
        if (token == "magicswap")
            return "btn_magicswap";

        if (token == "rainbowbust")
            return "btn_rainbowbust";

        return null;
    }
}
