using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using NOVNINE;

namespace NOVNINE
{

[System.Serializable]
public class InventoryItem
{
    public string id;
    //public Texture2D icon;
    public string name;
    //public LocaleString description;
    public int count;
    public bool consumable;
    public bool hidden;
    public string spriteName;

    public InventoryItem () {
        this.id = "";
        this.name = "";
        this.count = 0;
        this.consumable = false;
        this.hidden = true;
    }

    public InventoryItem(string _id)
    {
        InventoryItem from = null;
        from = Context.UncleBill.GetInventoryItemByID(_id);
        if(from == null)
		{
            // Debug.LogError("Inventory Item Not Found : "+_id);
            this.id = _id;
            return;
        }
        this.id = from.id;
        //this.icon = from.icon;
        this.name = from.name;
        //this.description = from.description;
        this.consumable = from.consumable;
        this.count = from.count;
        this.hidden = from.hidden;
        this.spriteName = from.spriteName;
        if(from.count < 0)
            this.count = int.MaxValue;
        else
            this.count = from.count;
    }

    public bool IsInfinite
    {
        get {
            return (count == int.MaxValue) || (count < 0);
        }
    }
}

}

