using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteUIAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private Image _imageToAnimate;
    private bool _isInLoop = false;

    void Start()
    {
        _imageToAnimate = GetComponent<Image>();
    }


    void FixedUpdate()
    {
        if (!_isInLoop) StartCoroutine(AnimateSprite());
    }

    private void OnEnable()
    {
        _isInLoop = false;
    }

    IEnumerator AnimateSprite()
    {
        _isInLoop = true;

        foreach (var sprite in sprites)
        {
            _imageToAnimate.sprite = sprite;
            yield return new WaitForSeconds(0.05f);
        }

        _isInLoop = false;
    }
}
