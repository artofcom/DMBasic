using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;

public class MysteryPiece : PieceDefinition {
    bool spawned;
    int moveForMystery = 0;
    int spawnableCount = 0;

    void OnEnable () {
        JMFRelay.OnGameReady += OnGameReady;
        JMFRelay.OnFinishDrop += OnPlayerMove;
        // JMFRelay.OnBoardStable += OnBoardStable;
    }

    void OnDisable () {
        JMFRelay.OnGameReady -= OnGameReady;
        JMFRelay.OnFinishDrop -= OnPlayerMove;
        // JMFRelay.OnBoardStable -= OnBoardStable;
    }

    void OnGameReady () {
        spawned = false;
        moveForMystery = 0;
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.mysteryPieceSpawnCountPerMove);
    }

    void OnPlayerMove () {

        OnBoardStable();

        if (Mystery.TotalCount < GM.CurrentLevel.maxMysteryPieceCount) {
            moveForMystery++;
        }
    }

    void OnBoardStable () {
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.mysteryPieceSpawnCountPerMove);

        if (spawned) {
            moveForMystery = 0;
            spawned = false;
        }
    }

	public override PieceDefinition GetSpawnableDefinition (Point pt) {
        if (GM.CurrentLevel.enableMysteryPiece == false) return null;
        return IsChanceOfSpawn() ? this : null;
	}

	protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        int score = destroyScore;

        if (gp.Owner.ShadedDurability >= 0) {
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("mysteryjewel_crack");
        return score;
    }

    protected override void OnPieceDestroyed (Board bd, int prevColor) {
        ConvertMysteryAfterDestroyed(bd);
    }

    public override bool IsChanceOfSpawn () {
        if (moveForMystery < GM.CurrentLevel.movePerMysteryPiece) return false;
        if (Mystery.TotalCount >= Mathf.Max(1, GM.CurrentLevel.maxMysteryPieceCount)) return false;

        if (Mystery.TotalCount < Mathf.Max(1, GM.CurrentLevel.minMysteryPieceCount)) {
            spawnableCount--;
            spawned = true;
            return true;
        }

        if (spawnableCount > 0) {
            spawnableCount--;
            spawned = true;
            return true;
        } else {
            return false;
        }
    }

    public override float ShowDestroyEffect (GamePiece gp) {
		BlockCrash effect = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
		Vector3 pos = gp.Position;
		pos.z -= 2.0f; 
		return effect.Play("play",pos,gp.Scale, gp.ColorIndex);//, false);
    }

    void ConvertMysteryAfterDestroyed (Board bd) {
        Debugger.Assert(bd != null, "MysteryPiece.ConvertMysteryAfterDestroyed : Board is null.");

        int strength = 0;
        int colorIndex = -1;
        PanelDefinition pnd = null;
        PieceDefinition pd = null;

        int index = GetMysteryConversionCaseIndex();

        if (bd.PND is ConveyorPanel) {
            while ((index != 0) && (index != 1) && (index != 2) && (index != 13) && (index != 14) && 
                (index != 15) && (index != 16)) {
                index = GetMysteryConversionCaseIndex();
            }
        }

        switch (index) {
            case 0 :
                if (NNTool.Rand(0, 2) == 0) {
                    pd = GM.GetPieceType<HorizontalPiece>();
                } else {
                    pd = GM.GetPieceType<VerticalPiece>();
                }
                break;
            case 1 :
                pd = GM.GetPieceType<BombPiece>();
                break;
            case 2 :
                pd = GM.GetPieceType<SpecialFive>();
                colorIndex = 0;
                break;
            case 3 :
                if (NNTool.Rand(0, 2) == 0) {
                    pd = GM.GetPieceType<HorizontalPiece>();
                } else {
                    pd = GM.GetPieceType<VerticalPiece>();
                }
                pnd = GM.GetPanelType<FrostPanel>();
                break;
            case 4 :
                pd = GM.GetPieceType<BombPiece>();
                pnd = GM.GetPanelType<FrostPanel>();
                break;
            case 5 :
                pd = GM.GetPieceType<SpecialFive>();
                pnd = GM.GetPanelType<FrostPanel>();
                colorIndex = 0;
                break;
            case 6 :
                pnd = GM.GetPanelType<RectChocoPanel>();
                strength = 0;
                break;
            case 7 :
                pnd = GM.GetPanelType<RectChocoPanel>();
                strength = 1;
                break;
            case 8 :
                pnd = GM.GetPanelType<RectChocoPanel>();
                strength = 2;
                break;
            case 9 :
                pnd = GM.GetPanelType<RectChocoPanel>();
                strength = 3;
                break;
            case 10 :
                pnd = GM.GetPanelType<RectChocoPanel>();
                strength = 4;
                break;
            case 11 :
                pnd = GM.GetPanelType<SnowmanPanel>();
                break;
            case 12 :
                pnd = GM.GetPanelType<SnowManFactoryPanel>();
                break;
            case 13 :
                pd = GM.GetPieceType<PenguinPiece>();
                break;
            case 14 :
                pd = GM.GetPieceType<RoundChocoPiece>();
                colorIndex = 0;
                break;
            case 15 :
                pd = GM.GetPieceType<TimeBombPiece>();
                break;
            case 16 :
                pd = GM.GetPieceType<ChameleonPiece>();
                break;
            case 17 :
                pd = GM.GetPieceType<NormalPiece>();
                pnd = GM.GetPanelType<CagePanel>();
                break;
        }

        if (colorIndex == -1) {
            colorIndex = GM.GetRandomColorIndex();
        }

        if (pnd != null) {
            bd.ResetPanel(pnd, strength, null);
        }

        if (pd != null) {
            bd.ResetPiece(pd, colorIndex);

            if (GM.CurrentLevel.defaultFallBackTime > 0) {
                bd.Piece.FallBackTime = GM.CurrentLevel.defaultFallBackTime;
            } else {
                bd.Piece.FallBackTime = 9;
            }
        }
    }

    int GetMysteryConversionCaseIndex () {
        int rangeMax = 0;
        int[] probability = GM.CurrentLevel.mysteryPieceConvertProbability;

        foreach (int i in probability) rangeMax += i;

        int probabilitySum = 0;
        int index = NNTool.Rand(0, rangeMax);

        for (int i = 0; i < probability.Length; i++) {
            probabilitySum += probability[i];
            if (index < probabilitySum) return i;
        }

        return -1;
    }
}
