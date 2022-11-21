using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 100);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.GetComponent<SkateStateManager>().AddCollectible();
            FindObjectOfType<AudioManager>().Play("collectableSong", 1.0f, true);
            GetComponent<MeshRenderer>().enabled = false;
            if (transform.childCount == 1) transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
