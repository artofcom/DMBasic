using UnityEngine;
using System.Reflection;
//using Holoville.HOTween;
//using Holoville.HOTween.Plugins;
//using Holoville.HOTween.Core;
using DG.Tweening;
using NOVNINE.Diagnostics;

namespace NOVNINE
{
	public static class GameObjectExt
	{
	
	    #region Reflection
	    
	    public static T CreateTemporaryGameObjectWithComponent<T>() where T : Component
	    {
	        GameObject go = new GameObject();
	        go.name = typeof(T).FullName;
	        //go.hideFlags = HideFlags.DontSave;
	        Object.DontDestroyOnLoad(go);
	        return go.AddComponent<T>();
	    }
	
	    public static T CreateTemporaryGameObjectWithComponent<T>(this GameObject thisGO) where T : Component
	    {
	        GameObject go = new GameObject();
	        go.name = typeof(T).FullName;
	        //go.hideFlags = HideFlags.DontSave;
	        Object.DontDestroyOnLoad(go);
	        go.transform.parent = thisGO.transform;
	        return go.AddComponent<T>();
	    }
	
	    public static T GetOrCreateComponent<T>(this GameObject thisGO) where T : Component
	    {
	        T co = thisGO.GetComponent<T>();
	        if(co == null) return thisGO.AddComponent<T>();
	        else return co;
	    }
	
	    public static GameObject CreateGameObjectWithName(this GameObject thisGO, string name)
		{
			GameObject go = new GameObject();
			go.name = name;
			go.transform.parent = thisGO.transform;
			go.transform.localPosition = Vector3.zero;
	        return go;
		}
	
	    public static GameObject GetOrCreateGameObjectWithName(this GameObject thisGO, string name)
		{
			Transform found = thisGO.transform.Find(name);
			if(found == null) {
				GameObject go = new GameObject();
				go.name = name;
				go.transform.parent = thisGO.transform;
				go.transform.localPosition = Vector3.zero;
				found = go.transform;
			}
	        return found.gameObject;
		}
	
	
	    #endregion
	
	    #region Transform, Color,
	    private static object FindColoredObject(GameObject go)
	    {
	        if (go.GetComponent<Renderer>() == null) return null;
	        if (go.GetComponent<Renderer>().sharedMaterial == null) return null;
	
	        var spr = go.GetComponent<tk2dBaseSprite>();
	        if (spr != null) return spr;
	
	        var textMesh = go.GetComponent<tk2dTextMesh>();
	        if (textMesh != null) return textMesh;
	
	        if (go.GetComponent<Renderer>().sharedMaterial.HasProperty("_Color")) return go.GetComponent<Renderer>().material;
	
	        Debug.LogError("FindColoredObject Fail : "+go.name);
	        return null;
	    }
		
		public static Tweener DelayRound(this GameObject go, int count, float dur, float delay = 0.0f, bool clockwise = true)
		{
			DOTween.Complete(go.transform);	
			float angle = ( clockwise == true) ? -360f * count : 360f * count ;
			TweenParams parm = new TweenParams();
			parm.SetEase(Ease.Linear);
			if(dur < 0) {
				parm.SetLoops((int)dur);
				dur = -dur;
			}
			parm.SetDelay(delay);
			return go.transform.DOLocalRotate(new Vector3(0,0,angle), dur,RotateMode.FastBeyond360 ).SetAs(parm);
		}
		
	    public static Tweener Round(this GameObject go, int count, float dur, bool clockwise = true)
	    {
			DOTween.Complete(go.transform);	
	        float angle = ( clockwise == true) ? -360f * count : 360f * count ;
			TweenParams parm = new TweenParams();
			parm.SetEase(Ease.Linear);
			if(dur < 0) {
					parm.SetLoops((int)dur);
					dur = -dur;
				}
			return go.transform.DOLocalRotate(new Vector3(0,0,angle), dur,RotateMode.FastBeyond360 ).SetAs(parm);
	    }
	
