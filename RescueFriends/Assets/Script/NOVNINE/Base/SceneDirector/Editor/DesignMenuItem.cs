using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NOVNINE.Diagnostics;

public static class DesignMenuItem
{
    private const string LINK_PREFIX = "@";

    [MenuItem("NOVNINE/Design/Sync Selected Handler #%u")]
    public static void UpdateHandlerMenuItem()
    {
        GameObject go = Selection.activeGameObject;
        if(go == null) return;
        GameObject root = Scene.FindHandler(go, typeof(IHandlerBase));
        if(root == null) return;

        Component comp = Scene.GetHandler(root);
        UpdateHandlerForView(root, comp.GetType());
    }

    [MenuItem("GameObject/Create Other/NOVNINE/Director", false, 20101)]
    public static void CreateDirectorMenuItem()
    {
        Transform parent = GetParentForType(typeof(IDirector));
        GameObject go = new GameObject("Director");
        go.transform.parent = parent;
		Selection.objects = new UnityEngine.Object[] { go };
        CreateHandlerForView(go, typeof(IDirector));
    }

    [MenuItem("GameObject/Create Other/NOVNINE/Widget", false, 20105)]
    public static void CreateWidgetMenuItem()
    {
        //ScriptableWizard.DisplayWizard<WidgetEditor>("Create");
        WidgetEditor.CreateWizard(typeof(IWidget), "NewWidget");
    }

    [MenuItem("GameObject/Create Other/NOVNINE/GameScene", false, 20102)]
    public static void CreateGameSceneMenuItem()
    {
        //CreateGameScene("NewGameScene");
        WidgetEditor.CreateWizard(typeof(IGameScene), "NewGameScene");
    }

    [MenuItem("GameObject/Create Other/NOVNINE/PopupWnd", false, 20103)]
    public static void CreatePopupWndMenuItem()
    {
        //CreatePopupWnd("NewPopupWnd");
        WidgetEditor.CreateWizard(typeof(IPopupWnd), "NewPopupWnd");
    }

    [MenuItem("GameObject/Create Other/NOVNINE/UIOverlay", false, 20104)]
    public static void CreateUIOverlayMenuItem()
    {
        //CreateUIOverlay("NewUIOverlay");
        WidgetEditor.CreateWizard(typeof(IUIOverlay), "NewUIOverlay");
    }

    private static GameObject GetOrCreate(string goName)
    {
        GameObject go = GameObject.Find(goName);
        if(go == null) {
            go = new GameObject(goName);
        }
        return go;
    }

    private static Transform GetParentForType(System.Type type)
    {
        if(typeof(IDirector).IsAssignableFrom(type))
            return GetOrCreate("1.Director").transform;
        else if(typeof(IGameScene).IsAssignableFrom(type))
            return GetOrCreate("3.GameScene").transform;
        else if(typeof(IPopupWnd).IsAssignableFrom(type))
            return GetOrCreate("4.PopupWnd").transform;
        else if(typeof(IUIOverlay).IsAssignableFrom(type))
            return GetOrCreate("5.UIOverlay").transform;
        //else if(typeof(IWidget).IsAssignableFrom(type))
        //    return GetOrCreate("6.Widget").transform;
        return null;
    }

    public static Component CreateWidget(string name)  //, System.Type dataType) {
    {
        GameObject go = new GameObject(name);
        return CreateHandlerForView(go, typeof(IWidget), null/*dataType*/);
    }

    public static Component CreateGameScene(string name)
    {
        Transform parent = GetParentForType(typeof(IGameScene));
        GameObject go = new GameObject(name);
        //go.tag = "_GameScene";
        go.transform.parent = parent;
        CreateCameraAndCurtain(go);

		Selection.objects = new UnityEngine.Object[] { go };
        return CreateHandlerForView(go, typeof(IGameScene));
    }

