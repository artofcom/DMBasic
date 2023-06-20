using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using Spine.Unity;
using Spine;

public enum PAUSE_RESULT { RESUME, QUIT, RESTART };

public class PausePopupHandler : BasePopupHandler
{   
	//readonly string[] BGMList = {"bgm_mission_collect","bgm_mission_fill","bgm_mission_defeat","bgm_mission_find", "bgm_mission_clear"};
	
	public tk2dUIItem _btnQuit;
	public tk2dUIItem _btnResume;
	public tk2dUIItem _btnRestart;

	protected override void  OnEnter (object param) 
	{
		base.OnEnter(param);
	}
	
	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		
		if(_btnQuit == item)
			Scene.ClosePopup(PAUSE_RESULT.QUIT);
		else if(_btnResume == item)
            Scene.ClosePopup(PAUSE_RESULT.RESUME);
        else if(_btnRestart == item)
            Scene.ClosePopup(PAUSE_RESULT.RESTART);
   
	}
}
