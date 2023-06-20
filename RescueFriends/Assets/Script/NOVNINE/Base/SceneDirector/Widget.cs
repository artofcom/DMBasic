using UnityEngine;
using System.Collections.Generic;

public static class Widget
{

    #region ResourceCache
    static private Dictionary<string, GameObject> resourceCache = new Dictionary<string, GameObject>();

    public static T Create<T> () where T : MonoBehaviour
    {
        string prefabName = typeof(T).Name.Replace("Handler", "");
        if ( !resourceCache.ContainsKey (prefabName) ) {
            //Debug.Log("Create Widget : " + prefabName);
            GameObject loaded = Resources.Load ("widgets/"+prefabName) as GameObject;
            if(loaded == null) {
                Debug.LogError("No "+prefabName+".prefab found in Resources folder!");
                return null;
            }
            resourceCache[prefabName] = loaded;
        }
        GameObject go = Object.Instantiate (resourceCache[prefabName], Vector3.zero, Quaternion.identity) as GameObject;
        if ( go == null ) {
            Debug.LogError("Widget.Create Fail : "+typeof(T).FullName);
            return null;
        }
        //go.name = widgetName+"Instance";
        return go.GetComponent<T> ();
    }
    #endregion

}

