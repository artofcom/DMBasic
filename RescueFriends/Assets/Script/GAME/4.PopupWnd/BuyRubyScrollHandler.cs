using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuyRubyScrollHandler : tk2dScrollHandler {

    public BuyRubyPopupHandler _popupHandler;

	// Use this for initialization
	override protected void Start () {

        base.Start();

        if(0 == allItems.Count)
            SetItemCount( NOVNINE.Context.UncleBill.shopItems.Count );

        _refreshUI();
	}
	
    protected override void OnEnable()
    {
        //if(0 == allItems.Count)
        //    SetItemCount( InfoLoader.GetRubyShopItemInfoCount() );

        base.OnEnable();

        _refreshUI();
    }

    override protected void CustomizeListObject( Transform contentRoot, int idxItem )
    {
		contentRoot.localPosition = new Vector3(vInitPos.x, vInitPos.y - idxItem * _fItemStride, 0);
        //int idx                 = _toUp ? allItems.Count - idxItem - 1 : idxItem;
        //contentRoot.Find("txtId").GetComponent<tk2dTextMesh>().text = idx.ToString();

        RubyShopItem item       = contentRoot.GetComponent<RubyShopItem>();
        if(null != item)        item.refresh( idxItem, _popupHandler );
	}

    void _refreshUI()
    {
        _scrollableArea.Value   = _toUp ? 1.0f : .0f;
    }
}
