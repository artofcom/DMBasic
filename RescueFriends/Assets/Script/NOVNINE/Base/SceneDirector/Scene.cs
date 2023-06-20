using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;
using DG.Tweening;

public static class Scene
{
	//public static PopupManager PM; // updated by PopupManager -> Awake()
	
    struct PopupItem 
	{
		public BasePopupHandler popup;
        public Action<object> callback;

		public PopupItem (BasePopupHandler _popup, Action<object> _callback)
        {
            this.popup = _popup;
            this.callback = _callback;
        }
    }

	static IGameScene currentScene;
	static BasePopupHandler currentPopup;

	static Dictionary<string, IGameScene> _sceneCache = new Dictionary<string, IGameScene>();
	static Dictionary<string, BasePopupHandler> _popupCache = new Dictionary<string, BasePopupHandler>();
	
	//IGameScene  IUIOverlay
	static Dictionary<string, IUIOverlay> _overlayCache = new Dictionary<string, IUIOverlay>();
    static Stack<PopupItem> _popupStack = new Stack<PopupItem>();
	static Dictionary<string, PopupItem>  _addPopupStack = new Dictionary<string, PopupItem>();
    static CircularQuene<string> _sceneHistory = new CircularQuene<string>(10);

    const string kGameSceneTag = "_SceneRoot";
    const string kPopupWndTag = "_PopupRoot";
    const string kUIOverlayTag = "_OverlayRoot";
    const string kBlockerTag = "_Blocker";
    const float ESCAPE_TIME_INTERVAL = 1f;

    static float popupBaseDepth = 0f;
    static float sceneChangeTime = 1f;
    static float passTime = 0f;
    static bool enableAndroidEscapeEvent = false;
    public static bool moveBackward = false;
    static ITransition _transition; 

    public static bool allowOverLap = true;

	static MessageBlock _blocker;
	static MessageBlock blocker
    {
        get {
            if( _blocker == null )
			{
                GameObject prefab = UnityEngine.Resources.Load("messageBlock") as GameObject;
				GameObject obj = GameObject.Instantiate(prefab) as GameObject;
				_blocker =obj.GetComponent<MessageBlock>();
                _blocker.name = kBlockerTag;
				_blocker.gameObject.SetActive(false);
				//_blocker.transform.SetParent(Camera.main.transform, false);
            }
            Debugger.Assert(_blocker);
            return _blocker;
        }
    }

    public static MessageBlock GetBlocker()
    {
        return _blocker;
    }

	static GameObject blurReplacement;

    public static bool EnableAndroidEscapeEvent
    {
        get {
            return enableAndroidEscapeEvent;
        } set {
            if (value) {
                tk2dUIManager.Instance.OnInputUpdate -= OnAndroidEscape;
                tk2dUIManager.Instance.OnInputUpdate += OnAndroidEscape;
            } else {
                tk2dUIManager.Instance.OnInputUpdate -= OnAndroidEscape;
            }
            enableAndroidEscapeEvent = value;
        }
    }

    static Dictionary<string, ITransition> _transitionDic = new Dictionary<string, ITransition> {
        { "Fade", new FadeTransition() },
        { "Delay", new DelayTransition() },
        { "Spotlight", new SpotlightTransition() },
        { "StarSpotlight", new StarSpotlightTransition() },
        { "HexagonSpotlight", new HexagonSpotlightTransition() },
        { "Slide", new SlideTransition() }, 
        { "Sprite", new SpriteTransition() }, 
    };

    public static string TransitionName {
        get { 
            return _transition.Name; 
        }
        set { 
            if (_transition.Name != value) {
                Debugger.Assert(_transitionDic.ContainsKey(value));
                _transition = _transitionDic[value];
                Debug.Log("New transition name - "+_transition.Name);
            }
        }
    }

    public static float SceneChangeTime
    {
        get { return sceneChangeTime;}
        set { sceneChangeTime = Mathf.Max(0f, value);}
    }

    public static bool IsBlocked
    {
        get {
			return (_blocker != null) && _blocker.gameObject.activeSelf;
        }
    }

    static Scene () {
        //amugana ( amugana@bitmango.com ) Last modified at : 2015.09.03 
        //_transition = new FadeTransition();
#if UNITY_EDITOR
        if(true == LevelEditorSceneHandler.EditorMode)
            _transition = new FadeTransition();
        else
#endif
            _transition = new SpriteTransition();
    }

//------------------------------------------------------------------------------
//                  public Methods
//------------------------------------------------------------------------------
    #region PublicMethods
    public static void ChangeTo (string sceneName, object param = null)
    {
        if (currentScene == null) 
			Init();
		
        moveBackward = false;
        TaskManager.StartCoroutine(ChangeSceneCoroutine(sceneName, param));
    }
	
	public static BasePopupHandler GetPopup (string name)
	{
		if(_popupCache.ContainsKey(name))
			return _popupCache[name];
		return null;
	}

