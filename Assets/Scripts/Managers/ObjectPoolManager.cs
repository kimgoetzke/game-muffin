using System.Collections.Generic;
using CaptainHindsight.Core;
using UnityEngine;

namespace CaptainHindsight.Managers
{
  public class ObjectPoolManager : MonoBehaviour
  {
    public static ObjectPoolManager Instance;
    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> _poolDictionary;

    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
      }
      else
      {
        Destroy(gameObject);
      }
    }

    private void Start()
    {
      _poolDictionary = new Dictionary<string, Queue<GameObject>>();

      foreach (var pool in pools)
      {
        var objectPool = new Queue<GameObject>();

        for (var i = 0; i < pool.size; i++)
        {
          var obj = Instantiate(pool.gameObjectPrefab, gameObject.transform, true);
          obj.SetActive(false);
          objectPool.Enqueue(obj);
        }

        _poolDictionary.Add(pool.tag, objectPool);
      }
    }

    public GameObject SpawnFromPool(string objTag, Vector3 position, Quaternion rotation)
    {
      if (_poolDictionary.ContainsKey(objTag) == false)
      {
        Helper.LogWarning("ObjectPoolManager: Couldn't find objects in a pool with tag '" +
                          objTag + "'.");
        return null;
      }

      var objectToSpawn = _poolDictionary[objTag].Dequeue();

      objectToSpawn.SetActive(true);
      objectToSpawn.transform.position = position;
      objectToSpawn.transform.rotation = rotation;

      _poolDictionary[objTag].Enqueue(objectToSpawn);

      return objectToSpawn;
    }
  }

  [System.Serializable]
  public class Pool
  {
    public string tag;
    public GameObject gameObjectPrefab;
    public int size;
  }
}