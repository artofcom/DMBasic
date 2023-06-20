using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Penguin : Block {
    public static int TotalCount { get; private set; }

    void OnEnable () {
        TotalCount++;
    }

    void OnDisable () {
        TotalCount--;
    }
}
