using UnityEngine;
using System.Collections;

public class Potion1Piece : TreasurePiece {
    public override TREASURE_TYPE Type { get { return TREASURE_TYPE.POTION1; } }

    public override bool CanSpawn () {
        return (GM.Potion1CollectCount + Potion1.TotalCount) < JMFUtils.GM.CurrentLevel.countPotion1;
    }
	
	public override string GetImageName (int colorIndex) 
	{	
		return "potion_1";
	}
}
