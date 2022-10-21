using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundFollower : MonoBehaviour
{
    [SerializeField] private Vector3 startPos;
    [SerializeField] private GameObject Camera;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = startPos;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            new Vector3(transform.position.x, startPos.y + Camera.transform.position.y, transform.position.z),
             0.1f
        );

    }
}
