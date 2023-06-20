using UnityEngine;
using System.Collections;
using JsonFx.Json;
using NOVNINE.Diagnostics;

public class ConveyorPanel : PanelDefinition {
    public enum TYPE { BEGIN, MIDDLE, END }
    public enum DIRECTION { LEFT, DOWN, RIGHT, UP }

    public class Info {
        public TYPE Type { get; set; }
        public DIRECTION In { get; set; }
        public DIRECTION Out { get; set; }
    }

	public override bool IsSolid (BoardPanel bp) { return false; }
	public override bool IsFallable (BoardPanel bp) { return true; }
	public override bool IsFillable (BoardPanel bp) { return true; }
	public override bool IsMatchable (BoardPanel bp) { return true; }
	public override bool IsStealable (BoardPanel bp) { return true; }
	public override bool IsSwitchable (BoardPanel bp) { return true; }
	public override bool IsDestroyable (BoardPanel bp) { return false; }
	public override bool IsSplashHitable (BoardPanel bp) { return false; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return true; }
    public override bool IsShufflable (BoardPanel bp) { return false; }

    public override object ConvertToPanelInfo (string data) {
        Debugger.Assert(string.IsNullOrEmpty(data) == false);
        return JsonReader.Deserialize<Info>(data);
    }

    public TYPE GetType (BoardPanel bp) {
        return (bp.info as Info).Type;
    }

    public DIRECTION GetIn (BoardPanel bp) {
        return (bp.info as Info).In;
    }

    public DIRECTION GetOut (BoardPanel bp) {
        return (bp.info as Info).Out;
    }

    public Board GetPrev (Board bd) {
        Debugger.Assert(bd.PnlRiver().PND is ConveyorPanel);
        ConveyorPanel.Info info = bd.PnlRiver().info as ConveyorPanel.Info;

        if (info.Type == TYPE.BEGIN) {
            return null;
        } else {
            return GetNeighbor(bd, info.In);
        }
    }

    public Board GetNext (Board bd) {
        Debugger.Assert(bd.PnlRiver().PND is ConveyorPanel);
        ConveyorPanel.Info info = bd.PnlRiver().info as ConveyorPanel.Info;

        if (info.Type == TYPE.END) {
            return null;
        } else {
            return GetNeighbor(bd, info.Out);
        }
    }

    public Vector3 GetInPosition (Board bd) {
        Debugger.Assert(bd.PnlRiver().PND is ConveyorPanel);
        ConveyorPanel.Info info = bd.PnlRiver().info as ConveyorPanel.Info;
        return GetNeighborPosition(bd, info.In);
    }

    public Vector3 GetOutPosition (Board bd) {
        Debugger.Assert(bd.PnlRiver().PND is ConveyorPanel);
        ConveyorPanel.Info info = bd.PnlRiver().info as ConveyorPanel.Info;
        return GetNeighborPosition(bd, info.Out);
    }

    Board GetNeighbor (Board bd, DIRECTION dir) {
        switch (dir) {
            case DIRECTION.LEFT : return bd.Left;
            case DIRECTION.DOWN : return bd.Bottom;
            case DIRECTION.RIGHT : return bd.Right;
            case DIRECTION.UP : return bd.Top;
        }

        return null;
    }

    Vector3 GetNeighborPosition (Board bd, DIRECTION dir) {
        Vector3 pos = bd.Position;

        // note : 적절한 비율 거리에서 나가고 들어오게 한다. - masking 이슈.
        const float fRate = 0.8f;

        switch (dir) {
            case DIRECTION.LEFT : 
                pos.x -= (JMFUtils.GM.Size* fRate);
                break;
            case DIRECTION.DOWN :
                pos.y -= (JMFUtils.GM.Size*fRate);
                break;
            case DIRECTION.RIGHT :
                pos.x += (JMFUtils.GM.Size*fRate);
                break;
            case DIRECTION.UP :
                pos.y += (JMFUtils.GM.Size*fRate);
                break;
        }

        return pos;
    }
}
