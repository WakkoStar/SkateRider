using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SkateInputCollider : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Display(Color targetColor)
    {
        StartCoroutine(DisplayCoroutine(targetColor));
    }

    private IEnumerator DisplayCoroutine(Color targetColor)
    {
        _meshRenderer.enabled = true;

        for (float x = 0; x < 1; x += Time.deltaTime * 2)
        {
            float alpha = 0.5f * Mathf.Sin(12f * x + 5f) + 0.5f;
            _meshRenderer.material.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }

        _meshRenderer.material.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0);
        _meshRenderer.enabled = false;
    }
}
