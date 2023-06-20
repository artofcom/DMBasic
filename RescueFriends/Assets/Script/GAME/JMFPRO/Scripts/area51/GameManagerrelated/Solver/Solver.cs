using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwapInfo {
	public Board From { get; private set; }
	public Board To { get; private set; }
	public long SwapValue { get; set; }
	public long ResultValue { get; set; }
	public SwapInfo (Board from, Board to) {
		From = from;
		To = to;
	}
}

#if !NN_DEPLOY

public class Orders {
    public float frost = 1F;
    public float snow = 1F;
    public float snowMan = 2F;
    public float cage = 1F;
    public float cake = 1.5F;
    public float spiralSnow = 0.5F;
    public float penguin = 3F;
    public float timeBomb = 3F;
}

public abstract class Solver {
    public enum STATE { IDLE, SEARCH }

    protected static List<SwapInfo> candidates = new List<SwapInfo>();

    protected GameManager GM { get; set; }

    public abstract STATE State { get; }

    public virtual void _Reset () {}
    public abstract Solver _Solve ();

    public void Reset () {
        candidates.Clear();

        _Reset();
    }

    public Solver Solve () {
        if (GM.State != JMF_GAMESTATE.PLAY) {
            if ((GM.State == JMF_GAMESTATE.FINAL) || (GM.State == JMF_GAMESTATE.BONUS)) {
                Debug.Log(string.Format("[Stage{0}.{1}] Move : {2}/{3} Score : {4}", 
                    GM.CurrentLevel.Index + 1, GM.Seed, GM.Moves, GM.CurrentLevel.allowedMoves, GM.Score));
            }
            return null;
        }

        return _Solve();
    }
}

public class SolverIdle : Solver {
    public override STATE State { get { return STATE.IDLE; } }

    public SolverIdle (GameManager gm) {
        GM = gm;
    }

    public override Solver _Solve () {
        List<Board> listDummy = new List<Board>();
        SortCandidates(GM.GetSuggestablePieces(ref listDummy));

        if (candidates.Count == 0) return this;

        return GM._SolverSearch.Solve();
    }

    void SortCandidates (Dictionary<Board, List<Board>> dict) {
        foreach (Board bd in dict.Keys) {
            List<Board> bds = dict[bd];

            for (int i = 0; i < bds.Count; i++) {
                SwapInfo info = new SwapInfo(bd, bds[i]);
                info.SwapValue = GetSwapValue(info.From, info.To);
                candidates.Add(info);
            }
        }

        if (candidates.Count <= 0) return;
        candidates.Sort((a,b) => { return b.SwapValue.CompareTo(a.SwapValue); });

        if (candidates[0].SwapValue > 0) {
            if (candidates.Count > 1) {
                candidates.RemoveRange(1, candidates.Count-1);
            }
        }
    }

    long GetSwapValue (Board from, Board to) {
        if (from.IsMatchable) {
            List<Board> row = GM.GetRowMatchableBoards(to, from.ColorIndex, to.GetNeighborDirection(from));
            List<Board> col = GM.GetColMatchableBoards(to, from.ColorIndex, to.GetNeighborDirection(from));

            if ((row.Count > 3) || (col.Count > 3)) return 4;
            if ((row.Count > 1) && (col.Count > 1)) return 3;
            if (col.Count > 3) return 2;
            if (row.Count > 3) return 1;
        } else {
            //Special Match Value Here
            if (from.PD is TreasurePiece) {
                if (ExistTreasureGoalCol(from) == false) {
                    if (ExistTreasureGoalCol(to) == true) {
                        return 5;
                    }
                }
            }
        }

        return 0;
    }

    bool ExistTreasureGoalCol (Board bd) {
        bool isNeedSwap = true;
        //for (int i = 0; i < GM.CurrentLevel.treasureGoal.Length; i += 2) {
        //    if (bd.PT.x == GM.CurrentLevel.treasureGoal[i]) {
        //        isNeedSwap = false;
        //        break;
        //    }
        //}/ 
        return isNeedSwap;
    }
}

public class SolverSearch : Solver {
    public Orders order;
    public override STATE State { get { return STATE.SEARCH; } }

    SwapInfo swapInfo;
    List<SwapInfo> swapInfos = new List<SwapInfo>();

    public SolverSearch (GameManager gm) {
        GM = gm;
#if UNITY_STANDALONE_OSX 
        string orderPath = Application.dataPath + "/../../"+GameManager.solverFileName+".txt";
#else
        string orderPath = Application.dataPath+"/Data/text/Solver/"+GameManager.solverFileName+".txt";
#endif
        if (!System.IO.File.Exists(orderPath)) {
            order = new Orders();
        } else {
            string jsonStr = System.IO.File.ReadAllText(orderPath);
            order = JsonFx.Json.JsonReader.Deserialize<Orders>(jsonStr);
        }
    }

