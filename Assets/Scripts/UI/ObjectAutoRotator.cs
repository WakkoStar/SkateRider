using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAutoRotator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 30, Space.World);
    }
}
