//#define LLOG_BANNERANCHOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Store;
using NOVNINE.Diagnostics;

public class BannerAnchor : MonoBehaviour 
{
	public float offsetMilli = 0;
    static bool bannerActive = true;

    public static bool BannerActive 
	{
        get { return bannerActive;}
        set {
            if(value != bannerActive) 
			{
                bannerActive = value;
                foreach(var b in bannerAnchors)
                    b.UpdatePositionInternal(OrientationObserver.Orientation);
            }
        }
    }
    static List<BannerAnchor> bannerAnchors = new List<BannerAnchor>();

	void OnEnable()
	{
		OrientationObserver.onChange += UpdatePosition;
		UpdatePosition();
        bannerAnchors.Add(this);
	}

	void OnDisable() 
	{
		OrientationObserver.onChange -= UpdatePosition;
        bannerAnchors.Remove(this);
	}

	public void UpdatePosition() 
	{
		StartCoroutine(coUpdatePosition(OrientationObserver.Orientation));
	}

    //OrientationObserver callback
	void UpdatePosition(ScreenOrientation _orient) 
	{
		StartCoroutine(coUpdatePosition(_orient));
	}

	IEnumerator coUpdatePosition(ScreenOrientation _orient) 
	{
		yield return null;
        UpdatePositionInternal(_orient);
	}

	void UpdatePositionInternal(ScreenOrientation _orient) 
	{
		//Camera cam = Scene.FindCameraForObject(gameObject);
		//Debugger.Assert(cam != null, "BannerAnchor.UpdatePosition() Camera is null : Go["+gameObject.name+"]");
		Camera cam = Camera.main;
		
		float unitLen = NNTool.ConvertMillimeterToWorldLength(cam, offsetMilli);

        float bannerHeightInPixel = 0;
        float bannerHeightInWorld = 0;
        if(bannerActive) 
		    bannerHeightInWorld = NNTool.ConvertPixelToWorldLength(cam, bannerHeightInPixel);

        tk2dCamera tk2dCam = cam.GetComponent<tk2dCamera>();
        if (tk2dCam.InheritConfig != null) tk2dCam = tk2dCam.InheritConfig;

        Rect screenExtents = tk2dCam.ScreenExtents;
		Vector3 pos = cam.transform.position;

        if (tk2dCam.CameraSettings.orthographicOrigin == tk2dCameraSettings.OrthographicOrigin.BottomLeft)
            pos.y = pos.y + screenExtents.y + bannerHeightInWorld + unitLen;
        else
            pos.y = pos.y - screenExtents.height*0.5f + bannerHeightInWorld + unitLen;
        
		Vector3 position = transform.position;
		position.y = pos.y;
		transform.position = position;
	}

}

