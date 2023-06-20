using UnityEngine;
using System.Collections;

public class Mystery : Block {

    public static int TotalCount { get; private set; }

    void OnEnable () {
        TotalCount++;
    }

    void OnDisable () {
        TotalCount--;
    }
}

