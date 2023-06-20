using UnityEngine;
using System.Collections;

public class BoardStateInfo {
    public int ShadedDurability { get; private set; }

    public PanelDefinition PND { get; private set; }
    public int PanelDurability { get; private set; }
    public object PanelInfo { get; private set; }

    public PieceDefinition PD { get; private set; }
    public int PieceColor { get; private set; }
    public int FallBackTime { get; private set; }
    public int ChameleonColor { get; private set; }

    public BoardStateInfo (Board board) {
        PND = board.PND;
        PanelDurability = board.Panel.Durability;
        ShadedDurability = board.ShadedDurability;
        PanelInfo = board.Panel.info;

        if (board.IsFilled) {
            PD = board.Piece.PD;
            PieceColor = board.ColorIndex;
            FallBackTime = board.Piece.FallBackTime;

            if (PD is ChameleonPiece) {
                ChameleonColor = board.Piece.GO.GetComponent<Chameleon>().NextIndex;
            }
        } else {
            PD = null;
            PieceColor = 0;
            FallBackTime = 0;
        }
    }
}
