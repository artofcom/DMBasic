using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor(typeof(NNSound))]
public class NNSoundEditor : Editor {

	NNSound instance;

    public override void OnInspectorGUI () {
        base.OnInspectorGUI();

		instance = (NNSound)target;

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Sound Import");

        if (GUILayout.Button("Import", GUILayout.MaxWidth(100))) {
            AudioClip[] clips  = Resources.FindObjectsOfTypeAll(typeof(AudioClip)) as AudioClip[];

            instance.audioClips = clips;

            Array.Sort(clips, delegate(AudioClip clipsA, AudioClip clipsB) {
                return clipsA.name.CompareTo(clipsB.name);
            });
        }

        GUILayout.EndHorizontal();

        if (GUI.changed) EditorUtility.SetDirty(instance);
    }
}
