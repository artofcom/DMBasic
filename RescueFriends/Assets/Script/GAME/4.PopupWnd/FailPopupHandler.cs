using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using NOVNINE;
//using NOVNINE.Diagnostics;

public enum FAIL_RESULT { CLOSE, REPLAY };

public class FailPopupHandler : BasePopupHandler 
{
	public tk2dTextMesh LevelText;
	public tk2dUIItem nextBTN;
	public tk2dUIItem retryBTN;
	
    // param is stage index as int.
	protected override void  OnEnter (object param) 
	{
		if(param == null)
			return;
		
        NNSoundHelper.PlayBGM("PFX_fail_intro", 0, false, false);
		
        LevelText.text          = string.Format("Level : {0}", (int)param + 1);

		base.OnEnter(param);
    }
	
	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		
		if(nextBTN == item)
			Scene.ClosePopup(FAIL_RESULT.CLOSE);
		else if(retryBTN == item)            
		{
            // test
            //== Scene.AddPopup("RefillPopup",true, null,(_param)=>{ EventLock(true);});

			bool bEditorMode = false; 
#if UNITY_EDITOR
			bEditorMode = LevelEditorSceneHandler.EditorMode;//UnityEditor.EditorPrefs.GetBool("JMK_EditorMode");
#endif
            if(bEditorMode == false)// && LGameData.GetInstance().IsInfiniteStamina() == false )
			{
                //if (NOVNINE.Wallet.GetItemCount("life") > 0)
                    Scene.ClosePopup(FAIL_RESULT.REPLAY);
                /*else
                {
                    Scene.AddPopup("RefillPopup", true, null, (_param) =>
                        {
                            if(bReceive)
                            {
                                EventLock(true);
                                if(NOVNINE.Wallet.GetItemCount("life") > 0)
                                    GameReplay();       
                            }
                            else
                            {
                                if (_param != null && (bool)_param == true)
                                    StartCoroutine(coWait());
                                else
                                    EventLock(true);   
                            }
                        });
                }*/
			}
			else
			{
				Scene.ClosePopup(FAIL_RESULT.REPLAY);	
			}
		}
	}

}
