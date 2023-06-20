using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using NOVNINE;

namespace NOVNINE
{
	public static class Wallet
	{
		public static event System.Action<string, int> itemChangedCallback;
	    
		public static bool IsValid()
	    {
			return (Root.Data != null && Context.UncleBill != null);
	    }
	
		public static void FireItemChangedEvent(string id,int earned)
	    {
			if(IsValid() && itemChangedCallback != null)
				itemChangedCallback(id,earned);
	    }
	
		public static InventoryItem Get(string id)
		{
			if(IsValid())
				return Context.UncleBill.GetInventoryItemByID(id);
			
			return null;
		}
		
	    public static bool Save()
	    {
			if(IsValid())
			{
				Root.Data.gameData.SaveContext();
				return true;
			}
	        return false;
	    }
		
	    public static int GetItemCount(string id)
	    {
            if (IsValid())
                return Root.Data.gameData.GetCountWalletItemDataByID(id);
            
			return 0;
	    }
		
	    public static void GainGameMoney(int earned)
	    {
			if(IsValid())
			{
				Root.Data.gameData.AddWalletItemDataByID("coin", earned);
				Wallet.FireItemChangedEvent("coin", earned);
			}
	    }
		
	    public static void Gain(ShopItem newItem)
	    {
			if(IsValid())
			{
//				InventoryItem iitem = null;
		
		        foreach(ShopItem.Package pkg in newItem.rewards) 
				{
					Wallet.FireItemChangedEvent(pkg.itemId, pkg.count);
					Root.Data.gameData.AddWalletItemDataByID(pkg.itemId, pkg.count);
					
//		            if(!pkg.IsCoin) 
//					{
//		                iitem = pkg.GetItem();
//		                if(iitem == null) 
//						{
//		                    iitem = new InventoryItem(pkg.itemId);
//							Root.Data.gameData.AddWalletInventoryDataByID(pkg.itemId, 1);
//		                }
//						
//		                if(pkg.IsInfinite) 
//						{
//		                    iitem.count = pkg.count;
//		                }
//						else
//						{
//		                    if(!iitem.IsInfinite)
//		                        iitem.count += pkg.count;
//		                }
//		
//		                
//		            }
//					else
//					{
//		                
//		                Wallet.FireCoinEvent(pkg.count);
//		            }
		        }
			}
	    }
			
        public static void Gain(string inventoryItemId, int count, bool bAutoSave = true)
	    {
			if(IsValid())
                Root.Data.gameData.AddWalletItemDataByID(inventoryItemId, count,bAutoSave);
	    }
		
        public static void unlockItem(string inventoryItemId)
        {
            if(IsValid())
				Root.Data.gameData.unlockItem(inventoryItemId);
        }

		public static int ChangeItemUniqueNumberByID(string ID)
		{
            if(null==ID || ID=="")  return (int)Data.ITEM_TYPE.NONE;

			Data.ITEM_TYPE itemType = (Data.ITEM_TYPE)System.Enum.Parse(typeof(Data.ITEM_TYPE), ID.ToUpper());
			return (int)itemType;
		}
		
//		public static string ChangeIDByItemUniqueNumber(int num)
//		{
//			switch((Data.ITEM_TYPE)num)
//			{
//				case Data.ITEM_TYPE.COIN: return "coin";
//				case Data.ITEM_TYPE.LIFE: return "life";
//				case Data.ITEM_TYPE.HAMMER: return "hammer";
//				case Data.ITEM_TYPE.FIRECRACKER: return "firecracker";
//				case Data.ITEM_TYPE.MAGICSWAP: return "magicswap";
//				case Data.ITEM_TYPE.RAINBOWBUST: return "rainbowbust";
//				case Data.ITEM_TYPE.MIXEDBOOSTER: return "mixedbooster";
//				case Data.ITEM_TYPE.TRIPLEBOOSTER: return "triplebooster";
//				case Data.ITEM_TYPE.SPECIALBOOSTER: return "specialbooster";
//				case Data.ITEM_TYPE.ING_SW01: return "Ing_Sw01";
//				case Data.ITEM_TYPE.ING_SW02: return "Ing_Sw02";
//				case Data.ITEM_TYPE.ING_SW03: return "Ing_Sw03";
//				case Data.ITEM_TYPE.ING_SW04: return "Ing_Sw04";
//				case Data.ITEM_TYPE.ING_FR01: return "Ing_Fr01";
//				case Data.ITEM_TYPE.ING_FR02: return "Ing_Fr02";
//				case Data.ITEM_TYPE.ING_FR03: return "Ing_Fr03";
//				case Data.ITEM_TYPE.ING_FR04: return "Ing_Fr04";
//				case Data.ITEM_TYPE.ING_SO01: return "Ing_So01";
//				case Data.ITEM_TYPE.ING_SO02: return "Ing_So02";
//				case Data.ITEM_TYPE.ING_SO03: return "Ing_So03";
//				case Data.ITEM_TYPE.ING_SO04: return "Ing_So04";
//				case Data.ITEM_TYPE.ING_FA01: return "Ing_Fa01";
//				case Data.ITEM_TYPE.ING_FA02: return "Ing_Fa02";
//				case Data.ITEM_TYPE.ING_FA03: return "Ing_Fa03";
//				case Data.ITEM_TYPE.ING_FA04: return "Ing_Fa04";
//			}
//			return null;
//		}

		public static bool Use(string id, int count = -1, bool bAutoSave = true)
	    {
#if UNITY_EDITOR
            if( LevelEditorSceneHandler.EditorMode )
                return true;
#endif
            if(IsValid())
			{
				//if(iitem.IsInfinite)
				//   return true;
//				InventoryItem iitem = Get(id);
//				if(iitem == null) 
//				{
//
//					IngrediantItem ing = getIngrediant(id);
//					ing.count -= count;
//					ing.count           = Mathf.Max(0, ing.count);
//					// fire ???
//					return true;
//				}

                if(Root.Data.gameData.GetCountWalletItemDataByID(id) > 0)
				{
                    Root.Data.gameData.AddWalletItemDataByID(id, count, bAutoSave);
                    Wallet.FireItemChangedEvent(id, Root.Data.gameData.GetCountWalletItemDataByID(id));	

                    Debug.Log(string.Format("Wallet Earned !!! : {0} : {1}", id, count));

					return true;
				}
			}

	        return false;
	    }
		
//	    public static bool hasNonConsumable
//	    {
//	        get {
//	            if(current != null)
//	                return current.hasNonConsumable;
//	            return false;
//	        }
//	    }
	}

}
