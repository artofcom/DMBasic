using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUISound))]
public class tk2dUISoundEditor : tk2dUIBaseItemControlEditor {
    int[] indices = new int[]{-1, -1, -1, -1};
    string[] clipNames = null;

    public override void OnInspectorGUI () {
        base.OnInspectorGUI();
        tk2dUISound item = (tk2dUISound)target;

        if (clipNames == null) clipNames = GetClipNames();

        item.buttonSounds[0] = DrawField("Down Sound", ref indices[0], item.buttonSounds[0], clipNames);
        item.buttonSounds[1] = DrawField("Up Sound", ref indices[1], item.buttonSounds[1], clipNames);
        item.buttonSounds[2] = DrawField("Click Sound", ref indices[2], item.buttonSounds[2], clipNames);
        item.buttonSounds[3] = DrawField("Release Sound", ref indices[3], item.buttonSounds[3], clipNames);

        if (GUI.changed) EditorUtility.SetDirty(item);
    }

    string DrawField (string fieldName, ref int index, string clipName, string[] clipNames) {
        index = GetIndex(clipName, clipNames);
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(fieldName);
        index = EditorGUILayout.Popup(index, clipNames);
        if (index != -1) clipName = clipNames[index];

        if (GUILayout.Button("Clear", GUILayout.MaxWidth(50))) {
            index = -1;
            clipName = null;
        }

        GUILayout.EndHorizontal();

        return clipName;
    }

    string[] GetClipNames () {
        List<string> _clipNames = new List<string>();

        if (NNSound.Instance.audioClips != null) {
            foreach (AudioClip clip in NNSound.Instance.audioClips) {
                if (clip == null) continue;
                _clipNames.Add(clip.name);
            }
        }

        return _clipNames.ToArray();
    }

    int GetIndex (string clipName, string[] clipNames) {
        int index = -1;

        if (clipNames == null) return index;

        for (int i = 0; i < clipNames.Length; i++) {
            if (clipName == clipNames[i]) {
                index = i;
                break;
            }
        }

        return index;
    }
}
