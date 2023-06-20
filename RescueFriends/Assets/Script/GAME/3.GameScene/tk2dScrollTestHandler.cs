using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class tk2dScrollTestHandler : MonoBehaviour {

    public tk2dUILayout         _prefabItem     = null;
    public tk2dUIScrollableArea _scrollableArea = null;
    public bool                 _toUp           = true;

    int _firstCachedItem        = -1;
    float _fItemStride          = 0;
    int _maxVisibleItems        = 0;
    List<Transform>             _unusedContentItems = new List<Transform>();
    List<Transform>             _cachedContentItems = new List<Transform>();

    Vector2 vInitPos            = //Vector3.zero;// 
                                  new Vector2(1.8f, .0f);

    class ItemDef
    {
		public string name      = "";
		public int score        = 10;
		public int time         = 200;
		public Color color      = Color.white;
	}
	List<ItemDef> allItems      = new List<ItemDef>();

	// Use this for initialization
	void Start () {
	
        _prefabItem.gameObject.SetActive( false );

        _fItemStride            = (_prefabItem.GetMaxBounds() - _prefabItem.GetMinBounds()).y;

        //if(_toUp)
        //    vInitPos            = new Vector2(vInitPos.x, -_scrollableArea.VisibleAreaLength+_fItemStride);
        
        _maxVisibleItems        = Mathf.CeilToInt(_scrollableArea.VisibleAreaLength / _fItemStride) + 1;


        // Buffer the prefabs that we will need
		float y                 = 0;
        float fDir              = -1.0f;
		for (int i = 0; i < _maxVisibleItems; ++i)
        {
			tk2dUILayout layout = Instantiate(_prefabItem) as tk2dUILayout;
			layout.transform.parent         = _scrollableArea.contentContainer.transform;
			layout.transform.localPosition  = new Vector3(vInitPos.x, vInitPos.y + fDir*y, 0);
            //layout.transform.localPosition  = new Vector3(vInitPos.x + y, vInitPos.y, 0);
			layout.gameObject.SetActive( false );
			_unusedContentItems.Add( layout.transform );
			y += _fItemStride;
		}

        SetItemCount(10);

        if(_toUp)               _scrollableArea.Value   = 1.0f;
	}
	
    void OnEnable() {
		_scrollableArea.OnScroll += OnScroll;
	}

	void OnDisable() {
		_scrollableArea.OnScroll -= OnScroll;
	}

	// Update is called once per frame
	void Update () {
	
	}

    void OnScroll(tk2dUIScrollableArea scrollableArea) {
		UpdateListGraphics();
	}

    // Populate the backing fields with some values
	void SetItemCount(int numItems)
    {
		if (numItems < allItems.Count) {
			allItems.RemoveRange(numItems, allItems.Count - numItems);
		}
		else {
			for (int j = allItems.Count; j < numItems; ++j) {
				string[] firstPart = { "Ba", "Po", "Re", "Zu", "Meh", "Ra'", "B'k", "Adam", "Ben", "George" };
				string[] secondPart = { "Hoopler", "Hysleria", "Yeinydd", "Nekmit", "Novanoid", "Toog1t", "Yboiveth", "Resaix", "Voquev", "Yimello", "Oleald", "Digikiki", "Nocobot", "Morath", "Toximble", "Rodrup", "Chillaid", "Brewtine", "Surogou", "Winooze", "Hendassa", "Ekcle", "Noelind", "Animepolis", "Tupress", "Jeren", "Yoffa", "Acaer" };
				string name = string.Format( "[{0}] {1} {2}", j, firstPart[Random.Range(0, firstPart.Length)], secondPart[Random.Range(0, secondPart.Length)] );
		 		Color color = new Color32((byte)Random.Range(192, 255), (byte)Random.Range(192, 255), (byte)Random.Range(192, 255), 255);
				ItemDef item = new ItemDef();
				item.name = name;
				item.color = color;
				item.time = Random.Range(10, 1000);
				item.score = (item.time * Random.Range(0, 30)) / 60;
				allItems.Add(item);
			}
		}

		UpdateListGraphics();
		//numItemsTextMesh.text = "COUNT: " + numItems.ToString();
	}


    // Synchronizes the graphics with the scroll amount
	// Figures out the first and last visible list items, and if that doesn't correspond
	// to what is cached, it rectifies the situation
	// Only the items that actually need to be changed are changed, so as you scroll the one that goes out 
	// of view is removed, recycled and reused for the one coming into view.
	void UpdateListGraphics()
    {
		// Previous offset - we will need to reset the value to match the new content length
		float previousOffset    = _scrollableArea.Value * (_scrollableArea.ContentLength - _scrollableArea.VisibleAreaLength);
		int firstVisibleItem    = Mathf.FloorToInt( previousOffset / _fItemStride );

		// If the number of elements has changed - we do some processing
		float newContentLength  = allItems.Count * _fItemStride;
		if (!Mathf.Approximately(newContentLength, _scrollableArea.ContentLength))
        {
			// If all items are visible, we simply populate as needed
			if (newContentLength < _scrollableArea.VisibleAreaLength)
            {
				_scrollableArea.Value = 0; // no more scrolling
				for (int i = 0; i < _cachedContentItems.Count; ++i)
                {
					_cachedContentItems[i].gameObject.SetActive( false );
					_unusedContentItems.Add(_cachedContentItems[i]); // clear whole list
				}
				_cachedContentItems.Clear();
				_firstCachedItem = -1;
				firstVisibleItem = 0;
			}

			// The total size required to display all elements
			_scrollableArea.ContentLength = newContentLength;
	
			// Rescale the previousOffset so it remains constant
			if (_scrollableArea.ContentLength > 0)
            {
				_scrollableArea.Value = previousOffset / (_scrollableArea.ContentLength - _scrollableArea.VisibleAreaLength);
			}
		}
		int lastVisibleItem = Mathf.Min(firstVisibleItem + _maxVisibleItems, allItems.Count);

		// If any items are visible that shouldn't need to be visible, get rid of them
		while (_firstCachedItem >= 0 && _firstCachedItem < firstVisibleItem)
        {
			_firstCachedItem++;
			_cachedContentItems[0].gameObject.SetActive( false );
			_unusedContentItems.Add(_cachedContentItems[0]);
			_cachedContentItems.RemoveAt(0);
			if (_cachedContentItems.Count == 0)
            {
				_firstCachedItem = -1;
			}
		}

		// Ditto for end of list
		while (_firstCachedItem >= 0 && (_firstCachedItem + _cachedContentItems.Count) > lastVisibleItem )
        {
			_cachedContentItems[_cachedContentItems.Count - 1].gameObject.SetActive( false);
			_unusedContentItems.Add(_cachedContentItems[_cachedContentItems.Count - 1]);
			_cachedContentItems.RemoveAt(_cachedContentItems.Count - 1);
			if (_cachedContentItems.Count == 0) {
				_firstCachedItem = -1;
			}
		}

		// Nothing visible, simply fill as needed
		if (_firstCachedItem < 0)
        {
			_firstCachedItem = firstVisibleItem;
			int maxToAdd = Mathf.Min( _firstCachedItem + _maxVisibleItems, allItems.Count );
			for (int i = _firstCachedItem; i < maxToAdd; ++i)
            {
				Transform t = _unusedContentItems[0];
				_cachedContentItems.Add(t);
				_unusedContentItems.RemoveAt(0);
				CustomizeListObject( t, i );
				t.gameObject.SetActive(true);
			}
		}
		else 
        {
			// Fill in items that should be visible but aren't
			while (_firstCachedItem > firstVisibleItem)
            {
				--_firstCachedItem;
				Transform t = _unusedContentItems[0];
				_unusedContentItems.RemoveAt(0);
				_cachedContentItems.Insert(0, t);
				CustomizeListObject(t, _firstCachedItem);
				t.gameObject.SetActive(true);
			}
			while (_firstCachedItem + _cachedContentItems.Count < lastVisibleItem)
            {
				Transform t = _unusedContentItems[0];
				_unusedContentItems.RemoveAt(0);
				CustomizeListObject(t, _firstCachedItem + _cachedContentItems.Count);
				_cachedContentItems.Add(t);
				t.gameObject.SetActive(true);
			}
		}
    }

    void CustomizeListObject( Transform contentRoot, int itemId )
    {
		//contentRoot.Find("Name").GetComponent<tk2dTextMesh>().text = allItems[itemId].name;
		//contentRoot.Find("Score").GetComponent<tk2dTextMesh>().text = "Score: " + allItems[itemId].score;
		//contentRoot.Find("Time").GetComponent<tk2dTextMesh>().text = "Time: " + allItems[itemId].time;
		//contentRoot.Find("Portrait").GetComponent<tk2dBaseSprite>().color = allItems[itemId].color;

        float fDir              = -1.0f;
		contentRoot.localPosition = new Vector3(vInitPos.x, vInitPos.y + fDir * itemId * _fItemStride, 0);
        int id                  = _toUp ? allItems.Count - itemId : itemId;
        contentRoot.Find("txtId").GetComponent<tk2dTextMesh>().text = id.ToString();
	}

    
#region ButtonHandlers
	int numToAdd = 10;

	// Event handler for "Add more..." button
	void AddMoreItems() {
		SetItemCount(allItems.Count + Random.Range(numToAdd / 10, numToAdd));
		numToAdd *= 2;
	}
	// Event handler for "Reset" button
	void ResetItems() {
		numToAdd = 100;
		SetItemCount(3);
	}
#endregion
}
