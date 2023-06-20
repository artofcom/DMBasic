using UnityEngine;
using System.Collections;
using NOVNINE;
using Data;
using Spine.Unity;
using Spine;

public class BoosterButton : MonoBehaviour 
{
    public tk2dSprite           _sprChecked;
    public tk2dTextMesh         _txtCount;

    
    public void refreshUI(string id, bool isChecked)
    {
        int count               = Wallet.GetItemCount(id);
        if(count > 0)           _txtCount.text  = count.ToString();
        else                    _txtCount.text  = "+";
        _txtCount.Commit();

        _sprChecked.gameObject.SetActive( isChecked );
    }

}
