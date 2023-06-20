using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Holoville.HOTween;
//using Holoville.HOTween.Plugins;
using DG.Tweening;

[RequireComponent(typeof(BoxCollider))]
public class ScrollView  : MonoBehaviour
{
    public Camera viewCamera;
	
    public bool haveExpendSlot=false;
	
    public GameObject itemRoot;
    public List<GameObject> items;

    public float margin = 0f;   // 최초 이동을 위한 필요거리

    [System.NonSerialized]
    public bool marginLock = true;    // 최초이동의 잠금여부

    public float scrollSensitivity = 1f;

    public float bounceBackSpeed = 1f;
    public float bounceBackRate = 0.25f;

    public Vector2 defaultDirection = Vector2.right;

    // Messaging
    public GameObject targetObject = null;
    public string messageScrollBegin = "";
    public string messageScroll = "";
    public string messageScrollEnd = "";

    public System.Action<ScrollView> onScrollBegin;
    public System.Action<ScrollView> onScroll;
    public System.Action<ScrollView> onScrollEnd;

    int currentIndex = 0;
    int targetIndex = 0;

    Vector3 initPosition = Vector3.zero;
    Vector3 rootPosition = Vector3.zero;

    Vector3 downPosition = Vector3.zero;		// Drag start position.
    Vector3 currentPosition = Vector3.zero;	// Drag current Position

    bool buttonDown = false;

    void Awake()
    {
        #region Assert Validation
        if (itemRoot.transform.parent != this.transform) {
            Debug.LogWarning("Item Root's Parent must be ScrollView!!!");
        }

        if (itemRoot.transform.localScale != Vector3.one) {
            Debug.LogWarning("Item Root's LocalScale must be Vector3.one!!!");
        }

        foreach (GameObject obj in items) {
            if (obj.transform.parent != itemRoot.transform) {
                Debug.LogWarning("Scroll Items Root's Parent must be ItemRoot!!!");
            }
        }
        #endregion

        if (null == viewCamera)
            viewCamera = Camera.main;

        initPosition = itemRoot.transform.localPosition;
    }

    void Start()
    {
        SetScrollIndex(currentIndex);
    }

    public int GetScrollIndex()
    {
        return currentIndex;
    }

    public void SetScrollIndex(int index, float time = 0f)
    {
        float bias = 0f;
        //HOTween.Complete(itemRoot.transform);
		DOTween.Complete(itemRoot.transform);
		
        index = Mathf.Min(this.items.Count-1, index);

        if (GetScrollIndex() != index) {
            if (targetObject && (null != messageScroll && messageScroll != ""))
                targetObject.SendMessage(messageScroll, this.gameObject);

            if (null != onScroll)
                onScroll(this);
        }

        currentIndex = index;
        targetIndex = index;

        if (items.Count <=0 || items.Count <= currentIndex)
            return;
		
		Vector3 mVec = new Vector3(0f,0f,0f);
         if(haveExpendSlot==true)  mVec = new Vector3(0f,-2f,0f);
		
        Vector3 targetPosition = itemRoot.transform.position - items[currentIndex].transform.position + initPosition + mVec;
        if( (targetPosition.y > 10f || index >= 1) && targetPosition.x >= 0)  // district case horizontal scrolling
            bias = 1f;

        Vector3 position = new Vector3(targetPosition.x , targetPosition.y + bias, targetPosition.z);

        if (time <= 0f) {
            itemRoot.transform.localPosition = position;
            ScrollEnd();
        } else {
//            HOTween.To(itemRoot.transform, time, new TweenParms().Prop("localPosition", position).Ease(EaseType.EaseInOutCubic).OnComplete(()=> {
//                ScrollEnd();
//            }));
			itemRoot.transform.DOLocalMove(position,time).SetEase(Ease.InOutCubic).OnComplete(()=> {
				ScrollEnd();
			});
        }
    }

