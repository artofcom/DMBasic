using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;
using DG.Tweening;
using Spine.Unity;
using Spine;

public class TutorialOverlayHandler : MonoBehaviour, IUIOverlay {

#if UNITY_EDITOR
    public const bool TEST_MODE = false;// true;
#else
    public const bool TEST_MODE = false;
#endif

	const float FADESEC = 0.3f;
	const float MSGFADESEC = 0.3f;
    
    public GameManager gameManager;

    public Transform form;
	public tk2dSprite hand;
    public Transform _trPopup;
	//= public TextMesh tapAnywhere;
	public GameObject curtain;
	public GameObject maskPrefab;
    public GameObject maskRoundPrefab;
	public Camera cam;

    //= public tk2dSprite character;
    public tk2dSprite balloonArrow;
    public tk2dSlicedSprite balloonBG;
    public GameObject skipBG;
    //= public GameObject skipIcon;

	public tk2dTextMesh msg;

    public tk2dUIItem           _btnOk, _btnSkip;

    public static Vector3 camPos;
    //= string[] characterList = { "avatar_dummy", "avatar_dummy", "avatar_dummy" };

    //public GameObject tutorial2Target;
    //public GameObject missionTarget;
    //public GameObject moveTarget;
    Sequence seq;
//    Sequence arrowSeq;
//    Sequence moveSeq;

    //Dictionary<int, string> _dicChocoAnis   = new Dictionary<int, string>();
    //SkeletonAnimation _skChoco;

	class Phase {
		public List<Point> blks;
		public int[] movableBlks;

		public List<GameObject> focusObj;
		public List<tk2dUIMask> masks;
		public int pivot;
		public bool touchOnTarget;
		public string text;
		public Vector3 textOffset;
		public bool done;
		public System.Action onEnter = null;
        public int dir;
        public float dimensionsX = 400F;
        public float dimensionsY = 160F;
        //public int charaterIndex = 0;       // no use at this point.
        public bool bBuildMask  = true;
        public string strOkBtn  = null;
        public bool isSkippable = true;
        public Vector3 vOkBtnPos    = new Vector3(0, -7.2f, .0f);
        //public Vector3 vOkBtnPos    = new Vector3(0, -6.16f, .0f);
        public Vector3 vMaskPos     = Vector3.zero;
        public Vector3 vMaskWH      = Vector3.zero;
        public Vector3 vArrowPos    = Vector3.zero;
        public float fArrowRot      = 0;
        //public int idxChocoAni      = 4;            // default animation.
        public string strActiveObject= "";          // if, len>0 then, 특정 object가 active될때까지 기다린다.
        public float fArrowScale    = 0.8f;         // 
        public float fFingerScale   = 0.8f;
        Dictionary<GameObject, int> dicOrderInLayer = new Dictionary<GameObject, int>();
        public void adjustOrderLayer(Transform trParent, int newOrder, bool reset=false)
        {
            if(null==trParent)  return;

            if(reset)           dicOrderInLayer.Clear();

            tk2dSprite[] sprites= trParent.GetComponentsInChildren<tk2dSprite>(true);
            for(int q = 0; q < sprites.Length; ++q)
            {
                if(true == dicOrderInLayer.ContainsKey(sprites[q].gameObject))
                    continue;
                dicOrderInLayer.Add( sprites[q].gameObject, sprites[q].SortingOrder );
                sprites[q].SortingOrder = newOrder;
            }
           /* tk2dTextMesh[] meshes= trParent.GetComponentsInChildren<tk2dTextMesh>();
            for(int q = 0; q < meshes.Length; ++q)
            {
                dicOrderInLayer.Add( meshes[q].gameObject, meshes[q].SortingOrder );
                meshes[q].SortingOrder = newOrder;
            }*/
            tk2dSlicedSprite[] slice= trParent.GetComponentsInChildren<tk2dSlicedSprite>(true);
            for(int q = 0; q < slice.Length; ++q)
            {
                if(true == dicOrderInLayer.ContainsKey(slice[q].gameObject))
                    continue;
                dicOrderInLayer.Add( slice[q].gameObject, slice[q].SortingOrder );
                slice[q].SortingOrder = newOrder;
            }
            MeshRenderer[] mesh = trParent.GetComponentsInChildren<MeshRenderer>(true);
            for(int q = 0; q < mesh.Length; ++q)
            {
                if(true == dicOrderInLayer.ContainsKey(mesh[q].gameObject))
                    continue;
                dicOrderInLayer.Add( mesh[q].gameObject, mesh[q].sortingOrder );
                mesh[q].sortingOrder = newOrder;
            }
        }
        public void recoverOrderLayer()
        {
            foreach(GameObject obj in dicOrderInLayer.Keys) 
		    {
                tk2dSprite spr  = obj.GetComponent<tk2dSprite>();
                if(null!= spr)  spr.SortingOrder= dicOrderInLayer[obj];

               // tk2dTextMesh mesh= obj.GetComponent<tk2dTextMesh>();
               // if(null!= mesh) mesh.SortingOrder= dicOrderInLayer[obj];

                tk2dSlicedSprite slice= obj.GetComponent<tk2dSlicedSprite>();
                if(null!= slice) slice.SortingOrder= dicOrderInLayer[obj];

                MeshRenderer ani    = obj.GetComponent<MeshRenderer>();
                if(null!=ani)       ani.sortingOrder    = dicOrderInLayer[obj];
            }
            dicOrderInLayer.Clear();
        }
	}
	Phase current;

