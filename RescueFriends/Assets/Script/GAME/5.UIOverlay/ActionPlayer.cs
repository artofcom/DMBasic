using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ActionPlayer : MonoBehaviour
{
    public bool _isPlayOnStart  = true;
	
    public class AniData
    {
        public enum Type { MOVE, SCALE };

        public Type eType;
        public Vector3 vTo;
        public float fDuratoin;
    };
    public AniData[] _arrDatas  = null;

	void Awake()
	{}

    void Start()
    { }
        
	public void OnEnter()
	{	
	}

    public void OnLeave()
    {

    }
    
}
