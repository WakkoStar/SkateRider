using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleCountDisplayer : MonoBehaviour
{
    [SerializeField] private Text collectableText;

    private void Start()
    {
        UpdateCollectableCount();
    }
    public void UpdateCollectableCount()
    {
        collectableText.text = "" + PlayerPrefs.GetInt("collectibleCount");
    }
    public void UpdateCollectableCountWithDelay(float delay)
    {
        StartCoroutine(UpdateCollectableCountCoroutine(delay));
    }

    IEnumerator UpdateCollectableCountCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        collectableText.text = "" + PlayerPrefs.GetInt("collectibleCount");
    }
}
