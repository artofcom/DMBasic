using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ScriptCompileWatcher : EditorWindow
{
    static float progress;

    public static void Watch()
    {
        progress = 0;
        EditorWindow window = EditorWindow.GetWindow<ScriptCompileWatcher> (false, "Script Compile Watcher");
        window.position = new Rect(0,0, 0,0);
        window.Show();
    }

    void Update()
    {
        if(EditorApplication.isCompiling) {
            progress+= 0.01f;
            EditorUtility.DisplayCancelableProgressBar("Building Handler Script", "", progress);
        } else {
            EditorUtility.ClearProgressBar();
            DesignMenuItem.OnCompileDone();
            Close();
        }
    }
}

