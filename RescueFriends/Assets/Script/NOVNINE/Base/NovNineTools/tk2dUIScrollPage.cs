using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using Holoville.HOTween;
using DG.Tweening;

public enum PAGE_ICON_ALIGN { DISABLE, TOP, BOTTOM }

public interface IScrollPage {
	void OnChangedPage (tk2dUIScrollPage scrollPage, GameObject page, int pageIndex);
	void OnUpdatePage (tk2dUIScrollPage scrollPage, GameObject page, int pageIndex);
}

public interface IScrollLevel : IScrollPage {
	void OnUpdateLevel (tk2dUIScrollPage scrollPage, GameObject level, int pageIndex, int levelIndex, out bool enable);
}

[RequireComponent(typeof(tk2dUIItem))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(tk2dUIScrollableArea))]
public class tk2dUIScrollPage : MonoBehaviour {
 	[HideInInspector][SerializeField] public float pageGap;
 	[HideInInspector][SerializeField] public float iconOffset = 0.5F;
 	[HideInInspector][SerializeField] public float iconMargin = 1F;
	[HideInInspector][SerializeField] public int packageWidth = 5;
	[HideInInspector][SerializeField] public int packageHeight = 5;
 	[HideInInspector][SerializeField] public bool designInEditor = true;
 	[HideInInspector][SerializeField] public bool useLevelPackage;
 	[HideInInspector][SerializeField] public int preparePageCount;
 	[HideInInspector][SerializeField] public bool autoDeactivePage = true;
 	[HideInInspector][SerializeField] public tk2dCamera pageCamera;
	[HideInInspector][SerializeField] public Vector2 packageMargin;
	[HideInInspector][SerializeField] public Vector2 packageOffset;
 	[HideInInspector][SerializeField] public GameObject pagePrefab;
 	[HideInInspector][SerializeField] public GameObject levelPrefab;
	[HideInInspector][SerializeField] public Vector2 levelSize;
 	[HideInInspector][SerializeField] public GameObject eventListener;
 	[HideInInspector][SerializeField] public GameObject[] customPages;
 	[HideInInspector][SerializeField] public GameObject activePageIcon;
 	[HideInInspector][SerializeField] public GameObject deactivePageIcon;
 	[HideInInspector][SerializeField] public PAGE_ICON_ALIGN pageIconAlign;
 	[HideInInspector][SerializeField] public bool pageLengthIsScreen = true;
 	[HideInInspector][SerializeField] public float movementSensitivity = 0.2F;
 	[HideInInspector][SerializeField] public float accelerationThresholdTime = 0.3F;
 	[HideInInspector][SerializeField] public float accelerationSensitivity = 0.5F;
 	[HideInInspector][SerializeField] public string eventMessageName = "";

    int pageIndex;
    int pageCount;
    int prevPageIndex;
    bool isDragging;
    bool initialized;
    float valuePerPage;
    float threshold;
    float downValue;
    float upValue;
    float pressTime;
    float prevDestinationValue;
	Tweener tweener;
    GameObject activeIcon;
	GameObject iconsParent;
    IScrollPage listener;
    BoxCollider boxCollider;
    tk2dUIScrollableArea scroll;
	GameObject[] pages;
	List<GameObject> designPages;
	List<GameObject> pageIcons = new List<GameObject>();

    public int PageIndex {
        get { return pageIndex; }
        private set {
            value = Mathf.Clamp(value, 0, PageCount - 1);

            if (pageIndex == value) {
                Invalidate();
                return;
            }

            pageIndex = value;
            if (designInEditor == false) UpdatePages();
            Invalidate();

            if (pageIconAlign != PAGE_ICON_ALIGN.DISABLE) UpdateIcons();
            if (listener != null) {
                if (designInEditor) {
                    listener.OnChangedPage(this, designPages[pageIndex], pageIndex);
                } else {
                    listener.OnChangedPage(this, pages[1], pageIndex);
                }
            }
        }
    }

    public int PageCount {
        get { return pageCount; }
        set {
            if (pageCount == value) return;

            pageCount = value;

			if(pageCount > 1)
				valuePerPage = 1F / (pageCount - 1);
			else
				valuePerPage = 1F;
            threshold = Mathf.Min(1F, valuePerPage * movementSensitivity);

            pageIndex = 0;
            UpdatePages();

            if (scroll != null) {
                scroll.Value = 0F;
                scroll.ContentLength = scroll.VisibleAreaLength * pageCount;
            }

            if (pageIconAlign != PAGE_ICON_ALIGN.DISABLE) UpdateIcons();
        }
    }

