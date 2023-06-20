using UnityEngine;
using System.Collections;

public class Potion3Piece : TreasurePiece {
    public override TREASURE_TYPE Type { get { return TREASURE_TYPE.POTION3; } }

    public override bool CanSpawn () {
        return (GM.Potion3CollectCount + Potion3.TotalCount) < JMFUtils.GM.CurrentLevel.countPotion3;
    }
	
	public override string GetImageName (int colorIndex) 
	{	
		return "potion_3";
	}
}
