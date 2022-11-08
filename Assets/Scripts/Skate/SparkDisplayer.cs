using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkDisplayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem sparks;
    [SerializeField] private SkateController skateController;

    [SerializeField] private Vector3 normalSparkPos;
    [SerializeField] private Vector3 switchSparkPos;

    private void Start()
    {
        sparks.transform.position = normalSparkPos;
        sparks.Stop();
    }

    private void FixedUpdate()
    {
        sparks.transform.localPosition = skateController.IsSwitch() ? switchSparkPos : normalSparkPos;
        sparks.transform.localEulerAngles = skateController.IsSwitch() ? new Vector3(0, 90, 180) : new Vector3(0, -90, 0);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Grind")
        {
            sparks.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        sparks.Stop();
    }
}