    public float PageGap {
        get { return pageGap; }
        set { 
            if (pageGap == value) return;
            pageGap = value;
            scroll.VisibleAreaLength = value;
            UpdatePages();

            if (scroll != null) {
                scroll.Value = 0F;
                scroll.ContentLength = scroll.VisibleAreaLength * pageCount;
            }
        }
    }

    void Awake () {
        scroll = GetComponent<tk2dUIScrollableArea>();
        boxCollider = GetComponent<BoxCollider>();

        if (scroll == null) {
            Debug.LogWarning("Can't find 'tk2dUIScrollableArea' component.");
            return;
        }

        if (boxCollider == null) {
            Debug.LogWarning("Can't find 'BoxCollider' component.");
            return;
        }

        if (pageCamera == null) {
            Debug.LogWarning("'Page Camera' must be set.");
            return;
        }

        if (pageLengthIsScreen && (pageCamera == null)) {
            Debug.LogWarning("Page Camera is null.");
            return;
        }

        if (eventListener != null) {
            if (useLevelPackage) {
                listener = eventListener.GetComponent(typeof(IScrollLevel)) as IScrollPage;
                if (listener == null) {
                    Debug.LogWarning("Event Listener must inherit 'IScrollLevel'");
                    return;
                }
            } else {
                listener = eventListener.GetComponent(typeof(IScrollPage)) as IScrollPage;
            }
        }

        scroll.VisibleAreaLength = pageLengthIsScreen ? pageCamera.ScreenExtents.width : pageGap;

        if (designInEditor) {
            designPages = new List<GameObject>(customPages);
            PageCount = designPages.Count;
        } else {
            designPages = new List<GameObject>();
            CreatePage(preparePageCount);
        }
#if LLOG_TK2DUI
        Debug.Log("tk2dUIScrollPage initialized.");
#endif
    }

    void OnEnable () {
        if (scroll.backgroundUIItem != null) {
            scroll.backgroundUIItem.OnDown += OnButtonDown;
            scroll.backgroundUIItem.OnRelease += OnButtonRelease;
        }
    }

    void OnDisable () {
        if (scroll.backgroundUIItem != null) {
            scroll.backgroundUIItem.OnDown -= OnButtonDown;
            scroll.backgroundUIItem.OnRelease -= OnButtonRelease;
        }
    }

    void OnButtonDown () {
		if (PageCount <= 1) return;

        StopTween();
        downValue = scroll.Value;
        pressTime = Time.realtimeSinceStartup;
    }

    void OnButtonRelease () {
        upValue = scroll.Value;
        ProcessPaging();
    }

    public void Invalidate () {
        if (PageCount == 0) return;

        if (PageIndex >= PageCount) {
            Debug.LogWarning("Wrong page index.");
            return;
        }

        if (designInEditor) {
            if (autoDeactivePage) foreach (GameObject page in designPages) page.SetActive(false);

            UpdatePage(designPages[PageIndex], PageIndex);
            if (PageIndex > 0) UpdatePage(designPages[PageIndex-1], PageIndex-1);
            if (PageIndex < (PageCount-1)) UpdatePage(designPages[PageIndex+1], PageIndex+1);
        } else {
            UpdatePage(pages[1], PageIndex);
            if (PageIndex > 0) UpdatePage(pages[0], PageIndex-1);
            if (PageIndex < (PageCount-1)) UpdatePage(pages[2], PageIndex+1);
        }
    }

    void UpdatePage (GameObject page, int updatePageIndex) {
        page.SetActive(true);

        if (listener == null) return;

        listener.OnUpdatePage(this, page, updatePageIndex);

        if (designInEditor) return;
        if (useLevelPackage == false) return;

        PackagePage packagePage = page.GetComponent<PackagePage>();
        if (packagePage == null) return;

        for (int y = 0; y < packageHeight; y++) {
            for (int x = 0; x < packageWidth; x++) {
                bool enable;
                int levelIndex = (packageWidth * y) + x;
                int fullLevelIndex = (packageWidth * packageHeight * updatePageIndex) + levelIndex;
                (listener as IScrollLevel).OnUpdateLevel(this, packagePage[levelIndex], updatePageIndex, fullLevelIndex, out enable);
                packagePage[levelIndex].SetActive(enable);
            }
        }
    }