    public class Condition {

        public enum STATUS
        {
            NONE = 0, STARTED, SKIPPED, FINISHED
        };

        public string id;
        public int themeIdx;
        public System.Func<int, string, bool> func;
        public bool once;
        public Condition(string _id, System.Func<int, string, bool> _func, bool _once) {
            id = _id;
            func = _func;
            once = _once;
        }
        
		public bool Check(int stage, string strPopupName) 
		{
            // test for now !!!
            // return false;
            // return func(stage, strPopupName);
#if UNITY_EDITOR
            // 에디터 모드 일때는 once 및 prefab data 무관하게, condition만 보고 다시 하게 한다.
            if(true==LevelEditorSceneHandler.EditorMode || true==TEST_MODE)
                return func(stage, strPopupName);
#endif            
            STATUS eStatus      = (STATUS)Root.Data.gameData.getTutorialStatusByID(id); // PlayerPrefs.GetInt(id, 0);
            if(true == once)    // 무조건 한번 나오고 다시 안나온다.
            {
                // 한번 했으면 다시 안나온다.
                if(eStatus != STATUS.NONE)
                    return false;

                // note : 초기화 되어있지 않은데(cache가 지워져서 없다 or 첫시도?) current level이 target 보다 크면 once는 clear(return false) 로 간주한다.
                int idxCurrentLevel = Root.Data.currentLevel.Index;                     // 현재 진행 중 레벨.
                int idxTutorialLevel= -1;
                if(true == func(stage, strPopupName))                                   // 대상 레벨 찾기.
                {
                   // if(strPopupName=="ExchangePopup" && stage>=0 && stage<Root.Data.treasureIndex.Length)
                   //     idxTutorialLevel    = Root.Data.treasureIndex[stage];
                   // else 
                        idxTutorialLevel    = stage - 1;
                }
                    
                if(-1==idxTutorialLevel || idxTutorialLevel<idxCurrentLevel)            // 대상레벨이 아니거나, 대상레벨을 지나쳐왔으면 - 안하기로.
                    return false;
            }
            else                                // 해당 stage를 skip하지 않고 clear 했을때만 다시 나오지 않도록.
            {
                Data.LevelResultData resultData = Root.Data.gameData.GetLevelResultDataByIndex(stage-1);
                if(eStatus==STATUS.FINISHED && null!=resultData && true==resultData.bCleared)
                    return false;
            }
            return func(stage, strPopupName);
        }

        public void setStatus(Condition.STATUS eStaus)
        {
			Data.Tutorial info  = new Data.Tutorial();
			info.strID          = id; 
			info.nStatus        = (int)eStaus;
			Root.Data.gameData.AddTutorialInfo(info /* , auto save*/);

			//Root.Data.gameData.SaveContext();// save			
            // PlayerPrefs.SetInt(id, (int)eStaus);
        }
    }
    static List<Condition> conditions;
    static Condition currentCond;

    static float bottomLine;
    static float dlgBound;

    void Awake() {
        
        bottomLine = GetBannerPositionYInWorld(gameObject, 0);
        //= tapAnywhere.transform.SetLocalPositionY(bottomLine+1);
        dlgBound = -bottomLine - 4;
    }

    public void DoDataExchange() {
    
    }
	
	public GameObject GetGameObject()
	{
		return gameObject;
	}

    static TutorialOverlayHandler instance;
    public void OnEnterUIOveray (object param) {
		
        instance = this;
	    JMFRelay.OnPlayerMove += OnPlayerMove;
        JMFRelay.OnBoardStable += OnBoardStable;

		//todo check list
        // re-position 할 필요 없음.
        // cam.transform.localPosition = camPos;
        // form.transform.localPosition = cam.transform.localPosition;
        form.transform.SetLocalPositionZ(0);
        form.transform.SetLocalPositionY(0.5f);

        StartCoroutine(currentCond.id);
    }

    public void OnLeaveUIOveray () {
        instance = null;
	    JMFRelay.OnPlayerMove -= OnPlayerMove;
        JMFRelay.OnBoardStable -= OnBoardStable;

		StopAllCoroutines();
		if(current != null)
			PhaseExit(current);
        StartCoroutine(TutorialExit());
        currentCond = null;
	}

