using UnityEngine;
using System.Collections;

public class YetiBall : Block {
    public static int TotalCount { get; private set; }

    void OnEnable () {
        TotalCount++;
    }

    void OnDisable () {
        TotalCount--;
    }
}
