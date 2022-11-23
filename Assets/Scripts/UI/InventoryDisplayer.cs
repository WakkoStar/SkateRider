using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject ProductElement;
    private Product[] _products;
    private List<Product> _filteredProducts;

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

    public void DisplayProducts(List<Product> filteredProducts)
    {
        DestroyChilds();

        foreach (var product in filteredProducts)
        {
            var productDisplayer = Instantiate(ProductElement, transform).GetComponent<ProductDisplayer>();
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

    void SelectObject(Product item)
    {
        //APPLY ITEM TO SKATE
    }

    List<Product> GetInventory()
    {
        var inventory = DataManager.LoadData<Product>();

        if (inventory == null)
        {
            var defaultInventory = new Product[] {
                new Product(ProductType.Wheels, "null", "null", -1, "wheels-default"),
                new Product(ProductType.Trucks, "null", "null", -1, "trucks-default"),
                new Product(ProductType.Board, "null", "null", -1, "board-default"),
                new Product(ProductType.Grip, "null", "null", -1, "grip-default"),
            };

            var inventoryWrapper = new Wrapper<Product>();
            inventoryWrapper.items = defaultInventory.ToList();
            DataManager.SaveData<Product>(inventoryWrapper);

            inventory = defaultInventory.ToList();
        }

        return inventory;
    }
}
