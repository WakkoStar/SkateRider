using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ProductDisplayer : MonoBehaviour
{
    [SerializeField] private Text priceText;
    [SerializeField] private CanvasGroup priceCanvas;
    [SerializeField] private CanvasGroup purchaseCanvas;

    public void SetProduct(Product product)
    {
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
}
