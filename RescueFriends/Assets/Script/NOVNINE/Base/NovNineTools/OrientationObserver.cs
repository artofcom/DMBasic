using UnityEngine;
using System.Collections;
using NOVNINE.Diagnostics;

public class OrientationObserver : MonoBehaviour {
    static public System.Action<ScreenOrientation> onChange;

    static protected ScreenOrientation orientation;
    static public ScreenOrientation Orientation 
    {
		get{ return orientation;}
	}

	void setOrientation(ScreenOrientation ori) {
		orientation = ori;
		StartCoroutine(checkOrientationComplete(ori));
	}

	static void UpdateCamResolution(ScreenOrientation ori) {
		Camera cam = Camera.main;
		Debugger.Assert(cam != null);
		tk2dCamera tk2dCam = cam.GetComponent<tk2dCamera>();
        if(tk2dCam == null) return;

		if(tk2dCam.InheritConfig != null)
			tk2dCam = tk2dCam.InheritConfig;

		if(ori == ScreenOrientation.Landscape) {
			tk2dCam.nativeResolutionWidth = 960;
			tk2dCam.nativeResolutionHeight = 640;
			tk2dCam.UpdateCameraMatrix();
		} else if(ori == ScreenOrientation.Portrait) {
			tk2dCam.nativeResolutionWidth = 640;
			tk2dCam.nativeResolutionHeight = 960;
			tk2dCam.UpdateCameraMatrix();
		}
	}

	void Awake() {
        orientation = Screen.orientation;
		UpdateCamResolution(orientation);
#if UNITY_ANDROID
		UpdateAndroidScreenLock();
#endif
	}

	void Update () {
#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.P)){
			if(orientation != ScreenOrientation.Portrait) setOrientation(ScreenOrientation.Portrait);
        }else if(Input.GetKeyDown(KeyCode.L)){
			if(orientation != ScreenOrientation.Landscape) setOrientation(ScreenOrientation.Landscape);
        }

#else

        if(IsPortrate()){
			if(orientation != ScreenOrientation.Portrait) setOrientation(ScreenOrientation.Portrait);
        }else{
			if(orientation != ScreenOrientation.Landscape) setOrientation(ScreenOrientation.Landscape);
        }
#endif	
	}

#if UNITY_ANDROID
	static bool androidLock = false;

	void UpdateAndroidScreenLock() {
		bool newLock = androidLock;
		using( var pluginClass = new AndroidJavaClass("com.bitmango.bitmangoext.bitmangoext"))
			newLock = pluginClass.CallStatic<bool>("isRotationLock");

		//Debug.Log("UpdateAndroidScreenLock : "+androidLock+" => "+newLock);

		if(newLock != androidLock) {
			androidLock = newLock;
			if(newLock) {
				OrientationLock(newLock);
				if(orientation != ScreenOrientation.Portrait) {
					Screen.orientation = ScreenOrientation.Portrait;
					setOrientation(ScreenOrientation.Portrait);
				}
			}
			else {
				OrientationLock(newLock);
			}
		}
	}

	void OnApplicationFocus() {
		UpdateAndroidScreenLock();
	}
#endif

    static public bool IsPortrate()
    {
#if UNITY_ANDROID
		if(androidLock) return true;
#endif
        bool isPortrait = false;
        if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            isPortrait = true;
        else if (Screen.orientation == ScreenOrientation.Landscape || Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
            isPortrait = false;
        else if (Screen.orientation == ScreenOrientation.AutoRotation) {
            if (Screen.autorotateToLandscapeLeft || Screen.autorotateToLandscapeRight)
                isPortrait = false;
            else
                isPortrait = true;
        } else
            isPortrait = true;

        return isPortrait;
    }

    static public void OrientationLock(bool isLock){
		//Debug.Log("OrientationLock : "+isLock);
#if UNITY_ANDROID
		if(androidLock && !isLock) return;
#endif
        Screen.autorotateToPortrait = (isLock == false);
	    Screen.autorotateToPortraitUpsideDown = (isLock == false);
        Screen.autorotateToLandscapeRight = (isLock == false);;
        Screen.autorotateToLandscapeLeft = (isLock == false);        

        if(isLock == false) Screen.orientation = ScreenOrientation.AutoRotation;
    }

	IEnumerator checkOrientationComplete(ScreenOrientation orientation) {
		bool isOrientationComplete = false;

		UpdateCamResolution(orientation);
		while (!isOrientationComplete) {
			if (orientation == ScreenOrientation.Portrait) {
				isOrientationComplete = Screen.height > Screen.width;	
			} else if (orientation == ScreenOrientation.Landscape) {
				isOrientationComplete = Screen.height < Screen.width;	
			}
			yield return null;
		}
		if( onChange != null ) onChange(orientation);
		Scene.SendMessage<IHandlerBase>("OnOrientationChanged", orientation, SendMessageOptions.DontRequireReceiver);

	}
}
