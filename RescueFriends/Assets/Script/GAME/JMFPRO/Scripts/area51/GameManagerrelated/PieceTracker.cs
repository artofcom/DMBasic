using UnityEngine;
using System.Collections;
//using Holoville.HOTween;
using DG.Tweening;

public class PieceTracker : MonoBehaviour 
{
	static Point dragOrigin;
	public Point PT = new Point();
	public static bool isBeingDragged = false;
	
	void OnMouseEnter () 
	{
        // [AI_MISSION] - Ai turn에는 mouse input을 무시한다.
        if(JMFUtils.GM.isAIFightMode && JMFUtils.GM.isCurPlayerAI)
            return;

		if (isBeingDragged && dragOrigin != PT) 
		{
            if (JMFUtils.GM[dragOrigin].Neighbours.Contains(JMFUtils.GM[PT]))
                JMFUtils.GM.DragFromHere(dragOrigin, PT);

			isBeingDragged = false;
		}
	}
	
	void OnMouseDown () 
	{
        // [AI_MISSION] - Ai turn에는 mouse input을 무시한다.
        if(JMFUtils.GM.isAIFightMode && JMFUtils.GM.isCurPlayerAI)
            return;

		dragOrigin = PT;
		isBeingDragged = true;
        /*
        if (JMFUtils.GM[PT].Piece != null && JMFUtils.GM[PT].Piece.GO != null && JMFUtils.GM.IsStable) {
            JMFUtils.autoScalePadded(JMFUtils.GM[PT].Piece.GO);
            Vector3 startScale = JMFUtils.GM[PT].Piece.GO.transform.localScale;
            if (seq != null) seq.Kill();
            seq = new Sequence();
            seq.Append(HOTween.To(transform, 0.2f, new TweenParms().Prop("localScale", startScale * 1.3F)));
            seq.Play();
        }
        */
	}

	void OnMouseUp () 
	{
		isBeingDragged = false;
        /*
        if (seq != null) seq.Kill();
        if (JMFUtils.GM[PT].Piece != null && JMFUtils.GM[PT].Piece.GO != null) {
            JMFUtils.autoScalePadded(JMFUtils.GM[PT].Piece.GO);
        }
        */
	}
	
	void OnMouseUpAsButton () 
	{
		isBeingDragged = false;
		JMFRelay.FireOnClickPiece(PT);
	}
}