	    public static Tweener RoundX(this GameObject go, float angle, float dur, bool clockwise = true)
	    {
			DOTween.Complete(go.transform);	
			TweenParams parm = new TweenParams();
			parm.SetEase(Ease.Linear);
			if(dur < 0) {
				parm.SetLoops((int)dur);
				dur = -dur;
			}
			return go.transform.DOLocalRotate(new Vector3(angle,0,0), dur,RotateMode.FastBeyond360 ).SetAs(parm);
	    }
	
	    public static Tweener RoundY(this GameObject go, float angle, float dur, bool clockwise = true)
	    {
			DOTween.Complete(go.transform);	
			TweenParams parm = new TweenParams();
			parm.SetEase(Ease.Linear);
			if(dur < 0) {
				parm.SetLoops((int)dur);
				dur = -dur;
			}
			return go.transform.DOLocalRotate(new Vector3(0,angle,0), dur,RotateMode.FastBeyond360 ).SetAs(parm);
	    }
	
	    public static Tweener ShakeRot(this GameObject go, float dur, float angle)
	    {
			DOTween.Complete(go.transform);	
			TweenParams parm = new TweenParams();
			parm.SetEase(Ease.OutElastic, 0.1f, 0.15f);
			
			return go.transform.DOLocalRotate(new Vector3(0,0,angle), dur).SetAs(parm).From();
	    }
	
	    public static Tweener Shake(this GameObject go, float dur, Vector2 offset)
	    {
			DOTween.Complete(go.transform);	
			TweenParams parm = new TweenParams();
			parm.SetEase(Ease.OutElastic, 0.1f, 0.15f);
	
			return go.transform.DOLocalMove(new Vector3(offset.x,offset.y,0), dur).SetAs(parm).From();
	    }
	
	    public static Tweener Rotate(this GameObject go, float eulerZ, float dur)
	    {
			DOTween.Complete(go.transform);
			return go.transform.DOLocalRotate(new Vector3(0,0,eulerZ), dur);
	    }
	
	    public static Tweener RotateY(this GameObject go, float eulerY, float dur)
	    {
			DOTween.Complete(go.transform);
			return go.transform.DOLocalRotate(new Vector3(0,eulerY,0), dur);
	    }
	
	    public static void PopRotate(this GameObject go)
	    {
	     	DOTween.Complete(go.name);
	        Vector3 scale = go.transform.localScale;
			Sequence seq = DOTween.Sequence();
			seq.SetId(go.name);
			seq.Append( go.transform.DOScale( scale*1.2f, 0.2f));
			seq.Append( go.Round(1, 1) );
			seq.Insert( 1.3f, go.transform.DOScale( scale, 0.2f));
				
	        seq.Play();
	    }
	
		public static Tweener FadeIn(this GameObject go, float duration=0.5f, float targetAlpha=1.0f, TweenCallback onComplete = null)
	    {
	        object colorProp = FindColoredObject(go);
	        if(colorProp != null) 
			{
	            DOTween.Complete(colorProp);
                Color clr = JMFUtils.GetPropValue<Color>(colorProp, "color");
	            clr.a = 0;
                JMFUtils.SetPropValue<Color>(colorProp, "color", clr);
	            //colorProp.color = clr;
	            clr.a = targetAlpha;
	            TweenParams tp = new TweenParams();
				tp.OnStart(() => {Activate(go);});
	            if (null != onComplete)
	                tp.OnComplete(onComplete);
	            	
				tk2dBaseSprite spr = colorProp as tk2dBaseSprite;
					
				if(spr != null)
					return DOTween.To(() => spr.color, x => spr.color = x, clr, duration).SetAs(tp);	
				else
				{
					tk2dTextMesh textMesh = colorProp as tk2dTextMesh;
					if(textMesh != null)
						return DOTween.To(() => textMesh.color, x => textMesh.color = x, clr, duration).SetAs(tp);
					else
					{
						Material m = colorProp as Material;	
						if(m != null)
							return DOTween.To(() => m.color, x => m.color = x, clr, duration).SetAs(tp);
					}
				}
	        }
				
	        Debug.LogWarning("GameObject ["+go.name+"] Has no color property");
	        return null;
	    }
	
