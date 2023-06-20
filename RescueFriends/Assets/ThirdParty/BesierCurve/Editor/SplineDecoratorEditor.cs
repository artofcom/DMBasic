using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineDecorator))]
public class SplineDecoratorEditor : Editor {
	public override void OnInspectorGUI () {
        DrawDefaultInspector();
		if (GUILayout.Button("Clear")) {
		    var deco = target as SplineDecorator;
            deco.Clear();
		}
		if (GUILayout.Button("Build")) {
		    var deco = target as SplineDecorator;
            deco.Build();
		}
	}
}
