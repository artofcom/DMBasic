using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CakePanelA : CakePanel {
    public override CAKE_TYPE Type { get { return CAKE_TYPE.A; } }

	protected override void OnPanelDestroy (BoardPanel bp) {
    /*
        List<Board> cakes = new List<Board>();
        List<Board> processed = new List<Board>();

        cakes.Add(bp.Owner);
        cakes.AddRange(GetOtherCakes(bp));
        processed.AddRange(cakes);

        for (int i = 1; i < GameManager.WIDTH; i++) {
            for (int j = 0; j < cakes.Count; j++) {
                List<Board> targets = new List<Board>();
                List<Board> candidates = cakes[j].GetBoardsFromDistance(i);

                for (int k = 0; k < candidates.Count; k++) {
                    if (processed.Contains(candidates[k])) continue;
                    if (candidates[k].PND is CakePanel) continue;

                    targets.Add(candidates[k]);
                }

                for (int k = 0; k < targets.Count; k++) {
                    targets[k].Hit(0.05F * i);
                }

                processed.AddRange(candidates);
            }
        }
    */
    }
}
