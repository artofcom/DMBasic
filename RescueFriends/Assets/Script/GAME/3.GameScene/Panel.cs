using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;
using DG.Tweening;
using System.Collections.Generic;

public class Panel : NNRecycler 
{
    const float LAUNCHER_FLY_SPEED  = 9.0f;
    public PanelTracker PT { get; private set; }
    public BoxCollider BC { get; private set; }

	//protected SkeletonAnimation SA = null;
	//Spine.AnimationState.TrackEntryDelegate completeDelegate;
	//Spine.AnimationState.TrackEntryEventDelegate eventDelegate;
    //public SkeletonAnimation getSA() { return SA; }

    public Transform _dlgGauge  = null;
    SpriteRenderer              _sprRenderer    = null;
    public Sprite[]             _sprSources     = null;

    public tk2dTextMesh         _txtNumber  = null;

	protected virtual void Awake ()
	{
        PT = GetComponent<PanelTracker>();
        if (PT == null) PT = gameObject.AddComponent<PanelTracker>();

        BC = GetComponent<BoxCollider>();
        if (BC == null) 
		{
            BC = gameObject.AddComponent<BoxCollider>();
            BC.center = new Vector3(0F, 0F, 10F);
        }
		
        _sprRenderer            = GetComponent<SpriteRenderer>();
        if(null==_sprRenderer)  _sprRenderer = gameObject.AddComponent<SpriteRenderer>();

		///SA = GetComponent<SkeletonAnimation>();
		//if(SA != null)
		{
			//completeDelegate = new Spine.AnimationState.TrackEntryDelegate(OnComplete);
		//	eventDelegate = new Spine.AnimationState.TrackEntryEventDelegate(OnEvent);
			//SA.AnimationState.Complete += completeDelegate;
		//	SA.AnimationState.Event += eventDelegate;
			//SA.GetComponent<Renderer>().sortingOrder = 0;
		}
		
        Init();
    }

    protected virtual void Init () { }

	protected virtual void OnEvent (TrackEntry entry, Spine.Event e)
	{
        /*
		if(e.Data.Name == "change")
		{
			int slotIndex = SA.Skeleton.FindSlotIndex(e.String);
			Slot _slot = SA.Skeleton.FindSlot(e.String);
			Slot _slot1 = SA.Skeleton.FindSlot("panal_normal");
			_slot.Attachment = SA.Skeleton.GetAttachment(slotIndex, _slot1.Attachment.Name);	
			return;
		}
		
		if(e.Data.Name == "random")
		{
			Bone _bone = SA.Skeleton.FindBone(e.String);
			_bone.Rotation = UnityEngine.Random.Range(0, 18) * 20.0f;
			return;
		}*/
	}
	
    public override void Reset () 
	{
        base.Reset();
        if (PT != null) PT.enabled = true;
        if (BC != null) BC.enabled = true;
		//if(SA != null)
		//	SA.GetComponent<Renderer>().sortingOrder = 0;
    }

    void OnDisable ()
    {
        StopAllCoroutines();
        transform.DOKill();
    }
	
	public override void Release () 
	{
        base.Release();
        if (PT != null) PT.enabled = false;
        if (BC != null) BC.enabled = false;
    }
	
    public virtual void UpdatePanel (object _info) { }
	
	public float Play(string animationName,bool loop, float delay = 0.0f) 
	{
		/*if(SA != null)
		{
			Spine.Animation ani = SA.skeleton.Data.FindAnimation(animationName);
			if(ani != null)
			{
				float d = ani.Duration;
				TrackEntry _trackEntry = SA.AnimationState.SetAnimation(0, animationName, loop);
				_trackEntry.Delay = delay;

				if(loop)
					return -1;
				else
					return d;
			}
			//SA.AnimationState.AddAnimation(0, "idle", true, .0f);
		}
        */
		return -2;
	}
	
	public virtual void ChangePanel( string fileName) 
	{
	    for(int g = 0; g < _sprSources.Length; ++g)
        {
            if(fileName == _sprSources[g].name)
            {
                _sprRenderer.sprite = _sprSources[g];
                break;
            }
        }
	}

    public virtual void ChangePanelWithSlot(string fileName, string strSlotName) 
	{
	}



