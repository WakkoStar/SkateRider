using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkateStartScreen : MonoBehaviour
{
    [SerializeField] private Text collectibleAllCountText;

    private void Start()
    {
        DisplayAllCollectibleCount(PlayerPrefs.GetInt("collectibleCount"));
    }

    public void DisplayAllCollectibleCount(float count)
    {
        collectibleAllCountText.text = "" + count;
    }
}
