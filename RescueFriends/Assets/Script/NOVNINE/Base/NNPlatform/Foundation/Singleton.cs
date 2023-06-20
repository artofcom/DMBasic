using UnityEngine;
using System.Collections;

public class Singleton<T> where T : class, new()
{
    protected static T instance = null;

    public static T Instance {
        get {
            if (instance == null) {
                instance = new T();
            }

            return instance;
        }
    }
}

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    private static T instance = null;
	bool bInit = true;
	
    public static T Instance
    {
        get {
            if (null == instance) {
                instance = GameObject.FindObjectOfType(typeof(T)) as T;

                if( instance == null ) {
                    Debug.LogWarning("No instance of " + typeof(T).ToString() + ", a temporary one is created.");
                    instance = new GameObject("Temp Instance of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();

                    // Problem during the creation, this should not happen
                    if( instance == null ) {
                        Debug.LogError("Problem during the creation of " + typeof(T).ToString());
                    }
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        //Debug.Log(GetType().Name+".Awake");
        if (null != instance && (this as T) != instance) 
		{
            Debug.LogError("Destory for SingletonMonoBehaviour");
			bInit = false;
            //GameObject.Destroy(this.gameObject);
            Object.Destroy(this);
        } else {
            instance = this as T;
			//instance.Init();
        }
    }
	
	void Start()
	{
		if(bInit)
			instance.Init();
	}

    // This function is called when the instance is used the first time
    // Put all the initializations you need here, as you would do in Awake
    public virtual void Init() {}

    // Make sure the instance isn't referenced anymore when the user quit, just in case.
    private void OnApplicationQuit()
    {
        instance = null;
    }

    // 명시적으로 Instance를 access하여 메모리상에 생성되기위해 사용하고 있습니다.
    public void Idle()
    {
    }

    // 상속받은녀석에서 중복여부를 확인하기위해 사용합니다.
    public bool IsValid()
    {
        if (null != Instance && (this as T) == instance)
            return true;

        return false;
    }
}

