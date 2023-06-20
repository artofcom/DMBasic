using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Remoting;
using NOVNINE.Diagnostics;
//using Holoville.HOTween;
using DG.Tweening;

public interface ITransition {
    string Name { get; }
    IEnumerator ShowTransitionEffect (float duration, Action onDidLeaveScene, Action onDidEnterScene);
}

public class DelayTransition : ITransition {
    public string Name { 
        get { return "Delay";} 
    }

    public IEnumerator ShowTransitionEffect (float duration, Action onDidLeaveScene, Action onDidEnterScene) {
        if (duration > 0f) yield return new WaitForSeconds (duration);
        if (onDidLeaveScene != null) onDidLeaveScene();
        if (onDidEnterScene != null) onDidEnterScene();
    }
}

public class FadeTransition : ITransition {
    public string Name { 
        get { return "Fade";} 
    }

    public IEnumerator ShowTransitionEffect (float duration, Action onDidLeaveScene, Action onDidEnterScene) {
        Debugger.Assert(duration > 0, "Duration time must be bigger than zero.");

        float fadeTime = duration * 0.5f;

        if (Scene.CurrentScene() != null) Scene.Lock();
        GUISceneFader.FadeOut(fadeTime, 1f);
        yield return new WaitForSeconds(fadeTime);
        if (Scene.CurrentScene() != null) Scene.Unlock();

        if (onDidLeaveScene != null) onDidLeaveScene();

        Scene.Lock();
        GUISceneFader.FadeIn(fadeTime);
        yield return new WaitForSeconds(fadeTime);
        if (onDidEnterScene != null) onDidEnterScene();
        Scene.Unlock();
    }
}

public class SpotlightTransition : ITransition {
    public string Name { 
        get { return "Spotlight";} 
    }

    public IEnumerator ShowTransitionEffect (float duration, Action onDidLeaveScene, Action onDidEnterScene) {
        Debugger.Assert(duration > 0, "Duration time must be bigger than zero.");

        float spotTime = duration * 0.5f;

        if (Scene.CurrentScene() != null) {
            Scene.Lock();
		GUISceneSpotlight.Show(spotTime, "Spotlight");
            yield return new WaitForSeconds(spotTime);
            Scene.Unlock();
        }

        if (onDidLeaveScene != null) onDidLeaveScene();

        Scene.Lock();
        yield return new WaitForSeconds(0.2f);
		GUISceneSpotlight.Hide(spotTime, "Spotlight");
        yield return new WaitForSeconds(spotTime);
        if (onDidEnterScene != null) onDidEnterScene();
        Scene.Unlock();
    }
}

public class StarSpotlightTransition : ITransition {
    public string Name { 
        get { return "StarSpotlight";} 
    }

    public IEnumerator ShowTransitionEffect (float duration, Action onDidLeaveScene, Action onDidEnterScene) {
        Debugger.Assert(duration > 0, "Duration time must be bigger than zero.");

        float spotTime = duration * 0.5f;

        if (Scene.CurrentScene() != null) {
            Scene.Lock();
            GUISceneSpotlight.Show(spotTime, "StarSpotlight");
            yield return new WaitForSeconds(spotTime);
            Scene.Unlock();
        }

        if (onDidLeaveScene != null) onDidLeaveScene();

        Scene.Lock();
        yield return new WaitForSeconds(0.2f);
        GUISceneSpotlight.Hide(spotTime, "StarSpotlight");
        yield return new WaitForSeconds(spotTime);
        if (onDidEnterScene != null) onDidEnterScene();
        Scene.Unlock();
    }
}

public class HexagonSpotlightTransition : ITransition {
    public string Name { 
        get { return "HexagonSpotlight";} 
    }

    public IEnumerator ShowTransitionEffect (float duration, Action onDidLeaveScene, Action onDidEnterScene) {
        Debugger.Assert(duration > 0, "Duration time must be bigger than zero.");

        float spotTime = duration * 0.5f;

        if (Scene.CurrentScene() != null) {
            Scene.Lock();
            GUISceneSpotlight.Show(spotTime, "HexagonSpotlight");
            yield return new WaitForSeconds(spotTime);
            Scene.Unlock();
        }

        if (onDidLeaveScene != null) onDidLeaveScene();

        Scene.Lock();
        yield return new WaitForSeconds(0.2f);
        GUISceneSpotlight.Hide(spotTime, "HexagonSpotlight");
        yield return new WaitForSeconds(spotTime);
        if (onDidEnterScene != null) onDidEnterScene();
        Scene.Unlock();
    }
}

public class SlideTransition : ITransition {
    public string Name { 
        get { return "Slide";} 
    }

