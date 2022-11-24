using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectTouchRotator : MonoBehaviour
{
    [SerializeField] private GameObject TouchZone;
    [SerializeField] private float sensitivity = 0.5f;
    private GameObject _objectToRotate;

    void Start()
    {
        _objectToRotate = gameObject;
    }


    void Update()
    {
        if (Input.touchCount == 1)
        {
            var touch = Input.GetTouch(0);
            if (IsPointerOverUITarget(GetEventSystemRaycastResults(touch), TouchZone))
            {
                _objectToRotate.transform.Rotate(new Vector3(touch.deltaPosition.y * sensitivity, -touch.deltaPosition.x * sensitivity, 0), Space.World);
            }
        }
    }

    private bool IsPointerOverUITarget(List<RaycastResult> eventSystemRaycastResults, GameObject Target)
    {
        for (int index = 0; index < eventSystemRaycastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaycastResults[index];
            if (GameObject.Equals(curRaysastResult.gameObject, Target))
                return true;
        }
        return false;
    }

    static List<RaycastResult> GetEventSystemRaycastResults(Touch touch)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touch.position;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

}
