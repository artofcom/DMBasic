﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tasks;

class MonoTask:MonoBehaviour
{
	List<IEnumerator> _enumerators = new List<IEnumerator>();

    public bool paused
    {
        set;
        private get;
    }

    void Awake()
    {
        paused = false;
    }

    public void StartCoroutineManaged(IEnumerator task)
    {
        _enumerators.Add(new SingleTask(task).GetEnumerator());
    }

    void FixedUpdate()
    {
        if (paused == false)
            _enumerators.RemoveAll(enumerator => enumerator.MoveNext() == false);
    }
}

