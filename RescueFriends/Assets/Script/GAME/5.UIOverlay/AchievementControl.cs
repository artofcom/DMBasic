using UnityEngine;
using System.Collections;

public class AchievementControl : MonoBehaviour,IControlBase
{
	public GameObject onDLG;
	public GameObject offDLG;
	
	void Awake()
	{
		bool active = false;
#if UNITY_ANDROID
		active = true;
#endif
		
		offDLG.SetActive(active);
		offDLG.SetActive(!active);
		
	}
        
	public void OnEnter()
	{	
	}

    public void OnLeave()
    {

    }
        
	public void EventLock(bool Lock)
	{
	}

	public void CLICK (tk2dUIItem item) 
	{
		NNSoundHelper.Play("FX_btn_on");

	}
}
