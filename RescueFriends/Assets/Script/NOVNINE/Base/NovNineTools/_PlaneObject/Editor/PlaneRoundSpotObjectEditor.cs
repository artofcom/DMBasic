using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(PlaneRoundSpotObject))]
public class PlaneRoundSpotObjectEditor : Editor
{

    public override void OnInspectorGUI()
    {
        PlaneRoundSpotObject plane = (PlaneRoundSpotObject)target;

        //base.OnInspectorGUI();

        int divideCount = EditorGUILayout.IntField("Divide Count", plane.divideCount);
        if (divideCount != plane.divideCount) {
            plane.divideCount = divideCount;
            GUI.changed = true;
        }
        float innerRadius = EditorGUILayout.FloatField("Inner Radius", plane.innerRadius);
        if (innerRadius != plane.innerRadius) {
            plane.innerRadius = innerRadius;
            GUI.changed = true;
        }

        float outerRadius = EditorGUILayout.FloatField("Outer Radius", plane.outerRadius);
        if (outerRadius != plane.outerRadius) {
            plane.outerRadius = outerRadius;
            GUI.changed = true;
        }

        Color innerColor = EditorGUILayout.ColorField("Inner Color", plane.innerColor);
        if (innerColor != plane.innerColor) {
            plane.innerColor = innerColor;
            GUI.changed = true;
        }
        Color outerColor = EditorGUILayout.ColorField("Outer Color", plane.outerColor);
        if (outerColor != plane.outerColor) {
            plane.outerColor = outerColor;
            GUI.changed = true;
        }

        if (true == GUI.changed) {
            plane.BuildMesh();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Commit")) {
            plane.BuildMesh();
        }
    }

    [MenuItem("GameObject/Create Other/NOVNINE/PlaneRoundSpotObject", false, 21102)]
    static void PlaneObject()
    {
        GameObject plane = new GameObject("PlaneRoundSpotObject");
        PlaneRoundSpotObject item = plane.AddComponent<PlaneRoundSpotObject>();

        item.BuildMesh();
    }
}

