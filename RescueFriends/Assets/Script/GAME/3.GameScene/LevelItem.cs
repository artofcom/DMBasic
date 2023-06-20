using UnityEngine;
using System.Collections;
using Data;
using NOVNINE.Diagnostics;
using NOVNINE;
using Spine.Unity;
using Spine;

public class LevelItem : NNRecycler 
{
    public tk2dTextMesh number;
 	public static System.Action<int> onClickLevel;

    SkeletonRenderer SpineMap;
	int levelIndex = -1;
    //BoxCollider boxCollider;
	string[] Symbol = new string[] { "Symbol_collect", "Symbol_fill", "Symbol_defeat", "Symbol_find", "Symbol_clear", "Symbol_score" };
	
    public int getIndexLevel()
    {
        return levelIndex;
    }

	
    //    void Awake () 
    //	{
    //        boxCollider = GetComponent<BoxCollider>();
    //    }

    // forceTurnOff - 특수한경우에 강제로 icon off 한다.
    public void UpdateLevelStatus(bool forceTurnOff=false) 
	{
        Data.Level level = Root.Data.GetLevel(levelIndex);
        Debug.Assert(level != null);

		Slot _slot = SpineMap.Skeleton.FindSlot(string.Format("Level{0}",levelIndex));
		int slotIndex = SpineMap.Skeleton.FindSlotIndex(string.Format("Level{0}",levelIndex));
		string color;
		bool cleared = Root.Data.gameData.GetClearLevelByIndex(levelIndex) && !forceTurnOff;
		int grade = Root.Data.gameData.GetGradeLevelByIndex(levelIndex);

        if (cleared)
			color = "7F0000FF";
		else
		{
			if(Root.Data.currentLevel.Index==levelIndex && false==forceTurnOff)
				color = "F2FFBAFF";
			else
				color = "3017A2FF";
		}
		
		_slot.R = JMFUtils.ToColor(color, 0);
		_slot.G = JMFUtils.ToColor(color, 1);
		_slot.B = JMFUtils.ToColor(color, 2);
		
        _slot       = SpineMap.Skeleton.FindSlot(string.Format("Node{0}", levelIndex));
	    slotIndex   = SpineMap.Skeleton.FindSlotIndex(string.Format("Node{0}", levelIndex));
        if (level.hardLevel)
        {
            _slot.Attachment = SpineMap.Skeleton.GetAttachment(slotIndex, "hard_level_stage_icon");
        }
        else
        {
            if(cleared)
            {   
                if(grade == 3)
                    _slot.Attachment = SpineMap.Skeleton.GetAttachment(slotIndex, "Node_Inactive_Base1");
                else
                    _slot.Attachment = SpineMap.Skeleton.GetAttachment(slotIndex, "Node_Inactive_Base");
            }
            else 
                _slot.Attachment = SpineMap.Skeleton.GetAttachment(slotIndex, "Node_Base");    
        }
		
		_slot = SpineMap.Skeleton.FindSlot(string.Format("Stars{0}",levelIndex));
		slotIndex = SpineMap.Skeleton.FindSlotIndex(string.Format("Stars{0}", levelIndex));
		_slot.Attachment = SpineMap.Skeleton.GetAttachment(slotIndex, string.Format("Stars{0}", grade));
		
        if (!level.hardLevel)
        {
            if(Root.Data.currentLevel.Index == levelIndex && false==forceTurnOff)
            {
                _slot = SpineMap.Skeleton.FindSlot(string.Format("Node{0}",levelIndex));
                slotIndex = SpineMap.Skeleton.FindSlotIndex(string.Format("Node{0}",levelIndex));
                _slot.Attachment = SpineMap.Skeleton.GetAttachment(slotIndex, "Node_Current_Base");
            }    
        }
		
		//Profile.IsEnableCheat
	}
	
	public void UpdateItem (int _levelIndex) 
	{
		Debugger.Assert(SpineMap != null);
	
        levelIndex = _levelIndex;
		Data.Level level = Root.Data.GetLevelFromIndex(levelIndex);		
		Slot _slot = SpineMap.Skeleton.FindSlot(string.Format("Level{0}",levelIndex));
		int slotIndex = SpineMap.Skeleton.FindSlotIndex(string.Format("Level{0}",levelIndex));
		
		if(level.missionType > -1 && level.missionType < Symbol.Length)
			_slot.Attachment = SpineMap.Skeleton.GetAttachment(slotIndex, Symbol[level.missionType]);	
		
		int lvid = level.Index + 1;
        number.text = lvid.ToString();

        UpdateLevelStatus();
    }
	
	public void SetSpineMap(SkeletonRenderer spineMap)
	{
		SpineMap = spineMap;
	}

    void OnClick (tk2dUIItem item) 
	{
		if (onClickLevel != null) onClickLevel(levelIndex);
    }

    public override void Release () 
	{
        base.Release();
        onClickLevel = null;
    }
}
