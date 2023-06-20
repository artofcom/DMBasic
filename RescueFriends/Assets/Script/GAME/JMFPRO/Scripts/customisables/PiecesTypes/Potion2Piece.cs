using UnityEngine;
using System.Collections;

public class Potion2Piece : TreasurePiece {
    public override TREASURE_TYPE Type { get { return TREASURE_TYPE.POTION2; } }

    public override bool CanSpawn () {
        return (GM.Potion2CollectCount + Potion2.TotalCount) < JMFUtils.GM.CurrentLevel.countPotion2;
    }
	
	public override string GetImageName (int colorIndex) 
	{	
		return "potion_2";
	}
}
