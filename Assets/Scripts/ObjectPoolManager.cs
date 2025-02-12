using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Enum to categorize object pools
public enum PoolType
{
    Bullets, 
    None    
}

public class ObjectPoolManager : MonoBehaviour
{
    // List to keep track of all object pools
    public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();

    // Parent object to hold all pooled objects
    private GameObject _objectPoolParent;

    // Parent object specifically for bullets
    private static GameObject bulletPoolParent;

    private void Awake()
    {
        SetupPoolParentObjects();
    }

    // Creates parent containers for organizing pooled objects in the hierarchy
    private void SetupPoolParentObjects()
    {
        // Create a general parent for all pooled objects
        _objectPoolParent = new GameObject("Pooled Objects");

        // Create a specific parent for bullets
        bulletPoolParent = new GameObject("Bullets");
        bulletPoolParent.transform.SetParent(_objectPoolParent.transform);
    }

    // Overloaded method to spawn an object at default position and rotation
    public static GameObject SpawnObject(GameObject objectToSpawn, PoolType poolType = PoolType.None)
    {
        return SpawnObject(objectToSpawn, Vector3.zero, Quaternion.identity, poolType);
    }

    // Spawns an object, either by reusing an inactive one from the pool or creating a new instance
    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation, PoolType poolType = PoolType.None)
    {
        // Find an existing pool for this object type
        PooledObjectInfo pool = ObjectPools.Find(x => x.LookupString == objectToSpawn.name);

        // If no pool exists, create a new one
        if (pool == null)
        {
            pool = new PooledObjectInfo() { LookupString = objectToSpawn.name };
            ObjectPools.Add(pool);
        }

        // Look for an inactive object in the pool
        GameObject spawnableObj = pool.InactiveObjects.FirstOrDefault();

        if (spawnableObj == null)
        {
            // No inactive objects available, create a new one
            GameObject parentObject = SetParentObject(poolType);
            spawnableObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);

            // Assign the object to the appropriate parent in the hierarchy
            if (parentObject != null)
            {
                spawnableObj.transform.SetParent(parentObject.transform);
            }
        }
        else
        {
            // Reuse an existing inactive object
            spawnableObj.transform.position = spawnPosition;
            spawnableObj.transform.rotation = spawnRotation;
            pool.InactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        return spawnableObj;
    }

    // Returns an object to its pool, deactivating it for future use
    public static void ReturnObjectToPool(GameObject obj)
    {
        // Remove "(Clone)" suffix from object name (if applicable) to find the correct pool
        string goName = (obj.name.Length >= 8) ? obj.name.Substring(0, obj.name.Length - 7) : obj.name;

        // Find the corresponding object pool
        PooledObjectInfo pool = ObjectPools.Find(x => x.LookupString == goName);

        if (pool == null)
        {
            Debug.LogWarning("Trying to release an object that is not pooled: " + obj.name);
        }
        else
        {
            obj.SetActive(false);
            pool.InactiveObjects.Add(obj);
        }
    }

    // Determines which parent object to assign the newly spawned object to
    private static GameObject SetParentObject(PoolType poolType)
    {
        switch (poolType)
        {
            case PoolType.Bullets:
                return bulletPoolParent;
            case PoolType.None:
                return null;
            default:
                return null;
        }
    }
}

// Represents a pool for a specific type of object
public class PooledObjectInfo
{
    public string LookupString;
    public List<GameObject> InactiveObjects = new List<GameObject>();
}
