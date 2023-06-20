using UnityEngine;
using System.Collections;
//using Holoville.HOTween;
using DG.Tweening;
using NOVNINE;

public class tk2dUIScrollbarExt : tk2dUIScrollbar {

    const float SCROLLBAR_OFFSET_MULIPLE = .05f;

    public Color matColor;

    Material mat;
    GameObject thumb, pivot, center, top, bot;
    
    float rightOffsetX = 0f;
    float pivotPosY = 0f;
    float thumbImgLength = 0f;
    float _SCROLLBAR_WIDTH = 0;
    
	float SCROLLBAR_WIDTH {
		get {
			if(_SCROLLBAR_WIDTH == 0) {
				Camera cam = Scene.FindCameraForObject(gameObject);
				_SCROLLBAR_WIDTH = NNTool.ConvertMillimeterToWorldLength(cam, 0.5f);
			}
			return _SCROLLBAR_WIDTH;
		}
	}

    void Awake ()
    {
        mat = Resources.Load("autoScrollMat") as Material;
        mat.color = new Color(0,0,0,0);
    }

    void Update ()
    {
        if (rawPercent < -0.0001f) {
            ResizeThumb(rawPercent);
        } else if (rawPercent > 1.0001f) {
            ResizeThumb(rawPercent - 1f);
        }
    }

    public void InitScrollBar (float visibleAreaLength, float contentLength)
    {
        SetBarData(visibleAreaLength, contentLength);
        CreateThumb();
    }

    public void UpdateScrollBar (float visibleAreaLength, float contentLength)
    {
        SetBarData(visibleAreaLength, contentLength);
        UpdateScrollBarAppearance();
    }

    void SetBarData (float visibleAreaLength, float contentLength)
    {
        scrollBarLength = visibleAreaLength;
		if(contentLength != 0)
			thumbLength = (visibleAreaLength / contentLength ) * visibleAreaLength;
		else
			thumbLength = 0;

        Camera cam = Scene.FindCameraForObject(gameObject);
		if(cam == null) cam = Camera.main;
		if(cam == null) {
			Debug.LogError("tk2dUIScrollableArea.CalculateContentAndVisibleLength : Camera not found");
			return;
		}
        Vector3 bottomRight = cam.ViewportToWorldPoint(new Vector3(1, 0));
		bottomRight.x -= SCROLLBAR_WIDTH*2;
		transform.position = bottomRight;
		rightOffsetX = transform.localPosition.x;
        transform.localPosition = Vector2.right * rightOffsetX;
        thumbImgLength = Mathf.Max((scrollBarLength * .1f), thumbLength);
        thumbImgLength -= thumbImgLength * SCROLLBAR_OFFSET_MULIPLE;
    }

    public void AnimateShow ()
    {
//        TweenParms parms = new TweenParms();
//        parms.Prop("matColor", new Color(0f, 0f, 0f, .5f))
//             .Id(gameObject.name)
//             .Ease(EaseType.Linear)
//             .OnUpdate(() => { mat.color = matColor; });
//        HOTween.To(this, .2f, parms);
		TweenParams parms = new TweenParams();
		parms.SetId(gameObject.name);
		parms.SetEase(Ease.Linear);
		parms.OnUpdate(() => { mat.color = matColor; });
		DOTween.To(()=> this.matColor, x=> this.matColor = x, new Color(0f, 0f, 0f, .5f), 0.2f).SetAs(parms);
    }

    public void AnimateHide ()
    {
//        TweenParms parms = new TweenParms();
//        parms.Prop("matColor", new Color(0, 0, 0, 0))
//             .Id(gameObject.name)
//             .Delay(.35f)
//             .Ease(EaseType.Linear)
//             .OnUpdate(() => { mat.color = matColor; });
//        HOTween.To(this, .3f, parms);
		TweenParams parms = new TweenParams();
		parms.SetId(gameObject.name);
		parms.SetEase(Ease.Linear);
		parms.OnUpdate(() => { mat.color = matColor; });
		DOTween.To(()=> this.matColor, x=> this.matColor = x, new Color(0f, 0f, 0f, .5f), 0.2f).SetAs(parms);
    }

    void CreateThumb ()
    {
        if (transform.childCount > 0) return;

        thumb = new GameObject("Thumb");
        thumb.transform.parent = transform;
        thumb.transform.localPosition = Vector3.zero;
        thumbTransform = thumb.transform;

        pivot = new GameObject("Pivot");
        pivot.transform.parent = thumb.transform;
        pivot.transform.localPosition = Vector3.zero;

        center = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bot = GameObject.CreatePrimitive(PrimitiveType.Cube);

        mat.SetColor("Main Color", new Color(0, 0, 0, 0));
        center.GetComponent<Renderer>().material = mat;
        top.GetComponent<Renderer>().material = mat;
        bot.GetComponent<Renderer>().material = mat;

        center.transform.parent = pivot.transform;
        top.transform.parent = pivot.transform;
        bot.transform.parent = pivot.transform;

        UpdateScrollBarAppearance();
    }

    void UpdateScrollBarAppearance ()
    {
        center.transform.localScale = new Vector3(SCROLLBAR_WIDTH, thumbImgLength, 1f);
        top.transform.localScale = Vector3.one * SCROLLBAR_WIDTH;
        bot.transform.localScale = Vector3.one * SCROLLBAR_WIDTH;

        center.transform.localPosition = Vector3.zero;
        Bounds cb = center.GetComponent<Renderer>().bounds;
        top.transform.localPosition = new Vector3(cb.max.x - cb.center.x - cb.extents.x, cb.max.y - cb.center.y, 0f);
        bot.transform.localPosition = new Vector3(cb.min.x - cb.center.x + cb.extents.x, cb.min.y - cb.center.y, center.transform.localPosition.z);

        Vector3 halfSizeYOfCenterBounds = (Vector3.down * cb.extents.y);
        Vector3 halfSizeYOfThumOffset = (Vector3.down * (thumbImgLength * SCROLLBAR_OFFSET_MULIPLE) * .5f);
        pivot.transform.localPosition = halfSizeYOfCenterBounds + halfSizeYOfThumOffset;
        pivotPosY = pivot.transform.localPosition.y;
    }

    void UpdateThumbsPosition ()
    {
        Bounds cb = center.GetComponent<Renderer>().bounds;
        center.transform.localPosition = Vector3.zero;
        top.transform.localPosition = new Vector3(cb.max.x - cb.extents.x, cb.max.y, 0f);
        bot.transform.localPosition = new Vector3(cb.min.x + cb.extents.x, cb.min.y, 0f);
    }

    void ResizeThumb (float flowAmount)
    {
        float length = thumbLength / scrollBarLength; 
        float multiple = flowAmount < 0 ? 1f : -1f;
        float amount = thumbImgLength + (flowAmount / length) * multiple;

        center.transform.localScale = new Vector3(SCROLLBAR_WIDTH, amount, 1f);
        center.transform.localPosition = Vector3.zero;
        Bounds cb = center.GetComponent<Renderer>().bounds;
        top.transform.localPosition = new Vector3(cb.max.x - cb.center.x - cb.extents.x, cb.max.y - cb.center.y, 0f);
        bot.transform.localPosition = new Vector3(cb.min.x - cb.center.x + cb.extents.x, cb.min.y - cb.center.y, center.transform.localPosition.z);

        float flowPosY = ((thumbImgLength - amount) * .5f) * multiple;
        pivot.transform.localPosition = new Vector3(0f, (pivotPosY + flowPosY) , 0f);
    }
}