    public static Component CreateUIOverlay(string name)
    {
        Transform parent = GetParentForType(typeof(IUIOverlay));
        GameObject go = new GameObject(name);
        //go.tag = "_UIOverlay";
        go.transform.parent = parent;
        CreateCameraAndCurtain(go);
        return CreateHandlerForView(go, typeof(IUIOverlay));
    }

    public static Component CreatePopupWnd(string name)
    {
        Transform parent = GetParentForType(typeof(IPopupWnd));
        GameObject go = new GameObject(name);
        //go.tag = "_PopupWnd";
        go.transform.parent = parent;
        CreateCameraAndCurtain(go);
        return CreateHandlerForView(go, typeof(IPopupWnd));
    }

    private static void CreateCameraAndCurtain(GameObject root)
    {
        GameObject camObj = new GameObject("cam");
        camObj.transform.parent = root.transform;
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Depth;
        cam.orthographic = true;
        cam.orthographicSize = 10;
        camObj.transform.localPosition = new Vector3(0,0,-100);

        GameObject curtainObj = new GameObject("curtain");
        //curtainObj.tag = "_Curtain";
        curtainObj.transform.parent = root.transform;
        curtainObj.AddComponent<BoxCollider>();
        curtainObj.transform.localPosition = new Vector3(0,0,-99);
        curtainObj.transform.localScale = new Vector3(1,1,1);
    }

    private static System.Type[] TypePriority = new System.Type[]{
        typeof(tk2dTiledSprite),
        typeof(tk2dSlicedSprite),
        typeof(tk2dSprite),
        typeof(TextMesh),
        typeof(tk2dTextMesh),
        typeof(PlaneMeshObject),
        typeof(PlaneRoundSpotObject),
        typeof(tk2dUIItem),
        typeof(tk2dCamera),
        typeof(Camera)
        //typeof(Transform)
    };

    private static System.Type GetRepresentativeType(GameObject go)
    {
        Debug.Log("GetRepresentativeType : "+go.name);
        foreach(var t in TypePriority) {
            var comp = go.GetComponent(t.Name);
            if(comp != null) {
                Debug.Log("  >> "+t.Name);
                return comp.GetType();
            }
        }
        Debug.Log("  >> GameObject");
        return typeof(GameObject);
    }

    public static Component CreateHandlerForView(GameObject viewRoot, System.Type type, System.Type dataType = null)
    {
        if(Scene.GetHandler(viewRoot) != null) {
            EditorUtility.DisplayDialog("CreateHandlerForView Fail",viewRoot.name+" already has a Handler, remove it first","Ok");
            return null;
        }

        CreateScript(viewRoot, type);
        if(type == typeof(IDirector))
            return viewRoot.GetComponent(viewRoot.name);
        else
            return viewRoot.GetComponent(viewRoot.name+"Handler");
    }

    private static Component UpdateHandlerForView(GameObject viewRoot, System.Type type)
    {
        UpgradeToSceneDirectorSeason2(viewRoot);

        UpdateScript(viewRoot, type);
        return viewRoot.GetComponent(viewRoot.name+"Handler");
    }