	//public static bool RegistPopup (BasePopupHandler popupRoot)
    //{
    //    if(_popupCache.ContainsKey(popupRoot.name)) {
    //        Debug.LogError("'"+popupRoot.name+"' is already registered.");
   //         return false;
   //     }
   //     _popupCache[popupRoot.name] = popupRoot;
//		popupRoot.gameObject.SetActive(false);
 //       return true;
  //  }

    public static void ShowPopup (string popupName, object param = null, Action<object> onClose = null)
    {
        Debugger.Assert(_popupCache.ContainsKey(popupName), "'"+popupName+"' popup not found.");

        Debug.Log(">>>>>>>>>>>>>>> [ "+popupName + " ]");
        
		BasePopupHandler popup = _popupCache[popupName];
		//Debugger.Assert(popup.gameObject.activeSelf == false, "'"+popupName+"' popup aleady shown.");
        if(popup.gameObject.activeSelf)
        {
            Debug.Log("popup already shown >>> [ "+popupName + " ]");
            return;
        }

		popup.gameObject.SetActive(true);
        //popup.SendMessage("DoDataExchange");
        //popup.SendMessage("OnEnterPopup", param == null ? popup : param);
		popup.OnEnterPopup(param);

        if (popupName != "MissionPopup")
            NNSoundHelper.Play("PFX_common_intro");

		SetCurtain(popup.gameObject, true);

        if (_popupStack.Count > 0) 
		{
            PopupItem item = _popupStack.Peek();
            //popupCamera.depth = GetCamera(item.popup).depth + 1f;
			SetCurtain(item.popup.gameObject, true);

            // give some z offset for depth.
            popup.transform.localPosition = new Vector3(popup.transform.localPosition.x, popup.transform.localPosition.y, item.popup.transform.localPosition.z-30.0f);
        }
		else
		{
            //popupCamera.depth = popupBaseDepth;
			SetCurtain(currentScene.GetGameObject(), true);

			foreach(IUIOverlay overlay in _overlayCache.Values) 
			{
				GameObject obj = overlay.GetGameObject(); 
				if (obj == null || obj.activeSelf == false) continue;
				SetCurtain(obj, true);
            }
        }

        PopupItem newItem = new PopupItem(popup, onClose);
        _popupStack.Push(newItem);

        currentPopup = popup;
    }
	
	/*public static void AddPopup (string popupName,bool curtain, object param = null, Action<object> onClose = null)
	{
		Debugger.Assert(_popupCache.ContainsKey(popupName), "'"+popupName+"' popup not found.");
		Debug.Log(">>>>>>>>>>>>>>> [ "+popupName + " ]");

        NNSoundHelper.Play("PFX_common_intro");
		BasePopupHandler popup = _popupCache[popupName];
        popup.AddPopup = true;
		Debugger.Assert(popup.gameObject.activeSelf == false, "'"+popupName+"' popup aleady shown.");
        //todo list
		popup.gameObject.SetActive(true);
		popup.OnEnterPopup(param);

		Debugger.Assert(_addPopupStack.ContainsKey(popupName) == false, "'"+popupName+"' popup added.");
		
		_addPopupStack.Add(popupName, new PopupItem(popup, onClose));
		
		SetCurtain(popup.gameObject, curtain);
	}
	
	public static void RemovePopup (string popupName,object param = null)
	{
        if(false == _popupCache.ContainsKey(popupName))
            return;
        if(false == _addPopupStack.ContainsKey(popupName))
            return;

		//Debugger.Assert(_popupCache.ContainsKey(popupName), "'"+popupName+"' popup not found.");
		Debug.Log(">>>>>>>>>>>>>>> [ "+popupName + " ]");
		//Debugger.Assert(_addPopupStack.ContainsKey(popupName) == true, "'"+popupName+"' popup no added.");
		
		PopupItem item = _addPopupStack[popupName];
		item.popup.OnLeavePopup();
		SetCurtain(item.popup.gameObject, true);

        if (false == popupName.Equals("MissionPopup"))
            NNSoundHelper.Play("PFX_common_close");

		if(item.callback != null)
		{
			Sequence seq = DOTween.Sequence();
			seq.AppendInterval(item.popup.GetAnimationDuration("hide"));
			seq.OnComplete(()=>{ item.callback(param); _addPopupStack.Remove(popupName);});
		}
		else
			_addPopupStack.Remove(popupName);
	}*/

	public static void ClosePopup (object param = null)
    {
        if (_popupStack.Count == 0) return;

        Debug.Log("<<<<<<<<<<<<<<< [ "+currentPopup.name+ " ]");

        PopupItem item = _popupStack.Pop();

        if (false == currentPopup.name.Equals("MissionPopup"))
            NNSoundHelper.Play("PFX_common_close");

        if (_popupStack.Count > 0) 
		{
            currentPopup = _popupStack.Peek().popup;
			SetCurtain(Scene.CurrentPopup().gameObject, false);
        }
		else
		{
			SetCurtain(currentScene.GetGameObject(), false);

			foreach(IUIOverlay overlay in _overlayCache.Values) 
			{
				GameObject obj = overlay.GetGameObject(); 
				if (obj == null || obj.activeSelf == false) continue;
				SetCurtain(obj, false);
			}
			
            currentPopup = null;
        }
		
		SetCurtain(item.popup.gameObject, false);

		//item.popup.SendMessage("OnLeavePopup" , SendMessageOptions.DontRequireReceiver);
		item.popup.OnLeavePopup();
		if(item.callback != null)
		{
			Sequence seq = DOTween.Sequence();
			seq.AppendInterval(item.popup.GetAnimationDuration("hide"));
			seq.OnComplete(()=>{ item.callback(param); });
		}
    }

