using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class TimeOrMove : MonoBehaviour 
{
    const float WARNING_TIME_AMOUNT = 10f;
    const int WARNING_MOVE_AMOUNT = 5;

    //public tk2dTextMesh title;
    public tk2dTextMesh         txtTime;
    public tk2dTextMesh         txtMoveCount;
    public tk2dTextMesh         txtLevelId;
    public tk2dSprite           sprInfinity;
    public tk2dSlicedSprite     sprScoreBar;
    public tk2dSprite           sprStar1, sprStar2, sprStar3;
    public ParticleSystem       effWarnning;
    public GameObject           _objBear;

    int _oldTime                = 0;

    void OnEnable () 
	{
        JMFRelay.OnGameReady += OnGameReady;
        JMFRelay.OnChangeRemainMove += OnChangeRemainMove;
        JMFRelay.OnChangeRemainTime += OnChangeRemainTime;
        JMFRelay.OnChangeScore += OnChangeScore;

        sprStar1.spriteName     = "star_1";
        sprStar2.spriteName     = "star_2";
        sprStar3.spriteName     = "star_2";
        _objBear.SetActive( false );

        txtMoveCount.transform.parent.gameObject.SetActive( !JMFUtils.GM.CurrentLevel.isTimerGame );
        txtTime.transform.parent.gameObject.SetActive( JMFUtils.GM.CurrentLevel.isTimerGame );
    }

    void OnDisable () 
	{
        JMFRelay.OnGameReady -= OnGameReady;
        JMFRelay.OnChangeRemainMove -= OnChangeRemainMove;
        JMFRelay.OnChangeRemainTime -= OnChangeRemainTime;
        JMFRelay.OnChangeScore -= OnChangeScore;
    }

    void OnGameReady ()
    {
        int fullScore           = JMFUtils.GM.CurrentLevel.scoreToReach[2];
        if(fullScore > 0)
        {
            float fMidRate      = (float)JMFUtils.GM.CurrentLevel.scoreToReach[1] / fullScore;
            Vector3 vMidPos     = Vector3.Lerp(sprStar1.transform.localPosition, sprStar3.transform.localPosition, fMidRate);
            sprStar2.transform.localPosition = vMidPos;
        }
        else 
            Debug.Log("Error : score setting is not valid !!!");
     /*
      *  temp
         Transform trTopBand     = transform.FindChild("Topband");
        Debug.Assert(null != trTopBand);
        trTopBand.gameObject.SetActive(true);

        if (JMFUtils.GM.CurrentLevel.isMaxMovesGame) {
            title.text = "MOVE";
            textMesh.text = JMFUtils.GM.CurrentLevel.allowedMoves.ToString();

            // only valid when allow move count > 0.
            trTopBand.gameObject.SetActive( JMFUtils.GM.CurrentLevel.allowedMoves > 0 );

        } else if (JMFUtils.GM.CurrentLevel.isTimerGame) {
            title.text = "TIME";
            textMesh.text = ((int)JMFUtils.GM.CurrentLevel.givenTime).ToString();
        } else {
            Debug.Log("Error : Unknown Type Game.");
        }
        title.Commit();

        textMesh.Commit();*/

        txtMoveCount.transform.parent.gameObject.SetActive( !JMFUtils.GM.CurrentLevel.isTimerGame );
        txtTime.transform.parent.gameObject.SetActive( JMFUtils.GM.CurrentLevel.isTimerGame );
        if(JMFUtils.GM.CurrentLevel.isTimerGame)
        {
            txtTime.text            = ((int)JMFUtils.GM.CurrentLevel.givenTime).ToString();
            txtTime.Commit();
        }
        else
        {
            txtMoveCount.gameObject.SetActive( JMFUtils.GM.CurrentLevel.allowedMoves>0 );
            sprInfinity.gameObject.SetActive( JMFUtils.GM.CurrentLevel.allowedMoves<=0 );
            if(true == txtMoveCount.gameObject.activeSelf)
            {
                txtMoveCount.text   = JMFUtils.GM.CurrentLevel.allowedMoves.ToString();
                txtMoveCount.Commit();
            }
            txtMoveCount.transform.localScale  = Vector3.one;
            DOTween.Complete(txtMoveCount.gameObject.GetInstanceID() + 1);
        }

        txtLevelId.text         = string.Format("Level {0}", JMFUtils.GM.CurrentLevel.Index+1);
        txtLevelId.Commit();

        effWarnning.gameObject.SetActive(false);

        OnChangeScore(0);
    }

    void OnChangeScore (long score) 
	{
        if(null==JMFUtils.GM.CurrentLevel.scoreToReach || JMFUtils.GM.CurrentLevel.scoreToReach.Length<3)
            return;

        int fullScore           = JMFUtils.GM.CurrentLevel.scoreToReach[2];
        float fRate             = (float)score / (float)fullScore;
        fRate                   = Mathf.Min(1.0f, fRate);

        // 80 ~ 980.
        fRate *= 900.0f;
        fRate += 80.0f;
        sprScoreBar.dimensions = new Vector2(fRate, sprScoreBar.dimensions.y);

        if(score>=JMFUtils.GM.CurrentLevel.scoreToReach[1] && sprStar2.spriteName!="star_1")
        {
            sprStar2.spriteName= "star_1";
            BlockCrash effect= NNPool.GetItem<BlockCrash>("NormalPieceCrash");
            effect.Play("play", sprStar2.transform.position, Vector3.one, 1);
        }
        else if(score >= JMFUtils.GM.CurrentLevel.scoreToReach[2] && sprStar3.spriteName!="star_1")
        {
            sprStar3.spriteName= "star_1";
            BlockCrash effect= NNPool.GetItem<BlockCrash>("NormalPieceCrash");
            effect.Play("play", sprStar3.transform.position, Vector3.one, 1);
        }

    }

    void OnChangeRemainTime (float remainTime)
    {
        txtTime.text            = Mathf.Max(0, (int)remainTime).ToString();
        txtTime.Commit();

        if(JMF_GAMESTATE.PLAY==JMFUtils.GM.State && _oldTime<remainTime)
        {
            // do some effects.
            Transform trAddTime = NNPool.GetItem<Transform>("PlusFiveSec");
            trAddTime.parent    = txtTime.transform.parent;
            trAddTime.transform.localPosition    = new Vector3(txtTime.transform.localPosition.x, txtTime.transform.localPosition.y, txtTime.transform.localPosition.z-1.0f);
            trAddTime.DOLocalMoveY(1.1f, 0.5f);//.SetEase(Ease.Linear);
            StartCoroutine( _coOnFinishEff(0.5f, trAddTime.gameObject) );
        }
//
//        if ((remainTime > 0f) && (remainTime <= WARNING_TIME_AMOUNT)) {
//            StartWarningEffect();
//            if (NNSoundHelper.IsPlaying("time_danger") == false) {
//                NNSoundHelper.Play("time_danger", true);
//            }
//        } else {
//            StopWarningEffect();
//            NNSoundHelper.Stop("time_danger");
//        }
        _oldTime                = (int)remainTime;
    }

    IEnumerator _coOnFinishEff(float time, GameObject obj)
    {
        yield return new WaitForSeconds(time);

        NNPool.Abandon(obj);
    }

    void OnChangeRemainMove (int remainMove) {

        // [AI_MISSION]
        if(JMFUtils.GM.isAIFightMode)
            return;

        txtMoveCount.text   = Mathf.Max(0, remainMove).ToString();
        txtMoveCount.Commit();

        bool playEff        = remainMove<=5 && JMF_GAMESTATE.PLAY==JMFUtils.GM.State;
        effWarnning.gameObject.SetActive(playEff);
        _objBear.SetActive( playEff );
        if(playEff)
        {
            TweenParams parms = new TweenParams();
            parms.SetLoops(-1, LoopType.Yoyo);
            parms.SetId(txtMoveCount.gameObject.GetInstanceID() + 1);
            txtMoveCount.transform.DOScale(1.1f, 0.5f);
        }
        else
        {
            txtMoveCount.transform.localScale  = Vector3.one;
            DOTween.Complete(txtMoveCount.gameObject.GetInstanceID() + 1);
        }
        //if(playEff)         

        /*
        delete all scene effects.
        warningMessage.transform.localPosition = new Vector3(0F, 0F, -30F);
        if ((remainMove > 0) && (remainMove == WARNING_MOVE_AMOUNT) && JMFUtils.GM.AllowedMoves >= 20 && JMFUtils.GM.State == JMF_GAMESTATE.PLAY) {
            warningMessage.SetActive(true);
            Vector3 targetPos = warningMessage.transform.localPosition;
            targetPos.x -= 15f;
            Vector3 exitPos = warningMessage.transform.localPosition;
            exitPos.x -= 15f;
//            Sequence seq = new Sequence(new SequenceParms().OnComplete(() => { 
//                warningMessage.SetActive(false);
//                StartWarningEffect();
//            }));
//            TweenParms parms = new TweenParms().Prop("localPosition", targetPos).Ease(EaseType.Linear);
//            TweenParms parms2 = new TweenParms().Prop("localPosition", exitPos).Ease(EaseType.Linear);
//            seq.Append(HOTween.From(warningMessage.transform, 0.5f, parms));
//            seq.Insert(2f, HOTween.To(warningMessage.transform, 1f, parms2));
			Sequence seq = DOTween.Sequence();
			seq.OnComplete(() => { 
				warningMessage.SetActive(false);
				StartWarningEffect();
			});
			TweenParams parms = new TweenParams();
			parms.SetEase(Ease.Linear);
			TweenParams parms2 = new TweenParams();
			parms2.SetEase(Ease.Linear);
			
			seq.Append( warningMessage.transform.DOLocalMove(targetPos, 0.5f).SetAs(parms).From());
			seq.Insert(2f, warningMessage.transform.DOLocalMove(exitPos, 1.0f).SetAs(parms2));
            seq.Play();
        } else {
            if ((remainMove > 0) && (remainMove <= WARNING_MOVE_AMOUNT)) {
                StartWarningEffect();
            } else {
                StopWarningEffect();
            }
        }
        */
        AnimateGain(txtMoveCount.gameObject);
    }

    void StartWarningEffect ()
    {
        ParticleSystem p = txtMoveCount.transform.GetComponentInChildren<ParticleSystem>();
        if (p.isPlaying == true) return;

        p.Play();
    }

   	Tweener AnimateGain (GameObject go)
    {
        return null;
//        HOTween.Complete(go.GetInstanceID());
//        TweenParms parms = new TweenParms().Prop("localScale", (Vector3.one * 1.2f))
//                                           .IntId(go.GetInstanceID())
//                                           .Loops(2, LoopType.Yoyo)
//                                           .Ease(EaseType.EaseInOutQuad);
//        return HOTween.To(go.transform, .14f, parms);
		
		DOTween.Complete(go.GetInstanceID());
		TweenParams parms = new TweenParams();
		parms.SetId(go.GetInstanceID());
		parms.SetEase(Ease.InOutQuad);
		parms.SetLoops(2, LoopType.Yoyo);
        parms.SetRelative(true);
		return go.transform.DOScale(Vector3.one * 1.2f, 0.14f);
    }
}