    public void ChangePage (int _pageIndex, bool withAnimation = false, float duration = 0.5F) {
		_pageIndex = Mathf.Clamp(_pageIndex, 0, PageCount - 1);

        if (_pageIndex == PageIndex) {
            Invalidate();
        } else {
            if (withAnimation) {
                GoTo(_pageIndex, duration);
            } else {
                StopTween();
                scroll.Value = valuePerPage * _pageIndex;
            }

            PageIndex = _pageIndex;
        }
    }

    public void GoToNextPage () {
        ChangePage(PageIndex + 1, true);
    }

    public void GoToPrevPage () {
        ChangePage(PageIndex - 1, true);
    }

    void GoTo (int targetIndex, float duration) {
		targetIndex = Mathf.Clamp(targetIndex, 0, PageCount - 1);

        if (targetIndex == PageIndex) return;

        TweenPage(duration, valuePerPage * targetIndex);
    }

	void ProcessPaging (float duration = 0.5F) { 
		float movement = upValue - downValue;
		float movementAbs = Mathf.Abs(movement);
		float prevValue = valuePerPage * PageIndex;
		int nextPageIndex = PageIndex;

        float movementSec = Time.realtimeSinceStartup - pressTime;
		
		if (movementSec < accelerationThresholdTime) {
            float movementPerSec = (movement / movementSec) * Time.deltaTime;
            float movementPerSecAbs = Mathf.Abs(movementPerSec);

            if (movementPerSecAbs > (threshold * accelerationSensitivity * Time.deltaTime)) {
                if (!pageLengthIsScreen) { 
                    nextPageIndex = Convert.ToInt32(Mathf.Max(0, Mathf.Round(scroll.contentContainer.transform.localPosition.x / -pageGap)));
                    if (PageIndex == nextPageIndex) {
                        if (movementPerSec > 0) {
                            nextPageIndex++;
                        } else {
                            nextPageIndex--;
                        }
                    }
                } else {
                    if (movementPerSec > 0) {
                        nextPageIndex++;
                    } else {
                        nextPageIndex--;
                    }
                }
                nextPageIndex = Mathf.Clamp(nextPageIndex, 0, PageCount - 1);
                //Debug.Log("Paging By Acceleration");
            } else {
                //Debug.Log("Acceleration Shortage");
            }
        } else if (movementAbs > threshold) {
            if (!pageLengthIsScreen) { 
                nextPageIndex = Convert.ToInt32(Mathf.Max(0, Mathf.Round(scroll.contentContainer.transform.localPosition.x / -pageGap)));
            } else {
                if (movement > 0) {
                    nextPageIndex++;
                } else {
                    nextPageIndex--;
                }
            }
            nextPageIndex = Mathf.Clamp(nextPageIndex, 0, PageCount - 1);
            //Debug.Log("Paging By Movement");
        } else {
            //Debug.Log("Movement Shortage");
        }

		if (movementAbs > 0) {
            TweenPage(duration - (duration * (movementAbs / valuePerPage)), nextPageIndex * valuePerPage);
			if (PageIndex != nextPageIndex) PageIndex = nextPageIndex;
		} else if (scroll.Value != prevValue) {
		    float deviativeMovement = (valuePerPage - Mathf.Abs(scroll.Value - prevValue));
            TweenPage(duration - (duration * (deviativeMovement / valuePerPage)), nextPageIndex * valuePerPage);
            //Debug.Log("Return To Current Page");
        }
	}

    void UpdatePages () {
        if (designInEditor) {
            if (designPages.Count < PageCount) CreatePage(PageCount - designPages.Count);

            for (int i = 0; i < designPages.Count; i++) {
                designPages[i].name = "Page" + i;
                designPages[i].transform.localPosition = Vector3.right * scroll.VisibleAreaLength * i;
                designPages[i].SetActive(i < PageCount);
            }
        } else {
            for (int i = 0; i < pages.Length; i++) {
                int index = (pageIndex - 1) + i;
                pages[i].name = "Page" + index;
                pages[i].transform.localPosition = Vector3.right * scroll.VisibleAreaLength * index;
                pages[i].SetActive((index >= 0) && (index < PageCount));
            }
        }
    }

