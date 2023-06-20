using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Holoville.HOTween;
using DG.Tweening;
using System.Linq;

[RequireComponent (typeof (tk2dUIItem))]
public class tk2dUIColorUpDown : MonoBehaviour
{
	GameObject target;
	public tk2dBaseSprite buttonSprite;

	public bool isChildTween = false;
	public float tweenDuration = 0.2f;

	class Tk2dColorSprite {
		public tk2dBaseSprite tk2dSprite;
		public Color upColor = Color.white;
	}
	
	class Tk2dColorTextMesh {
		public tk2dTextMesh tk2dTextMesh;
		public Color upColor = Color.white;
		public Color upBottomColor = Color.white;
	}
	
	class ColorTextMesh {
		public TextMesh textMesh;
		public Color upColor = Color.white;
	}

	public Color UpColor {
		get {
			return upColor;
		}
		set {
			upColor = value;
		}
	}

	public Color upColor = Color.white;
	public Color downColor = Color.white;
	
	List<Tk2dColorSprite> tk2dSprites = new List<Tk2dColorSprite> ();
	List<Tk2dColorTextMesh> tk2dTextMeshs = new List<Tk2dColorTextMesh> ();
	List<ColorTextMesh> textMeshs = new List<ColorTextMesh> ();

    void Start ()
    {
		SetColor (false);
    }

	void OnChangeTheme () {
		SetColor (true);
	}

	void SetColor(bool isChangeTheme = false) 
	{
		if (buttonSprite != null) target = buttonSprite.gameObject;
		if (target == null)	target = this.gameObject;
		
		tk2dBaseSprite[] arrayTk2dBaseSprite;
		tk2dTextMesh[] arrayTk2dTextMesh;
		TextMesh[] arrayTextMesh;
		if (isChildTween) {
			arrayTk2dBaseSprite = target.GetComponentsInChildren<tk2dBaseSprite> (true);
			arrayTk2dTextMesh = target.GetComponentsInChildren<tk2dTextMesh> (true);
			arrayTextMesh = target.GetComponentsInChildren<TextMesh> (true);
		} else {
			arrayTk2dBaseSprite = target.GetComponents<tk2dBaseSprite> ();
			arrayTk2dTextMesh = target.GetComponents<tk2dTextMesh> ();
			arrayTextMesh = target.GetComponents<TextMesh> ();
		}
		
		foreach (tk2dBaseSprite obj in arrayTk2dBaseSprite) {
			Tk2dColorSprite temp = new Tk2dColorSprite();
			temp.tk2dSprite = obj;
			if (temp.tk2dSprite.gameObject == target.gameObject) {
				temp.upColor = UpColor;
                if(isChangeTheme) temp.tk2dSprite.color = UpColor;
			} else {
				temp.upColor = obj.color;
			}
			tk2dSprites.Add(temp);
		}
		
		foreach (tk2dTextMesh obj in arrayTk2dTextMesh) {
			Tk2dColorTextMesh temp = new Tk2dColorTextMesh();
			temp.tk2dTextMesh = obj;
			if (temp.tk2dTextMesh.gameObject == target.gameObject) {
				temp.upColor = UpColor;
				if(isChangeTheme) temp.tk2dTextMesh.color = UpColor;
			} else {
				temp.upColor = obj.color;
			}
			if (obj.useGradient) temp.upBottomColor = obj.color2;
			tk2dTextMeshs.Add(temp);
		}
		
		foreach (TextMesh obj in arrayTextMesh) {
			ColorTextMesh temp = new ColorTextMesh();
			temp.textMesh = obj;
			if (temp.textMesh.gameObject == target.gameObject) {
				temp.upColor = UpColor;
				if(isChangeTheme) temp.textMesh.GetComponent<Renderer>().material.color = UpColor;
			} else {
				temp.upColor = obj.color;
			}
			textMeshs.Add(temp);
		}
	}

	void OnEnable()
	{
		tk2dUIItem item = GetComponent<tk2dUIItem>();
		if (item != null) {
			item.OnDownUIItem += OnDown;
			item.OnUpUIItem += OnUp;
		}
	}
	
