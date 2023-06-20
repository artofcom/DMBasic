using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dSpriteExpand))]
public class tk2dSpriteExpandEditor : Editor
{

    public override void OnInspectorGUI()
    {
        tk2dSpriteExpand sprite = (tk2dSpriteExpand)target;
        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Commit")) {
            sprite.Commit();
        }

    }

}