        //public static Sequence FadeInRecursively(this GameObject go, float opcaity)
		//{
	    //    Renderer[] rens = go.GetComponentsInChildren<Renderer>();
	    //    foreach(var r in rens) {
	    //        r. seq.Insert( 0, r.gameObject.FadeIn(duration, targetAlpha));
	    //    }
	    //}

		public static Sequence FadeInRecursively(this GameObject go, float duration=0.5f, float targetAlpha=1.0f, TweenCallback onComplete = null)
		{
			DOTween.Complete(go.name);
			Sequence seq = DOTween.Sequence();
			seq.SetId(go.name);
			if(onComplete != null)
				seq.OnComplete(onComplete);
			
	        seq.Insert( 0, go.FadeIn(duration, targetAlpha));
	
	        Renderer[] rens = go.GetComponentsInChildren<Renderer>();
	        foreach(var r in rens) {
	            seq.Insert( 0, r.gameObject.FadeIn(duration, targetAlpha));
	        }
	        seq.Play();
	        return seq;
	    }
	
		public static Tweener FadeOut(this GameObject go, float duration=0.5f, bool deActivate = false, TweenCallback onComplete = null)
	    {
	        object colorProp = FindColoredObject(go);
	        if(colorProp != null)
			{
				DOTween.Complete(colorProp);
                Color clr = JMFUtils.GetPropValue<Color>(colorProp, "color");
	            clr.a = 0;
	
				TweenParams tp = new TweenParams();
				tp.OnComplete(() => {Deactivate( go, deActivate, onComplete);});
						
				tk2dBaseSprite spr = colorProp as tk2dBaseSprite;
	
				if(spr != null)
					return DOTween.To(() => spr.color, x => spr.color = x, clr, duration).SetAs(tp);	
				else
				{
					tk2dTextMesh textMesh = colorProp as tk2dTextMesh;
					if(textMesh != null)
						return DOTween.To(() => textMesh.color, x => textMesh.color = x, clr, duration).SetAs(tp);
					else
					{
						Material m = colorProp as Material;	
						if(m != null)
							return DOTween.To(() => m.color, x => m.color = x, clr, duration).SetAs(tp);
					}
				}
			}
	
			Debug.LogWarning("GameObject ["+go.name+"] Has no color property");
	        return null;
	    }
	
		public static Sequence FadeOutRecursively(this GameObject go, float duration=0.5f, bool deActivate=false, TweenCallback onComplete = null)
	    {
			DOTween.Complete(go.name);
			Sequence seq = DOTween.Sequence();
			seq.SetId(go.name);
			seq.OnComplete(() => {Deactivate( go, deActivate, onComplete);});
	        seq.Insert( 0, go.FadeOut(duration, deActivate));
	
	        Renderer[] rens = go.GetComponentsInChildren<Renderer>();
	        foreach(var r in rens)
			{
	            seq.Insert( 0, r.gameObject.FadeOut(duration, deActivate));
	        }
	        seq.Play();
	        return seq;
	    }
	
	    public static Tweener Popup(this GameObject go, float dur)
	    {
			DOTween.Complete(go.transform);
			return go.transform.DOScale(Vector3.one,dur).SetEase(Ease.OutBack).OnStart(() => {Activate(go);});
	    }
	
	    public static Tweener Popout(this GameObject go, float dur, bool autoDestruct)
	    {
			DOTween.Complete(go.transform);
			return go.transform.DOScale(Vector3.zero,dur).SetEase(Ease.InQuad).OnComplete(() => {Deactivate(go, autoDestruct);});
	    }
	
