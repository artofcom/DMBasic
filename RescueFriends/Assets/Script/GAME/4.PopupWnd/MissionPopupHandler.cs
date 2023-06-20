using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NOVNINE;
using NOVNINE.Diagnostics;
using Spine.Unity;
using Spine;

public class MissionPopupHandler : BasePopupHandler 
{
    const float fAUTO_CLOSE_TIME= 1.0f;

	public tk2dTextMesh         _txtTitle;
    public CollectAnyMissionPopup _objCollectAni;
    public ScoreMissionPopup    _objScore;

    Data.Level _levelData;
    bool _isClosing             = false;

	protected override void  OnEnter (object param) 
	{
		if(param != null)		
			_levelData          = (Data.Level)param;
		
        _isClosing              = false;
		base.OnEnter(param);

        _refreshUI();

        NNSoundHelper.Play("PFX_info_intro");
	}
    
    protected override void OnEnterFinished()
    {
        DOVirtual.DelayedCall(fAUTO_CLOSE_TIME, () => _close() );
    }
    
	public override void CLICK (tk2dUIItem item) 
	{
        _close();
	}

    void _close()
    {
        if(false == _isClosing)
        {
            Scene.ClosePopup();
            _isClosing          = true;
        }
    }

    void _refreshUI()
    {
        string strTargetText    = "";
        switch(_levelData.missionType)
        {
        case (int)EditWinningConditions.MISSION_TYPE.COLLECT:
            strTargetText       = "Collect the blocks";
            _objCollectAni.gameObject.SetActive( true );
            break;
        case (int)EditWinningConditions.MISSION_TYPE.FILL:
            strTargetText       = "Collect the potions";
            _objCollectAni.gameObject.SetActive( true );
            break;
        case (int)EditWinningConditions.MISSION_TYPE.FIND:
            strTargetText       = "Find the Ice bar";
            _objCollectAni.gameObject.SetActive( true );
            break;
        case (int)EditWinningConditions.MISSION_TYPE.CLEAR:
            strTargetText       = "Clear all the lavas";
            _objCollectAni.gameObject.SetActive( true );
            break;
        case (int)EditWinningConditions.MISSION_TYPE.SCORE:
            strTargetText       = "Target Score";
            _objScore.gameObject.SetActive( true );
            _objScore.refreshUI(_levelData.goalScore);
            break;
        case (int)EditWinningConditions.MISSION_TYPE.DEFEAT:
        default:
            break;
        }   

        if(false == string.IsNullOrEmpty(strTargetText))
            _txtTitle.text      = strTargetText;
    }
}
