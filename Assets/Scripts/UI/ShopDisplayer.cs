using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(GridCustomableDisplayer))]
public class ShopDisplayer : MonoBehaviour
{
    public List<Customable> shopCustomables;

    public UnityEvent OnBuyAccept;
    public UnityEvent OnBuyDeny;

    private SerializableCustomable _customableToBuy;
    private GridCustomableDisplayer _gridCustomableDisplayer;
    private List<SerializableCustomable> _inventory;

    // Start is called before the first frame update
    void Start()
    {
        _gridCustomableDisplayer = GetComponent<GridCustomableDisplayer>();
        _gridCustomableDisplayer.OnClickOnElement = Buy;
        _gridCustomableDisplayer.OnDisplayElement = SetShopCustomableDisplayer;
        _gridCustomableDisplayer.customables = Customable.SerializeCustomables(shopCustomables);

        UpdateShop();
    }

    public void UpdateShop()
    {
        _inventory = DataManager.LoadData<SerializableCustomable>("inventory");
        _gridCustomableDisplayer.UpdateGrid();
    }

    void SetShopCustomableDisplayer(SerializableCustomable customable, CustomableDisplayer customableDisplayer)
    {
        if (_inventory.Find(item => customable.nameId == item.nameId) != null)
        {
            customableDisplayer.SetPurchased();
        }
    }

    void Buy(SerializableCustomable customable)
    {
        var collectableCount = PlayerPrefs.GetInt("collectibleCount");
        if (collectableCount >= customable.price)
        {
            OnBuyAccept.Invoke();
            _customableToBuy = customable;
        }
        else
        {
            OnBuyDeny.Invoke();
        }
    }



    public void ConfirmBuy()
    {
        PlayerPrefs.SetInt("collectibleCount", PlayerPrefs.GetInt("collectibleCount") - _customableToBuy.price);

        _customableToBuy.price = -1;
        DataManager.AddToData<SerializableCustomable>(_customableToBuy, "inventory");

        _inventory = DataManager.LoadData<SerializableCustomable>("inventory");
    }

}
