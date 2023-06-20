using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PackagePage : MonoBehaviour {
    List<GameObject> levels = new List<GameObject>();
    int packageWidth;
    int packageHeight;

    public GameObject this[int index] {
        get { 
            if (levels.Count == 0) return null;
            if (index >= levels.Count) return null;
            return levels[index]; 
        }
    }

    public void Reset (tk2dUIScrollPage scrollPage) {
        packageWidth = scrollPage.packageWidth;
        packageHeight = scrollPage.packageHeight;
        Vector2 levelSize = scrollPage.levelSize;
        Vector2 margin = scrollPage.packageMargin;
        Vector2 offset = scrollPage.packageOffset;

        int levelCount = packageWidth * packageHeight;

        if (levels.Count < levelCount) {
            int scarceCount = levelCount - levels.Count;

            for (int i = 0; i < scarceCount; i++) {
                GameObject level = Instantiate(scrollPage.levelPrefab) as GameObject;
                level.transform.parent = transform;
                levels.Add(level);
            }
        } 

        float totalWidth = ((levelSize.x * packageWidth) + (margin.x * (packageWidth - 1))) * 0.5f;
        float totalHeight = ((levelSize.y * packageHeight) + (margin.y * (packageHeight - 1))) * 0.5f;

        for (int j = 0; j < packageHeight; j++) {
            for (int i = 0; i < packageWidth; i++) {
                GameObject level = levels[(j * packageWidth) + i];

                float x = (i * (levelSize.x + margin.x)) - totalWidth + (levelSize.x * 0.5f) + offset.x;
                float y = (j * -(levelSize.y + margin.y)) + totalHeight - (levelSize.y * 0.5f) + offset.y;

                Vector3 pos = new Vector3(x, y, -1f);
                level.transform.localPosition = pos;
            }
        }
    }

}
