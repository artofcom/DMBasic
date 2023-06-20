using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ObjectRecycler
{
    public delegate void ObjectRecuyclerChangedEventHandler(int available, int total);
    public event ObjectRecuyclerChangedEventHandler onObjectRecyclerChanged;

    private List<GameObject> objectList;

    public GameObject parent;
    public GameObject objectToRecycle;
    public int totalObjectsAtStart;

    public ObjectRecycler()
    {
        Init ();
    }
    public ObjectRecycler(GameObject go, int totalObjectsAtStart)
    {
        Init(go, totalObjectsAtStart, null);
    }

    /* public ObjectRecycler(GameObject go, int totalObjectsAtStart, GameObject parent)
    {
        Init(go, totalObjectsAtStart, parent);
    } */

    public void Init()
    {
        Init(this.parent);
    }
    public void Init(GameObject parent)
    {
        Init(objectToRecycle, totalObjectsAtStart, parent);
    }

    public void Init(GameObject go, int totalObjectsAtStart, GameObject parent)
    {
        objectList = new List<GameObject>();
        objectToRecycle = go;
        this.parent = parent;

        for (int i = 0; i < totalObjectsAtStart; i++) {
            // Create a new instance and set ourself as the recycleBin
            GameObject newObject = Object.Instantiate(go) as GameObject;

            if (null != parent)
                newObject.transform.parent = parent.transform;

            newObject.SetActive(false);

            // ad int to object store for later use
            objectList.Add(newObject);
        }
    }

    private void fireRecycledEvent()
    {
        if (this.onObjectRecyclerChanged != null) {
            var allFree = from item in objectList
                          where item.activeSelf == false
                          select item;

            onObjectRecyclerChanged(allFree.Count(), objectList.Count);
        }
    }

    // Gets the next available free object or null;
    public GameObject nextFree
    {
        get {
            var freeObject = (from item in objectList
                              where item.activeSelf == false
                              select item).FirstOrDefault();

            if (freeObject == null) {
                freeObject = Object.Instantiate(objectToRecycle) as GameObject;
                if (null != parent)
                    freeObject.transform.parent = parent.transform;

                objectList.Add(freeObject);
            }

            freeObject.SetActive(true);

            fireRecycledEvent();

            return freeObject;
        }
    }

    // Must be called by and object that wants to be reused
    public void freeObject(GameObject objectToFree)
    {
        objectToFree.SetActive(false);

        fireRecycledEvent();

    }

    public void freeAll()
    {
        for (int i = 0; i < objectList.Count; i++) {
            objectList[i].gameObject.SetActive(false);
        }
    }

}

