using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StonePiece : PieceDefinition {
	public override bool IsSwitchableHorizontal (GamePiece gp) {
        if (gp.PD is StonePiece) {
            return (gp.ColorIndex == 1) || (gp.ColorIndex == 3);
        } else {
            return true;
        }
    }

	public override bool IsSwitchableVertical (GamePiece gp) {
        if (gp.PD is StonePiece) {
            return (gp.ColorIndex == 2) || (gp.ColorIndex == 3);
        } else {
            return true;
        }
    }

}
