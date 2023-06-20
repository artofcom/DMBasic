using UnityEngine;
using System.Collections;

public class Snowman : Panel {
    public static int TotalCount { get; private set; }

    void OnEnable () {
        TotalCount++;
    }

    void OnDisable () {
        TotalCount--;
    }
}
