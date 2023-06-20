using System;
using System.Collections;

namespace Tasks
{
public class FuncTask : ITask
{
    public event Action onComplete;
    public bool isDone
    {
        set;
        get;
    }

    System.Action func;

    public FuncTask (System.Action _func)
    {
        func = _func;
    }

    public void Execute ()
    {
        func();
        isDone = true;
        if (onComplete != null)
            onComplete();
    }
}
}

