using UnityEditor;
using UnityEngine;

public class DataSelectWindow : EditorWindow
{
    string postFix;

   /* [MenuItem("NOVNINE/Data/Select DataSet", false, 3300)]
    public static void SelectDataSet() {
        EditorWindow.GetWindow(typeof(DataSelectWindow));
    }*/

    void OnFocus()
    {
        postFix = "A";// PlayerPrefs.GetString("DataSet");
    }

    void OnGUI()
    {
        //string nPostFix = EditorGUILayout.TextField("DataSet Postfix", postFix);
        //if(nPostFix != postFix) {
        //    postFix = nPostFix;
            //Root.SetPostfix(postFix);
            //P//layerPrefs.SetString("DataSet", postFix);
        //}
    }
}
