using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NOVNINE;
using Spine;
using Spine.Unity;

public class ScoreProgress : MonoBehaviour 
{
    public tk2dTextMesh scoreText;
    public tk2dSpriteProgress progress;
    public tk2dSprite[] stars;

	//public SkeletonAnimation[] starEffect;
	//public SkeletonAnimation mSprIngredient;
	//public SkeletonAnimation mSprIngredientEffect;
	
	//Spine.AnimationState.TrackEntryEventDelegate eventDelegate;
	//Spine.AnimationState.TrackEntryDelegate completeDelegate;
	//Spine.AnimationState.TrackEntryDelegate completeDelegate1;
	
    bool[] achievedScores = new bool[] { false, false, false };
	
	int indexLevel;
	
	void Awake()
	{
		//i//f(starEffect != null)
		//{
		////	completeDelegate = new Spine.AnimationState.TrackEntryDelegate(OnCompleteStar);
		//	completeDelegate1 = new Spine.AnimationState.TrackEntryDelegate(OnCompleteIngredient);
		//	eventDelegate = new Spine.AnimationState.TrackEntryEventDelegate(OnEvent);
		//}
	}
	
	void Start()
	{
		/*if(starEffect != null)
		{
			for(int i = 0; i < starEffect.Length; ++i)
			{
				starEffect[i].AnimationState.Complete += completeDelegate;
				starEffect[i].AnimationState.Event += eventDelegate;
			}			
		}
		
		if(mSprIngredient != null)
		{
			mSprIngredient.AnimationState.Complete += completeDelegate1;
			mSprIngredient.AnimationState.Event += eventDelegate;			
		}8*/
	}
	
	void OnCompleteStar (TrackEntry entry)
	{
		//starEffect[entry.TrackIndex ].gameObject.SetActive(false);
	}
	
	void OnCompleteIngredient (TrackEntry entry)
	{
		// mSprIngredientEffect.gameObject.SetActive(false);
	}
	
	void OnEvent (TrackEntry entry, Spine.Event e)
	{
		if(e.Data.Name == "Star")
		{
			if(e.Int == 2)
				stars[e.Int].spriteName =  "UI_ingame_star_04";
			else
				stars[e.Int].spriteName =  "UI_ingame_star_03";
			
			indexLevel = e.Int + 1;
			PlayIngredientSprite("show", false);
		}
	}
	
    void OnEnable () 
	{
        JMFRelay.OnGameReady += OnGameReady;
        JMFRelay.OnGameStart += OnGameStart;
        JMFRelay.OnChangeScore += OnChangeScore;
        //JMFRelay.OnAchieveStar += OnAchieveStar;
    }

    // public void OnLeaveScene() {
    // fix release bug.
    void OnDisable()
    {
        JMFRelay.OnGameReady -= OnGameReady;
        JMFRelay.OnGameStart -= OnGameStart;
        JMFRelay.OnChangeScore -= OnChangeScore;
        //JMFRelay.OnAchieveStar -= OnAchieveStar;
    }

    void OnGameReady ()
	{
		/*for(int i = 0; i < starEffect.Length; ++i)
		{
			starEffect[i].gameObject.SetActive(false);
			achievedScores[i] = false;
		}			
		
        scoreText.text = "0";
        scoreText.Commit();
		
		progress.max = JMFUtils.GM.CurrentLevel.scoreToReach[2];
		progress.value = 0.0f;
		progress.Commit();
        /*
		progressMask.size = new Vector2(4F, 0.5F);
		progressMask.Build();

		*/
		float L1 = (float)JMFUtils.GM.CurrentLevel.scoreToReach[0] / (float)JMFUtils.GM.CurrentLevel.scoreToReach[2] * 5.16f - 2.7f; // 6.0f - 3.0f;
		float L2 = (float)JMFUtils.GM.CurrentLevel.scoreToReach[1] / (float)JMFUtils.GM.CurrentLevel.scoreToReach[2] * 5.16f - 2.7f;// 6.0f - 3.0f;
		
	//	stars[0].transform.localPosition = new Vector3(L1, 0F, -1F);
	//	stars[1].transform.localPosition = new Vector3(L2, 0F, -1F);
		
	//	for(int i = 0; i < stars.Length -1; i++)
	//		stars[i].spriteName = "UI_ingame_star_01";//"score_star";
		
	//	stars[stars.Length -1].spriteName = "UI_ingame_star_02";//"score_star";

        // level 에 맞게 재료 초기화.
	////	ChangeIngredientSprite(JMFUtils.GM.CurrentLevel.Index, 0);
	//	PlayIngredientSprite("normal", false); 
    }

