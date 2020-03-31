using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public List<GameObject> AddObjectPool(GameObject objectToPool, int amountToPool)
    {
        List<GameObject> pooledObjects = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject obj = Instantiate(objectToPool);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
        return pooledObjects;
    }

    public void IncreaseObjectPoolSize(List<GameObject> objectPool, GameObject objectToPool, int amountToAdd)
    {
        for (int i = 0; i < amountToAdd; i++)
        {
            GameObject obj = Instantiate(objectToPool);
            obj.SetActive(false);
            objectPool.Add(obj);
        }
    }

    public GameObject GetObjectFromPool(List<GameObject> objectPool)
    {
        for (int i = 0; i < objectPool.Count; i++)
        {
            if (!objectPool[i].activeInHierarchy)
            {
                return objectPool[i];
            }
        } 
        return null;
    }
}
