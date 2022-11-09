using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class TriggerColliderNotifier : MonoBehaviour
{
    public UnityEvent<Collider, GameObject> onTriggerEnter = new UnityEvent<Collider, GameObject>();
    public UnityEvent<Collider, GameObject> onTriggerStay = new UnityEvent<Collider, GameObject>();
    public UnityEvent<Collider, GameObject> onTriggerExit = new UnityEvent<Collider, GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.Invoke(other, this.gameObject);
    }
    private void OnTriggerStay(Collider other)
    {
        onTriggerStay.Invoke(other, this.gameObject);
    }
    private void OnTriggerExit(Collider other)
    {
        onTriggerExit.Invoke(other, this.gameObject);
    }
}
