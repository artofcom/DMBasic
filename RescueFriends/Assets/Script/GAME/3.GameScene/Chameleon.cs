using UnityEngine;
using System.Collections;

public class Chameleon : Block {
    public int defaultIndex;
    public Sprite[] sprites;

    SpriteRenderer spriteRenderer;

    public static int TotalCount { get; private set; }
    public int NextIndex { get; set; }

    void OnEnable () {
        TotalCount++;

        if (spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void OnDisable () {
        TotalCount--;
    }

    public override void Reset () {
        base.Reset();
        ChangeColor(defaultIndex);
    }

    public void ChangeColor (int index) {
        spriteRenderer.sprite = sprites[index];
    }
}
