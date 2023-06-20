using UnityEngine;
using System.Collections;
using NOVNINE;
using Data;
using Spine.Unity;
using Spine;

public class ItemButton : MonoBehaviour 
{
    public string               _strId;
    public tk2dTextMesh         itemCount;
    public tk2dSprite           _sprSelected;
    
    bool isOn                   = false;

    
    //public void EventLock(bool isLock)
    //{
    //    transform.GetComponent<BoxCollider2D>().enabled = isLock;
    //    tk2dSprite[] arrSprites = transform.GetComponentsInChildren<tk2dSprite>();
    //}

    public bool IsOn
	{
		get{ return isOn;}
		set{						
			if(isOn != value)
			{
                isOn            = value;
                _sprSelected.gameObject.SetActive( isOn );
			}
		}
	}
    
    public void AddItemCount(int num)
    {
      /*  bool isOpened = false;
        string fileName = null;

        switch (itemType) 
        {
            case GAME_ITEM.HAMMER:
                //isOpened = (Root.Data.gameData.record.playerData.HammerUnlocked || _WAYNE_TESTOR._UNLOCK_ITEM);
                fileName = "btn_hammer";
                break;
            case GAME_ITEM.FIRECRACKER:
                //isOpened = (Root.Data.gameData.record.playerData.FirecrackerUnlocked || _WAYNE_TESTOR._UNLOCK_ITEM);
                fileName = "btn_firecracker";
                break;
            case GAME_ITEM.MAGICSWAP:
               // isOpened = (Root.Data.gameData.record.playerData.MagicSwapUnlocked || _WAYNE_TESTOR._UNLOCK_ITEM);
                fileName = "btn_magicswap";
                break;
            case GAME_ITEM.RAINBOWBUST:
               // isOpened = (Root.Data.gameData.record.playerData.RainbowBustUnlocked || _WAYNE_TESTOR._UNLOCK_ITEM);
                fileName = "btn_rainbowbust";
                break;
        }*/
        /*
        button.enabled = isOpened;

        itemCount.gameObject.SetActive(num > 0);

        if(isOpened)
        {
            if (num > 0) 
            {
                countBG.SetSprite("btn_number");
                itemCount.text = num.ToString();
                itemCount.Commit();
            }
            else 
            {
                countBG.SetSprite("btn_plus");
            }   
        }
        else
        {
            fileName += "_dimd";
            countBG.SetSprite("icon_lock");
            itemCount.gameObject.SetActive(false);
        }

        Icon.SetSprite(fileName);*/
    }
	
	public void UpdateItem() 
	{
        int count               = Wallet.GetItemCount( _strId );
        itemCount.text          = count>0 ? count.ToString() : "+";
        _sprSelected.gameObject.SetActive( isOn );
	}
	
}
