using UnityEngine;
using System.Collections;
using Spine;
//using Holoville.HOTween;
//using DG.Tweening;
//using NOVNINE.Diagnostics;

public class BlockAddTime : Block {
    
    public static int TotalCount { get; private set; }

    void OnEnable () {
        //JMFRelay.OnPlayerMove += OnPlayerMove;
        TotalCount++;
    }

    void OnDisable () {
        //JMFRelay.OnPlayerMove -= OnPlayerMove;
        TotalCount--;
    }
	
	public override void ChangeBlockColor(int index, int type) 
	{
	}
}