    void ScrollEnd()
    {
        if (targetObject && (null != messageScrollEnd && messageScrollEnd != ""))
            targetObject.SendMessage(messageScrollEnd, this.gameObject);
        if (null != this.onScrollEnd)
            this.onScrollEnd(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (Time.deltaTime == 0f) return;

        if (false == viewCamera.gameObject.activeSelf || false == viewCamera.enabled) return;
        if (false == this.GetComponent<Collider>().enabled) return;

        if (Input.GetMouseButtonDown(0) && buttonDown == false) {
            buttonDown = true;
            OnMouseButtonDown();
        }

        if (Input.GetMouseButton(0) && false == Input.GetMouseButtonUp(0) && buttonDown == true) {
            OnMouseButtonPress();
        }

        if (Input.GetMouseButtonUp(0) && buttonDown == true) {
            buttonDown = false;
            OnMouseButtonUp();
        }
    }

    void OnMouseButtonDown()
    {
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (false == Physics.Raycast(ray, out hitInfo) || hitInfo.collider != this.GetComponent<Collider>())
            return;

        //HOTween.Complete(itemRoot.transform);
		DOTween.Complete(itemRoot.transform);

        marginLock = true;
        downPosition = viewCamera.ScreenToWorldPoint(Input.mousePosition);
        rootPosition = itemRoot.transform.localPosition;

        if (targetObject && (null != messageScrollBegin && messageScrollBegin != ""))
            targetObject.SendMessage(messageScrollBegin, this.gameObject);
        if (null != this.onScrollBegin)
            this.onScrollBegin(this);
    }

    void OnMouseButtonPress()
    {
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (false == Physics.Raycast(ray, out hitInfo) || hitInfo.collider != this.GetComponent<Collider>())
            return;

        Vector3 position = viewCamera.ScreenToWorldPoint(Input.mousePosition);
        currentPosition = position;

        Vector3 dragOffset = (currentPosition - downPosition) * scrollSensitivity;
        dragOffset.z = 0f;

        if (dragOffset == Vector3.zero)
            return;

        if (dragOffset.magnitude > this.margin) {
            if (true == marginLock) {
                // Scroll Begin...
                if (targetObject && (null != messageScroll && messageScroll != ""))
                    targetObject.SendMessage(messageScroll, this.gameObject);

                if (null != onScroll)
                    onScroll(this);
            }
            marginLock = false;
        }

        if (true == marginLock)
            return;

        if (items.Count <=0 )
            return;

        currentIndex = Mathf.Min(this.items.Count-1, currentIndex);

        GameObject objCurrent = items[currentIndex];
        if (items.Count > 1) {
            if (currentIndex == 0) {
                targetIndex = 1;
            } else if (currentIndex == (items.Count-1)) {
                targetIndex = items.Count-2;
            } else {
                GameObject objPrev = items[currentIndex-1];
                GameObject objNext = items[currentIndex+1];

                Vector3 prevOffset = objPrev.transform.localPosition - objCurrent.transform.localPosition;
                Vector3 nextOffset = objNext.transform.localPosition - objCurrent.transform.localPosition;

                Vector3 prevSum = prevOffset / prevOffset.magnitude + dragOffset / dragOffset.magnitude;
                Vector3 nextSum = nextOffset / nextOffset.magnitude + dragOffset / dragOffset.magnitude;

                if (nextSum.sqrMagnitude > prevSum.sqrMagnitude)
                    targetIndex = currentIndex-1;
                else
                    targetIndex = currentIndex+1;
            }
        }

        Vector3 direction = new Vector3(defaultDirection.x, defaultDirection.y, 0f);
        float dot = (direction.x * dragOffset.x + direction.y*dragOffset.y + direction.z * dragOffset.z);
        if (dot > 0f)
            direction *= -1f;

        if (currentIndex != targetIndex) {
            GameObject objTarget = items[targetIndex];
            direction = objCurrent.transform.localPosition - objTarget.transform.localPosition;
        }

        Vector3 vec1 = direction.normalized;
        Vector3 vec2 = dragOffset.normalized;

        dot = (vec1.x * vec2.x + vec1.y*vec2.y + vec1.z * vec2.z);
        if (Mathf.Abs(dot) < 0.5f)
            return;

        if (dot < 0f) dot *= 0.5f;
        //dot = (dot> 0f ) ? 1f : -0.5f;

        itemRoot.transform.localPosition = direction / direction.magnitude * dot * dragOffset.magnitude + rootPosition;

        if (direction.sqrMagnitude < dragOffset.sqrMagnitude && dot > 0f) {
            SetScrollIndex(targetIndex);

            downPosition = position;
            rootPosition = itemRoot.transform.localPosition;
        }
    }

    void OnMouseButtonUp()
    {
        if (items.Count <=0 )
            return;

        //int index = GetScrollIndex();
        int index = currentIndex;

        GameObject objCurrent = items[currentIndex];
        GameObject objTarget = items[targetIndex];

        Vector3 direction = objCurrent.transform.localPosition - objTarget.transform.localPosition;

        Vector3 dragOffset = (currentPosition - downPosition) * scrollSensitivity;
        float dot = (direction.x * dragOffset.x + direction.y*dragOffset.y + direction.z * dragOffset.z);
        if (dot> 0f  && dragOffset.magnitude > direction.magnitude * bounceBackRate)
            index = targetIndex;

        float time = 0f;
        if (bounceBackSpeed > 0f) {
            Vector3 distance = ((itemRoot.transform.localPosition - initPosition) + items[index].transform.localPosition);
            time = Mathf.Sqrt(distance.magnitude) / bounceBackSpeed;
        }

        SetScrollIndex(index, time);

    }

}

