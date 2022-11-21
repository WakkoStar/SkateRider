using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private List<GameObject> _objects = new List<GameObject>();
    private Vector3 _hidedPosition = new Vector3(-100, 0, 0);

    public void Create(GameObject gameObject, string objectName)
    {
        var newObject = GameObject.Instantiate(gameObject, _hidedPosition, Quaternion.identity, transform);
        newObject.name = objectName;

        _objects.Add(newObject);
    }

    public GameObject Instantiate(GameObject instance, Vector3 pos, Quaternion quaternion, Transform parent)
    {
        var selectedObject = _objects.Find(o => o.name.Contains(instance.name) && o.transform.position == _hidedPosition);

        if (selectedObject == null)
        {
            Debug.LogError(instance.name + " is not founded.");
            return null;
        }

        selectedObject.transform.position = pos;
        selectedObject.transform.rotation = quaternion;
        selectedObject.transform.parent = parent;

        return selectedObject;
    }

    public void Destroy(GameObject instance)
    {
        var selectedObject = _objects.Find(o => o.name == instance.name);

        if (selectedObject == null)
        {
            Debug.LogError(instance.name + " is not founded. Deleted");
        }

        selectedObject.transform.position = _hidedPosition;
        selectedObject.transform.rotation = Quaternion.identity;
        selectedObject.transform.parent = transform;
    }

    public void DestroyAll()
    {
        var selectedObjects = _objects.FindAll(o => o.transform.position != _hidedPosition);

        foreach (var selectedObject in selectedObjects)
        {
            this.Destroy(selectedObject);
        }
    }
}