    private static string GetHandlerScriptPath(System.Type type)
    {
        string result = "Script/";
        if(typeof(IGameScene).IsAssignableFrom(type))
            result += "3.GameScene/";
        else if(typeof(IPopupWnd).IsAssignableFrom(type))
            result += "4.PopupWnd/";
        else if(typeof(IUIOverlay).IsAssignableFrom(type))
            result += "5.UIOverlay/";
        else if(typeof(IWidget).IsAssignableFrom(type))
            result += "6.Widget/";
        //Debug.Log(result);
        return result;
    }

/*
    private static void ParseViewLayout(GameObject view, out Component[] connections, out tk2dButton[] buttons)
    {
        List<Component> conList = new List<Component>();
        List<tk2dButton> butList = new List<tk2dButton>();

        bool prefixOnly = EditorPrefs.GetBool("_prefix_only", false);

        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(view.transform);
        while(stack.Count > 0) {
            Transform cur = stack.Pop();
            if(cur.name.StartsWith(LINK_PREFIX)) {
                Component comp = null;
                comp = cur.GetComponent<tk2dSprite>();
                if(comp == null)
                    comp = cur.GetComponent<tk2dTextMesh>();
                if(comp == null)
                    comp = cur.GetComponent<PlaneMeshObject>();
                if(comp == null)
                    comp = cur.GetComponent<PlaneRoundSpotObject>();
                if(comp == null)
                    comp = cur.GetComponent<tk2dButton>();
                if(comp != null) {
                    conList.Add(comp);
                    //Debug.Log("Found Connector : "+comp.name);
                }
            }

            tk2dButton button = cur.gameObject.GetComponent<tk2dButton>();
            if(button != null) {
                if (false == prefixOnly || cur.name.StartsWith(LINK_PREFIX))  {
                    butList.Add(button);
                }
                //Debug.Log("Found Button: "+button.name);
            }
            foreach(Transform child in cur.transform)
                stack.Push(child);
        }
        connections = conList.ToArray();
        buttons = butList.ToArray();
    }
*/

    private static string GetTemplate(System.Type type)
    {
        string path = Application.dataPath + "/NOVNINE/Base/SceneDirector/Template/";
        if(typeof(IDirector).IsAssignableFrom(type))
            path += "Director.cs.txt";
        else if(typeof(IGameScene).IsAssignableFrom(type))
            path += "GameScene.cs.txt";
        else if(typeof(IPopupWnd).IsAssignableFrom(type))
            path += "PopupWnd.cs.txt";
        else if(typeof(IUIOverlay).IsAssignableFrom(type))
            path += "UIOverlay.cs.txt";
        else if(typeof(IWidget).IsAssignableFrom(type))
            path += "Widget.cs.txt";

        if (File.Exists (path))
            return File.ReadAllText (path);
        return null;
    }

    private static string Capitalize(string name)
    {
        name.Trim(new char[]{'!','@'});
        return name.Substring(0, 1).ToUpper() + name.Substring(1);
    }

    private static string Decapitalize(string name)
    {
        name.Trim(new char[]{'!','@'});
        return name.Substring(0, 1).ToLower() + name.Substring(1);
    }

    private static string MangleTypeName(GameObject go) 
    {
        if(go.transform.childCount > 0) 
            return "st"+Capitalize(go.name);
        else
            return GetRepresentativeType(go).Name;
    }

    private static string MangleMemberName(GameObject go) 
    {
        if(go.name == "Form")
            return "F";
        return Decapitalize(go.name);
    }

    private static string MangleMemberHirarchy(GameObject root, GameObject go)
    {
        if(root == go)
            return "Self";
        string path = go.name;
        if(path.StartsWith("@"))
            path = path.Substring(1);
        else if(path.StartsWith("!"))
            path = path.Substring(1);

        bool includeMemberPath = EditorPrefs.GetBool("_include_member_path", true);
        if (false == includeMemberPath)
            return path;

        Transform cur = go.transform.parent;
        while(cur != root.transform) {
            string tok = cur.name;
            if(tok.StartsWith("@"))
                tok = tok.Substring(1);
            else if(tok.StartsWith("!"))
                tok = tok.Substring(1);
            path = tok + "_" + path;
            cur = cur.parent;
            if(cur == null)
                break;
        }
        return path;
    }