    public static void ShowOverlay (string overlayName, object param = null)
    {
        Debugger.Assert(_overlayCache.ContainsKey(overlayName), "'"+overlayName+"' overlay not found.");

		IUIOverlay overlay = _overlayCache[overlayName];

		GameObject obj = overlay.GetGameObject(); 
		if(obj == null)
			return;
		
		obj.SetActive(true);
		overlay.DoDataExchange();
        //overlay.SendMessage("OnEnterUIOveray", param == null ? overlay : param);
		overlay.OnEnterUIOveray(param);

		SetCurtain(obj, false);
    }

    public static void CloseOverlay (string overlayName)
    {
        Debugger.Assert(_overlayCache.ContainsKey(overlayName), "'"+overlayName+"' overlay not found.");

		IUIOverlay overlay = _overlayCache[overlayName];
		GameObject obj = overlay.GetGameObject(); 
		if(obj == null)
			return;
		
		overlay.OnLeaveUIOveray();
		obj.SetActive(false);

		SetCurtain(obj, true);
    }

    public static bool Back (object param = null)
    {
        if (_sceneHistory.Count <= 0)
            return false;

        moveBackward = true;
        string backStage = _sceneHistory.Pop();
        TaskManager.StartCoroutine(ChangeSceneCoroutine(backStage, param));
        return true;
    }
	
	public static void ClearAll()
	{		
		if(currentScene != null)
		{
			SetCurtain(currentScene.GetGameObject(), true);
			currentScene.OnLeaveScene();
		}
		
		currentScene = null;
		currentPopup = null;
		
		_sceneCache.Clear();
		_popupCache.Clear();
		_overlayCache.Clear();
		_popupStack.Clear();
		_sceneHistory.Clear();
		_sceneHistory = new CircularQuene<string>(10);
		
		popupBaseDepth = 0f;
		sceneChangeTime = 1f;
		passTime = 0f;
		
		allowOverLap = true;
		moveBackward = false;
		enableAndroidEscapeEvent = false;
		
		if(_blocker != null)
		{
			GameObject.Destroy(_blocker.gameObject);
			_blocker = null;
		}
		
		if(blurReplacement != null)
		{
			GameObject.Destroy(blurReplacement);
			blurReplacement = null;
		}
	}

    public static void ClearHistory()
    {
        _sceneHistory.Clear();
    }

    public static void AddHistory(string name)
    {
        _sceneHistory.Add(name);
    }

    public static bool IsPopupOpen()
    {
        return (null != currentPopup);
    }

    public static bool IsPopupOpen(string popupName)
    {
        foreach (PopupItem item in _popupStack) {
            if (item.popup.name == popupName) return true;
        }
        return false;
    }

    public static string CurrentSceneName()
    {
        if(null == currentScene)    return null;
		return currentScene.GetGameObject().name;
    }

    public static GameObject CurrentScene()
    {
        if(null == currentScene)    return null;
        return currentScene.GetGameObject();
    }

	public static string CurrentPopupName()
	{
		if(currentPopup == null) return null;
		
		return currentPopup.name;
	}
	
	public static BasePopupHandler CurrentPopup()
	{
		return currentPopup;
	}
	
    public static bool IsExistSubView()
    {
        if (true == Scene.IsPopupOpen())
            return true;

        if (Scene.IsCurtain() == true)
            return true;

        return false;
    }

    public static bool CloseSubView()
    {
        if (false == IsExistSubView())
            return false;

        //if (true == Scene.ClosePopup()) return true;
        return false;
    }
    #endregion

    #region UtilityFunc
    public static bool SendMessage<T> (string messageName, object param = null, SendMessageOptions options = SendMessageOptions.RequireReceiver) where T : IHandlerBase
    {
        bool wasSended = false;

		if ((currentScene != null) && (currentScene.GetGameObject().GetComponent(typeof(T)) != null)) 
		{
			currentScene.GetGameObject().SendMessage(messageName, param, options);
            wasSended = true;
        }

        foreach (PopupItem item in _popupStack) 
		{
			if (item.popup.gameObject.activeSelf == false) continue;
            if (item.popup.GetComponent(typeof(T)) == null) continue;

            item.popup.SendMessage(messageName, param, options);
            wasSended = true;
        }

		foreach (IUIOverlay overlay in _overlayCache.Values) 
		{
			GameObject obj = overlay.GetGameObject();
			if (obj == null || obj.activeSelf == false) continue;
			if (obj.GetComponent(typeof(T)) == null) continue;

			obj.SendMessage(messageName, param, options);
            wasSended = true;
        }

        //Debugger.Assert(wasSended, string.Format("SendMessage Fail : Cant found '{0}' in Scene members", typeof(T).Name));
        return wasSended;
    }

