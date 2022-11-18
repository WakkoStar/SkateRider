using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectActivator : MonoBehaviour
{
    [SerializeField] private GameObject[] elements;

    public void EnableAll()
    {
        foreach (var element in elements)
        {
            element.SetActive(true);
        }
    }

    public void DisableAll()
    {
        foreach (var element in elements)
        {
            element.SetActive(false);
        }
    }
}