    private static void CreateScript(GameObject view, System.Type type)
    {
        string path = GetHandlerScriptPath(type) + view.name;
        if(type != typeof(IDirector))
            path += "Handler.cs";
        else
            path += ".cs";
        string dir = Application.dataPath+"/"+Path.GetDirectoryName(path);
        string filename = Path.GetFileNameWithoutExtension(path);

        if (!Directory.Exists (dir))
            Directory.CreateDirectory (dir);

        ScriptPrescription script = new ScriptPrescription();
        script.m_ClassName = filename;
        script.m_Template = GetTemplate(type);
        script.m_Lang = Language.CSharp;
        script.m_StringReplacements["$Connections"] = BuildConnectionSource(view, true);
        script.m_StringReplacements["$ButtonHandlers"] = BuildButtonHandlerSource(view, true);

        var writer = new StreamWriter(Application.dataPath+"/"+path);
        writer.Write(new NewScriptGenerator(script).ToString ());
        writer.Close();
        writer.Dispose();
        AssetDatabase.Refresh();

        EditorPrefs.SetString("_updating_gameobject", view.name);
		
		Type t = Type.GetType("InternalEditorUtility");
		MethodInfo method = t.GetMethod("AddScriptComponentUnchecked", BindingFlags.Public | BindingFlags.Static);
		
		if (method != null) 
			method.Invoke(null, new object[]{ view, (AssetDatabase.LoadAssetAtPath ("Assets/"+path, typeof (MonoScript)) as MonoScript)});	
	
//        InternalEditorUtility.AddScriptComponentUnchecked (view,
//                AssetDatabase.LoadAssetAtPath ("Assets/"+path, typeof (MonoScript)) as MonoScript);

        ScriptCompileWatcher.Watch();
    }

    private static void UpgradeToSceneDirectorSeason2(GameObject view)
    {
        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(view.transform);

        while(stack.Count > 0) {
            Transform cur = stack.Pop();

            if(cur.gameObject.GetComponent<tk2dButton>() != null) {
                var oldButton = cur.gameObject.GetComponent<tk2dButton>();
                //var oldSprite = cur.gameObject.GetComponent<tk2dBaseSprite>();
                var newButton = cur.gameObject.AddComponent<tk2dUIItem>();

                newButton.sendMessageTarget = oldButton.targetObject;
                newButton.SendMessageOnClickMethodName = oldButton.messageName;

                if(oldButton.buttonDownSound != null || oldButton.buttonPressedSound != null) {
                    var sound = cur.gameObject.AddComponent<tk2dUISoundItem>();
                    sound.uiItem = newButton;
                    sound.downButtonSound = oldButton.buttonDownSound;
                    sound.upButtonSound = oldButton.buttonUpSound;
                    sound.clickButtonSound = oldButton.buttonPressedSound;
                }

/*
                if( oldSprite != null && oldButton != null && 
                    oldButton.buttonDownSprite != oldSprite.spriteName || 
                    oldButton.buttonUpSprite != oldSprite.spriteName || 
                    oldButton.buttonPressedSprite != oldSprite.spriteName ) 
                {
                    var spr = cur.gameObject.AddComponent<tk2dUISpriteButton>();
                    spr.uiItem = newButton;
                    spr.buttonDownSprite = oldButton.buttonDownSprite;
                    spr.buttonUpSprite = oldButton.buttonUpSprite;
                }
*/
				UnityEngine.Object.DestroyImmediate(oldButton);
            }
        
           foreach(Transform child in cur.transform)
                stack.Push(child);
        }
    }

