using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 2x2 panel.
public class ColorBombSpritePanel : Panel
{
    public enum TYPE { NONE=0, _5=5, _6 };
    int _idx2x2                 = -1;
    public Sprite[] sprLinks    = null;

    LEItem.COLOR[] _targets     = new LEItem.COLOR[] { LEItem.COLOR.BLUE, LEItem.COLOR.GREEN, LEItem.COLOR.ORANGE, LEItem.COLOR.RED, LEItem.COLOR.YELLOW, LEItem.COLOR.VIOLET };
    TYPE _type                  = TYPE.NONE;
    public TYPE getType()       { return _type; }

    Board _bdParent             = null;
    public Board getParentBoard() { return _bdParent; }

    ColorBombSpritePanel        _dataHolder  = null;
    public ColorBombSpritePanel getDataHolder() { return _dataHolder; }

    // only valid on holder.
    bool[] _done                = new bool[] { false, false, false, false, false, false };
    List<SpriteImage> _listCovers = new List<SpriteImage>();
    //

    public Sprite getSpriteByName(string strName)
    {
        if (null == sprLinks)   return null;

        for (int j = 0; j < sprLinks.Length; ++j)
        {
            if (strName.Equals(sprLinks[j].name))
                return sprLinks[j];
        }
        return null;
    }

    public override void Reset()
    {
        base.Reset();

        _idx2x2                 = -1;
        for (int g = 0; g < _done.Length; ++g)
            _done[g]            = false;
        _dataHolder             = null;
        for (int q = 0; q < _listCovers.Count; ++q)
            NNPool.Abandon(_listCovers[q].gameObject);
        
        _listCovers.Clear();
    }
    public override void Release()
    {
        base.Release();

        _idx2x2                 = -1;
        for (int g = 0; g < _done.Length; ++g)
            _done[g]            = false;
        _dataHolder             = null;

        for (int q = 0; q < _listCovers.Count; ++q)
            NNPool.Abandon(_listCovers[q].gameObject);
        
        _listCovers.Clear();
    }

    // init with this.
    public override void UpdatePanel(object _info)
    {
        base.UpdatePanel(_info);
        ColorBombPanel.Info info    = (_info as ColorBombPanel.Info);
        if (null == info)   return;

        _idx2x2             = info.Index;
        
        if (0 == _idx2x2)   _dataHolder = this;
    }

    // data holder에 연결. => 2x2 므로 data는 0 index(left bottom) 하나만 가진다.
    public void initDataHolder(Board bdParent, TYPE eType)
    {
        if(_idx2x2 < 0)
        {
            Debug.Log("Error : You shoud init panel first. UpdatePanel() should be called before this.");
            return;
        }

        if (null == bdParent)   return;
        _bdParent               = bdParent;
        _type                   = eType;

        switch (_idx2x2)
        {
        case 1:                 // right bottom.
            if (null == bdParent.Left) break;
            if (false == bdParent.Left.PND is ColorBombPanel) break;

            _dataHolder         = bdParent.Left.Panel[BoardPanel.TYPE.BACK] as ColorBombSpritePanel;
            break;
        case 2:                 // left top.
            if (null == bdParent.Bottom) break;
            if (false == bdParent.Bottom.PND is ColorBombPanel) break;

            _dataHolder         = bdParent.Bottom.Panel[BoardPanel.TYPE.BACK] as ColorBombSpritePanel;
            break;
        case 3:                 // right top.
            if (null == bdParent.Left) break;
            if (null == bdParent.Left.Bottom) break;
            if (false == bdParent.Left.Bottom.PND is ColorBombPanel) break;

            _dataHolder         = bdParent.Left.Bottom.Panel[BoardPanel.TYPE.BACK] as ColorBombSpritePanel;
            break;
        case 0:                 // left bottom. => holder.
        default:
            break;
        }

        if(TYPE._5 == _type)
            GetComponent<SpriteRenderer>().sprite = getSpriteByName(string.Format("cakebomb_5{0}", _idx2x2 + 1));
        else 
            GetComponent<SpriteRenderer>().sprite = getSpriteByName(string.Format("cakebomb_6{0}", _idx2x2 + 1));

        // test
       /* if(0 == _idx2x2)
        {
            for(int g = 0 ; g < _targets.Length; ++g)
            {
                float moveRate  = JMFUtils.GM.Size * 0.5f;// * (1.0f/0.8f);
                SpritePiece spr = NNPool.GetItem<SpritePiece>("SpriteImage", JMFUtils.GM.transform);
                spr.GetComponent<SpriteRenderer>().sprite = spr.getSpriteByName(getSpriteName( _targets[g] ));
                spr.GetComponent<SpriteRenderer>().sortingOrder = 1;
                spr.transform.localPosition = _bdParent.LocalPosition + new Vector3(moveRate, moveRate, .0f);
                spr.transform.localScale    = transform.localScale;// Vector3.one * 0.8f;
            }
        }*/
    }

    public bool damage(LEItem.COLOR eColor)
    {
        if(null == _dataHolder) return false;

        return _dataHolder.damageAtHolder(eColor);
    }

    bool damageAtHolder(LEItem.COLOR eColor)
    {
        if(_dataHolder != this) return false; // only holder can do this.

        for (int k = 0; k < _targets.Length; ++k)
        {
            if(_targets[k]==eColor && false==_done[k])
            {
                StartCoroutine( _coCreateCover(eColor) );
                _done[k]        = true;
                return true;
            }
        }
        return false;
    }

    IEnumerator _coCreateCover(LEItem.COLOR eColor)
    {
        yield return new WaitForSeconds(0.3f);

        float moveRate          = JMFUtils.GM.Size * 0.5f;
        SpriteImage spr         = NNPool.GetItem<SpriteImage>("SpriteImage", JMFUtils.GM.transform);
        spr.GetComponent<SpriteRenderer>().sprite = spr.getSpriteByName(_getSpriteNameByColor(eColor));
        spr.transform.localPosition = new Vector3(getParentBoard().LocalPosition.x+moveRate, getParentBoard().LocalPosition.y+moveRate, transform.localPosition.z-1.0f);
        spr.transform.localScale    = transform.localScale;

        _listCovers.Add(spr);
    }

    string _getSpriteNameByColor(LEItem.COLOR eColor)
    {
        switch (eColor)
        {
        case LEItem.COLOR.BLUE:     return TYPE._5==_type ? "cakebomb_5B" : "cakebomb_6B";
        case LEItem.COLOR.GREEN:    return TYPE._5==_type ? "cakebomb_5G" : "cakebomb_6G";
        case LEItem.COLOR.ORANGE:   return TYPE._5==_type ? "cakebomb_5O" : "cakebomb_6O";
        case LEItem.COLOR.RED:      return TYPE._5==_type ? "cakebomb_5R" : "cakebomb_6R";
        case LEItem.COLOR.YELLOW:   return TYPE._5==_type ? "cakebomb_5Y" : "cakebomb_6Y";
        case LEItem.COLOR.VIOLET:   return TYPE._5==_type ? ""            : "cakebomb_6V";
        }

        return null;
    }
    
    public bool isExplodable()
    {
        if(null==_dataHolder)   return false;

        //for(int q = 0; q < (int)_type; ++q)
        //{
         //   if(true == _dataHolder._done[q])   return true;
        //}
        //return false;
        
        for(int q = 0; q < (int)_type; ++q)
        {
            if(false == _dataHolder._done[q])   return false;
        }
        return true;
    }
}