    public override void _Reset () {
        swapInfo = null;
        swapInfos.Clear();
    }

    public override Solver _Solve () {
        if (swapInfo != null) {
            swapInfo.ResultValue = GetBoardValue();
            swapInfos.Add(swapInfo);
            swapInfo = null;
            swapInfos.Sort((a,b) => { return b.ResultValue.CompareTo(a.ResultValue); });
            GM.Rollback();
        }

        if (candidates.Count > 0) {
            swapInfo = candidates[0];
            GM.DragFromHere(swapInfo.From.PT, swapInfo.To.PT);
            candidates.RemoveAt(0);
        } else {
            SwapInfo si = swapInfos[0]; 
            GM.DragFromHere(si.From.PT, si.To.PT);
            swapInfos.Clear();
            return GM._SolverIdle;
        }

        return this;
    }
    
    long GetBoardValue () {
        long boardValue = 0;
        if (GM.CurrentLevel.isClearShadedGame) {
            boardValue = GM.Score;
        } else if (GM.CurrentLevel.isClearChocolateGame) {
            boardValue = GM.Score - (Snowman.TotalCount * 10000);
        } else if (GM.CurrentLevel.isGetTypesGame) {
            long cost = GM.Score;
            
            for (int i = 0; i < (int)LEItem.COLOR.NORMAL_COUNT; i++) {
                if (GM.CurrentLevel.numToGet[i] <= 0) continue;

                int remainCount = GM.CurrentLevel.numToGet[i] - GM.JewelMatchCounts[i];

                if (remainCount > 0) {
                    cost -= (remainCount * 10000);
                }
            }

            boardValue = cost;
        } else if (GM.CurrentLevel.isTreasureGame) {
            long cost = GM.Score;

            cost += (((GM.Potion1CollectCount + Potion1.TotalCount) - GM.CurrentLevel.countPotion1) * 90000);
            cost += (((GM.Potion2CollectCount + Potion2.TotalCount) - GM.CurrentLevel.countPotion2) * 90000);
            cost += (((GM.Potion3CollectCount + Potion3.TotalCount) - GM.CurrentLevel.countPotion3) * 90000);

            foreach (Board bd in GM.Boards) {
                if ((bd.PD is TreasurePiece) == false) continue;
                cost -= (bd.Y * 10000);
            }
            
            boardValue = cost;
        } else if (GM.CurrentLevel.isSpecialJewelGame) {
            long cost = GM.Score;

            for (int i = 0; i < GM.CurrentLevel.specialJewels.Length; i++) {
                if (GM.CurrentLevel.specialJewels[i] <= 0) continue;

                int remainCount = GM.CurrentLevel.specialJewels[i] - GM.SpecialMatchCounts[i];

                if (remainCount > 0) {
                    cost -= (remainCount * 10000);
                }
            }

            boardValue = cost;
        } else if (GM.CurrentLevel.isPenguinGame) {
            boardValue = GM.Score + (GM.PenguinMatchCount * 10000);
        } else if (GM.CurrentLevel.isYetiGame) {
            boardValue = GM.Score - (GM.YetiHealth * 10000);
        } else if (GM.CurrentLevel.isTimerGame) {
            boardValue = GM.Score;
        } else {
            boardValue = GM.Score;
        }

        boardValue -= (long)(GM.GetBoards<FrostPanel>().Count * order.frost * 10000);
        boardValue -= (long)(GM.GetBoards<RectChocoPanel>().Count * order.snow * 10000);
        boardValue -= (long)(GM.GetBoards<SnowmanPanel>().Count * order.snowMan * 10000);
        boardValue -= (long)(GM.GetBoards<CagePanel>().Count * order.cage * 10000);
        boardValue -= (long)(GM.GetBoards<CakePanel>().Count * order.cake * 10000);
        boardValue -= (long)(GM.GetBoards<RoundChocoPiece>().Count * order.spiralSnow * 10000);
        boardValue -= (long)(GM.GetBoards<PenguinPiece>().Count * order.penguin * 10000);
        foreach (Board timeBombBoard in GM.GetBoards<TimeBombPiece>()) {
            if (timeBombBoard.Piece.FallBackTime < 10) {
                boardValue -= (long)(timeBombBoard.Piece.FallBackTime * order.timeBomb * 10000);
            } else {
                boardValue -= (long)(order.timeBomb * 10000);
            }
        }

        return boardValue;
    }
}
#endif