    private static void UpdateScript(GameObject view, System.Type type)
    {
        MonoScript script = MonoScript.FromMonoBehaviour(Scene.GetHandlerTyped(view, type));
        string path = AssetDatabase.GetAssetPath(script);
        path = path.Substring(7);

        StringBuilder builder = new StringBuilder();
        using(StreamReader reader = new StreamReader(Application.dataPath+"/"+path)) {
            bool numb = false;
            string line;
            while ((line = reader.ReadLine()) != null) {
                if(line.Trim() == "#region AutoConnected Members //DO NOT EDIT") {
                    numb = true;
                    builder.Append(line+"\n");
                    builder.Append(BuildConnectionSource(view, false));
                } else if(line.Trim() == "#endregion //AutoConnected Members") {
                    numb = false;
                } else if(line.Trim() == "#region tk2dButton Handlers") {

                } else if(line.Trim() == "#endregion //tk2dButton Handlers") {
                    string outBuf = builder.ToString();

                    Stack<Transform> stack = new Stack<Transform>();
                    stack.Push(view.transform);
                    while(stack.Count > 0) {
                        Transform cur = stack.Pop();
                        if(cur.gameObject.GetComponent<tk2dUIItem>() != null) {
                            string funcName = "OnClick_"+MangleMemberHirarchy(view, cur.gameObject);
                            if(outBuf.IndexOf(funcName) == -1) {
                                builder.Append("\tvoid "+funcName+"() {\n\n\t}\n");
                            }
                        }
                        foreach(Transform child in cur.transform)
                            if(!child.name.StartsWith("!")) stack.Push(child);
                    }
                }
                if(!numb) {
                    builder.Append(line+"\n");
                }
            }
        }

        File.WriteAllText(Application.dataPath+"/"+path, builder.ToString().Replace(script.name, view.name+"Handler"));
        if(view.name+"Handler" != script.name) {
            string ret = AssetDatabase.RenameAsset("Assets/"+path, view.name+"Handler");
            AssetDatabase.Refresh();
            if(ret != "") {
                Debug.LogError("asset rename fail : "+ret);
                return;
            }
        }

        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset("Assets/"+path, ImportAssetOptions.ForceUpdate);
        EditorPrefs.SetString("_updating_gameobject", view.name);
        ScriptCompileWatcher.Watch();
    }

    public static void OnCompileDone()
    {
        Debug.Log("Handler Compilation Done");
        string goName = EditorPrefs.GetString("_updating_gameobject");
        GameObject viewRoot = GameObject.Find(goName);
        if(viewRoot == null) {
            Debug.LogWarning("Root View Not Found");
            return;
        }

        bool success = true;
        Component handler = viewRoot.GetComponent(viewRoot.name+"Handler");
        if(handler == null) {
            success = false;
        } else {
            success = LinkConnections(handler);
        }
        EditorPrefs.DeleteKey("_updating_gameobject");
        if(!success) {
            EditorUtility.DisplayDialog("Handler Script compilation Fail", "Please Re-Make after compilation is done", "OK");
            return;
        }

        if(typeof(IWidget).IsAssignableFrom(handler.GetType())) {
            string prefabPath = "Assets/Resources/widgets/"+ handler.name.Replace("Handler", "")+".prefab";

            GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

            if(go == null) {
                string dir = Application.dataPath+"/"+Path.GetDirectoryName(prefabPath).Substring(7);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

				UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
                PrefabUtility.ReplacePrefab(viewRoot, prefab);
				Selection.objects = new UnityEngine.Object[] { prefab };
                GameObject.DestroyImmediate(viewRoot);
            } else {
				Selection.objects = new UnityEngine.Object[] { viewRoot };
            }
        }
    }

    private static string BuildConnectionSource(GameObject view, bool appendRegion)
    {
        StringBuilder connectionString = new StringBuilder();
        if(appendRegion) 
            connectionString.AppendLine("#region AutoConnected Members //DO NOT EDIT");

        List<Transform> structs = new List<Transform>();
        Stack<Transform> stack = new Stack<Transform>();
        foreach(Transform vc in view.transform) 
            if(!vc.name.StartsWith("!")) stack.Push(vc);

        while(stack.Count > 0) {
            Transform cur = stack.Pop();
            if(cur.childCount > 0)
                structs.Add(cur);
            foreach(Transform child in cur.transform)
                if(!child.name.StartsWith("!")) stack.Push(child);
        }
        foreach(var st in structs) {
            connectionString.AppendLine("\t[System.Serializable] public class "+MangleTypeName(st.gameObject)+" {");
            connectionString.AppendLine("\t\tpublic "+GetRepresentativeType(st.gameObject)+"\tself;");
            foreach(Transform child in st.transform) {
                if(!child.name.StartsWith("!")) 
                    connectionString.AppendLine("\t\tpublic "+MangleTypeName(child.gameObject)+"\t"+MangleMemberName(child.gameObject)+";");
            }
            connectionString.AppendLine("\t}");
        }

        foreach(Transform child in view.transform) {
            connectionString.AppendLine("\t[SerializeField] "
                +MangleTypeName(child.gameObject)+"\t"+MangleMemberName(child.gameObject)+";");
        }

        if(appendRegion) 
            connectionString.AppendLine("#endregion //AutoConnected Members");
        return connectionString.ToString();
    }

