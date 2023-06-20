using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

//public class RateCamera : SingletonMonoBehaviour<RateCamera> {
public class RateCamera : MonoBehaviour
{

//public GameObject m_objBackScissor;
    public enum eRateCamera {eViewPortRect, eOrthgraphicSize}

    public Color color = Color.black;
    public eRateCamera type = eRateCamera.eViewPortRect;

    Camera screenCamera;
    // Use this for initialization

    //protected override void Awake(){
    //    base.Awake();
    void Awake()
    {
        //DontDestroyOnLoad(this.gameObject);
        GenerateScreenCamera();
    }

    void Start()
    {
        UpdateResolution();
    }

    void GenerateScreenCamera()
    {

        GameObject obj = new GameObject("rateCamera");
        obj.transform.parent = this.transform;
        obj.transform.localPosition = Vector3.one * -5050f;

        this.screenCamera = obj.AddComponent<Camera>();

        this.screenCamera.depth = -99;
        this.screenCamera.clearFlags = CameraClearFlags.SolidColor;
        this.screenCamera.backgroundColor = color;
        this.screenCamera.orthographic = true;
        this.screenCamera.orthographicSize = 1;
    }

#if UNITY_5_4_OR_NEWER
	void OnEnable()
	{
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}
	
	void OnLevelFinishedLoading( UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
	{
		UpdateResolution();
	}
#else
    void OnLevelWasLoaded(int level)
    {
        UpdateResolution();
    }	
#endif

    public void UpdateResolution()
    {
        Camera viewCamera = Camera.main;

        float aspect = 0f;
        if (true == Application.isEditor)
            aspect = (viewCamera.aspect > 1f) ? 960f / 640f : 640f / 960f;
        else
            aspect = (false == GUIAnchor2.IsPortrate()) ? 960f / 640f : 640f / 960f;

        Camera [] objCameras = Resources.FindObjectsOfTypeAll(typeof(Camera)) as Camera[];
        foreach( Camera obj in objCameras) {
            if (obj == this.screenCamera) continue;
            if (null != obj.targetTexture) continue;	// for renderTexture;

            if (this.type == eRateCamera.eViewPortRect) {
                RateCameraViewPortRect rateCamera = obj.GetComponent<RateCameraViewPortRect>();
                if (null == rateCamera)
                    rateCamera = obj.gameObject.AddComponent<RateCameraViewPortRect>();

                rateCamera.UpdateResolution(aspect, 1f);
            } else {
                RateCameraOrthgraphicSize rateCamera = obj.GetComponent<RateCameraOrthgraphicSize>();
                if (null == rateCamera)
                    rateCamera = obj.gameObject.AddComponent<RateCameraOrthgraphicSize>();

                rateCamera.UpdateResolution(aspect, 1f);
            }
        }
    }

}

