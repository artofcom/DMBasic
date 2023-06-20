using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(PlaneMeshObject))]
public class PlaneMeshObjectEditor : Editor
{

    public override void OnInspectorGUI()
    {
        PlaneMeshObject plane = (PlaneMeshObject)target;
        base.OnInspectorGUI();

        //plane.BuildMesh();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Commit")) {
            plane.BuildMesh();
        }
        GUILayout.EndHorizontal();
    }

    [MenuItem("GameObject/Create Other/NOVNINE/PlaneMeshObject", false, 21101)]
    static void PlaneObject()
    {
        GameObject plane = new GameObject("PlaneMeshObject");
        PlaneMeshObject item = plane.AddComponent<PlaneMeshObject>();

        item.BuildMesh();
    }
}

