using UnityEngine;
using System.Collections;
//using Holoville.HOTween;
using DG.Tweening;
using Spine;
using Spine.Unity;

// [AI_MISSION]
public class AIMission : MonoBehaviour 
{
	//255 59 212 pink
	//35 221 49 green
	
	public SkeletonRenderer Envy;
	public tk2dTextMesh EnvyCount;
	public tk2dBaseSprite EnvyBG;
	public SkeletonRenderer Penelope;
	public tk2dTextMesh PenelopeCount;
	public tk2dBaseSprite PenelopeBG;
	public tk2dBaseSprite BG;
	public tk2dBaseSprite icon;
	public tk2dBaseSprite iconGolw;
    public tk2dTextMesh textMesh;

	void Awake()
	{
		if(Envy != null)
			Envy.Initialize(true);
		if(Penelope != null)
			Penelope.Initialize(true);
		
		icon.gameObject.SetActive(false);
		iconGolw.gameObject.SetActive(false);
	}
	
	void Start()
	{
		if(Envy != null)
		{
			int slotIndex = Envy.Skeleton.FindSlotIndex("ai_board_enemies");
			Slot _slot = Envy.Skeleton.FindSlot("ai_board_enemies");
			_slot.Attachment = Envy.Skeleton.GetAttachment(slotIndex, "ai_board_enemies");
		}
		
		if(Penelope != null)
		{
			int slotIndex = Penelope.Skeleton.FindSlotIndex("ai_board_enemies");
			Slot _slot = Penelope.Skeleton.FindSlot("ai_board_enemies");
			_slot.Attachment = Penelope.Skeleton.GetAttachment(slotIndex, "ai_board_mine");
		}
	}
	
    void OnEnable () 
	{
		EnvyBG.gameObject.SetActive(true);
		PenelopeBG.gameObject.SetActive(true);
		icon.gameObject.SetActive(true);
		//iconGolw.gameObject.SetActive(true);
		BG.color = Color.gray;
		
        textMesh.gameObject.SetActive(true);
		Debug.Assert(JMFUtils.GM.CurrentLevel.countAiWinBoard > 0, "countAiWinBoard null");
        textMesh.text           = JMFUtils.GM.CurrentLevel.countAiWinBoard.ToString();
        textMesh.Commit();

        EnvyCount.gameObject.SetActive(true);
        EnvyCount.text          = JMFUtils.GM.CountEnemyWinBoard.ToString();
        EnvyCount.Commit();

        PenelopeCount.gameObject.SetActive(true);
        PenelopeCount.text      = JMFUtils.GM.CountMyWinBoard.ToString();
        PenelopeCount.Commit();
		

        JMFRelay.OnPieceDestroy += OnPieceDestroy;
        JMFRelay.OnChangeAITurn += OnChangeAiTurn;
        JMFRelay.OnUpdateAiScoreUI += OnUpdateAiScoreUI;
        Debug.Log("### OnPieceDestroy Linked by AIMission. ");
        // SetBossIcon();
    }

    // 
    void OnChangeAiTurn(bool myTurn)
    {
        if(myTurn)
        {
            DOTween.To(()=>EnvyBG.color, x=>EnvyBG.color=x, new Color(1, 1, 1, 0), 0.2f);
            DOTween.To(()=>PenelopeBG.color, x=>PenelopeBG.color=x, Color.white, 0.2f);
        }
        else
        {
            DOTween.To(()=>PenelopeBG.color, x=>PenelopeBG.color=x, new Color(1, 1, 1, 0), 0.2f);
            DOTween.To(()=>EnvyBG.color, x=>EnvyBG.color=x, Color.white, 0.2f);
        }
    }

    void OnDisable ()
    {
        JMFRelay.OnPieceDestroy -= OnPieceDestroy;
        JMFRelay.OnChangeAITurn -= OnChangeAiTurn;
        JMFRelay.OnUpdateAiScoreUI -= OnUpdateAiScoreUI;

        Debug.Log("<<< OnPieceDestroy Released by AIMission. ");

        if(EnvyBG != null && EnvyBG.gameObject != null)
            EnvyBG.gameObject.SetActive(false);
        if(PenelopeBG != null && PenelopeBG.gameObject != null)
		    PenelopeBG.gameObject.SetActive(false);
    }

    void OnUpdateAiScoreUI()
    {
        if(false == JMFUtils.GM.isAIFightMode)
            return;

        EnvyCount.text          = JMFUtils.GM.CountEnemyWinBoard.ToString();
        PenelopeCount.text      = JMFUtils.GM.CountMyWinBoard.ToString();
    }

    void OnPieceDestroy(GamePiece piece)
    {
        if(false == JMFUtils.GM.isAIFightMode)
            return;

        if(JMF_GAMESTATE.BONUS==JMFUtils.GM.State || JMF_GAMESTATE.OVER==JMFUtils.GM.State || 
            JMF_GAMESTATE.FINAL==JMFUtils.GM.State )
            return;

        if(null!=piece && null!=piece.Owner)
        {
            Board.AI_SIDE eSide     = JMFUtils.GM.isCurPlayerAI ? Board.AI_SIDE.ENEMIES : Board.AI_SIDE.MINE;
            //if(eSide != piece.Owner.AiSide)            
                // piece.Owner.AiSide  = eSide; => moved GamePiece.cs.
                                
            EnvyCount.text      = JMFUtils.GM.CountEnemyWinBoard.ToString();
            PenelopeCount.text  = JMFUtils.GM.CountMyWinBoard.ToString();

            // 상대가 5회 미만 남았으면 ani trigger.
            //JMFUtils.GM.checkWorriedAction(SPINE_CHAR.AI_GIRL);
            //JMFUtils.GM.checkWorriedAction(SPINE_CHAR.MAIN_GIRL);
        }
    }

    /*void OnChangeBossHealth (int health, int damage) {
        string healthInfo = string.Format("{0} / {1}", JMFUtils.GM.BossHealth, JMFUtils.GM.CurrentLevel.bossHealth);
        textMesh.text = healthInfo;
        textMesh.Commit();
        AnimateGain(textMesh.gameObject);

        if (health <= 0) {
            if (JMFUtils.GM.State == JMF_GAMESTATE.PENDING) return;
            ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("ConvertToSpecial_02");
            pp.transform.position = textMesh.gameObject.transform.position;
            pp.Play();
            textMesh.gameObject.SetActive(false);
        }
    }

	Tweener AnimateGain (GameObject go) {
        //HOTween.Complete(go.GetInstanceID());
		DOTween.Complete(go.GetInstanceID());
//        TweenParms parms = new TweenParms().Prop("localScale", (Vector3.one * 1.5f))
//                                           .IntId(go.GetInstanceID())
//                                           .Loops(2, LoopType.Yoyo)
//                                           .Ease(EaseType.EaseInOutQuad);
//        return HOTween.To(go.transform, .2f, parms);
		
		TweenParams parms = new TweenParams();
		parms.SetId(go.GetInstanceID());
		parms.SetLoops(2, LoopType.Yoyo);
		parms.SetEase(Ease.InOutQuad);
		return go.transform.DOScale(Vector3.one * 1.5f, 0.2f);
	}*/
}


