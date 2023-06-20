using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;
using NOVNINE;
using DG.Tweening;

public class DailyPopupHandler : BasePopupHandler
{
    public const int MAX_CELL   = 10;
    //public const int IDX_TODAY_CELL = 3;

	public tk2dUIItem           _btnClose;
	public List<DailyCell>      _dailyCells;
	
	//int _idxCellToday           = -1;   // 0 ~ MAX_CELL-1
    //_DAILY_REWARD_INFO          _oTodayReward;

    Transform _trToday          = null;

	protected override void OnEnter (object param) 
	{   
        // note : we'd already have our daily gift before this popup.
        int idxToday            = Root.Data.gameData.GetDailyRewardedCount() - 1;

        _DAILY_REWARD_INFO      outInfo = new _DAILY_REWARD_INFO();
        for(int q = 0; q < MAX_CELL; ++q)
        {
            int idxTarget       = q;// cntReward + q - IDX_TODAY_CELL;
			InfoLoader.GetDailyRewardData(idxTarget+1, ref outInfo);

            NOVNINE.InventoryItem   invenItem = NOVNINE.Context.UncleBill.GetInventoryItemByID(outInfo.strRewardName);
            if(null==invenItem)     continue;

            _dailyCells[q].init(q+1, invenItem.spriteName, outInfo.count, idxToday+1);

            // set data.
            // outInfo.strRewardName
            if(q == idxToday)
                _trToday        = _dailyCells[q].transform;
        }
		base.OnEnter(param);
    }	

    protected override void OnEnterFinished()
	{
        base.OnEnterFinished();

        if(null != _trToday)
            _trToday.DOScale(_trToday.localScale.x*1.1f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
	}

    protected override void OnLeaveFinished()
	{
        if(null != _trToday)    _trToday.DOKill();
        base.OnLeaveFinished();
	}


	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		if(item == _btnClose)
			Scene.ClosePopup();
	}
}
