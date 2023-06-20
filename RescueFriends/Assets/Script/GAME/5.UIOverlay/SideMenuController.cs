using UnityEngine;
using System.Collections;
using DG.Tweening;

public interface IControlBase
{
	void OnEnter();
    void OnLeave();
    void EventLock( bool Lock );
	void CLICK( tk2dUIItem item ) ;
}

public class SideMenuController : MonoBehaviour 
{
	public Transform SideMenu;
	public GameObject[] controls;
	public tk2dUIItem backButton;
	
    public bool IsMoving = false;
//	SettingControl settingControl;
//	FriendsControl friendsControl;
//	MessageControl messageControl;
//	AchievementControl achievementControl;
    float MoveX = -10.0f;

	tk2dBaseSprite back;
	void Awake()
	{
//		settingControl = this.GetComponentInChildren<SettingControl>(true);
//		friendsControl = this.GetComponentInChildren<FriendsControl>(true);
//		messageControl = this.GetComponentInChildren<MessageControl>(true);
//		achievementControl = this.GetComponentInChildren<AchievementControl>(true);
		for(int i = 0; i < controls.Length; ++i)
		{
			controls[i].SetActive(false);
		}
		
		if(backButton != null)
			back = backButton.GetComponent<tk2dBaseSprite>();
	}
        
    public bool IsShowControl()
    {
        for(int i = 0; i < controls.Length; ++i)
        {
            if (controls[i].activeSelf)
                return true;
        }

        return false;
    }

	void ShowControl(int index)
	{
		for(uint i = 0; i < controls.Length; ++i)
		{
			IControlBase control = controls[i].GetComponent<IControlBase>();
			
			if(index == i)
			{
				controls[i].SetActive(true);
				control.EventLock(true);
				control.OnEnter();
			}
			else
			{
                control.OnLeave();
				controls[i].SetActive(false);
				control.EventLock(false);
			}
		}
	}
	
	public void SlideSideMenuController(bool show,int index = 0)
	{
		controls[index].GetComponent<IControlBase>().EventLock(false);
		NNSoundHelper.Play("PFX_slide");

		if(show)
		{
			if(gameObject.activeSelf)
			{
				ShowControl(index);
                IsMoving = false;
			}
			else
			{
                IsMoving = true;
				controls[index].GetComponent<IControlBase>().OnEnter();
				backButton.enabled = true;
				backButton.gameObject.SetActive(true);
				Sequence seq = DOTween.Sequence();
				gameObject.SetActive(show);
				
                seq.Append(SideMenu.DOLocalMoveX(MoveX, 0.5f));
				
				Color col = back.color;
				col.a = 1.0f;
				seq.Join(DOTween.To(() => back.color, x => back.color = x, col,0.5f));
				
				seq.SetEase(DG.Tweening.Ease.OutBounce);
				controls[index].SetActive(show);
				seq.AppendCallback(()=>{
					controls[index].GetComponent<IControlBase>().EventLock(true);
                    IsMoving = false;
                    //Scene.Unlock();
				});		
			}
		}
		else
		{
			if(gameObject.activeSelf)
			{
				Sequence seq = DOTween.Sequence();
				seq.Append(SideMenu.DOLocalMoveX(-1f, 0.5f));
				
				Color col = back.color;
				col.a = 0.0f;
				seq.Join(DOTween.To(() => back.color, x => back.color = x, col,0.5f));
                IsMoving = true;
				seq.SetEase(DG.Tweening.Ease.OutBounce);
                seq.AppendCallback(()=>{ 
                    controls[index].GetComponent<IControlBase>().OnLeave();
                    this.gameObject.SetActive(false); HideAll();
                    backButton.gameObject.SetActive(false);
                    backButton.enabled = false; 
                    IsMoving = false;
                });
			}
		}
	}
	
	void HideAll()
	{
        IsMoving = false;
		for(int i = 0; i < controls.Length; ++i)
		{
			controls[i].gameObject.SetActive(false);
		}
	}
}
