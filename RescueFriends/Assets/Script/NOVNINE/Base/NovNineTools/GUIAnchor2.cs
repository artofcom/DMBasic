using UnityEngine;
using System.Collections;

public class GUIAnchor2 : MonoBehaviour
{
    public enum eScaleType { NONE, ALL, HORIZON, ORTH_ALL, ORTH_HORIZON}

    [System.Serializable]
    public class Anchor
    {
        public bool left = false;
        public bool right = false;

        public bool top = false;
        public bool bottom = false;
    }

    [System.Serializable]
    public class Range
    {
        public float min = 0f;
        public float max = 2f;

        public override string ToString ()
        {
            return string.Format("min({0}), max({1})", min, max);
        }
    }

    public Camera viewCamera;
    public Range range = new Range();

    public Anchor anchor = new Anchor();
    public eScaleType scale = eScaleType.NONE;

    public float rate = 1f;

    Vector3 position;
    Vector3 localScale;

    float _aspect = 960f / 640f;

    void Awake()
    {
        position = transform.position;
        localScale = transform.localScale;
        RecalcLayout();
    }

    static public bool IsPortrate()
    {
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

    void RecalcLayout()
    {
        if (null == viewCamera) {
            if ((true == anchor.left && true == anchor.right) || (true == anchor.top && true == anchor.bottom)) {
                Debug.LogWarning("Invalid Camera !!!");
            }

            viewCamera = Camera.main;
        }

        if (true == Application.isEditor)
            _aspect = (viewCamera.aspect > 1f) ? 960f / 640f : 640f / 960f;
        else
            _aspect = (false == IsPortrate()) ? 960f / 640f : 640f / 960f;

        float cameraAspect = viewCamera.aspect;
        if (_aspect > 1f && cameraAspect < 1f || _aspect < 1f && cameraAspect > 1f)
            cameraAspect = 1f / cameraAspect;

        if (cameraAspect == _aspect)
            return;

        if (cameraAspect <= range.min || cameraAspect>= range.max)
            return;

        RecalcLayoutNormal(cameraAspect);
        RecalcLayoutOrthgraphic(cameraAspect);
    }

    void RecalcLayoutNormal(float cameraAspect)
    {
        Vector3 position = this.position;
        Vector3 localScale = this.localScale;

        if (true == anchor.left && true == anchor.right) {
            position.x = (position.x - viewCamera.transform.position.x) * cameraAspect / _aspect  * rate + viewCamera.transform.position.x;
            transform.position = position;

        } else if (true == anchor.left) {
            position.x = position.x + viewCamera.orthographicSize * (_aspect - cameraAspect) * rate;
            transform.position = position;
        } else if (true == anchor.right) {
            position.x = position.x - viewCamera.orthographicSize * (_aspect - cameraAspect) * rate;
            transform.position = position;
        }

        if (scale == eScaleType.ALL) {
            gameObject.transform.localScale *= cameraAspect * rate / _aspect;
        }
        if (scale == eScaleType.HORIZON) {
            this.transform.localScale = new Vector3(cameraAspect * rate / _aspect * localScale.x, localScale.y, localScale.z);
        }
    }

    void RecalcLayoutOrthgraphic(float cameraAspect)
    {
        if (cameraAspect >= _aspect)
            return;

        Vector3 position = this.position;
        Vector3 localScale = this.localScale;

        if (true == anchor.top && true == anchor.bottom) {
            position.y = (position.y - viewCamera.transform.position.y) * _aspect / cameraAspect  * rate  + viewCamera.transform.position.y;
            transform.position = position;
        } else if (true == anchor.top) {
            position.y = position.y + (_aspect / cameraAspect - 1f) * viewCamera.orthographicSize * rate ;
            transform.position = position;
        } else if (true == anchor.bottom) {
            position.y = position.y - (_aspect / cameraAspect - 1f) * viewCamera.orthographicSize * rate;
            transform.position = position;
        }

        if (scale == eScaleType.ORTH_ALL) {
            gameObject.transform.localScale *= _aspect * rate / cameraAspect;
        }
        if (scale == eScaleType.ORTH_HORIZON) {
            gameObject.transform.localScale = new Vector3(localScale.x, _aspect * rate / cameraAspect * localScale.y, localScale.z);
        }
    }
}