    public static GameObject FindInChildren(this GameObject go, string name)
    {
        foreach (Transform x in go.GetComponentsInChildren<Transform>()) {
            if  (x.gameObject.name == name)
                return x.gameObject;
        }
        return null;
    }

    public static Camera FindCameraForObject(GameObject go)
    {
        Transform root = go.transform;
        while(root != null) {
            MonoBehaviour handler = GetHandler(root.gameObject);
            if(handler != null) {
                root = handler.transform;
                Camera cam = root.GetComponentInChildren<Camera>();
                if(cam != null) return cam;
            }
            root = root.parent;
        }
        return null;
    }

    public static bool IsSceneRoot(GameObject go)
    {
        return HasHandlerTyped(go, typeof(IGameScene));
    }

    public static bool HasHandlerTyped(GameObject go, Type type)
    {
        MonoBehaviour[] comps = go.GetComponents<MonoBehaviour>();
        foreach( var comp in comps ) {
            if( comp != null && type.IsAssignableFrom(comp.GetType()) )
                return true;
        }
        return false;
    }

    public static MonoBehaviour GetHandler(GameObject go)
    {
        MonoBehaviour[] comps = go.GetComponents<MonoBehaviour>();
        foreach( var comp in comps ) {
            if( comp != null && typeof(IHandlerBase).IsAssignableFrom(comp.GetType()) )
                return comp;
        }
        return null;
    }

    public static MonoBehaviour GetHandlerTyped(GameObject go, Type type)
    {
        MonoBehaviour[] comps = go.GetComponents<MonoBehaviour>();
        foreach( var comp in comps ) {
            if( comp != null && type.IsAssignableFrom(comp.GetType()) )
                return comp;
        }
        return null;
    }

    public static GameObject FindHandler(GameObject go, Type type)
    {
        Transform cur = go.transform;
        while(cur != null) {
            if( HasHandlerTyped(cur.gameObject, type)) {
                return cur.gameObject;
            }
            cur = cur.parent;
        }
        return null;
    }

    public static GameObject FindSceneRoot(GameObject go)
    {
        return FindHandler(go, typeof(IGameScene));
    }

    public static Bounds GetRenderBound(GameObject go)
    {
        Bounds result = new Bounds(go.transform.position, Vector3.zero);
        foreach(var r in go.GetComponentsInChildren<Renderer>()) {
            result.Encapsulate(r.bounds);
        }
        return result;
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
        result.center = result.center - go.transform.position;
        return result;
    }

    public static Vector2 GetCameraSizeInWorldSpace (Camera cam)
    {
        tk2dCamera tk2dCam = cam.GetComponent<tk2dCamera>();
        if (tk2dCam == null) {
            return new Vector2( cam.orthographicSize * cam.aspect * 2f, cam.orthographicSize * 2f );
        } else {
            return new Vector2(tk2dCam.ScreenExtents.width, tk2dCam.ScreenExtents.height);
        }
    }

    public static void Lock ()
    {
        if (currentPopup != null) {
			SetCurtain(Scene.CurrentPopup().gameObject, true);
        } else {
			SetCurtain(currentScene.GetGameObject(), true);

			foreach(IUIOverlay overlay in _overlayCache.Values) 
			{
				GameObject obj = overlay.GetGameObject();
				if (obj == null || obj.activeSelf == false) continue;
				SetCurtain(obj, true);
            }
        }
    }

    public static void Unlock ()
    {
        if (currentPopup != null) {
			SetCurtain(Scene.CurrentPopup().gameObject, false);
        } else {
			SetCurtain(currentScene.GetGameObject(), false);

			foreach(IUIOverlay overlay in _overlayCache.Values)
			{
				GameObject obj = overlay.GetGameObject();
				if (obj == null || obj.activeSelf == false) continue;
				SetCurtain(obj, false);
            }
        }
    }

	public static T GetOverlayHandler<T>() where T : IUIOverlay
	{
		foreach(IUIOverlay overlay in _overlayCache.Values) 
		{
			GameObject obj = overlay.GetGameObject();
			if (obj == null) continue;
			
			T Value = obj.GetComponent<T>();
			if(Value != null)
				return Value;
		}
		
		return default(T);
	}
	
    public static void LockWithMsg (string message, float delay = 0.0f)
	{
        //blocker.gameObject.SetActive(true);
       // bl//ocker.message.text = message;// Locale.GetString(message);
        //blocker.message.Commit();
        //blocker.Show(Camera.main,delay);   
	}
	
	public static void UnlockWithMsg()
	{
        //if(false == blocker.gameObject.activeSelf)
        //    return;

		//blocker.Hide();		
	}

