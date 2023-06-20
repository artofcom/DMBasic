using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using Spine;

public enum DP_ITEM { RAINBOW,LINE,BOMB,COIN };
public enum CELL_STATE { NOT_OPEN, OPEN};
public enum DAILY_ITEM_TYPE { COIN, LINER, BOMB, RAINBOW };

public class DailyCell : MonoBehaviour 
{
    int _id;
    int _cntReward;
	string _strImgName;
    int _nToday;
    
    public void init(int id, string strPicName, int count, int today)
    {
        _id                     = id;
        _strImgName             = strPicName;
        _cntReward              = count;
        _nToday                 = today;

        _refreshUI();
    }

	void _refreshUI()
	{
        tk2dTextMesh txtMesh    = null;
        Transform trTarget      = transform.Find("txtDay");
        if(null != trTarget)
        {
            txtMesh             = trTarget.GetComponent<tk2dTextMesh>();
            if(_nToday!=_id)    txtMesh.text    = string.Format("Day {0}", _id);
            else                txtMesh.text    = "Today";
        }

        trTarget                = transform.Find("sprReward/txtCount");
        if(null != trTarget)
        {
            txtMesh             = trTarget.GetComponent<tk2dTextMesh>();
            txtMesh.text        = string.Format("{0}", _cntReward);
        }

        trTarget                = transform.Find("sprReward");
        if(null != trTarget)
        {
            tk2dSprite sprItem  = trTarget.GetComponent<tk2dSprite>();
            sprItem.spriteName  = _strImgName;
        }

        trTarget                = transform.Find("sprChecked");
        if(null != trTarget)    trTarget.GetComponent<MeshRenderer>().enabled = _id<=_nToday;

        tk2dSprite sprBG        = transform.GetComponent<tk2dSprite>();
        if(null != sprBG)
        {
            string strPic       = _id==_nToday ? "Pink" : (_id<_nToday ? "Blue" : "Green");
            sprBG.spriteName    = strPic;
        }
	}
	
    
}