	    public static Tweener PopupFadeOut(this GameObject go, float dur, bool autoDestruct = false)
	    {
	        go.FadeOut(dur, autoDestruct);
			DOTween.Complete(go.transform);
			return go.transform.DOScale( new Vector3(2,2,2),dur);
	    }
	
	    public static Tweener PopupFadeIn(this GameObject go, float dur = 1)
	    {
	        go.FadeIn(dur);
			DOTween.Complete(go.transform);
			return go.transform.DOScale(new Vector3(2,2,2),dur).From();
	    }
	
	    public static Tweener Bounce(this GameObject go, float offsetY, int count, float period = 0.2f, string tweenId = null)
	    {
			DOTween.Complete(go.transform);
	        Vector3 pos = go.transform.position;
	        Vector3 posUp = pos;
	        posUp.y += offsetY;
			TweenParams tween = new TweenParams();
			tween.SetEase(Ease.OutBounce, 10);
			tween.SetLoops(count);
			if(tweenId != null)
				tween.SetId(tweenId);
				
			return go.transform.DOPath(new Vector3[]{ pos, posUp, pos},period);
	    }
	
		public static Tweener Slide(this GameObject go, Vector3 offset, float dur, Ease ease = Ease.OutQuad, float easeParam = 1.7f)		
	    {
			DOTween.Complete(go.transform);
			return go.transform.DOMove(offset,dur, true).SetEase(ease, easeParam);
	    }
	
		public static Tweener Slide2(this GameObject go, Vector2 offset, float dur, Ease ease = Ease.OutQuad, float easeParam = 1.7f)
	    {
	        return go.Slide(new Vector3(offset.x, offset.y, 0), dur, ease, easeParam);
	    }
	
		public static Tweener SlideLocal(this GameObject go, Vector3 offset, float dur, Ease ease = Ease.OutQuad, float easeParam = 1.7f)
	    {
			DOTween.Complete(go.transform);
			return go.transform.DOLocalMove( offset,dur, true).SetEase(ease, easeParam);
	    }
	
		public static Tweener SlideLocal2(this GameObject go, Vector2 offset, float dur, Ease ease = Ease.OutQuad, float easeParam = 1.7f)
	    {
	        return go.SlideLocal(new Vector3(offset.x, offset.y, 0), dur, ease, easeParam);
	    }
	
		public static Tweener SlideLocalAbs(this GameObject go, Vector3 offset, float dur, Ease ease = Ease.OutQuad, float easeParam = 1.7f)
	    {
			DOTween.Complete(go.transform);
			return go.transform.DOLocalMove( offset,dur, false).SetEase(ease, easeParam);
	    }
	
		public static Tweener SlideXAbs(this GameObject go, float xPos, float dur, Ease ease = Ease.OutQuad, float easeParam = 1.7f)
	    {
			DOTween.Complete(go.transform);
	        Vector3 pos = go.transform.position;
	        pos.x = xPos;
			return go.transform.DOLocalMove( pos, dur, false).SetEase(ease, easeParam);
	    }
	
		public static Tweener SlideYAbs(this GameObject go, float yPos, float dur, Ease ease = Ease.OutQuad, float easeParam = 1.7f)
	    {
			DOTween.Complete(go.transform);
	        Vector3 pos = go.transform.position;
	        pos.y = yPos;
			return go.transform.DOLocalMove(pos, dur, false).SetEase(ease, easeParam);
	    }
	
		public static Tweener SlideX(this GameObject go, float xPos, float dur, Ease ease = Ease.OutQuad, float easeParam = 1.7f)
	    {
			DOTween.Complete(go.transform);
			return go.transform.DOLocalMove(new Vector3(xPos,0,0), dur, true).SetEase(ease, easeParam);
	    }
	
