using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject ProductElement;
    public Product[] products;
    private List<Product> _filteredProducts;

    public UnityEvent OnBuyAccept;
    public UnityEvent OnBuyDeny;

    private Product _wantedProduct;

    private List<Product> _inventory;
    // Start is called before the first frame update
    void Start()
    {
        _inventory = DataManager.LoadData<Product>();

        products = new Product[] {
            new Product(ProductType.Wheels, "null", "null", 5000, "wheels-test"),
            new Product(ProductType.Trucks, "null", "null", 5000, "trucks-test"),
            new Product(ProductType.Trucks, "null", "null", 5000, "trucks-2-test"),
            new Product(ProductType.Board, "null", "null", 5000, "board-test"),
            new Product(ProductType.Grip, "null", "null", 5000, "grip-test"),
        };

        DisplayAll();
    }

    public void FilterBy(int type)
    {
        DisplayProducts(products.ToList().FindAll(p => p.type == (ProductType)type));
    }

    public void DisplayAll()
    {
        DisplayProducts(products.ToList());
    }

    public void DisplayProducts(List<Product> filteredProducts)
    {
        DestroyChilds();

        foreach (var product in filteredProducts)
        {
            var productDisplayer = Instantiate(ProductElement, transform).GetComponent<ProductDisplayer>();
            productDisplayer.SetProduct(product);
            productDisplayer.gameObject.GetComponent<Button>().onClick.AddListener(() => Buy(product));

            //IF PLAYER PREFS CONTAINS 
            if (_inventory.Find(item => product.nameId == item.nameId) != null)
            {
                productDisplayer.SetPurchased();
            }
        }
    }

    void DestroyChilds()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    void Buy(Product product)
    {
        var collectableCount = PlayerPrefs.GetInt("collectibleCount");
        if (collectableCount >= product.price)
        {
            OnBuyAccept.Invoke();
            _wantedProduct = product;
            //AFFICHER CONFIRMATION
        }
        else
        {
            OnBuyDeny.Invoke();
            //AFFICHER MESSAGE D'ERREUR
        }
    }

    public void UpdateShop()
    {
        _inventory = DataManager.LoadData<Product>();
        DisplayAll();
    }

    public void ConfirmBuy()
    {
        PlayerPrefs.SetInt("collectibleCount", PlayerPrefs.GetInt("collectibleCount") - _wantedProduct.price);
        _wantedProduct.price = -1;
        _inventory.Add(_wantedProduct);
        _wantedProduct = null;

        var inventoryWrapper = new Wrapper<Product>();
        inventoryWrapper.items = _inventory;
        DataManager.SaveData<Product>(inventoryWrapper);

        DisplayAll();
    }
}
