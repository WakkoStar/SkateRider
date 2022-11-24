using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject ProductElement;
    private SerializableProduct[] _products;
    private List<Product> _filteredProducts;

    [SerializeField] private Product[] defaultInventory;
    [SerializeField] private Material WheelMat;
    [SerializeField] private Material TruckMat;
    [SerializeField] private Material GripMat;
    [SerializeField] private Material BoardMat;

    // Start is called before the first frame update
    void Start()
    {
        _products = GetInventory().ToArray();
        DisplayAll();
    }

    public void UpdateInventory()
    {
        _products = GetInventory().ToArray();
        DisplayAll();
    }

    public void FilterBy(int type)
    {
        DisplayProducts(_products.ToList().FindAll(p => p.type == (ProductType)type));
    }

    public void DisplayAll()
    {
        DisplayProducts(_products.ToList());
    }

    public void DisplayProducts(List<SerializableProduct> filteredProducts)
    {
        DestroyChilds();

        foreach (var product in filteredProducts)
        {
            var productDisplayer = Instantiate(ProductElement, transform).GetComponent<ProductDisplayer>();
            productDisplayer.SetSelected(IsProductSelected(product));
            productDisplayer.SetProduct(product);

            productDisplayer.gameObject.GetComponent<Button>().onClick.AddListener(() => SelectObject(product));


        }
    }

    void DestroyChilds()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    void SelectObject(SerializableProduct item)
    {
        var texToApply = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        texToApply.LoadRawTextureData(item.textureToApply);
        texToApply.Apply();

        switch (item.type)
        {
            case ProductType.Wheels:
                WheelMat.mainTexture = texToApply;
                PlayerPrefs.SetString("skate_wheels", item.nameId);
                break;
            case ProductType.Trucks:
                TruckMat.mainTexture = texToApply;
                PlayerPrefs.SetString("skate_trucks", item.nameId);
                break;
            case ProductType.Board:
                BoardMat.mainTexture = texToApply;
                PlayerPrefs.SetString("skate_board", item.nameId);
                break;
            case ProductType.Grip:
                GripMat.mainTexture = texToApply;
                PlayerPrefs.SetString("skate_grip", item.nameId);
                break;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            var productDisplayer = transform.GetChild(i).gameObject.GetComponent<ProductDisplayer>();
            productDisplayer.SetSelected(IsProductSelected(productDisplayer.GetProduct()));
        }
    }

    List<SerializableProduct> GetInventory()
    {
        var inventory = DataManager.LoadData<SerializableProduct>("inventory");

        if (inventory == null)
        {
            var inventoryWrapper = new Wrapper<SerializableProduct>();

            inventoryWrapper.items = defaultInventory.Select(item =>
            {
                return new SerializableProduct(
                item.type,
                item.textureToApply.GetRawTextureData(),
                item.thumbnail.GetRawTextureData(),
                item.price,
                item.nameId
                );

            }).ToList();

            foreach (var item in inventoryWrapper.items)
            {
                SelectObject(item);
            }

            DataManager.SaveData<SerializableProduct>(inventoryWrapper, "inventory");

            inventory = inventoryWrapper.items;
        }
        else
        {
            var selectedItemsInInventory = inventory.FindAll(item => IsProductSelected(item));
            foreach (var item in selectedItemsInInventory)
            {
                SelectObject(item);
            }
        }

        return inventory;
    }

    private bool IsProductSelected(SerializableProduct product)
    {
        var isSelected = false;
        switch (product.type)
        {
            case ProductType.Wheels:
                isSelected = PlayerPrefs.GetString("skate_wheels") == product.nameId;
                break;
            case ProductType.Trucks:
                isSelected = PlayerPrefs.GetString("skate_trucks") == product.nameId;
                break;
            case ProductType.Board:
                isSelected = PlayerPrefs.GetString("skate_board") == product.nameId;
                break;
            case ProductType.Grip:
                isSelected = PlayerPrefs.GetString("skate_grip") == product.nameId;
                break;
        }

        return isSelected;
    }
}
