using UnityEngine;
using System.Collections;
using DG.Tweening;
using Spine;
using Spine.Unity;

// [Score_MISSION]
public class ScoreMission : MonoBehaviour 
{	
    public tk2dTextMesh         _txtCurScore, _txtGoalScore;
    int                         _curScore;

	void Start()
	{}
	
    void OnEnable () 
	{
        JMFRelay.OnChangeScore += OnChangeScore;
        _curScore               = 0;
        _txtGoalScore.text      = JMFUtils.GM.CurrentLevel.goalScore.ToString("N0");
        OnChangeScore( _curScore ); 
    }

    void OnDisable ()
    {
        JMFRelay.OnChangeScore -= OnChangeScore;
        DOTween.Kill(_txtCurScore.gameObject.GetInstanceID());
    }

    void OnChangeScore (long score) 
	{
        int missionScore        = JMFUtils.GM.CurrentLevel.goalScore;
        int goal                = Mathf.Min((int)score, missionScore);

        DOTween.Kill(_txtCurScore.gameObject.GetInstanceID());
        int targetNumber        = _curScore;
        _curScore               = goal;

        float duration          = 0.5f;
        DOTween.To(() => targetNumber, (x) => targetNumber = x, goal, duration).SetEase(Ease.Linear)
            .SetId(_txtCurScore.gameObject.GetInstanceID()).OnUpdate( () => 
            {
                _txtCurScore.text= targetNumber.ToString("N0");
                //tm.Commit();
            }
       );
    }
}


