using UnityEngine;
using System.Collections.Generic;

public class SplineDecorator : MonoBehaviour {

	public BezierSpline spline;

	public int frequency;

	public bool lookForward;

	public Transform[] items;
    public Vector3 offset;

    public void Clear() {
        /*
        foreach (Transform child in transform.children) {
            Debug.Log(child.name);
            Object.DestroyImmediate(child.gameObject);
        }
        */
        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => DestroyImmediate(child));
    }

    public void Build() {
		if (frequency <= 0 || items == null || items.Length == 0) {
			return;
		}
		float stepSize = frequency * items.Length;
		if (spline.Loop || stepSize == 1) {
			stepSize = 1f / stepSize;
		}
		else {
			stepSize = 1f / (stepSize - 1);
		}
		for (int p = 0, f = 0; f < frequency; f++) {
			for (int i = 0; i < items.Length; i++, p++) {
				Transform item = Instantiate(items[i]) as Transform;
                item.name = items[i].name+"_"+p.ToString();
				Vector3 position = spline.GetPointByDistance(p * stepSize) + offset;
				//float scale = Mathf.Min(spline.GetVelocity(p * stepSize).magnitude/10f, 1f);
				item.transform.localPosition = position;
				//item.transform.localScale = Vector3.one * scale;
				if (lookForward) {
					item.transform.LookAt(position + spline.GetDirection(p * stepSize));
				}
				item.transform.parent = transform;
			}
		}
    }
}
