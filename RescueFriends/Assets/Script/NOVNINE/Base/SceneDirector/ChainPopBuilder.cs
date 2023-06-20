using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Holoville.HOTween;
using DG.Tweening;

public class ChainPopBuilder : MonoBehaviour {

    public enum Y_DIRECTION { UP, DOWN };
    public Y_DIRECTION direction = Y_DIRECTION.DOWN;
    public float delaySecOnStart = .0f;
    public float popIntervalSec = 0.03f;
    public Transform[] ignoreList;

    List<Transform> items = new List<Transform>();

    void Awake ()
    {
        Init();
    }

    void OnEnable ()
    {
        StartCoroutine(AnimateChainPop());
    }

    void Init ()
    {
        GetTk2dUIItem(transform);
        RemoveItemsByIgnoreList();
        ArrangeItems(IsAscending());
    }

    void GetTk2dUIItem (Transform item)
    {
        tk2dUIItem curItem = null;
        curItem = item.GetComponent<tk2dUIItem>();
        if (curItem != null && items.Contains(item) == false) items.Add(item);

        for (int i = 0; i < item.transform.childCount; i++) {
            GetTk2dUIItem(item.GetChild(i));
        }
    }

    void ArrangeItems (bool isAscending) {
        if (isAscending == true) {
            items.Sort((a, b) => a.position.y.CompareTo(b.position.y));
        } else {
            items.Sort((a, b) => b.position.y.CompareTo(a.position.y));
        }
    }

    bool IsAscending () {
        return (direction == Y_DIRECTION.DOWN) ? false : true;
    }

    void RemoveItemsByIgnoreList () 
    {
        if (ignoreList.Length <= 0) return;

        for (int i = 0; i < items.Count; i++) {
            for (int j = 0; j < ignoreList.Length; j++) {
                if (items[i].name == ignoreList[j].name) items.Remove(items[i]);
            }
        }
    }

    IEnumerator AnimateChainPop ()
    {
        //HOTween.Complete(transform.name);
		DOTween.Complete(transform.name);
		
        if (items.Count <= 0) {
            //Wait that generate package or level widget 
            yield return new WaitForSeconds(0.01f);
            Init();
        } 

        int count = 0;
        //Sequence seq = new Sequence(new SequenceParms().Id(transform.name));
		Sequence seq = DOTween.Sequence();
		seq.SetId(transform.name);
		
        foreach (Transform item in items) {
            if (IsSamePositionWithPrevItem(item) == true) count--;

            float sec = (count * popIntervalSec) + delaySecOnStart;
            seq.Insert(sec, MakeTweener(item));
            count++;
        }
        seq.Play();
    }

    bool IsSamePositionWithPrevItem (Transform item)
    {
        int index = items.IndexOf(item);
        if (index == 0) return false;

        return item.position.y == items[index - 1].position.y ? true : false;
    }

	Tweener MakeTweener (Transform item)
    {
//        TweenParms parms = new TweenParms().Prop("localScale", Vector3.zero)
//                                           .Ease(EaseType.EaseInQuad)
//                                           .OnStart(() => { if (item.gameObject.activeSelf == false) item.gameObject.SetActive(true);})
//                                           .OnComplete(() => { AnimateLikePudding(item); });
		TweenParams parms = new TweenParams();
		parms.SetEase(Ease.InQuad);
		parms.OnStart(() =>
		{
			if(item.gameObject.activeSelf == false)
				item.gameObject.SetActive(true);
		});
		parms.OnComplete(() => { AnimateLikePudding(item); });
		
        //return HOTween.From(item, 0.2f, parms);
		return item.DOLocalMove(Vector3.zero,0.2F).SetAs(parms).From();
    }

    void AnimateLikePudding (Transform item) 
    {
//        TweenParms parms = new TweenParms().Prop("localScale", (Vector3.one * 0.95f))
//                                           .Ease(EaseType.EaseInOutBack);
//        HOTween.From(item, 0.2f, parms);
		
		TweenParams parms = new TweenParams();
		parms.SetEase(Ease.InOutBack);
		item.DOScale((Vector3.one * 0.95f),0.2f).SetAs(parms).From();
    }
}

