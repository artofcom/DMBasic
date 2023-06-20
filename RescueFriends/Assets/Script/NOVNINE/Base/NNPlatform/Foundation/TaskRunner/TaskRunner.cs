using System.Collections;
using UnityEngine;

public class TaskRunner
{
    private MonoTask _runner;

    static private TaskRunner _instance;

    static public TaskRunner Instance
    {
        get {
            if (_instance == null)
                InitInstance ();

            return _instance;
        }
    }

    public Coroutine Run(IEnumerable task)
    {
        return Run(task.GetEnumerator());
    }

    public Coroutine Run(IEnumerator task)
    {
        if (_runner != null && _runner.enabled == true) {
            _runner.gameObject.SetActive(true);
            return _runner.StartCoroutine(task);
        } else {
            return null;
        }
    }

    public void RunSync(IEnumerable task)
    {
        RunSync(task.GetEnumerator());
    }

    public void RunSync(IEnumerator task)
    {
        while (task.MoveNext() == true);
    }

    public void RunManaged(IEnumerable task)
    {
        RunManaged(task.GetEnumerator());
    }

    public void RunManaged(IEnumerator task)
    {
        if (_runner != null && _runner.enabled == true) {
            _runner.gameObject.SetActive(true);
            _runner.StartCoroutineManaged(task);
        }
    }

    public void PauseManaged()
    {
        _runner.paused = true;
    }

    public void ResumeManaged()
    {
        _runner.paused = false;
    }

    public void Stop()
    {
        if (_runner != null)
            _runner.StopAllCoroutines();
    }
	
	public void Stop(IEnumerator task)
	{
		if (_runner != null)
			_runner.StopCoroutine(task);
	}

    public void Destroy()
    {
        Stop();

        if (_runner != null) {
            if (Application.isPlaying)
                GameObject.Destroy(_runner.gameObject);
            else
                GameObject.DestroyImmediate(_runner.gameObject);
        }

        _instance = null;
    }

    static void InitInstance()
    {
        GameObject go = GameObject.Find("TaskRunner");

        if (go == null) {
            go = new GameObject("TaskRunner");

            GameObject.DontDestroyOnLoad(go);
        }

        _instance = new TaskRunner();
        _instance._runner = go.AddComponent<MonoTask>();
    }
}

