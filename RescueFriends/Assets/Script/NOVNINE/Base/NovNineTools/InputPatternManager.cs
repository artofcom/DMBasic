using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NOVNINE.Diagnostics;

[RequireComponent(typeof(InputManager))]
public class InputPatternManager : SingletonMonoBehaviour<InputPatternManager>
{
    public Camera viewCamera;

    int patternIndex = -1;
    string pattern = "";

    Dictionary<string, System.Action> map = new Dictionary<string, System.Action>();

    // Use this for initialization
    void Start ()
    {
        if (null == viewCamera)
            viewCamera = Camera.main;

        InputManager.onMouseDown += OnMouseDown;
        InputManager.onMouseDrag += OnMouseDrag;
        InputManager.onMouseUp += OnMouseUp;
    }

    public static void RegisterPattern(string pattern, System.Action callback)
    {
        Debugger.Assert(!string.IsNullOrEmpty(pattern));
        Debugger.Assert(callback != null);

        if (string.IsNullOrEmpty(pattern)) return;
        if (null == callback) return;

        if (InputPatternManager.Instance.map.ContainsKey(pattern)) return;

        InputPatternManager.Instance.map.Add(pattern, callback);
    }

    public static void UnRegisterPattern(string pattern, System.Action callback)
    {
        Debugger.Assert(!string.IsNullOrEmpty(pattern));
        Debugger.Assert(callback != null);

        if (string.IsNullOrEmpty(pattern)) return;
        if (null == callback) return;

        if (false == InputPatternManager.Instance.map.ContainsKey(pattern))  return;
        if (InputPatternManager.Instance.map[pattern] != callback) return;

        InputPatternManager.Instance.map.Remove(pattern);
    }

    void OnMouseDown(Vector3 mousePosition)
    {
        patternIndex = PatternIndex(mousePosition);
        if (patternIndex < 0) return;

        pattern = patternIndex.ToString();
    }

    void OnMouseDrag(Vector3 mousePosition)
    {
        if (patternIndex < 0) return;

        int index = PatternIndex(mousePosition);
        if (index<0 || index == patternIndex)
            return;

        patternIndex = index;
        pattern += patternIndex.ToString();
    }

    void OnMouseUp(Vector3 mousePosition)
    {
        if (patternIndex < 0) return;
        int index = PatternIndex(mousePosition);
        if (index<0) return;

        if (map.ContainsKey(pattern)) {
            System.Action callback = map[pattern];
            callback();
        }
    }

    int PatternIndex(Vector3 mousePosition)
    {
        Vector3 localPosition = viewCamera.ScreenToWorldPoint(mousePosition) - viewCamera.transform.position;
        localPosition.z = 0f;

        float orthWidth = viewCamera.orthographicSize * viewCamera.aspect;
        float orthHeight = viewCamera.orthographicSize;

        int x = -1;
        int y = -1;

        if (localPosition.x < -orthWidth/3) x = -1;
        else if (localPosition.x < orthWidth/3) x = 0;
        else x = 1;

        if (localPosition.y < -orthHeight/3) y = -1;
        else if (localPosition.y < orthHeight/3) y = 0;
        else y = 1;

        Vector3 center = new Vector3(x * orthWidth * 2/3, y * orthHeight * 2/3, 0f);

        Vector3 diff = center - localPosition;
        if (diff.magnitude < (orthWidth + orthHeight) / 10)
            return (y + 1) * 3 + (x + 1) + 1;

        return -1;
    }

}

