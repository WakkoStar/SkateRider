using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSlamDisplayer : MonoBehaviour
{
    [SerializeField] private SkateController skateController;
    [SerializeField] private ParticleSystem boardSlamPS;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DisplayBoardSlam()
    {
        transform.position = skateController.transform.position;
        boardSlamPS.Play();
    }
}
