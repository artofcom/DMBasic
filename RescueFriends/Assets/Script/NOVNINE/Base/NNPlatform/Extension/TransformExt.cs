using UnityEngine;

namespace NOVNINE
{
public static class TransformExt
{
    public static T GetChildComponentNamed<T>(this Transform parent, string path) where T : Component
    {
        Transform child = parent.Find(path);
        if(child != null)
            return child.GetComponent<T>();
        //Debug.LogWarning("GetChildComponentNamed<"+typeof(T).FullName+"> path Not Found : "+path);
        return null;
    }

    public static void SetLocalPositionXY(this Transform tr, float x, float y)
    {
        tr.localPosition = new Vector3(x, y, tr.localPosition.z);
    }

    public static void AddLocalPositionX(this Transform tr, float x)
    {
        Vector3 pos = tr.localPosition;
        pos.x += x;
        tr.localPosition = pos;
    }

    public static void AddLocalPositionY(this Transform tr, float y)
    {
        Vector3 pos = tr.localPosition;
        pos.y += y;
        tr.localPosition = pos;
    }

    public static void AddLocalPositionZ(this Transform tr, float z)
    {
        Vector3 pos = tr.localPosition;
        pos.z += z;
        tr.localPosition = pos;
    }

    public static void SetLocalPositionX(this Transform tr, float x)
    {
        Vector3 pos = tr.localPosition;
        pos.x = x;
        tr.localPosition = pos;
    }

    public static void SetLocalPositionY(this Transform tr, float y)
    {
        Vector3 pos = tr.localPosition;
        pos.y = y;
        tr.localPosition = pos;
    }

    public static void SetLocalPositionZ(this Transform tr, float z)
    {
        Vector3 pos = tr.localPosition;
        pos.z = z;
        tr.localPosition = pos;
    }

    public static void SetLocalScaleXY(this Transform tr, float x, float y)
    {
        tr.localScale = new Vector3(x, y, tr.localScale.z);
    }

    /*
        #region GameObjectCache (aka Spawner.cs)
            public static void DestroyAllChildren(this Transform tr)
            {
                foreach(Transform c in tr)
                {
                    Spawner.Destroy(c.gameObject);
                }
            }
        #endregion
    */
}
}

