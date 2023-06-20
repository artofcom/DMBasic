using UnityEngine;
using System.Collections;
using DG.Tweening;
using Spine;
using Spine.Unity;

// [Score_MISSION]
public class ScoreMissionPopup : MonoBehaviour 
{	
    public tk2dTextMesh         _txtGoalScore;

	void Start()
	{}
	
    public void refreshUI(int targetScore)
    {
        _txtGoalScore.text      = targetScore.ToString("N0");
    }
}


