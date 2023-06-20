using UnityEngine;
using System.Collections;

public class visibleObstructionPanel : PanelDefinition {
	public override bool IsSolid (BoardPanel bp) { return true; }
	public override bool IsFallable (BoardPanel bp) { return false; }
	public override bool IsFillable (BoardPanel bp) { return false; }
	public override bool IsMatchable (BoardPanel bp) { return false; }
	public override bool IsStealable (BoardPanel bp) { return false; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsDestroyable (BoardPanel bp) { return false; }
	public override bool IsSplashHitable (BoardPanel bp) { return false; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
    public override bool IsShufflable (BoardPanel bp) { return false; }


    public override string GetImageName (int type) 
	{
		return "rect_rock";
	}
}
