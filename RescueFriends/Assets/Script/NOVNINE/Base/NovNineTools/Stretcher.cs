using UnityEngine;
using System.Reflection;
using System.Collections;

public enum STRETCH_SIDE { VIRTICAL, HORIZONTAL, BOTH }

public class Stretcher : MonoBehaviour {

    public tk2dCamera cam;
    public STRETCH_SIDE side = STRETCH_SIDE.BOTH; 
    public bool rateMaintain = true;

    void Start () {
        if (GetComponent<Renderer>() == null) return;

        object obj = GetDimensionObject();

        if (obj == null) {
            StretchScale();
        } else {
            StretchDimensions(obj);
        }
    }

    void StretchDimensions (object obj) {
        Vector2 orgDimensions = JMFUtils.GetPropValue<Vector2>(obj, "dimensions");
        Vector2 newDimensions = orgDimensions;

#if UNITY_EDITOR
        Vector2 nativeResolution = (cam.InheritConfig == null) ? cam.forceResolution : cam.InheritConfig.forceResolution;
#else
        Vector2 nativeResolution = (cam.InheritConfig == null) ? cam.NativeResolution : cam.InheritConfig.NativeResolution;
#endif

        switch (cam.CurrentResolutionOverride.autoScaleMode) {
            case tk2dCameraResolutionOverride.AutoScaleMode.FitWidth :
                float aspectFitWidth = nativeResolution.x / cam.TargetResolution.x;

                switch (side) {
                    case STRETCH_SIDE.VIRTICAL :
                        if (rateMaintain) newDimensions.y = cam.TargetResolution.y * aspectFitWidth;
                        break;
                    case STRETCH_SIDE.HORIZONTAL :
                        newDimensions.x = nativeResolution.x;
                        if (rateMaintain) newDimensions.y = orgDimensions.y * (nativeResolution.x / orgDimensions.x);
                        break;
                    case STRETCH_SIDE.BOTH :
                        newDimensions.x = nativeResolution.x;
                        newDimensions.y = cam.TargetResolution.y * aspectFitWidth;
                        break;
                }
                break;
            case tk2dCameraResolutionOverride.AutoScaleMode.FitHeight :
                float aspectFitHeight = nativeResolution.y / cam.TargetResolution.y;

                switch (side) {
                    case STRETCH_SIDE.VIRTICAL :
                        newDimensions.y = nativeResolution.y;
                        break;
                    case STRETCH_SIDE.HORIZONTAL :
                        newDimensions.x = cam.TargetResolution.x * aspectFitHeight;
                        if (rateMaintain) newDimensions.y = orgDimensions.y * (newDimensions.x / orgDimensions.x);
                        break;
                    case STRETCH_SIDE.BOTH :
                        newDimensions.x = cam.TargetResolution.x * aspectFitHeight;
                        newDimensions.y = nativeResolution.y;
                        break;
                }
                break;
        }

        JMFUtils.SetPropValue<Vector2>(obj, "dimensions", newDimensions);
    }

    void StretchScale () {
        if (GetComponent<Renderer>() == null) return;

        transform.localScale = Vector3.one;

        tk2dBaseSprite baseSprite = GetComponent<tk2dBaseSprite>();
        if (baseSprite != null) baseSprite.scale = Vector3.one;

        Vector3 sizeBG = GetComponent<Renderer>().bounds.size;

        float aspectFitHeight = cam.ScreenExtents.height / sizeBG.y;
        float aspectFitWidth = cam.ScreenExtents.width / sizeBG.x;

        if (baseSprite != null) {
            baseSprite.scale = new Vector3(aspectFitWidth, aspectFitHeight, 1F);
        } else {
            transform.localScale = new Vector3(aspectFitWidth, aspectFitHeight, 1F);
        }
    }

    object GetDimensionObject () {
        if (GetComponent<tk2dBaseSprite>() == null) return null;

        tk2dSlicedSprite slicedSprite = GetComponent<tk2dSlicedSprite>();
        if (slicedSprite != null) return slicedSprite;

        tk2dTiledSprite tiledSprite = GetComponent<tk2dTiledSprite>();
        if (tiledSprite != null) return tiledSprite;

        return null;
    }

}
