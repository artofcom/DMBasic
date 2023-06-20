using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public abstract class PieceDefinition : MonoBehaviour {
    public int createScore;
	public int destroyScore;
	public bool isFallable;
	public bool isMatchable;
	public bool ignoreShuffle;
	public bool isDestroyable;
	public bool isSlidableSide;
	public bool isSplashHitable;
	public bool hasMatchCondition;
	public bool hasPerformPower;
    public bool hasSoftChewing  = true;
	public GameObject[] skin;

    public float effectTime = 0.05F;

	public GameManager GM { get { return JMFUtils.GM; } }
    public string ClassName { get { return GetType().Name; } }

    public void FireOnHit (GamePiece gp) {
        OnHit(gp);
    }

	public void FireOnSplashHit (GamePiece gp) {
        OnSplashHit(gp);
    }

	public void FireOnPieceClick (GamePiece gp) {
	    OnPieceClick(gp);
    }

	public void FireOnPieceCreate (GamePiece gp) {
        OnPieceCreate(gp);
    }

    public int FireOnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        if(true == JMFUtils.GM._isRemovedByConveyor)
            return 0;
        int score = OnPieceDestroy(gp, isByMatch, colorIndex);
        JMFRelay.FireOnPieceDestroy(gp);
        return score;
    }

    public void FireOnPieceDestroyed (Board bd, int prevColor) {
        OnPieceDestroyed(bd, prevColor);
    }

	public virtual string GetImageName (int colorIndex) 
	{
		return null;
	}
	
	public GameObject GetMold (int colorIndex) {
        if(colorIndex>=0 && colorIndex<skin.Length)
            return skin[colorIndex]; 
        return null;
    }

    protected virtual void OnHit (GamePiece gp) {
    }

	protected virtual void OnSplashHit (GamePiece gp) {
    }

	protected virtual void OnPieceClick (GamePiece gp) {
    }

	protected virtual void OnPieceCreate (GamePiece gp) {
    }

    protected virtual int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        return 0;
    }

    protected virtual void OnPieceDestroyed (Board bd, int prevColor) {
    }

	public virtual int GetColorIndex () { 
        int colorIndex = GM.GetRandomColorIndex(); 
        return Mathf.Clamp(colorIndex, 0, skin.Length - 1);
    }

    public virtual float ShowDestroyEffect (GamePiece gp) { 
        return 0F; 
    }

    public virtual bool IsChanceOfSpawn () { 
        return false; 
    }

	public virtual PieceDefinition GetSpawnableDefinition (Point pt) { 
        return null; 
    }

	public virtual void PerformPower (GamePiece gp) {
    }

	public virtual bool IsCombinable (GamePiece other)
	{ 
        return false; 
    }

    public virtual float PerformCombinationPower (Board from, Board to, GamePiece gp, GamePiece other) { 
        return 0F;
    }

	public virtual bool Match (ref Board bd, List<Board> linkX, List<Board> linkY, out int score) { 
        score = 0;
        return false; 
    }

	public virtual bool IsSwitchableHorizontal (GamePiece gp)
    {
        if(null==gp || gp.LifeCover>0)
            return false;
        return true;
    }

	public virtual bool IsSwitchableVertical (GamePiece gp)
    {
        if(null==gp || gp.LifeCover>0)
            return false;
        return true;
    }

    protected Board FindFrogBoard (Board bd, List<Board> linkX, List<Board> linkY) {
        if (bd.PD is FrogPiece) return bd;

        for (int i = 0; i < linkX.Count; i++) {
            if (linkX[i].PD is FrogPiece) return linkX[i];
        }

        for (int i = 0; i < linkY.Count; i++) {
            if (linkY[i].PD is FrogPiece) return linkY[i];
        }

        return null;
    }

    protected bool SwapFrogPiece (ref Board bd, List<Board> linkX, List<Board> linkY) {
        if ((bd.PD is FrogPiece) == false) return false;

        if (SearchAndSwap(ref bd, linkX)) return true;
        if (SearchAndSwap(ref bd, linkY)) return true;

        return false;
    }

    protected bool SwapMysteryPiece (ref Board bd, List<Board> linkX, List<Board> linkY) { 
        if ((bd.PD is MysteryPiece) == false) return false;

        if (SearchAndSwap(ref bd, linkX)) return true;
        if (SearchAndSwap(ref bd, linkY)) return true;

        return false;
    }

    protected bool SearchAndSwap (ref Board bd, List<Board> link) {
        for (int i = 0; i < link.Count; i++) {
            if ((link[i].PD is NormalPiece) == false) continue;
            Board tmp = bd;
            bd = link[i];
            link[i] = tmp;
            return true;
        }

        return false;
    }

    protected void FeedToFrog (Board frog, Board bd, List<Board> link) {
        List<Board> bds = new List<Board>(link);
        bds.Add(bd);

        for (int i = 0; i < bds.Count; i++) {
            if (frog == bds[i]) continue;
            if (bds[i].IsFilled == false) continue;

            frog.Piece.GO.transform.Translate(0F, 0F, -2F);
            GameObject go = NNPool.GetItem(bds[i].Piece.GO.name, GM.gameObject.transform);
            go.transform.position = bds[i].Piece.GO.transform.position;
            go.transform.Translate(0F, 0F, -1F);
            go.transform.localScale = bds[i].Piece.GO.transform.localScale;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(bds[i].PD.effectTime);
            seq.Append(go.transform.DOMove(frog.Position, 0.3F));
            seq.InsertCallback(bds[i].PD.effectTime + 0.2F, () => { 
                frog.Piece.GO.GetComponent<Frog>().ShowEatAnimation(); 
            }); 

            seq.OnComplete(() => { 
                frog.Piece.GO.transform.Translate(0F, 0F, 2F);
                NNPool.Abandon(go); 
            }); 
        }   
    }   

    protected bool IsCageOrLockedPanel(List<Board> linkX, List<Board> linkY) {
        foreach (Board bd in linkX) {
            if (bd.Panel == null) continue;
            if ((bd.Panel.PND is CagePanel) || (bd.Panel.PND is LockedPanel)) return true;
        }

        foreach (Board bd in linkY) {
            if (bd.Panel == null) continue;
            if ((bd.Panel.PND is CagePanel) || (bd.Panel.PND is LockedPanel)) return true;
        }
        return false;
    }

    protected bool IsMergeEffect(Board bd) {
        if (IsSpecialPiece(bd)) return true;
        if (bd.Piece.PD is PenguinPiece) return true;
        if (bd.Piece.PD is FairyPiece) return true;
        if (bd.Piece.PD is FrogPiece) return true;
        if (bd.Panel.PND is CagePanel) return true;
        if (bd.Panel.PND is LockedPanel) return true;
        return false;
    }

    // [2X2_BURST]
    protected bool IsSpecialPiece(Board bd) {
        if (bd.Piece.PD is SpecialFive) return true;
        if (bd.Piece.PD is BombPiece) return true;
        if (bd.Piece.PD is VerticalPiece) return true;
        if (bd.Piece.PD is HorizontalPiece) return true;
        if (bd.Piece.PD is TMatch7Piece) return true;
        return false;
    }

    virtual public float getBurstTime()
    {
        return effectTime;
    }


    // [2X2_BURST]
    virtual public void set2x2BoardInfo(Board boardCornor) { }

    virtual public float bonusHit(Board bdSelf)
    {
        if(null != bdSelf)      bdSelf.Hit(false, .0f);
        return 0.2f;
    }

    virtual public string getDropSndName()
    {
        return "";
    }
    virtual public bool isShowDestroyAnimation()
    {
        return true;
    }

    // override this function by color changer.
    virtual public bool canColorChange(Board bd)
    {
        return false;
    }
    virtual public IEnumerator changePieceColorAsChanger(Board bd, float delay)
    {
        yield break;
    }
    //
    virtual public bool isLevelMissionTarget(GamePiece gp)
    {
        return false;
    }
}
