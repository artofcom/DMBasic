/**
* @file UncleBillContext.cs
* @brief
* @author Choi YongWu(amugana@bitmango.com)
* @version 1.0
* @date 2013-09-13
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NOVNINE;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UncleBillContext : MonoBehaviour
{
    bool Complete = false;

    public PlatformContext.StoreType storeType {
		get {
			return Context.NNPlatform.storeType;
		}
	}

    public InventoryItem[] inventoryItems;
    public List<ShopItem> shopItems = new List<ShopItem>();

    //public IngrediantItem[]     _ingredianItemInfos;
    //public IngrediantRecipes[]  _recipes;

    public ShopItem GetShopItemByID(string cashItemUniqueNumber)
    {
        for (int i = 0; i < shopItems.Count; ++i)
        {
            if(shopItems[i].cashItemUniqueNumber == cashItemUniqueNumber) return shopItems[i];
        }

        return null;
    }

    public bool IsComplete()
    {
        return Complete;
    }

    public ShopItem FindShopItemByRewardItemID(string id)
    {
        for (int i = 0; i < shopItems.Count; ++i)
        {
            for (int n = 0; n < shopItems[i].rewards.Count; ++n)
            {
                if(shopItems[i].rewards[n].itemId == id) return shopItems[i];
            }
        }

        return null;
    }

    public ShopItem GetShopItemByPriceID(string id)
    {
        for (int i = 0; i < shopItems.Count; ++i)
        {
            if(shopItems[i].productId == id) return shopItems[i];
        }

        return null;
    }

    public InventoryItem GetInventoryItemByID(string id)
    {
        foreach(InventoryItem item in inventoryItems)
            if(item.id == id) return item;

        return null;
    }
	
	void Awake()
	{
        Context.UncleBill = this;
    }
	
	IEnumerator Start()
	{
		while(!Director.PatchComplete)
        	yield return null;
		
		//buildItemList();
        Debug.Log("UncleBill Prepare To Open started.");
        NOVNINE.Store.UncleBill.PrepareToOpen( (success) =>
        {
            if(success)         Debug.Log("UncleBill Prepare Successed.");
            else                Debug.Log("UncleBill Prepare Failed.");
        });

        Complete = true;
        //Debug.Log("UncleBillContext TIME : " + (Time.realtimeSinceStartup - LManager.TIME));
        //LManager.TIME = Time.realtimeSinceStartup;
	}

    /*public IEnumerator ReStart()
    {
        yield return StartCoroutine(InfoLoader.init());
        //buildItemList();
        NOVNINE.Store.UncleBill.PrepareToOpen(null);
    }
	*/

    void OnApplicationPause(bool pause)
    {
#if !UNITY_EDITOR
        if(pause)
		{
            Wallet.Save();
        }
#endif
    }

    void OnApplicationQuit ()
    {
#if !UNITY_EDITOR
		Wallet.Save();
#endif
    }

    public void AddShopItem(ShopItem item)
    {
        shopItems.Add(item);
    }

#if UNITY_EDITOR

    public void AddInventoryItem(InventoryItem item)
    {
        List<InventoryItem> newarr = new List<InventoryItem>(inventoryItems);
        newarr.Add(item);
        inventoryItems = newarr.ToArray();
    }
#endif

	public string getInventoryItemSpriteNameById(string strItemId)
    {
		NOVNINE.InventoryItem inven = GetInventoryItemByID(strItemId);
		if(inven != null)
			return inven.spriteName;
		
		return null;
    }
}

