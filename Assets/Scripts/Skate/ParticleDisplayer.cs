using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParticleDisplayer : MonoBehaviour
{
    [SerializeField] ParticleSystem Ps;

    public void Display(Vector3 pos, Vector3 eulerAngles = new Vector3())
    {
        Ps.transform.localEulerAngles = eulerAngles;
        Ps.transform.localPosition = pos;
        Ps.Play();
    }

    public void Hide()
    {
        Ps.Stop();
    }
}