    private static string BuildButtonHandlerSource(GameObject view, bool appendRegion)
    {
        StringBuilder buttonHandlers = new StringBuilder();
        if(appendRegion) 
            buttonHandlers.Append("#region tk2dButton Handlers\n");

        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(view.transform);
        while(stack.Count > 0) {
            Transform cur = stack.Pop();
            if(cur.gameObject.GetComponent<tk2dUIItem>() != null) {
                string name = MangleMemberHirarchy(view, cur.gameObject);
                buttonHandlers.Append("\tvoid OnClick_"+name+"() {\n\n\t}\n");
            }
            foreach(Transform child in cur.transform)
                if( !child.name.StartsWith("!") ) 
                    stack.Push(child);
        }

        if(appendRegion) 
            buttonHandlers.Append("#endregion //tk2dButton Handlers\n");
        return buttonHandlers.ToString();
    }

    public static void SetFieldValue<T>( object src, string propName, T val )
    {
        src.GetType( ).GetField( propName, BindingFlags.Instance|BindingFlags.Public | BindingFlags.NonPublic ).SetValue( src, val );
    }

	private static void LinkComponent(string typename, string membername, GameObject go, object value)
	{
        Debug.Log("LinkComponent : "+typename+" , "+membername+" , "+go.name+" : "+ value);
        object pval;
        if(typename == "GameObject")
            pval = go;
        else 
            pval = go.GetComponent(typename);
        Debugger.Assert(pval != null, "LinkConnections Fail : Component "+typename+" not found on "+membername);
        if(pval != null)
            SetFieldValue(value, membername, pval);
	}

    private static bool LinkConnections(Component target)
    {
        Stack<KeyValuePair<Transform, object>> stack = new Stack<KeyValuePair<Transform, object>>();

        foreach(Transform vc in target.transform)
            stack.Push(new KeyValuePair<Transform, object>(vc, target));

        while(stack.Count > 0) {
            KeyValuePair<Transform, object> cur = stack.Pop();
            string pname = MangleMemberName(cur.Key.gameObject);
            string typename = MangleTypeName(cur.Key.gameObject);
            if(pname.StartsWith("!"))
                continue;
            Debug.Log(pname +" : "+typename);
            object pval = null;
            if( typename.StartsWith("st") ) {
                pval = cur.Value.GetType( ).GetField( pname, BindingFlags.Instance|BindingFlags.Public | BindingFlags.NonPublic ).GetValue( cur.Value );
                string selfTypeName = pval.GetType().GetField("self").FieldType.Name;
                LinkComponent(selfTypeName, "self", cur.Key.gameObject, pval);
                //SetFieldValue(pval, "self", cur.Key.gameObject.GetComponent(selfTypeName));
            } else {
                LinkComponent(typename, pname, cur.Key.gameObject, cur.Value);
            }

            foreach(Transform child in cur.Key.transform) {
                stack.Push(new KeyValuePair<Transform, object>(child, pval));
            }
        }

        Stack<Transform> stack2 = new Stack<Transform>();
        stack2.Push(target.transform);
        while(stack2.Count > 0) {
            Transform cur = stack2.Pop();
            if(cur.gameObject.GetComponent<tk2dUIItem>() != null) {
                tk2dUIItem item = cur.gameObject.GetComponent<tk2dUIItem>();
                item.sendMessageTarget = target.gameObject;
                item.SendMessageOnClickMethodName = "OnClick_"+MangleMemberHirarchy(target.gameObject, cur.gameObject);
            }
            foreach(Transform child in cur.transform)
                stack2.Push(child);
        }
        return true;
    }

