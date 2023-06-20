using UnityEngine;
using System.Collections;

public class YetiCave : Panel {
    SpriteRenderer spriteRenderer;
    public Sprite[] sprites;

    protected override void Init () { 
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void UpdatePanel (object _info) {
        if (_info == null) return;

        CavePanel.Info info = _info as CavePanel.Info;

        spriteRenderer.sprite = sprites[info.Index];

        if (info.Index == 0) {
            if (Yeti.Current != null) Yeti.Current.Recycle();
            GameObject yeti = NNPool.GetItem("Yeti", JMFUtils.GM.transform);
            Vector3 yetiPos = JMFUtils.GM[PT.PT].Position;
            yetiPos.x += (JMFUtils.GM.Size * 0.5F);
            yetiPos.y -= (JMFUtils.GM.Size * 0.2F);
            yetiPos.z -= 10F;
            yeti.transform.position = yetiPos;
        }
    }
}