    void CreatePage (int count) {
        if (pagePrefab == null) pagePrefab = new GameObject();

        if (useLevelPackage) {
            PackagePage packagePage = pagePrefab.GetComponent<PackagePage>();
            if (packagePage == null) pagePrefab.AddComponent<PackagePage>();
        }

        if (designInEditor) {
            for (int i = 0; i < count; i++) {
                GameObject page = Instantiate(pagePrefab) as GameObject;
                page.transform.parent = scroll.contentContainer.transform;

                if (useLevelPackage) {
                    PackagePage packagePage = page.GetComponent<PackagePage>();
                    packagePage.Reset(this);
                }

                designPages.Add(page);
            }
        } else {
            if (pages == null) {
                pages = new GameObject[3];

                for (int i = 0; i < 3; i++) {
                    pages[i] = Instantiate(pagePrefab) as GameObject;
                    pages[i].transform.parent = scroll.contentContainer.transform;

                    if (useLevelPackage) {
                        PackagePage packagePage = pages[i].GetComponent<PackagePage>();
                        packagePage.Reset(this);
                    }
                }
            }
        }
    }

	void UpdateIcons () {
        if (iconsParent == null) {
            iconsParent = new GameObject("PageIcons");
            iconsParent.transform.parent = transform;

            float margin = iconMargin;
            Renderer pageRenderer = null;

            if ((designPages.Count > 0) && (designPages[PageIndex].GetComponent<Renderer>() != null)) {
                pageRenderer = designPages[PageIndex].GetComponent<Renderer>();
            }

            if (pageRenderer == null) {
                if (useLevelPackage) {
                    margin += (((levelSize.y * packageHeight) + (packageMargin.y * (packageHeight - 1))) * 0.5F);
                }
            } else {
                margin += pageRenderer.bounds.size.y * 0.5f;
            }

            if (pageIconAlign == PAGE_ICON_ALIGN.TOP) {
                iconsParent.transform.localPosition = Vector3.up * margin;
            } else {
                iconsParent.transform.localPosition = Vector3.down * margin;
            }

            activeIcon = Instantiate(activePageIcon) as GameObject;
            activeIcon.transform.parent = iconsParent.transform;
            activeIcon.transform.localScale = activePageIcon.transform.localScale;
            pageIcons.Add(activeIcon);
        }
        
        if (pageIcons.Count < PageCount) {
            int scarceCount = PageCount - pageIcons.Count;

            for (int i = 0; i < scarceCount; i++) {
                GameObject pageIcon = Instantiate(deactivePageIcon) as GameObject;
                pageIcon.transform.parent = iconsParent.transform;
                pageIcon.transform.localScale = deactivePageIcon.transform.localScale;
                pageIcons.Add(pageIcon);
            }
        }

        pageIcons.Remove(activeIcon);
        pageIcons.Insert(PageIndex, activeIcon);

        for (int i = 0; i < pageIcons.Count; i++) {
            pageIcons[i].SetActive(i < PageCount);
        }

		float beginPosX = (iconOffset * (PageCount - 1)) * -0.5F;
		
		for (int i = 0; i < PageCount; i++) {
			pageIcons[i].transform.localPosition = new Vector3(beginPosX + (i * iconOffset), 0f, 0f);
		}
	}

    void TweenPage (float duration, float destinationValue) {
        if (destinationValue != prevDestinationValue) StopTween();

        if (IsTweening() == false) 
		{
//            TweenParms tp = new TweenParms().Prop("Value", destinationValue);
//            tp.Ease(EaseType.EaseOutQuad);
			
			TweenParams tp = new TweenParams();
			tp.SetEase(Ease.OutQuad);

			tp.OnComplete( () => {
                if (eventListener != null) {
                    eventListener.SendMessage("OnCompleteSwipe", prevPageIndex, 
                        SendMessageOptions.DontRequireReceiver);
                }
                prevPageIndex = PageIndex;
            });

            //tweener = HOTween.To(scroll, duration, tp);
			tweener = DOTween.To(() => scroll.Value, x => scroll.Value = x, destinationValue, duration).SetAs(tp);
            prevDestinationValue = destinationValue;
        }
    }

    void StopTween () {
        if (IsTweening()) tweener.Kill();
    }

    bool IsTweening () {
        // Don't use `Tweener.isComplete`
        //return (tweener != null) && (tweener.destroyed == false);
		return (tweener != null) && (tweener.IsPlaying());
    }
}
