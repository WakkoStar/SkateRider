using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LootBoxDisplayer : MonoBehaviour
{
    public UnityEvent OnBuyAccept;
    public UnityEvent OnBuyDeny;

    [SerializeField] private ShopDisplayer shopDisplayer;
    [SerializeField] private CanvasGroup duplicateSignCanvas;
    [SerializeField] private CanvasGroup moneyGainCanvas;
    [SerializeField] private Text moneyGainText;
    [SerializeField] ProductDisplayer productDisplayer;

    public void BuyLootBox()
    {
        var collectableCount = PlayerPrefs.GetInt("collectibleCount");
        if (collectableCount >= 10)
        {
            duplicateSignCanvas.alpha = 0;
            moneyGainCanvas.alpha = 0;
            PlayerPrefs.SetInt("collectibleCount", collectableCount - 10);

            var selectedProduct = shopDisplayer.SerializeProduct(shopDisplayer.products[Random.Range(0, shopDisplayer.products.Length)]);
            selectedProduct.price = -1;
            productDisplayer.SetProduct(selectedProduct);

            var inventory = DataManager.LoadData<SerializableProduct>("inventory");
            if (inventory.Find(item => item.nameId == selectedProduct.nameId) == null)
            {
                DataManager.AddToData<SerializableProduct>(selectedProduct, "inventory");
            }
            else
            {
                var gain = Random.Range(100, 1501);
                PlayerPrefs.SetInt("collectibleCount", collectableCount + gain);
                moneyGainCanvas.alpha = 1;
                moneyGainText.text = "+" + gain;
                duplicateSignCanvas.alpha = 1;
            }


            OnBuyAccept.Invoke();
            //AFFICHER CONFIRMATION
        }
        else
        {
            OnBuyDeny.Invoke();
            //AFFICHER MESSAGE D'ERREUR
        }
    }
}