    static void MakeConditions () {
        Debugger.Assert(conditions == null);

        conditions = new List<Condition>();
		if (Root.GetPostfix () == "A")
        {
		    conditions.Add (new Condition ("tutorial_A_1", (stage, strPopup) => (stage==1 && strPopup==""), true)); //match3
            conditions.Add (new Condition ("tutorial_A_2", (stage, strPopup) => (stage==2 && strPopup==""), true)); //가로,세로 match4
            conditions.Add (new Condition ("tutorial_A_3", (stage, strPopup) => (stage==3 && strPopup==""), true)); //L,T match5, 라인+폭탄 조합	
            conditions.Add (new Condition ("tutorial_A_5", (stage, strPopup) => (stage==5 && strPopup==""), true)); //2x2매치
			conditions.Add (new Condition ("tutorial_A_6", (stage, strPopup) => (stage==6 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_7", (stage, strPopup) => (stage==7 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_9", (stage, strPopup) => (stage==9 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_10", (stage, strPopup) => (stage==10 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_11", (stage, strPopup) => (stage==11 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_12", (stage, strPopup) => (stage==12 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_13", (stage, strPopup) => (stage==13 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_14", (stage, strPopup) => (stage==14 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_16", (stage, strPopup) => (stage==16 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_20", (stage, strPopup) => (stage==20 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_24", (stage, strPopup) => (stage==24 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_25", (stage, strPopup) => (stage==25 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_54", (stage, strPopup) => (stage==54 && strPopup==""), true)); //진흙바닥	
            conditions.Add (new Condition ("tutorial_A_58", (stage, strPopup) => (stage==58 && strPopup==""), true)); //진흙바닥	

		}
		
    }

	public static string GetAvailable(int stage, string strPopupName="") {
        if(conditions == null) 
            MakeConditions();

		//if(gameObject.activeSelf) return null;
        foreach(var c in conditions) {
            if(c.Check(stage, strPopupName)) {
                return c.id;
            }
        }
        return null;
	}


	public static void Activate(string condId) {
        foreach(var c in conditions) {
            if(c.id == condId) {
                currentCond = c;
                Scene.ShowOverlay("TutorialOverlay");   // 내부적으로 trigger 시킴.
                //gameObject.SetActive(true);
                //c.setStatus(Condition.STATUS.STARTED);
                //StartCoroutine(c.id);
                //Debug.Log("TutorialHandler.Activate");
                return;
            }
        }
	}

	public static void Deactivate() {
        if(instance != null)
            Scene.CloseOverlay("TutorialOverlay");
    }

    public static string getActiveTutorialId()
    {
        if(null == instance)    return null;
        return instance.GetActiveTutorialId();
    }

    public string GetActiveTutorialId() {
        if(!gameObject.activeSelf) return null;
        if(currentCond == null) return null;
        return currentCond.id;
    }

#region tutorial
	IEnumerator TutorialEnter() {

        _btnOk.gameObject.SetActive( false );
        _btnSkip.gameObject.SetActive( false );

        if(null!=currentCond)   currentCond.setStatus(Condition.STATUS.STARTED);

		//= tapAnywhere.gameObject.SetActive(false);
		hand.gameObject.SetActive(false);
		//arrow.gameObject.SetActive(false);
        yield return null;
		yield return StartCoroutine(WaitForStable());
		//curtain.FadeIn(FADESEC, 0.65f);
	}

	IEnumerator TutorialExit() {
		//curtain.FadeOut(FADESEC);

        if(null!=currentCond)   currentCond.setStatus(Condition.STATUS.FINISHED);

		yield return new WaitForSeconds(FADESEC);
		//gameObject.SetActive(false);
        Scene.CloseOverlay("TutorialOverlay");
	}

	IEnumerator WaitForStable() {
        while(!stable)
			yield return null;
	}
    
    float _getHeightByLineNumber(int line)
    {
        return 90.0f + 25.0f * ((float)line);
    }






//++++++++++++++++++++++++++++++++++++++++++++++++++++++
//++++++++++++++++++++++++++++++++++++++++++++++++++++++
//++++++++++++++++++++++++++++++++++++++++++++++++++++++
//++++++++++++++++++++++++++++++++++++++++++++++++++++++


// 첫 인사, 소개, 매치3
    IEnumerator tutorial_A_1() {
        yield return StartCoroutine(TutorialEnter());

        Phase phase             = null;
        
       
        //Red 가로 매치3
        phase                   = MakeExplicitBlockPhase(new int[]{ 5,4, 5,3, 4,3, 3,3 }, 0, 3, // 손가락 시작 위치 & 손가락 움직임 방향
                                    "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(3);
        phase.movableBlks       = new int[]{0,1}; // 터치 영역
        //phase.isSkippable       = false; // - On > skip 버튼 비활성.s

        yield return StartCoroutine(ProcessPhase(phase));
        yield return StartCoroutine(TutorialExit());
    }
    


//라인블럭     
    IEnumerator tutorial_A_2()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        phase = MakeExplicitBlockPhase(new int[]{ 2,3, 3,3, 4,3, 5,3, 4,4 }, 4, 3, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{2,4}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        //라인블럭 사용 액션유도
        phase = MakeExplicitBlockPhase(new int[]{ 4,3, 4,4, 4,5, 5,4 }, 3, 4, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(3);
        phase.movableBlks       = new int[]{1,3}; // 터치 영역
        //phase.idxChocoAni       = 2;
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_3()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        phase = MakeExplicitBlockPhase(new int[]{ 3,6, 3,5, 3,4, 3,3, 4,5, 5,5 }, 0, 3, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{0,1}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        //라인블럭 사용 액션유도
        phase = MakeExplicitBlockPhase(new int[]{ 3,3, 4,3, 5,3, 6,3 }, 0, 2, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(3);
        phase.movableBlks       = new int[]{0,1}; // 터치 영역
        //phase.idxChocoAni       = 2;
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_5()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        phase = MakeExplicitBlockPhase(new int[]{ 1,3, 1,2, 2,4, 2,3, 2,2 }, 2, 3, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{2,3}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

     IEnumerator tutorial_A_6()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        phase = MakeExplicitBlockPhase(new int[]{ 4,5, 4,4, 3,4, 2,4, 5,4, 6,4 }, 0, 3, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{0,1}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        //라인블럭 사용 액션유도 
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 4,4, 4,3 }, 0, 3, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(3);
        phase.movableBlks       = new int[]{0,1}; // 터치 영역
        //phase.idxChocoAni       = 2;
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_7()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 1,2, 2,2, 3,2, 4,2 }, 3, 4, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{2,3}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_9()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        phase = MakeExplicitBlockPhase(new int[]{ 2,1, 2,2, 2,3, 2,4 }, 0, 1, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{0,1}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        //라인블럭 사용 액션유도 
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 2,1, 3,1, 4,1, 5,1 }, 0, 2, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(3);
        phase.movableBlks       = new int[]{0,1}; // 터치 영역
        //phase.idxChocoAni       = 2;
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_10()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 3,3, 4,3, 5,3, 6,3 }, 3, 4, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{2,3}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_11()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 2,1, 3,1, 4,1, 5,1 }, 3, 4, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{2,3}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_12()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 1,4, 2,4, 3,4, 4,4 }, 3, 4, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{2,3}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_13()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 3,5, 4,5, 5,5, 6,5 }, 3, 4, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{2,3}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_14()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 4,3, 4,4, 4,5, 4,6 }, 0, 1, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{0,1}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_16()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 3,3, 3,4, 3,5, 3,6 }, 0, 1, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{0,1}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_20()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 2,2, 2,3, 2,4, 2,5 }, 0, 1, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{0,1}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_24()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 2,0, 2,1, 2,2, 2,3 }, 3, 3, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{2,3}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }

    IEnumerator tutorial_A_25()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 1,2, 2,2, 3,2, 4,2 }, 3, 4, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{2,3}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }
    IEnumerator tutorial_A_54()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 0,2, 1,2, 2,2, 1,3 }, 3, 3, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{1,3}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }
    IEnumerator tutorial_A_58()
    {
        yield return StartCoroutine(TutorialEnter());

        Phase phase = null;
        //가로4매치 액션유도
        // 1-up, 2-right, 3-down, 4-left
        phase = MakeExplicitBlockPhase(new int[]{ 6,1, 6,2, 6,3, 6,4 }, 3, 3, "", Vector3.one * 1000.0f);
        phase.dimensionsY       = _getHeightByLineNumber(1);
        phase.movableBlks       = new int[]{2,3}; // 터치 영역
        yield return StartCoroutine(ProcessPhase(phase));
        
        yield return StartCoroutine(TutorialExit());
    }


