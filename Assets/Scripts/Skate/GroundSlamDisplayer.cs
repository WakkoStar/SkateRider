using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSlamDisplayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem groundSlam;

    public void DisplayGroundSlam()
    {
        groundSlam.transform.position = transform.position;
        groundSlam.Play();
    }
}
