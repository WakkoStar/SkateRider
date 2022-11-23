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

    public void BuyLootBox()
    {
        var collectableCount = PlayerPrefs.GetInt("collectibleCount");
        if (collectableCount >= 1000)
        {
            duplicateSignCanvas.alpha = 0;
            moneyGainCanvas.alpha = 0;
            PlayerPrefs.SetInt("collectibleCount", collectableCount - 1000);

            var selectedProduct = shopDisplayer.products[Random.Range(0, shopDisplayer.products.Length)];

            var inventory = DataManager.LoadData<Product>();
            if (inventory.Find(item => item.nameId == selectedProduct.nameId) == null)
            {
                var inventoryWrapper = new Wrapper<Product>();
                selectedProduct.price = -1;
                inventory.Add(selectedProduct);
                inventoryWrapper.items = inventory;

                DataManager.SaveData<Product>(inventoryWrapper);
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
