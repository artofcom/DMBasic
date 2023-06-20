using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;
using DG.Tweening;

class FaderController : MonoBehaviour
{
    public SpriteRenderer       _sprDark;
    public SpriteRenderer       _sprBG, _sprSymbol;

    public Sprite[] sprLinks    = null;

    int _idxCurLoop             = 0;

    // 특정 이름의 sprite를 array에서 찾아서 반환한다.
    public Sprite getSprite(string strName)
    {
        if(null==sprLinks)      return null;

        for(int j = 0; j < sprLinks.Length; ++j)
        {
            if(strName.Equals( sprLinks[j].name))
                return sprLinks[j];
        }
        return null;
    }

    // 레벨에 따른 로직에 의해 적절한 tip sprite를 반환한다.
    public Sprite getProperSymbolByLevel(Data.Level lv)
    {
        return getSprite("tip_01");
        /* -
        if(null == lv)          return getSprite("tip_01");

        // Rules.
        // 1. 각 미션의 3번째 level에 한해서 특정 미션의 팁 이미지를 보여준다.
        // 2. 1번을 제외한 모든 미션에서 존재하는 기타 tip(1~7)이미지를 2회씩 순차적으로 보여준다.

        //lv.missionType
        if( lv.missionType<=(int)EditWinningConditions.MISSION_TYPE.NONE || 
            lv.missionType>=(int)EditWinningConditions.MISSION_TYPE.OLD_TYPE)
            return _getLoopTip();
        
        int maxTime             = 3;
        for(int q = 0; q<Root.Data.levels.Length; ++q)
        {
            if(Root.Data.levels[q].missionType == lv.missionType)
            {
                if(Root.Data.levels[q].Index == lv.Index)
                    return _getMissionTip(lv.missionType);
                
                --maxTime;      // 2(1), 1(2), 0(3), -1(4), ....
                if(maxTime<0)   break;
            }
        }
        return _getLoopTip();
        */
    }

    // 순환하면서 tip image를 반환한다.
    Sprite _getLoopTip()
    {
        ++_idxCurLoop;
        Sprite sprRet           = getSprite(string.Format("tip_{0:D2}", _idxCurLoop));

        // 없으면 처음부터 다시 돈다.
        if (null == sprRet)
            _idxCurLoop         = 1;

        sprRet                  = getSprite(string.Format("tip_{0:D2}", _idxCurLoop));
        return sprRet;
    }

    // 각 mission에 따른 tip image를 반환한다.
    Sprite _getMissionTip(int missionType)
    {
        switch( (EditWinningConditions.MISSION_TYPE)missionType )
        {
        case EditWinningConditions.MISSION_TYPE.CLEAR:      return getSprite("tip_mud");
        case EditWinningConditions.MISSION_TYPE.COLLECT:    return getSprite("tip_collect");
        case EditWinningConditions.MISSION_TYPE.FILL:       return getSprite("tip_syrup");
        case EditWinningConditions.MISSION_TYPE.FIND:       return getSprite("tip_cheesebar");
        case EditWinningConditions.MISSION_TYPE.DEFEAT:     return getSprite("tip_vs");
        default: return _getLoopTip();
        } 
    }
}