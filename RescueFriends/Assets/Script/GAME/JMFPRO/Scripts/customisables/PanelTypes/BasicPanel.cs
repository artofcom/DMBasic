using UnityEngine;
using System.Collections;

public class BasicPanel : PanelDefinition 
{
	public override bool IsSolid (BoardPanel bp) { return false; }
	public override bool IsFallable (BoardPanel bp) { return true; }
	public override bool IsFillable (BoardPanel bp) { return true; }
	public override bool IsMatchable (BoardPanel bp) { return true; }
	public override bool IsStealable (BoardPanel bp) { return true; }
	public override bool IsSwitchable (BoardPanel bp) { return true; }
	public override bool IsDestroyable (BoardPanel bp) { return false; }
	public override bool IsSplashHitable (BoardPanel bp) { return false; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return true; }
    public override bool IsShufflable (BoardPanel bp) { return true; }
}
