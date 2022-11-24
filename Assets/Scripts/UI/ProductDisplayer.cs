using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ProductDisplayer : MonoBehaviour
{
    [SerializeField] private Text priceText;
    [SerializeField] private Image productRect;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Image thumbnailImg;
    [SerializeField] private CanvasGroup priceCanvas;
    [SerializeField] private CanvasGroup purchaseCanvas;
    [SerializeField] private SerializableProduct _product;

    public void SetProduct(SerializableProduct product)
    {
        _product = product;

        Texture2D tex = new Texture2D(500, 500, TextureFormat.RGB24, false);
        tex.LoadRawTextureData(product.thumbnail);
        tex.Apply();

        thumbnailImg.sprite = Sprite.Create(tex, thumbnailImg.sprite.rect, thumbnailImg.sprite.pivot, thumbnailImg.pixelsPerUnit);

        if (product.price == -1)
        {
            priceCanvas.alpha = 0;
            return;
        }

        priceText.text = "" + product.price;

    }

    public void SetPurchased()
    {
        priceCanvas.alpha = 0;
        GetComponent<Button>().interactable = false;
        purchaseCanvas.alpha = 1;
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            productRect.sprite = selectedSprite;
            return;
        }

        productRect.sprite = normalSprite;
    }

    public SerializableProduct GetProduct()
    {
        return _product;
    }
}
