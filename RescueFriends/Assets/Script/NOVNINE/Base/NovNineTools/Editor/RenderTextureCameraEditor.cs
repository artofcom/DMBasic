using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RenderTextureCamera))]
public class RenderTextureCameraEditor : Editor {

    public override void OnInspectorGUI ()
    {
        RenderTextureCamera myTarget = (RenderTextureCamera)target;

        myTarget.useFullScreen = EditorGUILayout.Toggle("Use FullScreen", myTarget.useFullScreen);

        if (myTarget.useFullScreen == false) {
            myTarget.width = (int)EditorGUILayout.IntField("Width", myTarget.width);
            myTarget.height = (int)EditorGUILayout.IntField("Height", myTarget.height);
        }
    }
}
