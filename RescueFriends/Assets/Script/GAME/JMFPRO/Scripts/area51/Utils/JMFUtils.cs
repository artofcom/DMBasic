/// <summary>
/// JMF utils. use as a helper class for various static function calls
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Text;
using Spine;
using Spine.Unity;
using System.Reflection;
using System.Globalization;

public static class JMFUtils
{
	public static GameManager GM;           // updated by GameManager -> Awake()
    public static PlayOverlayHandler POH;   
    public static CultureInfo CI = null;

    public static string FormatCurrency(float amount)
    {
        if (CI != null)
            return string.Format(CI, "{0:C}", amount);
        else
        {
            return null;
        }
    }
        
    public static void FindSymbolCurrency(string symbol)
    {
        Debug.Log("------------symbol:" + symbol);
        if (CI == null)
        {
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                RegionInfo r = new RegionInfo(ci.LCID);

                if(r != null && r.CurrencySymbol.ToUpper() == symbol.ToUpper())
                {
                    Debug.Log("------------OK");
                    CI = ci;
                    break;
                }
            }
        }
    }

    public static string ConvertMoney (string money,string currency)
    {
        if (CI == null)
        {
            //#if !UNITY_IOS
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                RegionInfo r = new RegionInfo(ci.LCID);
                if(r != null)
                {
                    
                    Debug.Log(string.Format(" ISOCurrencySymbol:{0} , CurrencySymbol:{1} , ThreeLetterISORegionName:{2} ,ci.LCID :{3},TwoLetterISORegionName:{4} ",r.ISOCurrencySymbol.ToUpper(),r.CurrencySymbol.ToUpper(),r.ThreeLetterISORegionName.ToUpper(),ci.LCID,r.TwoLetterISORegionName.ToUpper()));
                }

                if(r != null && r.CurrencySymbol.ToUpper() == currency.ToUpper())
                {
                    CI = ci;
                    break;
                }
            }   
                
            if (CI == null)
                return null;
            //#endif
        }

        if (string.IsNullOrEmpty(money))
            return null;

        int numValue;
        int indexStart = 0;
        while (Int32.TryParse(money[indexStart].ToString(), out numValue) == false)
        {
            ++indexStart;
            if(indexStart == money.Length)
                return null;
        }

        int indexEnd = money.Length;
        while (Int32.TryParse(money[indexEnd -1].ToString(), out numValue) == false)
        {
            --indexEnd;
            if(indexEnd < 2)
                return null;
        }

        money = money.Substring(indexStart, indexEnd - indexStart);

        string[] list = money.Split(CI.NumberFormat.NumberDecimalSeparator[0]);
        if(list.Length == 0 || list.Length > 2 )
            return null;
        
        StringBuilder sb = new StringBuilder();

        indexStart = 0;
        numValue = 0;
        for (int i = 0; i < list[0].Length; ++i)
        {
            if (Int32.TryParse(list[0][indexStart].ToString(), out numValue))
            {
                sb.Append(list[0][indexStart]);
            }
            ++indexStart;
        }

        if (list.Length > 1)
        {
            sb.Append(".");
            sb.Append(list[1]);
        }

        return sb.ToString();
    }
	
    public static Bounds GetBoundsRecursive ( GameObject go, bool keepCenter = false)
    {
        if (go.activeSelf == false)
        {
            Debug.LogError("Can't calculate bounds from deactivated GameObject.");
            return default(Bounds);
        }

        Bounds bounds = default(Bounds);
        Stack<Transform> candidates = new Stack<Transform>();

        candidates.Push(go.transform);

        while (candidates.Count > 0) 
        {
            Transform tf = candidates.Pop();

            if (tf.gameObject.activeSelf == false) continue;
            if (tf.GetComponent<ParticleSystem>() != null) continue;

            Renderer rd = tf.GetComponent<Renderer>();

            if (rd != null)
            {
                if (bounds == default(Bounds)) bounds = new Bounds(rd.bounds.center, Vector3.zero);
                bounds.Encapsulate(rd.bounds.max);
                bounds.Encapsulate(rd.bounds.min);
            }

            for (int i = 0; i < tf.childCount; i++) candidates.Push(tf.GetChild(i));
        }

        if (keepCenter) bounds.center = go.transform.position - (bounds.center - go.transform.position); 

        return bounds;
    }

    public static float MeasureContentLength(Transform TM, tk2dUIScrollableArea.Axes scrollAxes )
    {
        Vector3 vector3Min = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        Vector3 vector3Max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3[] minMax = new Vector3[] {
            vector3Max,
            vector3Min
        };

        GetRendererBoundsInChildren(TM.worldToLocalMatrix, minMax,  TM, false);

        Bounds b = new Bounds();
        if (minMax[0] != vector3Max && minMax[1] != vector3Min) 
        {
            b.SetMinMax(minMax[0], minMax[1]);
        }

//        Bounds result = new Bounds(TM.position, Vector3.zero);
//        foreach(var r in TM.GetComponentsInChildren<Renderer>()) 
//        {
//            result.Encapsulate(r.bounds);
//        }

        return (scrollAxes == tk2dUIScrollableArea.Axes.YAxis) ? (b.size.y) : (b.size.x);
    }

    static readonly Vector3[] boxExtents = new Vector3[] 
    {
        new Vector3(-1, -1, -1),
        new Vector3( 1, -1, -1),
        new Vector3(-1,  1, -1),
        new Vector3( 1,  1, -1),
        new Vector3(-1, -1,  1),
        new Vector3( 1, -1,  1),
        new Vector3(-1,  1,  1),
        new Vector3( 1,  1,  1),
    };


    static void GetRendererBoundsInChildren( Matrix4x4 rootWorldToLocal, Vector3[] minMax, Transform t, bool includeAllChildren)
    {
        MeshFilter mf = t.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null) 
        {
            Bounds b = mf.sharedMesh.bounds;
            Matrix4x4 relativeMatrix = rootWorldToLocal * t.localToWorldMatrix;
            for (int j = 0; j < 8; ++j) 
            {
                Vector3 localPoint = b.center + Vector3.Scale(b.extents, boxExtents[j]);
                Vector3 pointRelativeToRoot = relativeMatrix.MultiplyPoint(localPoint);
                minMax[0] = Vector3.Min(minMax[0], pointRelativeToRoot);
                minMax[1] = Vector3.Max(minMax[1], pointRelativeToRoot);
            }
        }

        for (int i = 0; i < t.childCount; ++i) 
        {
            Transform child = t.GetChild(i);
            GetRendererBoundsInChildren(rootWorldToLocal, minMax,  child, includeAllChildren);
        }
    }


	// look for an object bounds
	public static Bounds findObjectBounds(GameObject obj){
		// includes all mesh types (filter; renderer; skinnedRenderer)
		Renderer ren = obj.GetComponent<Renderer>();
		if(ren == null)
			ren = obj.GetComponentInChildren<Renderer>();
		
		if(ren != null)
			return ren.bounds;//ren.bounds	{Center: (394.1, 3.4, -30.0), Extents: (0.4, 0.4, 0.1)}	UnityEngine.Bounds
		
//		Debug.LogError("Your prefab" + obj.ToString() + "needs a mesh to scale!!!");
		return new Bounds(Vector3.zero,Vector3.zero); // fail safe
	}
	
	public static void SpineObjectAutoScalePadded( GameObject obj )
	{
        // do nothing.
        return;
        
		obj.transform.localScale = Vector3.one; // resets the scale first

		BoxCollider bc = obj.GetComponent<BoxCollider>();

		if (bc != null) 
		{
			// auto scaling feature
			Bounds bounds = JMFUtils.findObjectBounds(obj);
			// bounds.size 0인 경우 스파인 애니에 전체가 0일때
			float val = GM.Size / Mathf.Clamp(Mathf.Max(bounds.size.x,bounds.size.y), 0.0000001F, float.MaxValue);
			float maxSize = Mathf.Max( new float[] {bounds.size.x,bounds.size.y,bounds.size.z} );
			bc.size = new Vector3(maxSize * val, maxSize * val, bounds.size.z + 0.01f) ;
			bc.center = Vector3.zero;
		}
	}
	
	public static float ToColor(string hexString, int colorIndex)
	{
		if (hexString.Length != 8)
			throw new ArgumentException("Color hexidecimal length must be 8, recieved: " + hexString, "hexString");
		return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
	}
	
	// auto scale objects to fit into a board box size
	public static void autoScale(GameObject obj){
		autoScaleRatio(obj, 1F); // default ratio of 1
	}
	
	// auto scale objects to fit into a board box size - with padding!
	public static void autoScalePadded (GameObject obj) {
		autoScaleRatio(obj, 1F); // default ratio of 1
	}
	
	public static void autoScaleHexagon(GameObject obj){
		autoScaleRatio(obj,1.156f); // 1.156f is the hexagon's scale
	}
	
	// auto scale objects to fit into a board box size with ratio
	public static void autoScaleRatio(GameObject obj, float ratio)
	{
		obj.transform.localScale = Vector3.one; // resets the scale first
		
		// auto scaling feature
		Bounds bounds = default(Bounds);
        Block b = obj.GetComponent<Block>();

        if (b == null)
            bounds = findObjectBounds(obj);
        else
            bounds = b.GetBounds();

		float val = GM.Size / Mathf.Clamp(Mathf.Max(bounds.size.x,bounds.size.y), 0.0000001F, float.MaxValue);
		obj.transform.localScale = new Vector3 (val, val, val ); // the final scale value
		
		// adjust the box collider if present...
		BoxCollider bc = obj.GetComponent<BoxCollider>();

		if (bc != null) 
		{
			float maxSize = Mathf.Max( new float[] {bounds.size.x,bounds.size.y,bounds.size.z} );
			bc.size = new Vector3(maxSize, maxSize, bounds.size.z + 0.01f);
			bc.center = Vector3.zero;
		}
	}

    public static void ShowItemGainEffect (string itemName, Vector3 startPos, Vector3 targetPos, float duration) {
        GameObject go = NNPool.GetItem(itemName, GM.transform);
		JMFUtils.autoScalePadded(go);

        PieceTracker pt = go.GetComponent<PieceTracker>();
        if (pt != null) pt.enabled = false;

        startPos.z -= 10F;
        targetPos.z -= 10F;

        go.transform.position = startPos;

        Sequence seq = DOTween.Sequence();
        seq.Append(go.transform.DOScale(go.transform.localScale * 1.5F, duration * 0.5F).SetEase(Ease.OutQuad));
        seq.Append(go.transform.DOMove(targetPos, duration * 0.5F).SetEase(Ease.OutQuad));
        seq.Insert(duration * 0.5F, go.transform.DOScale(Vector3.zero, duration * 0.5F).SetEase(Ease.InQuad));
        seq.OnComplete(() => { NNPool.Abandon(go); });
    }
	
	public static PathAttachment CreateBezierPointsForSpine(PathAttachment pathAttachment, Vector3 start, Vector3 end )
	{		
		PathAttachment _pathAttachment = new PathAttachment(pathAttachment.Name);
		if(pathAttachment.Vertices != null)
		{
			_pathAttachment.Vertices = new float[pathAttachment.Vertices.Length];
			for(int i = 0; i < pathAttachment.Vertices.Length; ++i)
			{
				_pathAttachment.Vertices[i] = pathAttachment.Vertices[i];
			}	
		}
		
		if(pathAttachment.Lengths != null)
		{
			_pathAttachment.Lengths = new float[pathAttachment.Lengths.Length];
			for(int i = 0; i < pathAttachment.Lengths.Length; ++i)
			{
				_pathAttachment.Lengths[i] = pathAttachment.Lengths[i];
			}	
		}

		if(pathAttachment.Bones != null)
		{
			_pathAttachment.Bones = new int[pathAttachment.Bones.Length];
			for(int i = 0; i < pathAttachment.Bones.Length; ++i)
			{
				_pathAttachment.Bones[i] = pathAttachment.Bones[i];
			}	
		}

		_pathAttachment.Closed = pathAttachment.Closed;
		_pathAttachment.ConstantSpeed = pathAttachment.ConstantSpeed;
		_pathAttachment.WorldVerticesLength = pathAttachment.WorldVerticesLength;
		
		float XX = start.x - _pathAttachment.vertices[2];
		float YY = start.y - _pathAttachment.vertices[3];
		
		for(int i = 0; i < 3; ++i)
		{
			_pathAttachment.vertices[(i*2)] += XX;
			_pathAttachment.vertices[(i*2) + 1] += YY;
		}
		
		if(start.x < -4.0f || start.y < -4.0f)
		{
			float rx = (start.x - _pathAttachment.vertices[4]) * Mathf.Cos(DEGREES_TO_RADIANS(180f)) - (start.y - _pathAttachment.vertices[5]) * Mathf.Sin(DEGREES_TO_RADIANS(180f)) + start.x;
			float ry = (start.x - _pathAttachment.vertices[4]) * Mathf.Sin(DEGREES_TO_RADIANS(180f)) - (start.y - _pathAttachment.vertices[5]) * Mathf.Cos(DEGREES_TO_RADIANS(180f)) + start.y;
			_pathAttachment.vertices[4] = rx;
			_pathAttachment.vertices[5] = ry;
		}
		
		XX = end.x - _pathAttachment.vertices[_pathAttachment.WorldVerticesLength -4];
		YY = end.y - _pathAttachment.vertices[_pathAttachment.WorldVerticesLength -3];
		
		for(int i = 0; i < 3; ++i)
		{
			_pathAttachment.vertices[(i*2) + 6] += XX;
			_pathAttachment.vertices[(i*2) + 7] += YY;
		}
		return _pathAttachment;		
	}
	
	public static PathAttachment CreateBezierPointsForSpine(PathAttachment pathAttachment, Vector3 end )
	{		
		PathAttachment _pathAttachment = new PathAttachment(pathAttachment.Name);
		if(pathAttachment.Vertices != null)
		{
			_pathAttachment.Vertices = new float[pathAttachment.Vertices.Length];
			for(int i = 0; i < pathAttachment.Vertices.Length; ++i)
			{
				_pathAttachment.Vertices[i] = pathAttachment.Vertices[i];
			}	
		}

		if(pathAttachment.Lengths != null)
		{
			_pathAttachment.Lengths = new float[pathAttachment.Lengths.Length];
			for(int i = 0; i < pathAttachment.Lengths.Length; ++i)
			{
				_pathAttachment.Lengths[i] = pathAttachment.Lengths[i];
			}	
		}

		if(pathAttachment.Bones != null)
		{
			_pathAttachment.Bones = new int[pathAttachment.Bones.Length];
			for(int i = 0; i < pathAttachment.Bones.Length; ++i)
			{
				_pathAttachment.Bones[i] = pathAttachment.Bones[i];
			}	
		}

		_pathAttachment.Closed = pathAttachment.Closed;
		_pathAttachment.ConstantSpeed = pathAttachment.ConstantSpeed;
		_pathAttachment.WorldVerticesLength = pathAttachment.WorldVerticesLength;
		
//		float XX = start.x - _pathAttachment.vertices[2];
//		float YY = start.y - _pathAttachment.vertices[3];
//
//		for(int i = 0; i < 3; ++i)
//		{
//			_pathAttachment.vertices[(i*2)] += XX;
//			_pathAttachment.vertices[(i*2) + 1] += YY;
//		}

		float XX = end.x - _pathAttachment.vertices[_pathAttachment.WorldVerticesLength -4];
		float YY = end.y - _pathAttachment.vertices[_pathAttachment.WorldVerticesLength -3];

		for(int i = 0; i < 3; ++i)
		{
			_pathAttachment.vertices[(i*2) + 6] += XX;
			_pathAttachment.vertices[(i*2) + 7] += YY;
		}
		return _pathAttachment;		
	}
	
	public static Vector3 PointOnBezier(List<Vector3> pointList, float t, ref Quaternion outRotation)
	{
		Vector3 result = Vector3.zero;

		int n = 0;
		while( pointList.Count > 2 )
		{
			Vector3 val = pointList[0] + (pointList[1] - pointList[0]) * t;
			for(int i = 0; i < pointList.Count -1; ++ i)
			{
				pointList[i] = pointList[i + 1];
			}

			pointList[pointList.Count -1] = val;
			++n;
			if(n == pointList.Count)
			{
				pointList.RemoveAt(pointList.Count -1);
				n = 0;
			}
		}

		result = pointList[0] + (pointList[1] - pointList[0]) * t;
		//float angle = Mathf.Atan2(result.y - tempList[1].y,result.x - tempList[1].x);
		float angle = Mathf.Atan2(pointList[0].y - pointList[1].y,pointList[0].x - pointList[1].x);
		angle = JMFUtils.RADIANS_TO_DEGREES(angle) - 180;

		Vector3 _r = outRotation.eulerAngles;
		_r.z = angle;
		outRotation.eulerAngles = _r;

		return result;
	}
	

    public static PathAttachment CreateLineForSpine(PathAttachment pathAttachment, Vector3 start, Vector3 end )
	{		
        if(null != pathAttachment.Bones)
        {
            Debug.Log("We dont support bone path systems for now....");
            return null;
        }

        //
        // http://ko.esotericsoftware.com/spine-api-reference#PathAttachment
        // v0(v1컨트롤 포인트1), v1(시점), v2(v1컨트롤 포인트2), 
        // v3(v4컨트롤 포인트1), v4(종점), v5(v4컨트롤 포인트)
        //
        PathAttachment _pathAttachment = new PathAttachment(pathAttachment.Name);
        const int countCtrl     = 2;
        const int countVert     = 2*(countCtrl+1) * 2;  // (x,y) * vert 수(컨트롤2+진짜정점) * 2(출발,끝점)

        _pathAttachment.Closed              = pathAttachment.Closed;
		_pathAttachment.ConstantSpeed       = pathAttachment.ConstantSpeed;
		_pathAttachment.WorldVerticesLength = countVert;
		{
            _pathAttachment.Vertices = new float[countVert];
			for(int i = 0; i < countVert; i+=2)
			{
                float fx        = i<countVert/2 ? start.x : end.x;
                float fy        = i<countVert/2 ? start.y : end.y;
                
                _pathAttachment.Vertices[i]     = fx;
                _pathAttachment.Vertices[i+1]   = fy;
			}
        }
		
		if(pathAttachment.Lengths != null)
		{
			_pathAttachment.Lengths = new float[pathAttachment.Lengths.Length];
			for(int i = 0; i < pathAttachment.Lengths.Length; ++i)
			{
				_pathAttachment.Lengths[i] = pathAttachment.Lengths[i];
			}	
		}
		return _pathAttachment;		
	}

	public static float RADIANS_TO_DEGREES(float angle)
	{
		return angle * Mathf.Rad2Deg;
	}

	public static float DEGREES_TO_RADIANS(float angle)
	{
		return angle * Mathf.Deg2Rad;
	}


    // listAddTo list에 listAddTargets item들을 중복 없이 add 한다.
    public static void AddItemWithoutConflict<T>(ref List<T> listAddTo, List<T> listAddTargets)
    {
        if(null==listAddTo || null==listAddTargets)
            return;

        for(int g = 0; g < listAddTargets.Count; ++g)
        {
            if(false == listAddTo.Contains( listAddTargets[g] ))
                listAddTo.Add( listAddTargets[g] );
        }
    }

    public static T GetPropValue<T>(object target, string name)
    {
        FieldInfo fi = target.GetType().GetField(name);
        if(fi != null)
            return (T)fi.GetValue(target);
        PropertyInfo pi = target.GetType().GetProperty(name);
        if(pi != null)
            return (T)pi.GetValue(target, null);
        return default(T);
    }

    public static void SetPropValue<T>(object target, string name, T val)
    {
        FieldInfo fi = target.GetType().GetField(name);
        //Debugger.Assert(fi != null, "SetPropValue Field ["+name+"] not found");
        if(fi != null)
            fi.SetValue(target, val);
        PropertyInfo pi = target.GetType().GetProperty(name);
        if(pi != null)
            pi.SetValue(target, val, null);
    }

    public static  void FindColoredObject(GameObject go, ref List<object> list)
    {
        Renderer render = go.GetComponent<Renderer>();
        if(render != null && render.sharedMaterial != null)
        {
            var spr = go.GetComponent<tk2dBaseSprite>();
            if(spr != null)
                list.Add(spr);
            else
            {
                var textMesh = go.GetComponent<tk2dTextMesh>();
                if(textMesh != null)
                    list.Add(textMesh);
                else
                {
                    if(render.sharedMaterial.HasProperty("_Color"))
                        list.Add(go.GetComponent<Renderer>().material);
                }
            }
        }

        for(int i = 0; i < go.transform.childCount; ++i)
        {
            FindColoredObject(go.transform.GetChild(i).gameObject, ref list);   
        }
    }

    public static void interactTk2dButton(bool on, tk2dUIItem item, Transform trColorTarget)
    {
        if(null==item)          return;
        item.GetComponent<BoxCollider>().enabled    = on;
        Color toColor           = on ? Color.white : Color.grey;

        if(null != trColorTarget)
        {
            if(trColorTarget.GetComponent<tk2dSprite>())
                trColorTarget.GetComponent<tk2dSprite>().color  = toColor;
            else if(trColorTarget.GetComponent<tk2dSlicedSprite>())
                trColorTarget.GetComponent<tk2dSlicedSprite>().color = toColor;
        }
    }


    public static float tween_move(Transform trTarget, Vector3 from, Vector3 to, float fSpeed, int numBezierPoint=1, bool rotate=true)
    {
        trTarget.transform.position = from;

        float duration          = Vector2.Distance(from, to) / fSpeed;
        if(0 == numBezierPoint)
            trTarget.DOMove( to, duration);
        else
        {
            const float deepz   = -100.0f;
            Renderer rdr        = trTarget.GetComponent<Renderer>();
            if(null!=rdr)       rdr.sortingOrder        = 11;
            trTarget.transform.position = new Vector3(from.x, from.y, deepz);
            Vector3[] arrPoints = new Vector3[ numBezierPoint+1 ];
            for(int q = 0; q < numBezierPoint; q++)
            {
                Vector3 v1      = Vector3.Lerp(from, to, (float)q/((float)numBezierPoint+1));
                Vector3 v2      = Vector3.Lerp(from, to, ((float)q+1.0f)/((float)numBezierPoint+1));

                float hOffset   = Vector2.Distance(v1, v2) * 0.5f;
                v2 += Vector3.up*hOffset;
                v2              = new Vector3(v2.x, v2.y, deepz);
                arrPoints[q]    = v2;
            }
            arrPoints[ numBezierPoint ] = new Vector3(to.x, to.y, deepz);

            trTarget.DOPath(arrPoints, duration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(Ease.Linear);//.OutBack);
            //SR.transform.DOScale(1.0f, duration);
        }

        if(rotate)                  trTarget.DORotate(new Vector3(0, 0, 90), duration*0.25f).SetLoops(4, LoopType.Incremental);

        return duration;
    }
}