		public static Tweener SlideY(this GameObject go, float yPos, float dur, Ease ease = Ease.OutQuad, float easeParam = 1.7f)
	    {
			DOTween.Complete(go.transform);
			return go.transform.DOLocalMove(new Vector3(0,yPos,0), dur, true).SetEase(ease, easeParam);
	    }
	
	    public static Tweener SlideXYElastic(this GameObject go, float xPos, float yPos, float dur, float amplitude, float period)
	    {
			DOTween.Complete(go.transform);
			return go.transform.DOLocalMove( new Vector3(xPos, yPos, 0), dur, true).SetEase(Ease.OutElastic, amplitude, period);
	    }
	
	    public static Tweener Blink(this GameObject go, float freq)
	    {
	        object colorProp = FindColoredObject(go);
	        if(colorProp != null) 
			{
				DOTween.Kill(colorProp);
	            if(freq < 0) return null;
                Color clr = JMFUtils.GetPropValue<Color>(colorProp, "color");
	            clr.a = 0;
                JMFUtils.SetPropValue<Color>(colorProp, "color", clr);
	            clr.a = 1;
	
	            float period = 1.0f/(freq*2);
				TweenParams tp = new TweenParams();
				tp.SetLoops(-1, LoopType.Yoyo).OnStart(() => {Activate(go);});
				
				tk2dBaseSprite spr = colorProp as tk2dBaseSprite;
	
				if(spr != null)
					return DOTween.To(() => spr.color, x => spr.color = x, clr, period).SetAs(tp);	
				else
				{
					tk2dTextMesh textMesh = colorProp as tk2dTextMesh;
					if(textMesh != null)
						return DOTween.To(() => textMesh.color, x => textMesh.color = x, clr, period).SetAs(tp);
					else
					{
						Material m = colorProp as Material;	
						if(m != null)
							return DOTween.To(() => m.color, x => m.color = x, clr, period).SetAs(tp);
					}
				}
			}
	
			Debug.LogWarning("GameObject ["+go.name+"] Has no color property");
	        return null;
	    }
	
		//Infinity : count=-1
	    public static Tweener Blink2(this GameObject go, int count, float period = 0.6f, float alphaMax = 1)
	    {
	        object colorProp = FindColoredObject(go);
	        if(colorProp != null) 
			{
				DOTween.Kill(colorProp);
                Color clr = JMFUtils.GetPropValue<Color>(colorProp, "color");
	            clr.a = 0;
                JMFUtils.SetPropValue<Color>(colorProp, "color", clr);
	            clr.a = alphaMax;
	            period *= 0.5f;
				TweenParams tp = new TweenParams();
				tp.SetLoops(count*2, LoopType.Yoyo).OnStart(() => {Activate(go);});
					
				tk2dBaseSprite spr = colorProp as tk2dBaseSprite;
	
				if(spr != null)
					return DOTween.To(() => spr.color, x => spr.color = x, clr, period).SetAs(tp);	
				else
				{
					tk2dTextMesh textMesh = colorProp as tk2dTextMesh;
					if(textMesh != null)
						return DOTween.To(() => textMesh.color, x => textMesh.color = x, clr, period).SetAs(tp);
					else
					{
						Material m = colorProp as Material;	
						if(m != null)
							return DOTween.To(() => m.color, x => m.color = x, clr, period).SetAs(tp);
					}
				}
			}
	
			Debug.LogWarning("GameObject ["+go.name+"] Has no color property");
	        return null;
	    }
	
	    public static bool StopBlink(this GameObject go/*, Color newColor = Color.white*/)
	    {
	        object colorProp = FindColoredObject(go);
	        if(colorProp != null) 
			{
				DOTween.Kill(colorProp);
                JMFUtils.SetPropValue<Color>(colorProp, "color", Color.white);
				return true;
			}
			return false;
	    }
	    #endregion
	