    int mCountWaitingAction     = 0;
    #region // JAM Gen Processor. - Egg
    // 후에 상속받아 처리 할 것.    
    public IEnumerator coProcessJamGen(BoardPanel bdPanel)
    {
        if(null==bdPanel)       yield break;
        if(JMFUtils.GM.State != JMF_GAMESTATE.PLAY)
            yield break;

        const int MaxLife       = 3;
        const int TargetCount   = 3;

        bdPanel.IsOnSkilling    = true;

        // do jump 
        Vector3 vOrgPos         = transform.position;

        // => 바로 처리하는 것으로 변경 !!!
        {
            //transform.DOMoveY(0.16f, 0.2f).SetRelative(true).SetEase(Ease.InBack).SetLoops(-1, LoopType.Yoyo);
            //yield return new WaitForSeconds(0.3f);
            // wait till stable.
            // yield return StartCoroutine( JMFUtils.GM.coWaitTillStable(0.3f) );
        }

        // => 대상 검색.        
        int cntTry              = 0;
        List<Board> listTargets = new List<Board>();
        while(true)
        {
            _searchTargetBoard(ref listTargets, 1);
            if(listTargets.Count >= TargetCount) break;
            else                                 yield return null;
            ++cntTry;
            if(cntTry >= 1000)  break;
        }

        // 확장 연출.
        transform.DOKill();
        transform.position      = vOrgPos;
        Vector3 orgScale        = transform.localScale;
        //transform.DOScale( orgScale*1.4f, 0.3f ).SetEase(Ease.InOutBack);
        
        ++JMFUtils.GM.mCntMakingSpecialPiece;

        // fire.
        float duration          = 0.8f;
        int arrSize             = Mathf.Min(TargetCount, listTargets.Count);
        int idx                 = 0;
        Board[] arrDone         = new Board[TargetCount];
        for(int q = 0; q < arrSize; ++q)
        {
            Board bdTarget      = listTargets[ Random.Range(0, listTargets.Count) ];
            bool all_ok         = true;
            for (int z = 0; z < TargetCount; ++z)
            {
                if(arrDone[z] == bdTarget)
                {
                    all_ok      = false;
                    --q;
                    break;
                }
            }
            if(false == all_ok)
                continue;

            arrDone[idx++]      = bdTarget;

            const float deepz   = -100.0f;     
            Vector3 vStart      = new Vector3(bdPanel.Owner.Position.x, bdPanel.Owner.Position.y, deepz);
            BlockCrash effect   = NNPool.GetItem<BlockCrash>("ObstacleBgstoneHit");
            effect.Play("play", vStart, Vector3.one, 0);

            SpriteRenderer SR   = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
            SR.sprite           = JMFUtils.POH.collectAnyMission.getObjectSkinSprite("Strawberry");
            SR.transform.position   = vStart;
            SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;

            Vector3 v2          = new Vector3(bdTarget.Position.x, bdTarget.Position.y, deepz);
            Vector3 v1          = vStart + (v2 - vStart) * 0.5f - Vector3.left * 2.0f;
            SR.gameObject.transform.position   = vStart;
            SR.gameObject.transform.DOPath(new Vector3[]{ v1, v2 }, duration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(Ease.Linear).OnComplete( () =>
            {
                SR.transform.DOKill();
                NNPool.Abandon( SR.gameObject );
            });
            SR.gameObject.transform.DORotate(new Vector3(.0f, .0f, 180.0f), 0.1f).SetLoops(-1, LoopType.Incremental);

			//Tail effectTail     = NNPool.GetItem<Tail>("TailEffect");
            //const int idxVilolet= 7;
			//float duration      = effectTail.Play("move", bp.Owner.Position, bdTarget.Position, idxVilolet, false);
            //
            // ===> bubble로 연출 변경
            /*
            const float deepz   = -100.0f;            
            BubbleParticle  eff = NNPool.GetItem<BubbleParticle>("BubbleParticle");
            eff.play("bubble_violet");            
            Vector3 vStart      = new Vector3(bdPanel.Owner.Position.x, bdPanel.Owner.Position.y, deepz);
            Vector3 v2          = new Vector3(bdTarget.Position.x, bdTarget.Position.y, deepz);
            Vector3 v1          = vStart + (v2 - vStart) * 0.5f - Vector3.left * 2.0f;
            eff.gameObject.transform.position   = vStart;
            eff.gameObject.transform.DOPath(new Vector3[]{ v1, v2 }, duration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(Ease.OutBack); //.SetEase(_gainObjectCurve);// Ease.InSine);//.OutCubic);
            NNSoundHelper.Play("IFX_bubble_earning");
            */
            //eff.gameObject.transform.DOMove(v2, duration).SetEase(Ease.OutBack);
            //
            //
            //bdTarget.ChangePiece(0.1f, bdTarget.Position,bdTarget.Piece.Scale, 0, null);
            StartCoroutine( _coChangeToJam(bdTarget, duration, null));// eff) );
            mCountWaitingAction++;
        }

        //yield return new WaitForSeconds(0.6f);
        //transform.localScale    = orgScale;

        while(mCountWaitingAction > 0)
            yield return null;

        transform.localScale    = orgScale;

        //
        // note : Invalidate() 가 호출되면서 이 panel은 abandon 된다. 따라서 coroutine이 죽지 않게 care 잘하자.
        bdPanel.Durability      = MaxLife - 1;
        bdPanel.Invalidate();
        bdPanel.IsOnSkilling    = false;
        --JMFUtils.GM.mCntMakingSpecialPiece;
    }

    IEnumerator _coChangeToJam(Board bdTarget, float duration, NNRecycler rec)
    {
        if(null==bdTarget)      yield break;
        yield return new WaitForSeconds(duration);

        Vector3 vStart          = new Vector3(bdTarget.Position.x, bdTarget.Position.y, -100.0f);
        BlockCrash effect       = NNPool.GetItem<BlockCrash>("ObstacleBgstoneHit");
        effect.Play("play", vStart, Vector3.one, 0);

        if(null!= rec)          NNPool.Abandon( rec.gameObject );
        bdTarget.eShadeType     = (int)LEItem.SHADE_TYPE.JAM;
        bdTarget.ShadedDurability= 0;
        --mCountWaitingAction;
    }
    #endregion


    #region // ZELLATO Gen Processor. - ice cream.
    public IEnumerator coProcessZellatoGen(BoardPanel bdPanel)
    {
        if(null==bdPanel)       yield break;
        if(JMFUtils.GM.State != JMF_GAMESTATE.PLAY)
            yield break;

        const int MaxLife       = 3;
        const int TargetCount   = 3;

        bdPanel.IsOnSkilling    = true;

        // do jump 
        Vector3 vOrgPos         = transform.position;

        #region // 바로 처리 하는 것으로 변경 !
        {
            //transform.DOMoveY(0.16f, 0.2f).SetRelative(true).SetEase(Ease.InBack).SetLoops(-1, LoopType.Yoyo);
            //yield return new WaitForSeconds(0.3f);
            //BlockCrash effect   = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
            //const int idxYellow = 2;
		    //effect.Play("play", transform.position, Vector3.zero, idxYellow);//, false);

            // wait till stable.
            // yield return StartCoroutine( JMFUtils.GM.coWaitTillStable(0.1f) );
        }
        #endregion

        // burst delay.
        yield return new WaitForSeconds(0.2f);

        // => 대상 검색.        
        int cntTry              = 0;
        List<Board> listTargets = new List<Board>();
        while(true)
        {
            _searchTargetBoard(ref listTargets, 2);
            if(listTargets.Count >= TargetCount) break;
            else                                 yield return null;
            ++cntTry;
            if(cntTry >= 1000)  break;
        }
        
        // 확장 연출.
        transform.DOKill();
        transform.position      = vOrgPos;
        Vector3 orgScale        = transform.localScale;
        //transform.DOScale( orgScale*1.4f, 0.3f ).SetEase(Ease.InOutBack);
 
        ++JMFUtils.GM.mCntMakingSpecialPiece;

        // 이때 패널 이미지도 살짝 full 짜리로 교체.
        const string strMapName = "GelatoboxL4";
        ChangePanel( strMapName );
        //

        // fire.
        float duration          = 0.45f;
        int arrSize             = Mathf.Min(TargetCount, listTargets.Count);
        int idx                 = 0;
        Board[] arrDone         = new Board[TargetCount];
        for(int q = 0; q < arrSize; ++q)
        {
            Board bdTarget      = listTargets[ Random.Range(0, listTargets.Count) ];
            bool all_ok         = true;
            for (int z = 0; z < TargetCount; ++z)
            {
                if(arrDone[z] == bdTarget)
                {
                    all_ok      = false;
                    --q;
                    break;
                }
            }
            if(false == all_ok)
                continue;

            arrDone[idx++]      = bdTarget;
            const float deepz   = -100.0f;     
			//Tail effectTail     = NNPool.GetItem<Tail>("TailEffect");
			//float duration      = effectTail.Play("move", bdPanel.Owner.Position, bdTarget.Position, 0, false);

            Vector3 vStart      = new Vector3(bdPanel.Owner.Position.x, bdPanel.Owner.Position.y, deepz);
            BlockCrash effect   = NNPool.GetItem<BlockCrash>("ObstacleBgstoneHit");
            effect.Play("play", vStart, Vector3.one, 0);

            SpriteRenderer SR   = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
            SR.sprite           = JMFUtils.POH.collectAnyMission.getObjectSkinSprite("Zellato");
            SR.transform.position   = vStart;
            SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;

            Vector3 v2          = new Vector3(bdTarget.Position.x, bdTarget.Position.y, deepz);
            Vector3 v1          = vStart + (v2 - vStart) * 0.5f - Vector3.left * 2.0f;
            SR.gameObject.transform.position   = vStart;
            SR.gameObject.transform.DOPath(new Vector3[]{ v1, v2 }, duration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(Ease.Linear).OnComplete( () =>
            {
                SR.transform.DOKill();
                NNPool.Abandon( SR.gameObject );
            });
            SR.gameObject.transform.DORotate(new Vector3(.0f, .0f, 180.0f), 0.1f).SetLoops(-1, LoopType.Incremental);

   /*         SkeletonRenderer SR = NNPool.GetItem<SkeletonRenderer>("GainedMissionItem");
		    int slotIndex       = SR.Skeleton.FindSlotIndex("RegionList");
		    Slot _slot          = SR.Skeleton.FindSlot("RegionList");
		    _slot.Attachment    = SR.Skeleton.GetAttachment(slotIndex, "Gelato");
		    SR.transform.position= bdPanel.Owner.Position;            
		    JMFUtils.SpineObjectAutoScalePadded(SR.gameObject);
            SR.transform.localScale = Vector3.one*0.9f;
	        SR.GetComponent<MeshRenderer>().sortingOrder    = 11;
            //
            Vector3 vStart      = new Vector3(bdPanel.Owner.Position.x, bdPanel.Owner.Position.y, deepz);
            Vector3 v2          = new Vector3(bdTarget.Position.x, bdTarget.Position.y, deepz);
            Vector3 v1          = vStart + (v2 - vStart) * 0.5f - Vector3.left * 2.0f;
            SR.gameObject.transform.position   = vStart;
            SR.gameObject.transform.DOPath(new Vector3[]{ v1, v2 }, duration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(Ease.Linear);//.OutBack); //.SetEase(_gainObjectCurve);// Ease.InSine);//.OutCubic);
            //
            */
            //NNSoundHelper.Play("IFX_bubble_earning");

            //SpriteRenderer SR   = NNPool.GetItem<SpriteRenderer>("GainedMissionItem");
            //JMFUtils.GM.makingSpecialPiece   = true;
            bdTarget.ChangePiece(duration-0.05f, bdTarget.Position,bdTarget.Piece.Scale, 0, null);
            StartCoroutine( _coChangeToZellato(bdTarget, duration, SR.GetComponent<NNRecycler>()) );
            mCountWaitingAction++;
        }

        while(mCountWaitingAction > 0)
            yield return null;

        transform.localScale    = orgScale;

        // roll-back init stat.
        //
        // note : Invalidate() 가 호출되면서 이 panel은 abandon 된다. 따라서 coroutine이 죽지 않게 care 잘하자.
        bdPanel.Durability      = MaxLife - 1;
        bdPanel.Invalidate();
        bdPanel.IsOnSkilling    = false;
        --JMFUtils.GM.mCntMakingSpecialPiece;
    }

    IEnumerator _coChangeToZellato(Board bdTarget, float duration, NNRecycler rec)
    {
        if(null==bdTarget)          yield break;
        yield return new WaitForSeconds(duration);

        if(null!= rec)              NNPool.Abandon( rec.gameObject );
        
        Vector3 vStart      = new Vector3(bdTarget.Position.x, bdTarget.Position.y, -100.0f);
        BlockCrash effect   = NNPool.GetItem<BlockCrash>("ObstacleBgstoneHit");
        effect.Play("play", vStart, Vector3.one, 0);

        bdTarget.ResetPiece( JMFUtils.GM.PieceTypes[23], JMFUtils.GM.PieceTypes[23].GetColorIndex());
        AnimationNormalToSpecial(bdTarget);
        --mCountWaitingAction;
    }
    void AnimationNormalToSpecial(Board bd)
	{
        Vector3 startScale = bd.Piece.GO.transform.localScale;
		Sequence seq = DOTween.Sequence();
		//seq.OnComplete(ResetMakingSpecialPieceStatus);

		seq.Append(bd.Piece.GO.transform.DOScale( startScale * 1.2F,0.1F).SetEase(Ease.Linear));
		seq.Append(bd.Piece.GO.transform.DOScale( startScale,0.1F).SetEase(Ease.Linear));
		
        seq.Play();
    }
    // searchType ; 1 - jam, 2 - zellato
    void _searchTargetBoard(ref List<Board> listTargets, int searchType)
    {
        const int cntCollectGoal    = 3;
        List<Board> listCandidates = new List<Board>();
        listTargets.Clear();
        foreach (Board _bd in JMFUtils.GM.Boards)
        {
			if(_bd.PD is NormalPiece == false)
                continue;
            if(_bd.PND is BasicPanel == false)
                continue;
            if(false==_bd.IsStable || true==_bd.IsNeedDropCheck || true==_bd.IsNeedMatchCheck)
                continue;
            if(_bd._changeColorIndex>=0 || true==_bd._isTreasureGoal)
                continue;
            if(1==searchType && (int)LEItem.SHADE_TYPE.NONE!=_bd.eShadeType && _bd.ShadedDurability>=0)
            {
                listCandidates.Add( _bd );
                continue;
            }

            listTargets.Add( _bd );
        }

        if(listTargets.Count < cntCollectGoal)
            listTargets.AddRange( listCandidates );
    }
    #endregion


    float _fireBlocker(Board bdTarget, string strMap, PanelDefinition pd, bool deletePiece)
    {
        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
        SR.sprite               = JMFUtils.POH.collectAnyMission.getObjectSkinSprite( strMap );
        SR.transform.position   = transform.position;
        SR.transform.localScale = Vector3.one * 0.5f;
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;

        BubbleHit effect        = NNPool.GetItem<BubbleHit>("BubbleHit");
        Vector3 pos             = transform.position;
        effect.Play(pos, Vector3.one, 1, false, 0.45f);

        float duration          = JMFUtils.tween_move(SR.transform, transform.position, bdTarget.Position, LAUNCHER_FLY_SPEED);
        SR.transform.DOScale(1.0f, duration);
       
        StartCoroutine( _coFinalizeFire(duration, SR.gameObject, bdTarget, pd, deletePiece) );

        return duration;
    }


    public float fireIce(Board bdTarget)
    {
        return _fireBlocker(bdTarget, "OverbubbleL1", JMFUtils.GM.GetPanelType<FrostPanel>(), false);
    }

    public float fireStone(Board bdTarget)
    {
        return _fireBlocker(bdTarget, "obstacle_bubble_01", JMFUtils.GM.GetPanelType<BubblePanel>(), true);
    }

    public float fireWeb(Board bdTarget)
    {
        return _fireBlocker(bdTarget, "wire_cage1", JMFUtils.GM.GetPanelType<CagePanel>(), false);
    }

    IEnumerator _coFinalizeFire(float delay, GameObject objFly, Board bd, PanelDefinition pd, bool removePiece=false)
    {
        yield return new WaitForSeconds(delay);

        if(removePiece)         bd.RemovePiece();
        objFly.transform.localScale = Vector3.one;
        NNPool.Abandon( objFly );
        bd.ResetPanel(pd, 0);

        BubbleHit effect        = NNPool.GetItem<BubbleHit>("BubbleHit");
        effect.Play(bd.Position, Vector3.one, 1, false, 0.45f);
    }

    public void enableNumber(bool enable)
    {
        if(null != _txtNumber)
            _txtNumber.gameObject.SetActive( enable );
    }
    public void refreshNumber(int number)
    {
        if(_txtNumber.gameObject.activeSelf)
            _txtNumber.text = number.ToString();
    }

    public void enableGauge(bool enable)
    {
        if(null != _dlgGauge)
            _dlgGauge.gameObject.SetActive(enable);
    }

    public void refreshGauge(float value, float fMax)
    {
        if(null==_dlgGauge || !gameObject.activeSelf || !_dlgGauge.gameObject.activeSelf)
            return;

        tk2dSlicedSprite sprGauge   = _dlgGauge.Find("sprGauge").GetComponent<tk2dSlicedSprite>();
        if(null!=sprGauge && fMax>0)
        {
            // 70 ~ 550
            float fDiff             = 480.0f;   // 550-70;
            float fvalue            = fDiff * (value/fMax);
            sprGauge.dimensions     = new Vector2(70.0f + fvalue, sprGauge.dimensions.y);
        }
    }
}
