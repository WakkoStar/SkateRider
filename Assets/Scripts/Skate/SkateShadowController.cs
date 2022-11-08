using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateShadowController : MonoBehaviour
{
    public GameObject Player;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Player.transform.position + Vector3.up * 5;
        transform.eulerAngles = new Vector3(90, 0, 0);
    }
}
