//#define LLOG_BMPOOL

using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NNPool : ScriptableObject {
	const string poolSettingsAssetName = "NNPoolSetting";
    const string poolSettingsPath = "Resources";
    const string poolSettingsAssetExtension = ".asset";

    public GameObject[] prefabs;

	static NNPool instance;
    static bool initialized;
    Dictionary<string, GameObject> molds = new Dictionary<string, GameObject>();
    Dictionary<string, List<NNRecycler>> pools = new Dictionary<string, List<NNRecycler>>();
    Queue<NNRecycler> prepareQueue = new Queue<NNRecycler>();
    static int minDestroyCount  = 1;

	public static NNPool Instance 
	{
        get {
            if (instance == null) 
			{

				instance = Resources.Load(poolSettingsAssetName) as NNPool;
				
                if (instance == null) 
				{
					instance = CreateInstance<NNPool>();
#if UNITY_EDITOR
                    string properPath = Path.Combine(Application.dataPath, poolSettingsPath);

                    if (!Directory.Exists(properPath)) {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    string fullPath = Path.Combine(Path.Combine("Assets", poolSettingsPath),
                                                   poolSettingsAssetName + poolSettingsAssetExtension
                                                  );
                    AssetDatabase.CreateAsset(instance, fullPath);
#endif
                }

                if (Application.isPlaying) instance.Initialize();
            }
            return instance;
        }
    }

#if UNITY_EDITOR
	[MenuItem("NOVNINE/ObjectPool Settings")]
    public static void Edit () {
        Selection.activeObject = Instance;
    }
#endif

    void OnEnable () {
        DontDestroyOnLoad(this);
    }

	public void Reset()
	{
		TaskManager.StopCoroutine(CoRecycle());
		initialized = false;
		
		molds.Clear();
//		Dictionary<string, List<NNRecycler>>.Enumerator it = pools.GetEnumerator();
//		while(it.MoveNext())
//		{
//			it.Current.Value.Clear();	
//		}
		pools.Clear();
		prepareQueue.Clear();
		Initialize();
		
//		//Resources.UnloadAsset(instance);
		//instance = null;
	}
	
	IEnumerator CoRecycle () {
        while (true) {
            yield return null;

            while (prepareQueue.Count > 0) {
                prepareQueue.Dequeue().State = RECYCLE_STATE.PREPARED;
            }
        }
    }

    public static bool Contains (string itemName) {
        return Instance._Contains(itemName);
    }

    public static void WarmUpItem (string itemName, int count) {
        Instance._WarmUpItem(itemName, count);
    }

    public static GameObject GetItem (string itemName, Transform parent = null) {
        return Instance._GetItem(itemName, parent);
    }

    public static T GetItem<T> (string itemName, Transform parent = null) where T : Component {
        return Instance._GetItem<T>(itemName, parent);
    }

    public static List<GameObject> GetUsingItems (string itemName = null) {
        return Instance._GetUsingItems(itemName);
    }
    
    public static List<GameObject> GetUsingItems<T> (string itemName = null) where T : MonoBehaviour {
        return Instance._GetUsingItems<T>(itemName);
    }

    public static bool IsOccupied (GameObject go) {
        NNRecycler recycler = go.GetComponent<NNRecycler>();

        if (recycler == null) {
            Debug.LogError("Can't manage GameObject - "+go.name);
        } else {
            return recycler.State == RECYCLE_STATE.OCCUPIED;
        }

        return false;
    }

    public static void Abandon (GameObject go) {
        //go.transform.localRotation  = Quaternion.identity;
        //go.transform.position       = Vector3.zero;
        go.transform.localPosition  = Vector3.zero;
        //go.transform.localScale     = Vector3.one;
        //go.transform.parent         = null;

        NNRecycler recycler = go.GetComponent<NNRecycler>();
        Debugger.Assert(recycler != null, go.name+" is not 'NNRecycler' item");
        Instance._Abandon(recycler);
    }

	void Initialize() 
	{
        if (initialized) return;

        if (prefabs != null) {
            foreach (GameObject prefab in prefabs) {
                if (prefab == null) continue;

                NNRecycler recycler = prefab.GetComponent<NNRecycler>();

                Debugger.Assert(recycler != null, prefab.name+" has not 'NNRecycler'");

                molds.Add(prefab.name.ToLower(), prefab);
                pools.Add(prefab.name.ToLower(), new List<NNRecycler>());
            }
        }

        TaskManager.StartCoroutine(CoRecycle());

        initialized = true;
    }

    bool _Contains (string itemName) {
        return molds.ContainsKey(itemName.ToLower());
    }

    void _WarmUpItem (string itemName, int count) 
	{
        GameObject item = null;
        itemName = itemName.ToLower();

        Debugger.Assert(_Contains(itemName), "Can't find item - "+itemName);
        List<NNRecycler> pool = pools[itemName];

        if( pool.Count >= count) return;
        int toMake = count - pool.Count;
        
        for(int i=0; i<toMake; ++i) {
            item = Instantiate(molds[itemName]) as GameObject;
            item.name = itemName.ToUpper()+"_"+i.ToString();
            var itemRecycler = item.GetComponent<NNRecycler>();
            itemRecycler.Release();
            pool.Add(itemRecycler);
        }
    }

    GameObject _GetItem (string itemName, Transform parent = null) 
	{
        itemName = itemName.ToLower();
		NNRecycler itemRecycler = null;
		
        Debugger.Assert(Contains(itemName), "Can't find item - "+itemName);

        List<NNRecycler> pool = pools[itemName];

		for(int i = 0; i < pool.Count; ++i)
		{
			if (pool[i].IsPrepared) 
			{
				itemRecycler = pool[i];
				break;
			}
		}
		
		if (itemRecycler == null) 
		{
			GameObject item = Instantiate<GameObject>(molds[itemName]);
			item.name = itemName.ToUpper();
			itemRecycler = item.GetComponent<NNRecycler>();
			Debugger.Assert(itemRecycler != null, "Can't find NNRecycler - "+itemName);
            pool.Add(itemRecycler);
        }
		
		itemRecycler.transform.SetParent(parent, false);
		
        itemRecycler.Reset();
        itemRecycler.State = RECYCLE_STATE.OCCUPIED;

		return itemRecycler.gameObject;
    }

    T _GetItem<T> (string itemName, Transform parent = null) where T : Component {
        GameObject go = _GetItem(itemName, parent);
        if (go == null) {
            return null;
        } else {
            return go.GetComponent<T>();
        }
    }

    List<GameObject> _GetUsingItems (string itemName) {
        List<GameObject> usingItems = new List<GameObject>();
        List<List<NNRecycler>> targetPools = new List<List<NNRecycler>>();

        if (itemName == null) {
            foreach (List<NNRecycler> pool in pools.Values) targetPools.Add(pool);
        } else {
            itemName = itemName.ToLower();
            if (Contains(itemName)) targetPools.Add(pools[itemName]);
        }

        foreach (List<NNRecycler> pool in targetPools) {
            foreach (NNRecycler recycler in pool) {
                if (recycler.IsPrepared == false) usingItems.Add(recycler.gameObject);
            }
        }

        return usingItems;
    }

    List<GameObject> _GetUsingItems<T> (string itemName) where T : MonoBehaviour {
        List<GameObject> usingItems = new List<GameObject>();
        List<List<NNRecycler>> targetPools = new List<List<NNRecycler>>();

        if (itemName == null) {
            foreach (List<NNRecycler> pool in pools.Values) targetPools.Add(pool);
        } else {
            itemName = itemName.ToLower();
            if (Contains(itemName)) targetPools.Add(pools[itemName]);
        }

        foreach (List<NNRecycler> pool in targetPools) {
            foreach (NNRecycler recycler in pool) {
                if ((recycler.IsPrepared == false) && (recycler.GetComponent<T>() != null)) {
                    usingItems.Add(recycler.gameObject);
                }
            }
        }

        return usingItems;
    }

    void _Abandon (NNRecycler recycler) {
        recycler.State = RECYCLE_STATE.ABANDONED;
        prepareQueue.Enqueue(recycler);
        recycler.Release();
    }

    public static bool destroyAbandoned()
    {
        return Instance._destroyAbandoned();
        //return instance._destroyAbandoned();
    }

    bool _destroyAbandoned()
    {
        foreach(string name in pools.Keys)
        {
            List<NNRecycler>    pool = pools[name];
            if(pool.Count <= minDestroyCount)
                continue;

            for(int i = 0; i < pool.Count; ++i)
		    {
			    if(true == pool[i].IsPrepared)  // || false==pool[i].gameObject.activeSelf)
                {
                    NNRecycler rec  = pool[i];
                    pool.Remove( rec );

                    // note : chilc 중에 관리되는 recycler는 제외.(별도 삭제)
                    NNRecycler[] recyclers  = rec.transform.GetComponentsInChildren<NNRecycler>(true);
                    for(int z = 0; z < recyclers.Length; ++z)
                        recyclers[z].transform.parent   = null;

                    Destroy( rec.gameObject );
                    return true;
                }
            }
        }
        return false;
    }
}
