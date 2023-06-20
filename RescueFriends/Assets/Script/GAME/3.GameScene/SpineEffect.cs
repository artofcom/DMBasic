using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;

public class SpineEffect : NNRecycler 
{
	protected string[] blockColorName = {"_red", "_yellow", "_green", "_blue", "_purple", "_orange", "_skybule", "_violet", "_rainbow" };

	protected bool bRemove = true;
	protected int index = 0;

	protected virtual void Awake () 
	{
		resetSA();
	}

    public void resetSA()
    {
    }

	public bool IsPlay()
	{
		//if(SA != null && SA.enabled)
		{
		//	TrackEntry trEntry = SA.AnimationState.GetCurrent(0);
		//	if(trEntry != null)
		//		return !trEntry.IsComplete;
		}
		
		return false;
	}
	
	public void Stop(bool complete)
	{
		if(complete)            this.OnComplete(null);
	}
	
	protected virtual void OnComplete (TrackEntry entry)
	{
		if(bRemove)
		{
		///	SA.enabled = false;
			Recycle();
		}
	}

	protected virtual void OnEvent (TrackEntry entry, Spine.Event e)
	{
	}

	public void Reset (Vector3 position, float zPos = -50f) 
	{
		Vector3 newPos = position;
		newPos.z = zPos;
		transform.position = newPos;
		transform.localScale = Vector3.one;
	}

	protected virtual void Recycle () {
		NNPool.Abandon(gameObject);
	}

    public float play(string strAniName, float delay)
    {
        Reset(Vector3.zero);
        return 0.1f;
    }

    public void ChangePicWithSlot(string fileName, string strSlotName) 
	{
	}

}
