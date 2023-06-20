using UnityEngine;
using System.Collections;
using JsonFx.Json;
using NOVNINE.Diagnostics;
using System.Collections.Generic;
using Spine.Unity;
using Spine;

public class ColorBombPanel : PanelDefinition {
    public static bool HitThisTurn { get; set; }
    public static int StraightNotHitCount { get; set; }

    public class Info {
        public int Index { get; set; }
    }

	public override bool IsSolid (BoardPanel bp) { return true; }
	public override bool IsFallable (BoardPanel bp) { return false; }
	public override bool IsFillable (BoardPanel bp) { return false; }
	public override bool IsMatchable (BoardPanel bp) { return false; }
	public override bool IsStealable (BoardPanel bp) { return false; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsDestroyable (BoardPanel bp) { return false; }
	public override bool IsSplashHitable (BoardPanel bp) { return true; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
    public override bool IsShufflable (BoardPanel bp) { return false; }

    public override object ConvertToPanelInfo (string data) {
        Debugger.Assert(string.IsNullOrEmpty(data) == false);
        return JsonReader.Deserialize<Info>(data);
    }

	protected override void OnHit (BoardPanel bp, bool isSpecialAttack)
    {
        // color로 판단 필요.

        ColorBombSpritePanel    spritePanel    = (bp[BoardPanel.TYPE.BACK] as ColorBombSpritePanel);
        if(null == spritePanel) return;

        bool ret                = spritePanel.damage((LEItem.COLOR)bp.getDamagingColor());

        if(false == ret)        return;

      /*  ColorBombSpritePanel    pnlHolder = spritePanel.getDataHolder();
        if(null == pnlHolder)   return;
        string strEffName       = ColorBombSpritePanel.TYPE._5==pnlHolder.getType() ? "ColorBomb2x2_5" : "ColorBomb2x2_6";
        SpineEffect effect      = NNPool.GetItem<SpineEffect>( strEffName );

        string strSlotName      = "cakebomb";
        if(ColorBombSpritePanel.TYPE._5==pnlHolder.getType())
            strSlotName += "_5x5";
        effect.resetSA();
        effect.ChangePicWithSlot("none", strSlotName );
        Vector3 pos             = pnlHolder.getParentBoard().LocalPosition + new Vector3(JMFUtils.GM.Size*0.5f, JMFUtils.GM.Size*0.5f, pnlHolder.transform.localPosition.z-1.0f);
        string strColor         = "";
        switch((LEItem.COLOR)bp.getDamagingColor())
        {
        case LEItem.COLOR.RED:      strColor = "R";     break;
        case LEItem.COLOR.YELLOW:   strColor = "Y";     break;
        case LEItem.COLOR.GREEN:    strColor = "G";     break;
        case LEItem.COLOR.BLUE:     strColor = "B";     break;
        case LEItem.COLOR.VIOLET:   strColor = "V";     break;
        case LEItem.COLOR.ORANGE:   strColor = "O";     break;
        default:                    return;
        }
        float fD                = effect.play( strColor+"_enter", .0f );
        effect.transform.parent = JMFUtils.GM.transform;
        effect.transform.localPosition = pos;        
        effect.transform.localScale = pnlHolder.transform.localScale;
        */
        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) {
//            BossHitEffect effect = NNPool.GetItem<BossHitEffect>("BossHitEffect");
//            effect.transform.position = bp.Owner.Position;
//            effect.ShowHitEffect(damage);
        }
	}

    public bool IsExplodable(BoardPanel bp)
    {
        ColorBombSpritePanel    spritePanel    = (bp[BoardPanel.TYPE.BACK] as ColorBombSpritePanel);
        if(null == spritePanel) return false;

        return spritePanel.getDataHolder().isExplodable();
    }

    public List<Board> getOtherColorBombPanel(BoardPanel bp)
    {
        ColorBombSpritePanel    spritePanel    = (bp[BoardPanel.TYPE.BACK] as ColorBombSpritePanel);
        if(null == spritePanel) return null;
        if(spritePanel.getDataHolder() != spritePanel)
            return null;

        List<Board> listOut     = new List<Board>();
        listOut.Add( bp.Owner.Right );
        listOut.Add( bp.Owner.Top );
        listOut.Add( bp.Owner.Top.Right );

        return listOut;
    }

    // info를 통해 map name 재정의 됨.
    public override string GetImageName(int index) 
	{
		return "";
	}
}
