using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NOVNINE;

public class GameStateInfo {
    public int Seed { get; private set; }
	public int Move { get; private set; }
	public long Score { get; private set; }
	public float PlayTime { get; private set; }
	public int YetiHealth { get; private set; }
	public int BossHealth { get; private set; }
    public int PenguinMatchCount { get; private set; }
    public int FairyMatchCount { get; private set; }
	public int Potion1CollectCount { get; private set; }
	public int Potion2CollectCount { get; private set; }
    public int Potion3CollectCount { get; private set; }
    public ReadOnlyCollection<int> JewelMatchCounts { get; private set; }
    public ReadOnlyCollection<int> SpecialJewelCollectCounts { get; private set; }
    public ReadOnlyCollection<BoardStateInfo> BoardStateInfos { get; private set; }

    public BoardStateInfo this [int x, int y] {
        get { return BoardStateInfos[(GameManager.WIDTH * y) + x]; }
    }

    public GameStateInfo (GameManager gm) {
        Seed = NNTool.Seed;
        Move = gm.Moves;
        Score = gm.Score;
        PlayTime = gm.PlayTime;
        YetiHealth = gm.YetiHealth;
        BossHealth = gm.BossHealth;
        PenguinMatchCount = gm.PenguinMatchCount;
        FairyMatchCount = gm.FairyMatchCount;
        Potion1CollectCount = gm.Potion1CollectCount;
        Potion2CollectCount = gm.Potion2CollectCount;
        Potion3CollectCount = gm.Potion3CollectCount;
        
        JewelMatchCounts = (new List<int>(gm.JewelMatchCounts)).AsReadOnly();
        SpecialJewelCollectCounts = (new List<int>(gm.SpecialMatchCounts)).AsReadOnly();

        List<BoardStateInfo> infos = new List<BoardStateInfo>();

        for (int y = 0; y < GameManager.HEIGHT; y++) {
            for (int x = 0; x < GameManager.WIDTH; x++) {
                infos.Add(new BoardStateInfo(gm[x,y]));
            }
        }

        BoardStateInfos = infos.AsReadOnly();
    }
}