	    /* remove candidate, tk2d Dependency codde
	        public static void TextPopupEffect(this GameObject go, GameObject prefab, string message, Color color)
	        {
	            Vector3 pos = go.transform.position;
	            if(go.renderer != null)
	                pos = go.renderer.bounds.center;
	            pos.z -= 0.5f;
	            GameObject inst = GameObject.Instantiate(prefab, pos, go.transform.rotation) as GameObject;
	            tk2dTextMesh mesh = inst.GetComponent<tk2dTextMesh>();
	            mesh.text = message;
	            //mesh.color = color;
	            mesh.Commit();
	            inst.PopupFadeOut(true);
	        }
	
	        public static void CommitTextMesh(TweenEvent data)
	        {
	            tk2dTextMesh tx = (tk2dTextMesh)data.parms[0];
	            tx.Commit();
	        }
	        */
	
	//    #region HOTween callbacks
	//    public static void Activate(TweenEvent data)
	//    {
	//        GameObject go = data.parms[0] as GameObject;
	//        if(go == null) return;
	//        if(go.GetComponent<Renderer>() != null)
	//            go.GetComponent<Renderer>().enabled = true;
	//        if(!go.activeSelf)
	//            go.SetActive(true);
	//    }
	//    public static void Deactivate(TweenEvent data)
	//    {
	//        GameObject go = data.parms[0] as GameObject;
	//        if(go == null) return;
	//        bool autoDestruct = (bool)data.parms[1];
	//        if(autoDestruct)
	//            go.SetActive(false);
	//        else if(go.GetComponent<Renderer>() != null)
	//            go.GetComponent<Renderer>().enabled = false;
	//        else {
	//            Debug.LogWarning("Deactivate On Empty gameObejct");
	//        }
	//
	//        if(data.parms.Length >= 3) {
	//            TweenDelegate.TweenCallback customCallback = data.parms[2] as TweenDelegate.TweenCallback;
	//            if(customCallback != null)
	//                customCallback();
	//        }
	//    }
	//
	//    #endregion
	#region DOTween callbacks
		public static void Activate(GameObject go)
		{
			if(go == null) return;
			if(go.GetComponent<Renderer>() != null)
				go.GetComponent<Renderer>().enabled = true;
			if(!go.activeSelf)
				go.SetActive(true);
		}
			
		public static void Deactivate(GameObject go,bool deActivate = false, TweenCallback onComplete = null)
		{
			if(go == null) return;
			
			if(deActivate)
				go.SetActive(false);
			else if(go.GetComponent<Renderer>() != null)
				go.GetComponent<Renderer>().enabled = false;
			else
				Debug.LogWarning("Deactivate On Empty gameObejct");
			
			if(onComplete != null)
				onComplete();
		}
	
	#endregion
	
	    public static void PositionFromBottom(this GameObject go, float pixelOffset)
	    {
	        Vector3 localPos = go.transform.localPosition;
	        float boundsOffset = 0;
	        if(go.GetComponent<Renderer>() != null)
	            boundsOffset = go.GetComponent<Renderer>().bounds.extents.y;
	        
	        float scale = Camera.main.orthographicSize / Screen.height * 2;
	        localPos.y = -Camera.main.orthographicSize + boundsOffset + pixelOffset*scale;
	        go.transform.localPosition = localPos;
	    }
	
	    /*
	    #region NGUI
	        public static T GetOrCreateWidgetNamed<T>( this GameObject parent, string widgetName) where T : UIWidget
	        {
	            T newWidget = null;
	            Transform child = parent.transform.Find(widgetName);
	            if(child == null)
	            {
	                newWidget = NGUITools.AddWidget<T>(parent);
	                newWidget.gameObject.name = widgetName;
	            }
	            else
	            {
	                newWidget = child.GetComponent<T>();
	    #if UNITY_EDITOR
	                if(newWidget == null)
	                    Debug.LogError("GetOrCreateWidgetNamed error : Component "+typeof(T).FullName+" Not Found on Widget "+widgetName);
	    #endif
	            }
	            return newWidget;
	        }
	
	    #endregion
	    */
	
	}
}

