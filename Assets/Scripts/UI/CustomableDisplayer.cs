using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CustomableDisplayer : MonoBehaviour
{
    [SerializeField] private Image thumbnailImg;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite normalSprite;

    [SerializeField] private Text priceText;
    [SerializeField] private Image customableRect;
    [SerializeField] private CanvasGroup priceCanvas;

    [SerializeField] private CanvasGroup purchaseCanvas;


    private SerializableCustomable _customable;

    public void DeserializeCustomable(SerializableCustomable customable)
    {
        SetCustomable(customable);

        Texture2D tex = new Texture2D(500, 500, TextureFormat.RGB24, false);
        tex.LoadRawTextureData(customable.thumbnail);
        tex.Apply();

        thumbnailImg.sprite = Sprite.Create(tex, thumbnailImg.sprite.rect, thumbnailImg.sprite.pivot, thumbnailImg.pixelsPerUnit);

        if (customable.price == -1)
        {
            priceCanvas.alpha = 0;
            return;
        }
        priceCanvas.alpha = 1;
        priceText.text = "" + customable.price;
    }


    //SHOP DISPLAYER FUNCTION
    public void SetPurchased()
    {
        priceCanvas.alpha = 0;
        GetComponent<Button>().interactable = false;
        purchaseCanvas.alpha = 1;
    }


    //INVENTORY DISPLAYER FUNCTION
    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            customableRect.sprite = selectedSprite;
            GetComponent<Button>().interactable = false;
            return;
        }

        customableRect.sprite = normalSprite;
        GetComponent<Button>().interactable = true;
    }


    public SerializableCustomable GetCustomable()
    {
        return _customable;
    }
    private void SetCustomable(SerializableCustomable customable)
    {
        _customable = customable;
    }
}
