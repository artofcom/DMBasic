/**
* @file WidgetEditor.cs
* @brief Code from CreateScriptDialog (http://forum.unity3d.com/threads/120678-Create-Script-Dialog)
* @author ChoiYongWu (amugana@bitmango.com)
* @version 1.0
* @date 2013-03-07
*/

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

public class WidgetEditor : ScriptableWizard
{
    static string widgetName;
    static System.Type type;
    static System.Type[] dataTypes;
    static string[] dataTypeNames;
    static int dataTypeIndex;

    private static System.Type[] GetTypesInNamespace(string nameSpace)
    {
        List<System.Type> classTypes = new List<System.Type>();
        System.AppDomain app = System.AppDomain.CurrentDomain;
        Assembly[] ass = app.GetAssemblies();
        foreach(Assembly assembly in ass)
            classTypes.AddRange(
                assembly.GetTypes().
                Where(t => t.Namespace == nameSpace && !t.IsEnum).ToArray());
        return classTypes.ToArray();
    }

    public static void CreateWizard (System.Type _type, string name)
    {
        widgetName = name;
        type = _type;
        ScriptableWizard.DisplayWizard<WidgetEditor>("Create New "+ type.FullName.Substring(1));
        /*
                dataTypes = GetTypesInNamespace("Data");
                dataTypeNames = new string[dataTypes.Length];
                for(int i=0; i<dataTypeNames.Length; ++i)
                    dataTypeNames[i] = dataTypes[i].FullName.Replace("+",".");
        */
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();

        if(type == typeof(IGameScene))
            GUILayout.Label("GameScene : ");
        else if(type == typeof(IUIOverlay))
            GUILayout.Label("UIOverlay : ");
        else if(type == typeof(IPopupWnd))
            GUILayout.Label("PopupWnd : ");
        else if(type == typeof(IWidget)) {
            GUILayout.Label("Widget : ");
        }
        widgetName = GUILayout.TextField(widgetName);
        GUILayout.EndHorizontal();

        //if(type == typeof(IWidget))
        //    dataTypeIndex = EditorGUILayout.Popup("DataType", dataTypeIndex, dataTypeNames);

        if(GUILayout.Button("Create")) {
            if(type == typeof(IGameScene))
                DesignMenuItem.CreateGameScene(widgetName);
            else if(type == typeof(IUIOverlay))
                DesignMenuItem.CreateUIOverlay(widgetName);
            else if(type == typeof(IPopupWnd))
                DesignMenuItem.CreatePopupWnd(widgetName);
            else if(type == typeof(IWidget))
                DesignMenuItem.CreateWidget(widgetName);//, dataTypes[dataTypeIndex]);
            Close();
        }
    }
}