	public float PlayIngredientSprite(string animationName,bool loop, float delay = 0.0f) 
	{
	/*	if(mSprIngredient != null)
		{
            if(JMF_GAMESTATE.PLAY == JMFUtils.GM.State)
                NNSoundHelper.Play("IFX_star_equip");

			if(animationName == "show")
            {
                // mSprIngredientEffect.gameObject.SetActive(true);
                mSprIngredientEffect.AnimationState.SetAnimation(0, "play", false);
            }
			
			Spine.Animation ani = mSprIngredient.skeleton.Data.FindAnimation(animationName);
			if(ani != null)
			{
				float d = ani.Duration;
				TrackEntry _trackEntry = mSprIngredient.AnimationState.SetAnimation(0, ani, loop);
				_trackEntry.Delay = delay;

				if(loop)
					return -1;
				else
					return d;
			}
		}
        */
		return -2;
	}
	
    void OnGameStart ()
	{
		scoreText.text = "0";
    }

    void OnChangeScore (long score) 
	{
        scoreText.IncreaseNumberWithAnimation((int)score,0.4f);
		//AnimateGain(scoreText.transform,0.4f);

	//	progress.value = score;
	//	progress.Commit();
		
	/*	if(achievedScores[0] == false && score >= JMFUtils.GM.CurrentLevel.scoreToReach[0])
		{            
			achievedScores[0] = true;
			starEffect[0].gameObject.SetActive(true);
			PlayStartEffect(0);
		}
		
		if(achievedScores[1] == false && score >= JMFUtils.GM.CurrentLevel.scoreToReach[1])
		{
			achievedScores[1] = true; 
			starEffect[1].gameObject.SetActive(true);
			PlayStartEffect(1);
		}
		
		if(achievedScores[2] == false && score >= JMFUtils.GM.CurrentLevel.scoreToReach[2] )
		{
			achievedScores[2] = true; 
			starEffect[2].gameObject.SetActive(true);
			PlayStartEffect(2);
		}*/
    }

	Tween AnimateGain (Transform tm, float duration = 0.5f)
    {		
		DOTween.Complete(tm.GetInstanceID());
		float d = duration * 0.7f;
		Sequence seq = DOTween.Sequence();
		seq.Append(tm.DOScale(Vector3.one * 1.2f, d).SetEase(Ease.OutExpo));
		seq.Insert(0.7F, tm.DOScale(Vector3.one, duration - d).SetEase(Ease.InOutExpo));
		seq.SetId(tm.GetInstanceID());
		
		return seq;
    }
	
	public void PlayStartEffect (int index) 
	{
        NNSoundHelper.Play("IFX_star_earning");

	/*	string name = string.Format("star_move0{0}",index +1);
		PathAttachment attachment = starEffect[index].Skeleton.GetAttachment(starEffect[index].Skeleton.Data.FindSlotIndex(name), name) as PathAttachment;
		PathAttachment _pathAttachment = JMFUtils.CreateBezierPointsForSpine(attachment, stars[index].transform.position+Vector3.up*0.5f);

		Slot _slot = starEffect[index].Skeleton.FindSlot(name);
		_slot.Attachment = _pathAttachment;	
		starEffect[index].AnimationState.SetAnimation(index, name, false);*/
	}
}
