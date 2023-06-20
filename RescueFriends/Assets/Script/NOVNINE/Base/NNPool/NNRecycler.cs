using UnityEngine;
using System.Collections;

public enum RECYCLE_STATE { ABANDONED, PREPARED, OCCUPIED }

public class NNRecycler : MonoBehaviour {
    public RECYCLE_STATE State = RECYCLE_STATE.PREPARED;

    public bool IsPrepared {
        get { return (State == RECYCLE_STATE.PREPARED) && IsRecycleable; }
    }

    public virtual bool IsRecycleable { 
        get { return gameObject.activeSelf == false; } 
    }

    public virtual void Release () {
        gameObject.SetActive(false);
    }

    public virtual void Reset () {
        gameObject.SetActive(true);
    }
}
