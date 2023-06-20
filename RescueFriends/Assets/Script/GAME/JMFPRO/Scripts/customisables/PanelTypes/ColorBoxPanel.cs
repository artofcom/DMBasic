using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ColorBoxPanel : PanelDefinition {
	protected override void OnPanelDestroy (BoardPanel bp) {

        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY)
            return;

        NNSoundHelper.Play("IFX_soft_bust");

        if(JMFUtils.GM.CurrentLevel.countColorBox > 0)
        {
            JMFUtils.GM.countMatchColorBox++;
            int remainCount     = JMFUtils.GM.CurrentLevel.countColorBox - JMFUtils.GM.countMatchColorBox;
            if(remainCount >= 0)
            {
                JMFUtils.GM.AnimationMissionObjectWithSprite(bp.Owner, remainCount, "ColorBox", _getSpriteImageName(bp.getBoardColor()));
            }    
        }
	}
	
    public override float ShowHitEffect (BoardPanel bp)
    {
        Panel pnl               = bp[BoardPanel.TYPE.BACK];
        if(null == pnl)         return .0f;
        if(0==bp.Durability)    return .0f;
        return (0.1f+pnl.Play(string.Format("colorbox_level{0}_hit", bp.Durability+1), false));
    }

    string _getSpriteImageName(int color)
    {
        string strHead          = "colobox";
        switch(color)
        {
        case 1: strHead         += "R1";  break;
        case 2: strHead         += "Y1";  break;
        case 3: strHead         += "G1";  break;
        case 4: strHead         += "B1";  break;
        case 5: strHead         += "P1";  break;
        case 6: strHead         += "O1";  break;
        case 7: strHead         += "M1";  break;
        case 8: strHead         += "PP1";  break;
        default: return null;
        }
        return strHead;
    }

	public override string GetImageName(int color) 
	{
        string strHead          = "";
        switch(color)
        {
        case 1: strHead         = "R";  break;
        case 2: strHead         = "Y";  break;
        case 3: strHead         = "G";  break;
        case 4: strHead         = "B";  break;
        case 5: strHead         = "P";  break;
        case 6: strHead         = "O";  break;
        case 7: strHead         = "M";  break;
        case 8: strHead         = "V";  break;
        default: return null;
        }
        
        strHead += "_colorbox";
        return strHead;
	}
	
    public override bool IsDamageable(BoardPanel bp, int matchedColor)
    {
        return (matchedColor==bp.getBoardColor());
    }

	public override bool IsSolid (BoardPanel bp) { return true; }
	public override bool IsFallable (BoardPanel bp) { return false; }
	public override bool IsFillable (BoardPanel bp) { return false; }
	public override bool IsMatchable (BoardPanel bp) { return false; }
	public override bool IsStealable (BoardPanel bp) { return false; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsDestroyable (BoardPanel bp) { return true; }
	public override bool IsSplashHitable (BoardPanel bp) { return true; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
    public override bool IsShufflable (BoardPanel bp) { return false; }
}
