using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using DG.Tweening;
using NOVNINE.Diagnostics;
using Data;
using Spine.Unity;
using Spine;

public enum PANEL_TYPE { NONE, RATE, SHARE };
public enum CLEAR_RESULT { CLOSE, NEXT_LEVEL, REPLAY };

public struct ClearInfo 
{
    public Level level;
    public int prevGrade;
    public int currGrade;
	public System.Int64 score;      // best score.
    public System.Int64 cur_score;  // current score.
	public bool highScore;
}

public class ClearPopupHandler : BasePopupHandler//MonoBehaviour, IPopupWnd 
{  
    public tk2dTextMesh scoreText, levelText;
	public tk2dUIItem btnNext;
	public tk2dUIItem btnReplay;
    
    public Transform[]          _trStars;

    ClearInfo info;
	GameObject _objDummy;
	
	protected override void  OnEnter (object param)
	{
		Debugger.Assert(param is ClearInfo, "ClearPopupHandler.OnEnterPopup : Param is wrong.");
		base.OnEnter(param);
		if(param != null)       info = (ClearInfo)param;

		scoreText.text          = "0";// string.Format("{0:#,###0}", info.cur_score);  // cur score.
        levelText.text          = string.Format("Level : {0}", info.level.Index + 1);

        for(int z = 0; z < 3; ++z)
            _trStars[z].GetComponent<tk2dSprite>().spriteName = "star_big_grey";
        
        NNSoundHelper.PlayBGM("PFX_win_intro", 0, true, false);
    }
	

    IEnumerator _playSound(string strSndName, float delay)
    {
        yield return new WaitForSeconds(delay);
        NNSoundHelper.Play( strSndName );
    }


    protected override void OnEnterFinished()
    {
        base.OnEnterFinished();

        float delay             = .0f;
        for(int q = 0; q < info.currGrade; ++q)
        {
            Sequence doSeq      = DOTween.Sequence();
            doSeq.PrependInterval(delay);
            doSeq.Append( _trStars[q].DOScale(1.1f, 0.2f) );
            StartCoroutine( _coChangeStarSkin( _trStars[q], delay+0.2f ));
            doSeq.Append( _trStars[q].DOScale(1.0f, 0.2f) );
            delay += 0.4f;
        }

        _objDummy               = new GameObject();
        _objDummy.transform.localPosition    = Vector3.right * 0;
        _objDummy.transform.DOLocalMoveX(info.cur_score, delay + 0.5f).SetEase(Ease.Linear)
        .OnUpdate( () =>{
            scoreText.text      = string.Format("{0:#,###0}", (int)_objDummy.transform.localPosition.x);
        })
        .OnComplete( () => GameObject.Destroy(_objDummy) );


        //scoreText.DOText( string.Format("{0:#,###0}", info.cur_score), 3.0f ).SetEase(Ease.Linear);
    }

    IEnumerator _coChangeStarSkin(Transform trStar, float delay)
    {
        yield return new WaitForSeconds(delay);

        tk2dSprite sprte        = trStar.GetComponent<tk2dSprite>();
        if(null != sprte)
        {
            sprte.spriteName    = "star_big";
        }

        ParticlePlayer effect   = NNPool.GetItem<ParticlePlayer>("clearStarHit");
		effect.Play(trStar.transform.position);
        NNSoundHelper.Play("FX_result_star");
    }

    public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		if(btnReplay == item)
		{
			Scene.ClosePopup(CLEAR_RESULT.REPLAY);
		}
		else if(btnNext == item)
		{
			//Scene.ClosePopup(FAIL_RESULT.REPLAY);
			NNSoundHelper.Play("FX_btn_on");
			Scene.ClosePopup(CLEAR_RESULT.NEXT_LEVEL);
		}
	}

}
