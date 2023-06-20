using UnityEngine;
using UnityEngine.SocialPlatforms;
using System;
using System.Collections;
using System.Collections.Generic;
using Tasks;

public static class TaskManager
{

    #region coroutines
    static IEnumerator yieldThenAction(Action act, int yieldCount)
    {
        int iterations = 0;
        while (iterations < yieldCount) {
            iterations++;
            yield return null;
        }
        act();
    }

    static IEnumerator waitThenAction(Action act, float waitSec)
    {
        yield return new WaitForSeconds(waitSec);
        act();
    }

    #endregion

    public static void DoNextFrame(Action act, int yieldCount = 1 )
    {
        TaskRunner.Instance.Run(yieldThenAction(act, yieldCount));
    }

    public static void DoSecondsAfter(Action act, float sec)
    {
        TaskRunner.Instance.Run(waitThenAction(act, sec));
    }

    public static Coroutine StartCoroutine(IEnumerator coR)
    {
        return TaskRunner.Instance.Run(coR);
    }
	
	public static void StopCoroutine(IEnumerator coR)
	{
		TaskRunner.Instance.Stop(coR);
	}

#if USE_PlatformUI
    static IEnumerator coLoadScore(ILeaderboard lb, System.Action<bool> callback)
    {
        bool done = false;
        lb.LoadScores((ret)=> {
            done = true;
            NOVNINE.Platform.SafeCallback(callback, ret);
        });
        while(!done) yield return true;
    }

    static IEnumerator coLoadUser(string userId, System.Action<IUserProfile[]> callback)
    {
        bool done = false;
        //Debug.Log("------- coLoadUser start : "+userId);
        Social.LoadUsers(new string[] {userId}, (ret)=> {
            done = true;
            NOVNINE.Platform.SafeCallback(callback, ret);
        });
        while(!done) {
            //Debug.Log(" >> coLoadUser working -------: "+userId);
            yield return true;
        }
        //Debug.Log("coLoadUser end -------: "+userId);
    }

    public static void SerialLoadScore(ILeaderboard lb, System.Action<bool> callback)
    {
        //Debug.Log("TaskManager.SerialLoadScore : "+lb.id);
        /*if(Social.Active == ScoreoidSocialPlatform.Instance)
        {
            //Instance.AddTask(new CoroutineTask(coLoadScore(lb, callback)));
            lb.LoadScores(callback);
        }
        else*/
        TaskRunner.Instance.Run(new SingleTask(coLoadScore(lb, callback)));
    }

    public static void SerialLoadUser(string userId, System.Action<IUserProfile[]> callback)
    {
        //Debug.Log("------- SerialLoadUser : "+userId);
        /*if(Social.Active == ScoreoidSocialPlatform.Instance)
        {
            //Instance.AddSerialTask(new CoroutineTask(coLoadUser(userId, callback)));
            Social.LoadUsers(new string[]{userId}, callback);
        }
        else*/
        TaskRunner.Instance.Run(new SingleTask(coLoadUser(userId, callback)));
    }
#endif

}