    public static Camera FindCameraForObject(GameObject go)
    {
        GameObject root = FindSceneRoot(go);
        if(root != null)
            return root.GetComponentInChildren<Camera>();
        return Camera.main;
    }

    public static bool IsSceneRoot(GameObject go)
    {
        MonoBehaviour[] comps = go.GetComponents<MonoBehaviour>();
        foreach( var comp in comps ) {
            if( comp is IGameScene || comp is IUIOverlay || comp is IPopupWnd )
                return true;
        }
        return false;
    }

    public static GameObject FindSceneRoot(GameObject go)
    {
        Transform cur = go.transform;
        while(cur != null) {
            if( IsSceneRoot(cur.gameObject) ) {
                return cur.gameObject;
            }
            cur = cur.parent;
        }
        return null;
    }

    public static Bounds GetBound(GameObject go)
    {
        Bounds result = new Bounds(go.transform.position, Vector3.zero);
        foreach(var r in go.GetComponentsInChildren<Renderer>()) {
            result.Encapsulate(r.bounds);
        }
        foreach(var col in go.GetComponentsInChildren<Collider>()) {
            result.Encapsulate(col.bounds);
        }
        return result;
    }

    static void ArrangeObjectByTag(string tagName, Vector3 offset)
    {
        Vector3 pos = offset;//Vector3.zero;
        GameObject[] goes = GameObject.FindGameObjectsWithTag(tagName);
        System.Array.Sort(goes, (go1, go2)=> {
            return go1.name.CompareTo(go2.name);
        });
        foreach(var go in goes) {
            go.transform.localPosition = pos;
            pos += offset;
        }
    }

    [MenuItem("NOVNINE/Design/Arrange Scene Positions")]
    public static void ArrangeScenePositions()
    {
        //Undo.RegisterSceneUndo("Arrange Scene Positions");

        ArrangeObjectByTag("_GameScene", new Vector3(50,0,0));
        ArrangeObjectByTag("_PopupWnd", new Vector3(50,0,0));
        ArrangeObjectByTag("_UIOverlay", new Vector3(50,0,0));
    }

    [MenuItem("NOVNINE/Design/Bound to Scene Range", true)]
    public static bool BoundToSceneRangeValidation()
    {
        if( Selection.activeGameObject == null )
            return false;
        return FindSceneRoot(Selection.activeGameObject) != null;
    }

    [MenuItem("NOVNINE/Design/Bound to Scene Range", false)]
    public static void BoundToSceneRange()
    {
        GameObject sel = Selection.activeGameObject;
        Camera cam = FindCameraForObject(sel);
        if(cam == null)
            return;
        if(!cam.orthographic) {
            Debug.LogError("Camera Should be orthographic : "+cam.name);
            return;
        }

        //Undo.RegisterSceneUndo("Bound to Scene Range");

        Bounds bound = GetBound(sel);

        //FIXME Hardcoded due to UnityEngine.Camera.aspect BUG
        //float aspect = cam.aspect;
        float aspect = 2.0f/3.0f;
        float sizeY = cam.orthographicSize;
        float sizeX = sizeY * aspect;
        Vector3 camPos = cam.transform.position;
        Vector2 topRightPos = new Vector2(camPos.x + sizeX, camPos.y + sizeY);
        Vector2 bottomLeftPos = new Vector2(camPos.x - sizeX, camPos.y - sizeY);
        Vector3 boundPosition = bound.center;
        if(bound.max.x > topRightPos.x) {
            boundPosition.x = topRightPos.x - bound.extents.x;
        } else if(bound.min.x < bottomLeftPos.x) {
            boundPosition.x = bottomLeftPos.x + bound.extents.x;
        }
        if(bound.max.y > topRightPos.y) {
            boundPosition.y = topRightPos.y - bound.extents.y;
        } else if(bound.min.y < bottomLeftPos.y) {
            boundPosition.y = bottomLeftPos.y + bound.extents.y;
        }
        sel.transform.position = boundPosition + (sel.transform.position - bound.center);
    }
}