#endregion




























#region Phase
	tk2dUIMask MakeMaskFor(Board board, bool touch) {
		var go = Instantiate(maskPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //var go = Instantiate(maskRoundPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		go.transform.parent     = transform;
		var mask                = go.GetComponent<tk2dUIMask>();

        GameObject target;
        if (board.Piece == null || board.Piece.GO == null) {
			target              = board.Panel[BoardPanel.TYPE.BACK].gameObject;
			if (target == null) target = board.Panel[BoardPanel.TYPE.FRONT].gameObject;
        } else {
            target              = board.Piece.GO;
        }

		//Bounds bound = Scene.GetRenderBound(target);
		//Bounds _bound = GetActivedRenderBound(target);
		//Vector2 size = new Vector2(_bound.size.x, _bound.size.y);
		//mask.size = size + new Vector2(0.15f, 0.15f);// - new Vector2(0.02f, 0.02f);
        Vector2 size            = Vector2.one * (JMFUtils.GM.Size);//+0.1f);
        mask.size               = size;
		mask.Build();

        //go.transform.localScale = new Vector3(1.0f, size.y*0.45f, size.x*0.45f);
        //go.transform.localEulerAngles   = new Vector3(.0f, 90.0f, .0f);

        var item                = //go.transform.Find("uiItem").GetComponent<tk2dUIItem>();
                                go.AddComponent<tk2dUIItem>();
        item.isHoverEnabled     = false;
        item.sendMessageTarget  = gameObject;
        //item.OnClickUIItem += OnClickTarget;
        var box                 = item.GetComponent<BoxCollider>();
        box.size                = new Vector3(size.x, size.y, 10);

		if (touch) {
            PieceTracker tracker= item.gameObject.AddComponent<PieceTracker>();
            tracker.PT          = board.PT;
		}

		//go.transform.localPosition = target.transform.localPosition;
        go.transform.position   = target.transform.position;
		go.transform.SetLocalPositionZ(-123);
		return mask;
	}

	tk2dUIMask MakeMaskFor(GameObject target, bool touch) {
		//var go = Instantiate(maskPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        var go = Instantiate(maskRoundPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		go.transform.parent = transform;
		var mask = go.GetComponent<tk2dUIMask>();

		//Bounds bound = Scene.GetRenderBound(target);
		Bounds _bound = GetActivedRenderBound(target);

        go.transform.localScale = new Vector3(1.0f, _bound.size.y*0.45f, _bound.size.x*0.45f);
        go.transform.localEulerAngles   = new Vector3(.0f, 90.0f, .0f);

		Vector2 size = new Vector2(_bound.size.x, _bound.size.y);
		mask.size = size - new Vector2(0.02f, 0.02f);
		mask.Build();

		if(touch) {
			//var item = go.AddComponent<tk2dUIItem>();
            var item = go.transform.Find("uiItem").GetComponent<tk2dUIItem>();
			item.isHoverEnabled = false;
			item.sendMessageTarget = gameObject;

            item.OnClickUIItem += OnClickTarget;

			var box = item.GetComponent<BoxCollider>();
			box.size = new Vector3(size.x, size.y, 1);
		}

		go.transform.localPosition = target.transform.localPosition;
		go.transform.SetLocalPositionZ(1.2f);
		return mask;
	}

	tk2dUIMask MakeMaskFor(Bounds _bound, bool touch) {
		//var go = Instantiate(maskPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        var go = Instantiate(maskRoundPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		go.transform.parent = transform;
		var mask = go.GetComponent<tk2dUIMask>();

        go.transform.localScale = new Vector3(1.0f, _bound.size.y*0.45f, _bound.size.x*0.45f);
        go.transform.localEulerAngles   = new Vector3(.0f, 90.0f, .0f);

		Vector2 size = new Vector2(_bound.size.x, _bound.size.y);
		mask.size = size - new Vector2(0.02f, 0.02f);
		mask.Build();

        tk2dUIItem uiItem       = go.transform.Find("uiItem").GetComponent<tk2dUIItem>();
        var box = uiItem.GetComponent<BoxCollider>();
		if(touch) {
			uiItem.isHoverEnabled = false;
			uiItem.sendMessageTarget = gameObject;
			uiItem.OnClickUIItem += OnClickTarget;

            box.enabled = true;
			box.size = new Vector3(size.x, size.y, 1);
		} else {
            box.enabled = false;
        }

		go.transform.localPosition = _bound.center;//target.transform.position;
		go.transform.SetLocalPositionZ(1.2f);
		return mask;
	}

	Phase MakeExplicitBlockPhase(int[] pts, int pivot, int dir, string txt, Vector3 txtOffset) {
		Phase p = new Phase();

        p.dir = dir;
		p.touchOnTarget = true;
		p.blks = new List<Point>();
		p.pivot = pivot;
        for(int i=0; i<pts.Length; i+=2) {
            //var board = gameManager.iBoard(new int[]{pts[i], pts[i+1]});
			p.blks.Add(new Point(pts[i], pts[i+1]));
		}
		p.text = txt;
		p.textOffset = txtOffset;
		return p;
	}

	Phase MakePhase(GameObject[] focus, int pivot, bool targeted, string txt, Vector3 txtOffset) {
		Phase p = new Phase();

		p.touchOnTarget = targeted;
        if(focus != null)
            p.focusObj = new List<GameObject>(focus);
		p.pivot = pivot;
		p.text = txt;
		p.textOffset = txtOffset;
		return p;
	}

    // 특정 object가 activate 될때까지 기다린다.
    IEnumerator _coWaitActiveObject(Phase p)
    {
        if(null==p || p.strActiveObject.Length==0)
            yield break;

        while(true)
        {
            GameObject obj      = GameObject.Find( p.strActiveObject );
            if(null!=obj && true==obj.activeSelf)
                break;
            yield return new WaitForSeconds(0.1f);
        }
    }

	void PhaseEnter(Phase p)
    {
		current = p;

        NNSoundHelper.Play("PFX_common_intro");
        curtain.SetActive( true );
        curtain.FadeIn(FADESEC, 0.8f);

        balloonBG.dimensions = new Vector2(p.dimensionsX, p.dimensionsY);

		msg.text = p.text;

		_trPopup.localPosition = p.textOffset;
		_trPopup.SetLocalPositionX(0);
		_trPopup.SetLocalPositionZ(-1);
        _trPopup.gameObject.SetActive(true);
        _trPopup.transform.localScale   = Vector3.one*0.01f;
        _trPopup.transform.DOScale(1.0f, FADESEC);

        _btnOk.gameObject.SetActive( null!=p.strOkBtn );
        _btnSkip.gameObject.SetActive( p.isSkippable );
        if(null != p.strOkBtn)
        {
            _btnOk.transform.Find("tm/Text").GetComponent<tk2dTextMesh>().text = p.strOkBtn;
            _btnOk.transform.position   = new Vector3(p.vOkBtnPos.x, p.vOkBtnPos.y, _btnOk.transform.position.z);
        }
        //= character.spriteName = characterList[p.charaterIndex];
		msg.gameObject.FadeIn(MSGFADESEC);
        balloonArrow.gameObject.SetActive( false );
        hand.gameObject.SetActive( false );
        balloonBG.gameObject.FadeIn(MSGFADESEC);
        //= character.gameObject.FadeIn(MSGFADESEC);
        skipBG.FadeIn(MSGFADESEC);
        //= skipIcon.FadeIn(MSGFADESEC);

		p.masks = new List<tk2dUIMask>();

        //arrow.gameObject.SetActive(false);
		//tapAnywhere.gameObject.SetActive(false);
        //
        if(p.blks != null) {
			//= bound.transform.localPosition = gameManager.spineBoard.transform.localPosition;//Vector3.one * -5.88f;
			//= bound.transform.SetLocalPositionZ(0);

            Board[] boards = p.blks.ConvertAll(g => gameManager[g.X,g.Y]).ToArray(); 
            //= bound.blockScale = gameManager.Size;
            //= bound.SetBlockPositions(p.blks.ToArray());
            //= bound.AnimateTex();
            //= bound.gameObject.FadeIn(FADESEC);

            for(int b=0; b<boards.Length; ++b)  {
                p.masks.Add( MakeMaskFor(boards[b], (p.movableBlks==null) ? (true) : (System.Array.IndexOf(p.movableBlks, b) != -1) ));
            }

            if(p.touchOnTarget && p.dir != 0) {
                hand.gameObject.SetActive( true );
                hand.transform.position = p.masks[p.pivot].transform.position;
                hand.transform.SetLocalPositionZ(-150);
                //hand.gameObject.Blink(0.5f);
                hand.gameObject.FadeIn(0.3f);

//                seq = new Sequence(new SequenceParms().Loops(-1));
				seq = DOTween.Sequence();
				seq.SetLoops(-1);

                float dur = 1;
                switch(p.dir) {
                case 1://up
                    //seq.Append( hand.gameObject.SlideY(gameManager.Size, dur) );
                    seq.Append( hand.gameObject.transform.DOMoveY(gameManager.Size, dur).SetRelative(true) );
                    break;
                case 2://right
                    //seq.Append( hand.gameObject.SlideX(gameManager.Size, dur) );
                    seq.Append( hand.gameObject.transform.DOMoveX(gameManager.Size, dur).SetRelative(true) );
                    break;
                case 3://down
                    //seq.Append( hand.gameObject.SlideY(-gameManager.Size, dur) );
                    seq.Append( hand.gameObject.transform.DOMoveY(-gameManager.Size, dur).SetRelative(true) );
                    break;
                case 4://left
                    //seq.Append( hand.gameObject.SlideX(-gameManager.Size, dur) );
                    seq.Append( hand.gameObject.transform.DOMoveX(-gameManager.Size, dur).SetRelative(true) );
                    break;
                case 5://touch
                    float offset = 0.2f;
                    //seq.Append( hand.gameObject.SlideLocal(new Vector3(offset, -offset, 0), dur*0.5f) );
                    //seq.Append( hand.gameObject.SlideLocal(new Vector3(-offset, offset, 0), dur*0.5f) );
                    seq.Append( hand.gameObject.transform.DOMove(new Vector3(offset, -offset, 0), dur*0.5f).SetRelative(true) );
                    seq.Append( hand.gameObject.transform.DOMove(new Vector3(-offset, offset, 0), dur*0.5f).SetRelative(true) );
                    break;
                }
                if(p.dir != 5) {
                    //seq.Append( hand.gameObject.SlideLocalAbs(hand.transform.localPosition,0));
                    seq.Append( hand.gameObject.transform.DOMove(hand.transform.position, 0) );
                    seq.AppendInterval( 0.5f );
                }
                seq.Play();
            } 
            //if(p.dir == 0)
             //   p.touchOnTarget = false;

        } else if(p.focusObj != null){
			//= bound.transform.localPosition = Vector3.zero;
			//= bound.transform.SetLocalPositionZ(-30);
			var bb = Scene.GetRenderBound(p.focusObj[0]);
			for(int i=0; i<p.focusObj.Count; ++i)
            {
                if(i>0)         bb.Encapsulate(Scene.GetRenderBound(p.focusObj[i]));     
            }
            bb.Expand(new Vector3(0.5f,0.5f,0.5f));

            var ownCam = Scene.FindCameraForObject(p.focusObj[0]);
            if(null != ownCam)  
                bb.center       = ownCam.transform.InverseTransformPoint(bb.center);
            bb.center += camPos;
            balloonArrow.gameObject.SetActive( false==p.vArrowPos.Equals(Vector3.zero) );
            if(false == p.fArrowRot.Equals( .0f ))
                balloonArrow.transform.localEulerAngles = new Vector3(0, 0, p.fArrowRot);
            //balloonArrow.transform.position = bb.center;
			//= bound.SetRect(new Rect(bb.min.x, bb.min.y, bb.size.x, bb.size.y));
            //= bound.AnimateTex();
            bb.Expand(new Vector3(0.1f,0.1f,0.1f));

            // mask offset.
            if(false == p.vMaskPos.Equals(Vector3.zero))
                bb.center       = p.vMaskPos;
            if(false == p.vMaskWH.Equals(Vector3.zero))
                bb.extents      = p.vMaskWH * 0.5f;
            if(false == p.vArrowPos.Equals(Vector3.zero))
            {
                balloonArrow.transform.position = new Vector3(p.vArrowPos.x, p.vArrowPos.y, balloonArrow.transform.position.z);
                balloonArrow.gameObject.FadeIn(MSGFADESEC);
                //튜토리얼 화살표 에니 옵션값
                balloonArrow.transform.DOMoveY(0.5f, 0.5f).SetRelative(true).SetLoops(-1, LoopType.Yoyo);
            }
            //

            if(p.bBuildMask)    p.masks.Add(MakeMaskFor(bb, p.touchOnTarget));

            if(!p.touchOnTarget) {
                hand.gameObject.SetActive(p.vArrowPos.Equals(.0f) );
                Vector3 arrowPos = bb.center;
                arrowPos.y = bb.min.y;
                hand.transform.position = arrowPos;
                hand.transform.SetLocalPositionZ(-5);
//                HOTween.To(hand.transform, 0.3f, 
//                    new TweenParms().Prop("localPosition", new Vector3(0, 0.2f, 0), true).Loops(-1, LoopType.Yoyo));
				hand.transform.DOMoveY(0.2f, 1.0f).SetRelative(true).SetLoops(-1, LoopType.Yoyo);
                hand.transform.DOMoveX(0.2f, 1.0f).SetRelative(true).SetLoops(-1, LoopType.Yoyo);
            }
            //= bound.gameObject.FadeIn(FADESEC);
            if(p.touchOnTarget) {
                hand.gameObject.SetActive(p.vArrowPos.Equals(.0f) );
                hand.transform.position = p.focusObj[p.pivot].transform.position;
                hand.transform.SetLocalPositionZ(-1);
                //hand.gameObject.Blink(0.5f);
                hand.gameObject.FadeIn(0.3f);

                hand.transform.DOMoveY(0.2f, 1.0f).SetRelative(true).SetLoops(-1, LoopType.Yoyo);
                hand.transform.DOMoveX(0.2f, 1.0f).SetRelative(true).SetLoops(-1, LoopType.Yoyo);
            } 
		}

		if(!p.touchOnTarget /*|| (p.blks!=null && p.blks.Count==1)*/) {
			//= tapAnywhere.gameObject.SetActive(true);
			//= tapAnywhere.gameObject.Blink2(-1, 1f, 0.7f);
		} else {
			//= tapAnywhere.gameObject.SetActive(false);
        }

        if(true == hand.gameObject.activeSelf)
        {
            hand.transform.localScale           = Vector3.one * p.fFingerScale;
            balloonArrow.gameObject.SetActive( false );
            balloonArrow.transform.DOKill();
        }
        if(true == balloonArrow.gameObject.activeSelf)
        {
            balloonArrow.transform.localScale   = Vector3.one * p.fArrowScale;
            hand.gameObject.SetActive( false );
            hand.transform.DOKill();
            hand.transform.position = Vector3.down * 1000.0f;
        }


		if (p.onEnter != null) p.onEnter();
	}

	void PhaseExit(Phase p) {
		curtain.FadeOut(FADESEC);
		hand.gameObject.FadeOut(FADESEC);
//        HOTween.Kill(hand.transform);
		DOTween.Kill(hand.transform);
        DOTween.Kill(balloonArrow.transform);
        hand.gameObject.SetActive(false);
        p.recoverOrderLayer();
        //= tapAnywhere.gameObject.FadeOut(FADESEC);
        //= bound.gameObject.FadeOut(FADESEC);
        if(balloonArrow.gameObject.activeSelf)
        {
            balloonArrow.gameObject.FadeOut(FADESEC);
            balloonArrow.DOKill();
        }
        balloonArrow.transform.localEulerAngles = Vector3.zero;
            
		foreach(var m in p.masks)
			Object.Destroy(m.gameObject);
		p.masks = null;
        if(null != p.focusObj)
        {
            for (int q = 0; q < p.focusObj.Count; ++q)
            {
                tk2dUIItem tItem    = p.focusObj[q].GetComponent<tk2dUIItem>();
                if(null != tItem)   tItem.OnClickUIItem -= OnClickTarget;
            }
            p.focusObj = null;
        }

        if(seq != null) {
            seq.Kill();
            seq = null;
        }

		current = null;

		msg.gameObject.FadeOut(MSGFADESEC);
        balloonBG.gameObject.FadeOut(MSGFADESEC);
        //= character.gameObject.FadeOut(MSGFADESEC);
        skipBG.FadeOut(MSGFADESEC);
        //=  skipIcon.FadeOut(MSGFADESEC);

        _btnOk.gameObject.SetActive( false );
        _btnSkip.gameObject.SetActive( false );

        //        if(board.localPosition.y > 0) 
        //            HOTween.To(board, MSGFADESEC, 
        //                new TweenParms().Prop("localPosition", new Vector3(0, 8, -1)).Ease(EaseType.Linear));
        //        else
        //            HOTween.To(board, MSGFADESEC, 
        //                new TweenParms().Prop("localPosition", new Vector3(0, -8, -1)).Ease(EaseType.Linear));

        //if (board.localPosition.y > 0)
		//	board.DOLocalMove(new Vector3(0, 8, -1), MSGFADESEC).SetEase(Ease.Linear);
		//else
	//		board.DOLocalMove(new Vector3(0, -8, -1), MSGFADESEC).SetEase(Ease.Linear);
    
        //= _trPopup.transform.DOScale(0.01f, FADESEC).OnComplete( () =>
        //{
        // 바로 끄는 것으로 변경.
            _trPopup.gameObject.SetActive( false );
            curtain.SetActive( false );
        //});

        NNSoundHelper.Play("PFX_common_close");
	}

	IEnumerator ProcessPhase(Phase p) {
        while(_trPopup.gameObject.activeSelf)                   // 이전 action 종료까지 대기.
            yield return null;
        yield return StartCoroutine( _coWaitActiveObject(p) );  // 즉정 object를 기다려야 한다면 기다린다.
        PhaseEnter(p);
		while(!current.done)
			yield return null;
        PhaseExit(p);
		yield return new WaitForSeconds(FADESEC);
		yield return StartCoroutine(WaitForStable());
	}

#endregion

#region handlers
    void OnClickTarget(tk2dUIItem _item){
		if(current == null) return;

		if(!current.touchOnTarget) {
			current.done = true;
			return;
		}

        tk2dUIMask mask         = null!=_item.transform.parent ? _item.transform.parent.GetComponent<tk2dUIMask>() : null;
        if(null == mask)        // 이것은 masking이 아닌 direct하게 button에 연결된 것이다. 따라서...
        {
            current.done        = true;
			return;
        }

        if(current.focusObj != null) {
            //Debug.Log("OnClickTarget");
            tk2dUIItem item = current.focusObj[current.pivot].GetComponent<tk2dUIItem>();
            if(item != null) {
               item.SimulateClick ();
            } 
            current.done = true;
        }

        if(current.blks != null) {
            if(current.blks.Count == 1) {
                //JMFRelay.dlgOnPanelClick(current.blks[0].X,current.blks[0].Y);
                current.done = true;
            }
        }
	}

    void OnClickNext()
    {
        if(current == null)     return;
        current.done            = true;
    }

    void OnClickAny(tk2dUIItem btnCurtain){
		if(current == null)     return;
		
        // => anyplace skip 은 이제 없다....
        // if(!current.touchOnTarget) {
		//	current.done = true;
		//}

        // focus object에 touch하면 해당 object로 event를 보내준다.
        if(null!=current.focusObj && current.pivot>=0 && current.pivot<current.focusObj.Count)
        {
            tk2dUIItem item     = current.focusObj[current.pivot].GetComponent<tk2dUIItem>();
            if(null != item)
            {
                Bounds bound    = Scene.GetRenderBound( item.gameObject );
                Vector3 vTouch  = Camera.main.ScreenToWorldPoint( new Vector2(btnCurtain.Touch.position.x, btnCurtain.Touch.position.y) );
                if(bound.Contains( new Vector3(vTouch.x, vTouch.y, bound.center.z) ))
                    item.SimulateClick();
            }
        }
	}

    void OnClickSkip(){

        if(null != currentCond)
        {
            //if(currentCond.id=="tutorial_A_13" && -1==JMFUtils.GM.getAiMoveWaited())
            if(JMFUtils.GM.isAIFightMode && -1==JMFUtils.GM.getAiMoveWaited())
                JMFUtils.GM.fire_AiTurn(!JMFUtils.GM.isCurPlayerAI, false);
        }

        Deactivate();
		//if(current == null) return;
        //current.done = true;

        if(null != currentCond) 
            currentCond.setStatus(Condition.STATUS.SKIPPED);
    }

#endregion

    bool stable = true;
    void OnPlayerMove () {
        stable = false;

		if(current == null) return;
		if(current.touchOnTarget) {
			current.done = true;
			return;
		}
    }

    void OnBoardStable () {
        stable = true;
    }

	float GetBannerPositionYInWorld (GameObject go, float _offsetMilli) {
		//Camera cam = Scene.FindCameraForObject(go);
		Camera cam = Camera.main;

		if(cam == null) 
		{
			Debugger.Assert(cam != null, "mainCamera is null");
			return 0F;
		}

		float unitLen = NNTool.ConvertMillimeterToWorldLength(cam, _offsetMilli);
//		float bannerHeight = AdManager.GetBannerHeightInPixel(); 
		float bannerHeight = 0.0f;
		float yOffset = NNTool.ConvertPixelToWorldLength(cam, bannerHeight) + unitLen;
		Vector3 pos = cam.ScreenToWorldPoint(new Vector3(0f, 0f, 0f));
		return pos.y + yOffset;
	}

    Bounds GetActivedRenderBound(GameObject go)
    {
        Bounds result = new Bounds(go.transform.position, Vector3.zero);
        result.Encapsulate(go.GetComponent<Renderer>().bounds);
        return result;
    }

    // process when doing tutorial.
    public static bool onEscapeTutorial()
    {
        string strId            = getActiveTutorialId();
        if(null == strId)       return false;

        instance.OnClickSkip();
        return true;
    }
}