	void OnDisable()
	{
		tk2dUIItem item = GetComponent<tk2dUIItem>();
		if (item != null) {
			item.OnDownUIItem -= OnDown;
			item.OnUpUIItem -= OnUp;
		}
	}

	void OnDown (tk2dUIItem item)
	{
		foreach (Tk2dColorSprite obj in tk2dSprites) {
//			HOTween.Kill(obj.tk2dSprite);
			DOTween.Kill(obj.tk2dSprite);
			Color dest = downColor * obj.upColor;
			if (obj.tk2dSprite.gameObject == target.gameObject) {
				dest = downColor;
			}
//			HOTween.To( obj.tk2dSprite, tweenDuration, "color", dest );
			DOTween.To(() => obj.tk2dSprite.color, x => obj.tk2dSprite.color = x, dest, tweenDuration);
		}
		foreach (Tk2dColorTextMesh obj in tk2dTextMeshs) {
//			HOTween.Kill(obj.tk2dTextMesh);
			DOTween.Kill(obj.tk2dTextMesh);			
			Color dest = downColor * obj.upColor;
			if (obj.tk2dTextMesh.gameObject == target.gameObject) {
				dest = downColor;
			}
//			HOTween.To( obj.tk2dTextMesh, tweenDuration, "color", dest );
			DOTween.To(() => obj.tk2dTextMesh.color, x => obj.tk2dTextMesh.color = x, dest, tweenDuration);
//			if (obj.tk2dTextMesh.useGradient) HOTween.To( obj.tk2dTextMesh, tweenDuration, "color2", downColor * obj.upBottomColor );
			if (obj.tk2dTextMesh.useGradient) DOTween.To(() => obj.tk2dTextMesh.color2, x => obj.tk2dTextMesh.color2 = x, downColor * obj.upBottomColor, tweenDuration);
			
		}
		
		foreach (ColorTextMesh obj in textMeshs) {
			//HOTween.Kill(obj.textMesh.GetComponent<Renderer>().material);
			DOTween.Kill(obj.textMesh.GetComponent<Renderer>().material);
			Color dest = downColor * obj.upColor;
			if (obj.textMesh.gameObject == target.gameObject) {
				dest = downColor;
			}
//			HOTween.To( obj.textMesh.GetComponent<Renderer>().material, tweenDuration, "color", dest );
			DOTween.To(() => obj.textMesh.color, x => obj.textMesh.color = x, dest, tweenDuration);
		}
	}
	
	void OnUp (tk2dUIItem item)
	{
		foreach (Tk2dColorSprite obj in tk2dSprites) {
//			HOTween.Kill(obj.tk2dSprite);
//			HOTween.To( obj.tk2dSprite, tweenDuration, "color", obj.upColor );
			
			DOTween.Kill(obj.tk2dSprite);
			DOTween.To(() => obj.tk2dSprite.color, x => obj.tk2dSprite.color = x, obj.upColor, tweenDuration);
		}
		foreach (Tk2dColorTextMesh obj in tk2dTextMeshs) {
//			HOTween.Kill(obj.tk2dTextMesh);
//			HOTween.To( obj.tk2dTextMesh, tweenDuration, "color", obj.upColor );
//			if (obj.tk2dTextMesh.useGradient) HOTween.To( obj.tk2dTextMesh, tweenDuration, "color2", obj.upBottomColor );
			DOTween.Kill(obj.tk2dTextMesh);
			DOTween.To(() => obj.tk2dTextMesh.color, x => obj.tk2dTextMesh.color = x, obj.upColor, tweenDuration);
			if (obj.tk2dTextMesh.useGradient) DOTween.To(() => obj.tk2dTextMesh.color2, x => obj.tk2dTextMesh.color2 = x, obj.upBottomColor, tweenDuration);
			
		}
		foreach (ColorTextMesh obj in textMeshs) {
//			HOTween.Kill(obj.textMesh.GetComponent<Renderer>().material);
//			HOTween.To( obj.textMesh.GetComponent<Renderer>().material, tweenDuration, "color", obj.upColor );
			DOTween.Kill(obj.textMesh);
			DOTween.To(() => obj.textMesh.color, x => obj.textMesh.color = x, obj.upColor, tweenDuration);
		}
	}
}
