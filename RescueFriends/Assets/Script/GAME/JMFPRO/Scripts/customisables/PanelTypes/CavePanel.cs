using UnityEngine;
using System.Collections;
using JsonFx.Json;
using NOVNINE.Diagnostics;

public class CavePanel : PanelDefinition {
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
}
