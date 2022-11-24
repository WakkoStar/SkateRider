using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LootBoxDisplayer : MonoBehaviour
{
    public UnityEvent OnBuyAccept;
    public UnityEvent OnBuyDeny;

    [SerializeField] private int lootboxPrice;
    [SerializeField] private Text lootBoxPriceText;

    [SerializeField] private ShopDisplayer shopDisplayer;
    [SerializeField] private CanvasGroup duplicateSignCanvas;
    [SerializeField] private CanvasGroup moneyGainCanvas;
    [SerializeField] private Text moneyGainText;
    [SerializeField] CustomableDisplayer customableDisplayer;

    private void Start()
    {
        lootBoxPriceText.text = "" + lootboxPrice;
    }

    public void BuyLootBox()
    {
        var collectableCount = PlayerPrefs.GetInt("collectibleCount");

        if (collectableCount >= lootboxPrice)
        {
            PlayerPrefs.SetInt("collectibleCount", collectableCount - lootboxPrice);

            var selectedCustomable = Customable.SerializeCustomable(
                shopDisplayer.shopCustomables[Random.Range(0, shopDisplayer.shopCustomables.Count)]
            );
            selectedCustomable.price = -1;
            customableDisplayer.DeserializeCustomable(selectedCustomable);

            var inventory = DataManager.LoadData<SerializableCustomable>("inventory");
            var gain = Random.Range(100, 1501);
            var isDuplicate = inventory.Find(customable => customable.nameId == selectedCustomable.nameId) != null;

            SetDuplicateInterface(isDuplicate, gain);

            if (!isDuplicate)
            {
                DataManager.AddToData<SerializableCustomable>(selectedCustomable, "inventory");
            }
            else
            {
                PlayerPrefs.SetInt("collectibleCount", collectableCount - lootboxPrice + gain);
            }


            OnBuyAccept.Invoke();
        }
        else
        {
            OnBuyDeny.Invoke();
        }
    }


    void SetDuplicateInterface(bool isDuplicate, int gain = 0)
    {
        if (isDuplicate)
        {
            duplicateSignCanvas.alpha = 1;
            moneyGainCanvas.alpha = 1;
            moneyGainText.text = "+" + gain;
        }
        else
        {
            duplicateSignCanvas.alpha = 0;
            moneyGainCanvas.alpha = 0;

        }
    }


}
