using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;
using Data;
using Spine.Unity;
using Spine;
using DG.Tweening;
//using Facebook.Unity;

public struct PlayReadyInfo
{
	public int currentLevelIndex;
	public bool InGame;
}

public enum READY_RESULT { CLOSE, PLAY }

public class PlayReadyPopupHandler :BasePopupHandler
{
	public tk2dUIItem           closeBTN;
	public tk2dUIItem           playBTN;
    public tk2dTextMesh         _txtLevel;
    public tk2dSprite           _sprStar0, _sprStar1, _sprStar2;
    public tk2dTextMesh         _txtHelp;
	
    public BoosterButton[]      _buttons = null;
    public string[]             _strBoosterId   = null;
    bool[] _isBoosterChecked    = null;
    
	PlayReadyInfo info;
	Data.Level currentLevel;
	
	protected override void OnEnter (object param) 
	{
        //updateItemIndex = -1;
		
        if(null == _isBoosterChecked)
            _isBoosterChecked   = new bool[ _buttons.Length ];
        for(int z = 0; z < _buttons.Length; ++z)
            _isBoosterChecked[z]= false;

        if(param != null)
		{
			info                = (PlayReadyInfo)param;
#if UNITY_EDITOR
            if(true == LevelEditorSceneHandler.EditorMode)
                currentLevel    = Director.CurrentSceneLevelData;
            else
#endif
            {
                currentLevel    = Root.Data.GetLevelFromIndex(info.currentLevelIndex);
            }
		}
		base.OnEnter(param);

        _refreshUI();
        _txtHelp.text           = "Select your booster.";
        _txtHelp.Commit();
	}

    void _refreshUI()
    {
        _txtLevel.text          = string.Format("Level {0}", currentLevel.Index+1);
        _sprStar0.spriteName    = "star_big_grey";
        _sprStar1.spriteName    = "star_big_grey";
        _sprStar2.spriteName    = "star_big_grey";
        switch( Root.Data.gameData.GetGradeLevelByIndex(currentLevel.Index) )
        {
        case 1:
            _sprStar0.spriteName= "star_big";
            break;
        case 2:
            _sprStar0.spriteName= "star_big";
            _sprStar1.spriteName= "star_big";
            break;
        case 3:
            _sprStar0.spriteName= "star_big";
            _sprStar1.spriteName= "star_big";
            _sprStar2.spriteName= "star_big";
            break;
        case 0:
        default:
            break;
        }

        // init booster items.
        Debug.Assert(_buttons.Length == _strBoosterId.Length);
        for(int z = 0; z < _buttons.Length; ++z)
        {
            _buttons[z].refreshUI(_strBoosterId[z], _isBoosterChecked[z]);
        }
    }

	public override void OnEscape() 
	{
		Scene.ClosePopup(READY_RESULT.CLOSE);
	}
	
	public override void CLICK (tk2dUIItem item) 
	{
        StartCoroutine( _coClick(item) );
    }

    IEnumerator _coClick(tk2dUIItem item)
    {
        yield return new WaitForSeconds(0.1f);

		base.CLICK(item);

		if(closeBTN == item)
		{
			OnEscape();
			yield break;
		}

		if(playBTN == item)
		{ 
            Debug.Assert(_strBoosterId.Length == _isBoosterChecked.Length);
            List<string> listIds    = new List<string>();
            for(int q = 0; q < _strBoosterId.Length; ++q)
            {
                if(_isBoosterChecked[q])
                {
                    listIds.Add( _strBoosterId[q] );
                    Wallet.Use( _strBoosterId[q], -1 );
                }
            }

            GameManager.EquipItems( ref listIds );
            Scene.ClosePopup(READY_RESULT.PLAY);
		}	
	}

    void onClickBooster(tk2dUIItem item)
    {
        for(int q = 0; q < _buttons.Length; ++q)
        {
            if(true == _buttons[q].GetComponent<tk2dUIItem>().Equals(item))
            {
                int count       = Wallet.GetItemCount(_strBoosterId[q]);
                if(count > 0)
                {
                    _isBoosterChecked[q]    = !_isBoosterChecked[q];
                    _buttons[q].refreshUI(_strBoosterId[q], _isBoosterChecked[q]);
                }
                else
                {
                    Scene.ShowPopup("BuyItemPopup", _strBoosterId[q], (param) =>
                    {
                        _refreshUI();
                    });
                }

                _setHelpString( _strBoosterId[q] );

                return;
            }
        }
    }

    void _setHelpString(string id)
    {
        string strHelp          = "";

        if(id == "moreturnbooster")
            strHelp             = "Play with 5 more turns.";
        else if(id == "hbombbooster")
            strHelp             = "Play with H Bomb Booster.";
        else if(id == "vbombbooster")
            strHelp             = "Play with V Bomb Booster.";
        else if(id == "bombbooster")
            strHelp             = "Play with Bomb Booster.";
        else if(id == "rainbowbooster")
            strHelp             = "Play with Rainbow Booster.";

        _txtHelp.text           = strHelp;
        _txtHelp.Commit();
    }
}
    