    public IEnumerator ShowTransitionEffect (float duration, Action onDidLeaveScene, Action onDidEnterScene) {
        Debugger.Assert(duration > 0, "Duration time must be bigger than zero.");

		DOTween.Complete("SlideTransition");

        GameObject prevRoot = null;
        Camera prev = null;
        Camera next = null;
        if (Scene.CurrentScene() != null) {
			prev = Camera.main;
            prevRoot = Scene.FindSceneRoot(prev.gameObject);
        }

        float offsetX = 20;

        if (onDidLeaveScene != null) onDidLeaveScene();
        if (Scene.CurrentScene() != null) {
			next = Camera.main;
        }

        if(prev != null) {
            offsetX = Scene.GetCameraSizeInWorldSpace(prev).x;
        } else if(next != null) {
            offsetX = Scene.GetCameraSizeInWorldSpace(next).x;
        }
        if(Scene.moveBackward)
            offsetX = -20;

        Scene.Lock();

		Sequence seq = DOTween.Sequence();
		seq.SetId("SlideTransition");
		seq.OnComplete(() =>
		{
			if(onDidEnterScene != null)
				onDidEnterScene();
			Scene.Unlock();
			if(prev != null)
			{
				prev.transform.Translate(new Vector3(-offsetX, 0, 0));
				prevRoot.SetActive(false);
			}
		});
		
        if(prev != null) {
            prevRoot.SetActive(true);
			seq.Insert(0, prev.transform.DOLocalMove(new Vector3(offsetX,0,0),duration, true));
        }
        if(next != null) {
            next.transform.Translate(new Vector3(-offsetX,0,0));
			seq.Insert(0, next.transform.DOLocalMove( new Vector3(offsetX,0,0),duration, true));
        }
        yield return null;
        seq.Play();
    }
}

public class SpriteTransition : ITransition {

    SpriteRenderer _objSprite     = null;
    SpriteRenderer _objSubSprite  = null;
    //SpriteRenderer[] _sprChildren       = null;

    public void setSprite(SpriteRenderer target, SpriteRenderer subTarget=null)
    {
        _objSprite              = target;
        _objSubSprite           = subTarget;
    }

    public string Name { 
        get { return "Sprite";} 
    }

    public IEnumerator ShowTransitionEffect (float duration, Action onDidLeaveScene, Action onDidEnterScene) {
        Debugger.Assert(duration > 0, "Duration time must be bigger than zero.");
        
        float fadeTime = duration * 0.25f;

        if (Scene.CurrentScene() != null) Scene.Lock();
        //GUISceneFader.FadeOut(fadeTime, 1f);
        
        if(null != _objSprite)
        { 
            _objSprite.gameObject.SetActive( true );
            _objSprite.DOFade(1.0f, fadeTime);
        }
        if(null != _objSubSprite)
        { 
            _objSubSprite.gameObject.SetActive( true );
            _objSubSprite.DOFade(1.0f, fadeTime);
        }
        //if(null == _sprChildren)
        //    _sprChildren        = _sprHeadRenderer.GetComponentsInChildren<SpriteRenderer>();
	    //foreach(var r in _sprChildren) {
        //    r.enabled           = true;
        //    r.DOFade(1.0f, fadeTime);
	    //}
        yield return new WaitForSeconds(fadeTime);
        if (Scene.CurrentScene() != null) Scene.Unlock();
        if (onDidLeaveScene != null) onDidLeaveScene();

        yield return new WaitForSeconds(fadeTime*2.0f);

        Scene.Lock();
        //GUISceneFader.FadeIn(fadeTime);
        //NOVNINE.GameObjectExt.FadeOutRecursively(_sprHeadRenderer.gameObject, fadeTime, false);
        if(null!=_objSprite)    _objSprite.DOFade(.0f, fadeTime);
        if(null!=_objSubSprite) _objSubSprite.DOFade(.0f, fadeTime);
	    //foreach(var r in _sprChildren) {
        //    r.DOFade(.0f, fadeTime);
	   // }
        yield return new WaitForSeconds(fadeTime);
        if(null!=_objSprite)    _objSprite.gameObject.SetActive( false );
        if(null!=_objSubSprite) _objSubSprite.gameObject.SetActive( false );
        //foreach(var r in _sprChildren) {
        //    r.enabled           = false;
	    //}
        if (onDidEnterScene != null) onDidEnterScene();
        Scene.Unlock();
    }
}

/*
 * //amugana ( amugana@bitmango.com ) Last modified at : 2015.09.03 
 * IL2CPP fix for unity 4.6.71f
 * DO NOT USE Activator on Runtime
public static class TransitionFactory {
    public static ITransition CreateTransition (string transitionName) {
        ObjectHandle handle = Activator.CreateInstance(null, transitionName+"Transition"); 
        return (ITransition)handle.Unwrap();
    }
}
*/
