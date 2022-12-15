using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private Vector3 _hidedPosition = new Vector3(-100, 0, 0);

    public void Create(GameObject gameObject, string objectName)
    {
        var newObject = GameObject.Instantiate(gameObject, _hidedPosition, Quaternion.identity, transform);
        newObject.name = objectName;
    }

    public GameObject Instantiate(GameObject instance, Vector3 pos, Quaternion quaternion, Transform parent)
    {
        var selectedObject = FindInTransform(o => o.name.StartsWith(instance.name) && o.transform.localPosition == _hidedPosition);

        if (selectedObject == null)
        {
            Debug.LogError(instance.name + " is not founded.");
            return null;
        }

        selectedObject.transform.position = pos;
        selectedObject.transform.rotation = quaternion;

        return selectedObject;
    }

    public void Destroy(GameObject instance)
    {
        var selectedObject = FindInTransform(o => o.Equals(instance));

        if (selectedObject == null)
        {
            Debug.LogError(instance.name + " is not founded (to delete)");
        }

        selectedObject.transform.localPosition = _hidedPosition;
        selectedObject.transform.rotation = Quaternion.identity;
    }

    public void DestroyAll()
    {
        var selectedObjects = FindAllInTransform(o => o.transform.position != _hidedPosition);

        foreach (var selectedObject in selectedObjects)
        {
            this.Destroy(selectedObject);
        }
    }

    private GameObject FindInTransform(System.Predicate<GameObject> match)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (match(transform.GetChild(i).gameObject) == true)
            {
                return transform.GetChild(i).gameObject;
            };
        }

        return null;
    }

    private List<GameObject> FindAllInTransform(System.Predicate<GameObject> match)
    {
        List<GameObject> gameobjects = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            if (match(transform.GetChild(i).gameObject) == true)
            {
                gameobjects.Add(transform.GetChild(i).gameObject);
            };
        }

        return gameobjects;
    }
}
