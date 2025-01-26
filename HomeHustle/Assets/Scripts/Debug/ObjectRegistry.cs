using UnityEngine;
using System.Collections.Generic;

public class ObjectRegistry : MonoBehaviour
{
    private Dictionary<int, GameObject> objectRegistry = new Dictionary<int, GameObject>();

    // Add an object to the registry
    public void RegisterObject(GameObject obj)
    {
        // Generate a global object ID hash
        int hash = obj.GetInstanceID();

        if (!objectRegistry.ContainsKey(hash))
        {
            objectRegistry.Add(hash, obj);
        }
    }

    // Find an object using its hash
    public GameObject FindObjectByHash(int hash)
    {
        if (objectRegistry.TryGetValue(hash, out GameObject obj))
        {
            return obj;
        }
        else
        {
            Debug.LogWarning("Object not found by hash.");
            return null;
        }
    }
}