    public static void BlockWithMsg (string message, Camera cam, float delay = 0.0f)
    {
        //Debugger.Assert(cam != null);
        //if(cam == null) return;

		//blocker.gameObject.SetActive(true);
        //blocker.message.text = message;//Locale.GetString(message);
		//blocker.message.Commit();
        //blocker.Show(cam,delay);
    }

    public static void Block (Camera cam)
    {
        Debugger.Assert(cam != null);
        if(cam == null) return;

        tk2dCamera tk2dCam = cam.GetComponent<tk2dCamera>();
        BoxCollider boxCol = cam.GetComponent<BoxCollider>();

        if (boxCol == null) boxCol = cam.gameObject.AddComponent<BoxCollider>();

        boxCol.center = new Vector3(0,0,1f);
        if (tk2dCam == null) 
		{
            boxCol.size = new Vector3(cam.orthographicSize * cam.aspect * 2f, cam.orthographicSize * 2f, 0.1f);
        }
		else
		{
			boxCol.size = new Vector3(tk2dCam.ScreenExtents.width, tk2dCam.ScreenExtents.height, 1f);
            tk2dCameraSettings settings = (tk2dCam.InheritConfig == null) ? tk2dCam.CameraSettings : tk2dCam.InheritConfig.CameraSettings;
            if (settings.orthographicOrigin == tk2dCameraSettings.OrthographicOrigin.BottomLeft) 
			    boxCol.center = new Vector3(tk2dCam.NativeScreenExtents.width * 0.5F, tk2dCam.NativeScreenExtents.height * 0.5F, 1F);
        }

        boxCol.enabled = true;
    }

    public static void UnBlock (Camera cam)
    {
        Debugger.Assert(cam != null);
        if(cam == null) return;
        BoxCollider boxCol = cam.GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            boxCol.enabled = false;
        }
    }

    #endregion
    static void Init()
    {
        InitCache<IGameScene>(kGameSceneTag, _sceneCache, (scene) => {
			popupBaseDepth = Mathf.Max(popupBaseDepth, Camera.main.depth);
        });

		InitCache<BasePopupHandler>(kPopupWndTag, _popupCache);
        InitCache<IUIOverlay>(kUIOverlayTag, _overlayCache, (overlay) => {
			popupBaseDepth = Mathf.Max(popupBaseDepth, Camera.main.depth);
        });

        popupBaseDepth = (int)(popupBaseDepth + 1f);
    }

	static void InitCache<T> (string tagName, Dictionary<string, T> dict, Action<GameObject> eachAction = null) where T : IHandlerBase
    {
        GameObject[] goes = GameObject.FindGameObjectsWithTag(tagName);
        Debugger.Assert(goes.Length == 1, "Root must be one : "+tagName);

		for (int i = 0; i < goes[0].transform.childCount; i++) 
		{
			GameObject go = goes[0].transform.GetChild(i).gameObject;
			T com =	go.GetComponent<T>();
			if(com != null)
			{
				dict[go.name] = com;
				go.SetActive(false);

				if (eachAction != null) eachAction(go);	
			}
        }
    }

