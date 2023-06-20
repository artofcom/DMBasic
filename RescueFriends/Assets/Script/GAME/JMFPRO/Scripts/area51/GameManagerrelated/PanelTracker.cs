using UnityEngine;
using System.Collections;

public class PanelTracker : MonoBehaviour {
	public Point PT = new Point();

	
//  EmptyPanel 때문에 주석
//	void OnEnable () {
//        JMFUtils.autoScale(gameObject);
//	}

	void OnMouseUpAsButton () {
		JMFRelay.FireOnClickPanel(PT);
	}
}