//	static void InitCache<T> (string tagName, Dictionary<string, BasePopupHandler> dict, Action<GameObject> eachAction = null) where T : IHandlerBase
//	{
//		GameObject[] goes = GameObject.FindGameObjectsWithTag(tagName);
//		Debugger.Assert(goes.Length == 1, "Root must be one : "+tagName);
//
//		for (int i = 0; i < goes[0].transform.childCount; i++) {
//			BasePopupHandler go = goes[0].transform.GetChild(i).GetComponent<BasePopupHandler>();
//			if(go != null)
//			{
//				dict[go.name] = go;
//				go.gameObject.SetActive(false);
//
//				if (eachAction != null) eachAction(go.gameObject);	
//			}
//		}
//	}
	
	
    public static void SetCurtain (GameObject sceneMember, bool isOn)
    {
        GameObject curtain = GetCurtain(sceneMember);
        if(null == curtain)     return;

		Camera cam = Camera.main;

        tk2dCamera tk2dCam = cam.GetComponent<tk2dCamera>();
        BoxCollider boxCol = curtain.GetComponent<BoxCollider>();

        if (isOn)
		{
            if (tk2dCam == null) 
			{
                boxCol.size = new Vector3(cam.orthographicSize * cam.aspect * 2f, cam.orthographicSize * 2f, 1f) * 1.2f;
            }
			else 
			{
                Vector3 newSize = new Vector3(tk2dCam.ScreenExtents.width, tk2dCam.ScreenExtents.height, 1f) * 1.2f;
                boxCol.size = newSize;
                
//                tk2dCameraSettings settings = (tk2dCam.InheritConfig == null) ? tk2dCam.CameraSettings : tk2dCam.InheritConfig.CameraSettings;
//                if (settings.orthographicOrigin == tk2dCameraSettings.OrthographicOrigin.BottomLeft)
//                    boxCol.center = new Vector3(tk2dCam.ScreenExtents.width * 0.5F, tk2dCam.ScreenExtents.height * 0.5F, 0F);    
            }
        }

        curtain.SetActive(isOn);
    }

    static GameObject GetCurtain (GameObject sceneMember)
    {
        Debugger.Assert(sceneMember.GetComponent(typeof(IHandlerBase)) != null);

        Transform curtain = sceneMember.transform.Find("curtain");
        //Debugger.Assert((curtain != null) && (curtain.GetComponent<BoxCollider>() != null));
        if(null==curtain)       return null;

        return curtain.gameObject;
    }

    static void OnAndroidEscape ()
    {
        passTime += Time.deltaTime;
 
        if(IsBlocked) return;

        if ((Application.platform == RuntimePlatform.Android) || Application.isEditor) 
		{
            if (Input.GetKeyUp(KeyCode.Escape) && (passTime >= ESCAPE_TIME_INTERVAL)) 
			{
                passTime = 0f;
                Escape();
            }
        }
    }

    static void Escape ()
    {
        // process tutorial first.
        //if(true == TutorialOverlayHandler.onEscapeTutorial())
        //    return;
        
        //WorldOverlayHandler handler = Scene.GetOverlayHandler<WorldOverlayHandler>();
        //if(true == handler.IsPlayerOnMove())
        //    return;

        //if(CurrentSceneName()=="WorldScene" && true==WorldSceneHandler.IsOnDailyBonus)
        //    return;
   
        if (currentPopup != null)
			Scene.CurrentPopup().OnEscape();
        else if (currentScene != null)
			currentScene.OnEscape();
        else
            Debug.LogWarning("Escape : Does not response any scene or popup.");
    }

    // title fading은 data loading과 맞물려 있으므로, 다소 다른 타이밍에 적용할 필요가 있다.
    // 이 fading의 해소는 ChangeSceneCoroutine 에서 filter한다.
    public static IEnumerator StartTitleFading()
    {
        //if (false == MainSceneHandler.sLOGIN_FROM_TITLE)
        //    yield break;
        //Scene.Lock();
        Scene.Block(Camera.main);
        // note : 조합 방식으로 변경. by wayne [170901]
        //
        //SpriteRenderer rdrFader = GameObject.Find("5.UIOverlay/dlgFader/sprFaderLogo").GetComponent<SpriteRenderer>();
        //if(null==rdrFader)      yield break;
        //rdrFader.enabled        = true;
        //rdrFader.DOFade(1.0f, 0.5f);
        //
        FaderController fader   = GameObject.Find("5.UIOverlay/dlgFader").GetComponent<FaderController>();
        if(null== fader)        yield break;
        fader._sprBG.gameObject.SetActive( true );
        fader._sprSymbol.gameObject.SetActive( true );
        fader._sprSymbol.sprite = fader.getSprite("tip_logo");
        fader._sprBG.DOFade(1.0f, 0.5f);
        fader._sprSymbol.DOFade(1.0f, 0.5f);

        yield return new WaitForSeconds(0.5f);

        //Scene.Unlock();
    }

    static IEnumerator ChangeSceneCoroutine (string sceneName, object param = null)
    {   
        Debugger.Assert(_sceneCache.ContainsKey(sceneName), "'"+sceneName+"' scene not found.");
        Scene.Block(Camera.main);//Scene.LockWithMsg("");
		IGameScene nextScene = _sceneCache[sceneName];

        // note : 조합 방식으로 변경. by wayne [170901]
        FaderController fader   = GameObject.Find("5.UIOverlay/dlgFader").GetComponent<FaderController>();
        SpriteRenderer rdrFader     = null;
        SpriteRenderer rdrSubFader  = null;

        #region !!! NOTE : title 에서 진입 로딩은 데이터 처리가 있으므로, 타이밍 관계상 다른 루틴을 태운다.
       /* if (currentScene!=null && MainSceneHandler.sLOGIN_FROM_TITLE)
        {
            MainSceneHandler.sLOGIN_FROM_TITLE  = false;
            if(sceneName=="WorldScene" || sceneName=="PlayScene")
            {
                //GameObject obj      = currentScene.GetGameObject();
			   // SetCurtain(obj, true);

                ChangeScene(nextScene, param);
                //PlayerPrefs.Save();

                yield return new WaitForSeconds(Scene.SceneChangeTime);

                // note : StartTitleFading 과 연결됨. 같이 조화되게 처리 할 것. !!!
                fader._sprSymbol.DOFade(.0f, 0.5f);
                fader._sprBG.DOFade(0.0f, 0.5f);

                yield return new WaitForSeconds(0.5f);

                if(false == IsPopupOpen())
                {
                    SetCurtain(nextScene.GetGameObject(), false);
                }

                fader._sprBG.gameObject.SetActive( false );
                fader._sprSymbol.gameObject.SetActive( false );
                Scene.UnBlock(Camera.main);//Scene.UnlockWithMsg();
                yield break;
            }
        }*/
        #endregion

        SpriteTransition sprTr  = _transition as SpriteTransition;        
        if (currentScene != null) 
		{
            GameObject obj      = currentScene.GetGameObject();
			SetCurtain(obj, true);
            if(sceneName == "WorldScene")
            {
                if (currentScene == _sceneCache["PlayScene"])   // 게임 -> 월드맵
                {
                    // 검은색 로딩으로 빠르게 처리하는 경우.
                    Scene.SceneChangeTime = 1.0f;
                    rdrFader = fader._sprDark;

                    // 로딩없이 그냥 처리하는 경우.
                    {
                    //    ChangeScene(nextScene, param);
                    //    Scene.UnBlock(Camera.main);
			        //    if(false == IsPopupOpen())
                    //        SetCurtain(nextScene.GetGameObject(), false);
                    //    yield break;
                    }
                }
                else                                            // 로긴 -> 월드맵
                {
                     // 검은색 로딩으로 빠르게 처리하는 경우.
                    Scene.SceneChangeTime = 1.0f;
                    rdrFader = fader._sprDark;

                    /*Scene.SceneChangeTime = 1.0f;
                    rdrFader    = fader._sprBG; 
                    rdrSubFader = fader._sprSymbol;
                    rdrSubFader.sprite  = fader.getSprite("tip_logo");*/
                }
            }
            else if(sceneName == "PlayScene")                   
            {
                //if (currentScene == _sceneCache["PlayScene"])   // 게임 -> 게임 ( re-try )
                {
                    Scene.SceneChangeTime = 1.0f;
                    rdrFader = fader._sprDark;
                }
            }
        }

        // 모두 아닌 경우       => 월드맵 -> 게임  그리고 기타 나머지.
        // CPR update - 빠르게 로딩하자.
        if (null == rdrFader)
        {
            // 검은색 로딩으로 빠르게 처리하는 경우.
            Scene.SceneChangeTime   = 1.5f;
            rdrFader                = fader._sprDark;

            // 로딩없이 그냥 처리하는 경우.
            /*{
                ChangeScene(nextScene, param);
                Scene.UnBlock(Camera.main);
			    if(false == IsPopupOpen())
                    SetCurtain(nextScene.GetGameObject(), false);
                yield break;
            }*/
        }
        /* CPR update 이전 기존 코드.
        if (null == rdrFader)
        {
            Scene.SceneChangeTime = 1.0f;
            rdrFader            = fader._sprBG; 
            rdrSubFader         = fader._sprSymbol;

            Data.Level lv       = null==param ? null : param as Data.Level;
            rdrSubFader.sprite  = fader.getProperSymbolByLevel(lv);//  fader.getSprite("tip_01");
        }*/
            
        if (null != sprTr)
            sprTr.setSprite(rdrFader, rdrSubFader);
        
        yield return TaskManager.StartCoroutine(_transition.ShowTransitionEffect(Scene.SceneChangeTime,
            () => {
                //Scene.UnlockWithMsg();
                ChangeScene(nextScene, param);
                //PlayerPrefs.Save();
                Scene.UnBlock(Camera.main);
            },  
            () => {
				if(false == IsPopupOpen()) SetCurtain(nextScene.GetGameObject(), false);
            }));
    } 

    public static IEnumerator ShowCurtain( Action todo)
    {   
        // note : 조합 방식으로 변경. by wayne [170901]
        FaderController fader   = GameObject.Find("5.UIOverlay/dlgFader").GetComponent<FaderController>();
        SpriteRenderer rdrFader     = null;
        SpriteRenderer rdrSubFader  = null;
        string sceneName = CurrentSceneName();
        #region !!! NOTE : title 에서 진입 로딩은 데이터 처리가 있으므로, 타이밍 관계상 다른 루틴을 태운다.
       /* if ( MainSceneHandler.sLOGIN_FROM_TITLE)
        {
            MainSceneHandler.sLOGIN_FROM_TITLE  = false;

            if(sceneName=="WorldScene" || sceneName=="PlayScene")
            {
                GameObject obj      = currentScene.GetGameObject();
                SetCurtain(obj, true);

                yield return new WaitForSeconds(Scene.SceneChangeTime);

                // note : StartTitleFading 과 연결됨. 같이 조화되게 처리 할 것. !!!
                fader._sprSymbol.DOFade(.0f, 0.5f);
                fader._sprBG.DOFade(0.0f, 0.5f);
                yield return new WaitForSeconds(0.5f);

                fader._sprBG.gameObject.SetActive( false );
                fader._sprSymbol.gameObject.SetActive( false );

                yield break;
            }
        }*/
        #endregion

        SpriteTransition sprTr  = _transition as SpriteTransition;        
        if (currentScene != null) 
        {
            GameObject obj      = currentScene.GetGameObject();
            SetCurtain(obj, true);
            if(sceneName == "WorldScene")
            {
                if (currentScene == _sceneCache["PlayScene"])   // 게임 -> 월드맵
                    rdrFader    = fader._sprDark;
                else                                            // 로긴 -> 월드맵
                {
                    rdrFader    = fader._sprBG; 
                    rdrSubFader = fader._sprSymbol;
                    rdrSubFader.sprite  = fader.getSprite("tip_logo");
                }
            }
            else if(sceneName == "PlayScene")                   
            {
                if (currentScene == _sceneCache["PlayScene"])   // 게임 -> 게임 ( re-try )
                    rdrFader    = fader._sprDark;
            }
        }   
        // 모두 아닌 경우       => 월드맵 -> 게임  그리고 기타 나머지.
        if(null == rdrFader)
        {
            rdrFader            = fader._sprBG; 
            rdrSubFader         = fader._sprSymbol;
            rdrSubFader.sprite  = fader.getProperSymbolByLevel(null);//  fader.getSprite("tip_01");
        }
            
        if (null!=sprTr)         sprTr.setSprite( rdrFader, rdrSubFader );

        yield return TaskManager.StartCoroutine(_transition.ShowTransitionEffect(Scene.SceneChangeTime,null, null));
    } 

	public static void DirectChangeScene (string sceneName, object param)
	{
		if (currentScene == null) 
			Init();
		
		Debugger.Assert(_sceneCache.ContainsKey(sceneName), "'"+sceneName+"' scene not found.");

		IGameScene nextScene = _sceneCache[sceneName];
		GameObject obj;
		if (currentScene != null) 
		{
			obj = currentScene.GetGameObject();
			if(obj != null)
			{
				SetCurtain(obj, true);
				currentScene.OnLeaveScene();
				obj.SetActive(false);	
			}
		}

		currentScene = nextScene;
		obj = nextScene.GetGameObject();
		obj.SetActive(true);
		nextScene.DoDataExchange();
		nextScene.OnEnterScene(param);
	}
	
	static void ChangeScene (IGameScene nextScene, object param)
    {
        Debugger.Assert(nextScene != null, "ChangeScene : NextScene can't be null");
		GameObject obj;
        if (currentScene != null) 
		{
			obj = currentScene.GetGameObject();
			currentScene.OnLeaveScene();
			obj.SetActive(false);

			if (!moveBackward && (allowOverLap || currentScene != nextScene)) 
				_sceneHistory.Add(obj.name);
        }

        currentScene = nextScene;
		obj = nextScene.GetGameObject();
		obj.SetActive(true);
		nextScene.OnEnterScene(param);
    }

    public static bool IsCurtain()
    {
		return GetCurtain(currentScene.GetGameObject()).activeSelf;
    }

    public static bool SupportsFastBlur() 
	{
        return !SystemInfo.graphicsDeviceName.ToLower().Contains("adreno");
    }

    public static void SetBlur (Camera cam, bool on,float delay = 0.0f) 
	{
        if(SupportsFastBlur()) 
		{
//            Blur blur = cam.GetComponent<Blur>();
//            if(on) 
//			{
//                if(blur == null)
//                    blur = cam.gameObject.AddComponent<Blur>();
//                blur.downsample = 2;
//                blur.blurShader = Shader.Find("Hidden/FastBlur");
//                blur.blurSize = 0;
//                blur.blurIterations = 1;
//				DOTween.Kill(blur);
//				DOTween.To(() => blur.blurSize, x => blur.blurSize = x, 9, 0.3f);
//				DOTween.To(() => blur.blurIterations, x => blur.blurIterations = x, 3, 0.3f);
//				
//                blur.enabled = true;
//            } 
//			else 
//			{
//                if(blur != null) 
//				{
//					DOTween.Kill(blur);
//					DOTween.To(() => blur.blurSize, x => blur.blurSize = x, 0, 0.3f).OnComplete(()=> { blur.enabled = false;});
//					DOTween.To(() => blur.blurIterations, x => blur.blurIterations = x, 1, 0.3f);
//                }
//            }
        } 
		else 
		{
            /*Debug.Log("Scene.SetBlur FastBlur is not supported, switching to BlurEffect");
            BlurEffect blur = cam.GetComponent<BlurEffect>();
            if(on) {
                if(blur == null)
                    blur = cam.gameObject.AddComponent<BlurEffect>();
                blur.blurShader = Shader.Find("Hidden/BlurEffectConeTap");
                blur.enabled = true;
            } else {
                if(blur != null)
                    blur.enabled = false;
            }
			*/
			if(on) 
			{
				if(blurReplacement == null) 
				{
					GameObject prefab = UnityEngine.Resources.Load("messageBlock") as GameObject;
					blurReplacement = GameObject.Instantiate(prefab) as GameObject;
					blurReplacement.name = "blurReplacement";
				}
				blurReplacement.SetActive(true);
				MessageBlock messageBlock = blurReplacement.GetComponent<MessageBlock>();
                messageBlock.Show(cam,delay);
			}
			else 
			{
				Debugger.Assert(blurReplacement != null);
				if(blurReplacement == null) return;
				blurReplacement.SetActive(false);
			}
        }
	}
}
